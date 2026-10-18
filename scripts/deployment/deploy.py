#!/usr/bin/python3
"""Restricted SSH gateway and detached deployment worker. No shell execution."""
import fcntl
import json
import os
from pathlib import Path
import re
import signal
import subprocess
import sys
import tempfile
import uuid

CONFIG = Path('/etc/olivaw/deployment.json')
ENVIRONMENTS = {'dev', 'qas', 'stg', 'rls', 'prd'}


class DeploymentNotFound(LookupError):
    pass


def validate_request(request):
    if not isinstance(request, dict):
        raise ValueError('invalid_request')
    folder, branch, environment = (request.get(key) for key in ('folder', 'branch', 'environment'))
    if not isinstance(folder, str) or len(folder) > 200:
        raise ValueError('invalid_request')
    if not re.fullmatch(r'[a-zA-Z0-9_-]+(/[a-zA-Z0-9_-]+)*', folder):
        raise ValueError('invalid_request')
    if not isinstance(branch, str) or len(branch) > 200:
        raise ValueError('invalid_request')
    if not re.fullmatch(r'[a-zA-Z0-9][a-zA-Z0-9._/-]*', branch):
        raise ValueError('invalid_request')
    if '..' in branch or branch == 'HEAD':
        raise ValueError('invalid_request')
    if any(not part or part.startswith('.') or part.endswith('.') or part.lower().endswith('.lock')
           for part in branch.split('/')):
        raise ValueError('invalid_request')
    if not isinstance(environment, str) or environment not in ENVIRONMENTS:
        raise ValueError('invalid_request')
    return {'folder': folder, 'branch': branch, 'environment': environment}


def safe_path(path):
    path = Path(path)
    if not path.is_absolute():
        raise ValueError('invalid_request')
    for part in [path, *path.parents]:
        if part.is_symlink():
            raise ValueError('invalid_request')
    return path.resolve(strict=True)


def resolve_repository(config, request):
    request = validate_request(request)
    if request['folder'] not in config['allowedFolders']:
        raise ValueError('invalid_request')
    root = safe_path(config['rootDirectory'])
    target = root / request['folder']
    # A pasta do serviço pode ainda não existir; seus ancestrais não podem ser links.
    for parent in [target, *target.parents]:
        if parent.is_symlink():
            raise ValueError('invalid_request')
    for parent in [target, *target.parents]:
        if parent == root:
            break
        git = parent / '.git'
        if git.is_dir():
            safe_path(git)
            return safe_path(parent)
    raise ValueError('invalid_request')


def resolve_folder(config, request):
    resolve_repository(config, request)
    directory = safe_path(Path(config['rootDirectory']) / request['folder'])
    if not directory.is_dir():
        raise ValueError('invalid_request')
    return directory


def systemd_environment():
    environment = dict(os.environ)
    environment['XDG_RUNTIME_DIR'] = f'/run/user/{os.getuid()}'
    environment['DBUS_SESSION_BUS_ADDRESS'] = f'unix:path={environment["XDG_RUNTIME_DIR"]}/bus'
    environment['PATH'] = '/usr/local/bin:/usr/bin:/bin'
    environment['GIT_TERMINAL_PROMPT'] = '0'
    environment['GCM_INTERACTIVE'] = 'Never'
    return environment


class Executor:
    def __init__(self, config):
        self.config = config
        self.state = Path(config['stateDirectory'])
        self.state.mkdir(parents=True, exist_ok=True, mode=0o700)
        safe_path(self.state)
        if self.state.stat().st_mode & 0o077:
            raise OSError('State directory must be private')

    def path(self, job_id):
        job_id = str(uuid.UUID(str(job_id)))
        return self.state / f'{job_id}.json'

    def save(self, job):
        with tempfile.NamedTemporaryFile(mode='w', dir=self.state, delete=False) as output:
            json.dump(job, output)
            output.flush()
            os.fsync(output.fileno())
        os.replace(output.name, self.path(job['id']))

    def load(self, job_id):
        path = self.path(job_id)
        if not path.exists():
            raise DeploymentNotFound('not_found')
        safe_path(path)
        return json.loads(path.read_text())

    @staticmethod
    def public(job):
        return {key: job.get(key, '') for key in ('id', 'status', 'log', 'folder')}

    def submit(self, request):
        request = validate_request(request)
        resolve_repository(self.config, request)
        return self.enqueue(request)

    def submit_cleanup(self, request):
        if set(request) != {'action'}:
            raise ValueError('invalid_request')
        return self.enqueue({'kind': 'disk-cleanup'})

    def enqueue(self, request):
        with (self.state / 'submit.lock').open('a') as lock:
            fcntl.flock(lock, fcntl.LOCK_EX)
            pending = sum(self.status(path.stem)['status'] in ('queued', 'running')
                          for path in self.state.glob('*.json'))
            if pending >= 20:
                raise RuntimeError('executor_busy')
            initial_log = 'Deployment queued.'
            if request.get('kind') == 'disk-cleanup':
                initial_log = 'Disk cleanup queued.'
            job = dict(request, id=str(uuid.uuid4()), status='queued', log=initial_log)
            self.save(job)
            try:
                subprocess.run([
                    '/usr/bin/systemd-run', '--user', '--quiet', '--collect',
                    f'--unit=olivaw-deploy-{job["id"]}', '--property=RuntimeMaxSec=1800',
                    '--', '/usr/bin/python3', str(Path(__file__).resolve()), 'run', job['id']
                ], check=True, timeout=15, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL,
                    env=systemd_environment())
            except (OSError, subprocess.SubprocessError):
                job.update(status='failed', log='Unable to schedule deployment worker.')
                self.save(job)
                raise RuntimeError('executor_unavailable')
        return self.public(job)

    def status(self, job_id):
        job = self.load(job_id)
        if job['status'] in ('queued', 'running'):
            result = subprocess.run([
                '/usr/bin/systemctl', '--user', 'is-active', f'olivaw-deploy-{job["id"]}.service'
            ], capture_output=True, text=True, timeout=10, env=systemd_environment())
            if result.returncode not in (0, 3, 4):
                raise RuntimeError('executor_unavailable')
            if result.stdout.strip() not in ('active', 'activating', 'reloading'):
                job = self.load(job_id)
                if job['status'] in ('queued', 'running'):
                    job.update(status='failed', log=job['log'] + '\nWorker interrupted or expired.')
        return self.public(job)

    def run(self, job_id):
        with (self.state / 'deployment.lock').open('a') as lock:
            fcntl.flock(lock, fcntl.LOCK_EX)
            job = self.load(job_id)
            if job['status'] != 'queued':
                return
            initial_log = 'Deployment started.'
            if job.get('kind') == 'disk-cleanup':
                initial_log = 'Disk cleanup worker started.'
            job.update(status='running', log=initial_log)
            self.save(job)
            try:
                if job.get('kind') == 'disk-cleanup':
                    self.run_cleanup(job)
                    return
                repository = resolve_repository(self.config, job)
                project_name = None
                if job['folder'].startswith('olivaw-hub/'):
                    service_name = Path(job['folder']).name.lower()
                    project_name = f'{service_name}-{job["environment"]}'
                for label, arguments in commands(
                    job['branch'],
                    job['environment'],
                    job['folder'] in self.config.get('sshOverrideFolders', []),
                    project_name
                ):
                    directory = repository
                    if label == 'Compose':
                        if project_name:
                            job['log'] += f'\nCompose project: {project_name}.'
                            self.save(job)
                        job['log'] += '\nCompose: validating service directory after Git update.'
                        self.save(job)
                        directory = resolve_folder(self.config, job)
                        if resolve_repository(self.config, job) != repository:
                            raise ValueError('invalid_request')
                        for filename in [f'docker/.env.{job["environment"]}', 'docker/docker-compose.yml',
                                         f'docker/docker-compose.{job["environment"]}.yml']:
                            if not safe_path(directory / filename).is_file():
                                raise ValueError('invalid_request')
                        if project_name:
                            job['log'] += '\nCompose: removing this service from the legacy shared project if present.'
                            self.save(job)
                            cleanup = compose_command(
                                job['environment'], 'docker',
                                job['folder'] in self.config.get('sshOverrideFolders', [])
                            )
                            cleanup.extend(['rm', '--stop', '--force'])
                            result = run_step(
                                cleanup, directory, int(self.config.get('stepTimeoutSeconds', 300))
                            )
                            job['log'] += f'\nLegacy Compose cleanup: exit code {result}.'
                            self.save(job)
                            if result != 0:
                                raise RuntimeError('step_failed')
                    job['log'] += f'\n{label}: started.'
                    self.save(job)
                    result = run_step(arguments, directory, int(self.config.get('stepTimeoutSeconds', 300)))
                    job['log'] += f'\n{label}: exit code {result}.'
                    self.save(job)
                    if result != 0:
                        raise RuntimeError('step_failed')
                job['status'] = 'succeeded'
            except (ValueError, OSError, RuntimeError, subprocess.SubprocessError):
                job.update(status='failed', log=job['log'] + '\nDeployment failed; subsequent steps were skipped.')
            self.save(job)


    def run_cleanup(self, job):
        try:
            before = os.statvfs('/')
            job['log'] += f"\nDisk cleanup started. Free bytes before: {before.f_bavail * before.f_frsize}."
            self.save(job)
            for label, arguments in cleanup_commands():
                job['log'] += f'\n{label}: started.'
                self.save(job)
                result = run_step(arguments, Path('/'), int(self.config.get('stepTimeoutSeconds', 300)))
                job['log'] += f'\n{label}: exit code {result}.'
                self.save(job)
                if result != 0:
                    raise RuntimeError('step_failed')
            after = os.statvfs('/')
            job['log'] += f"\nFree bytes after: {after.f_bavail * after.f_frsize}."
            reclaimed = max(0, after.f_bavail * after.f_frsize - before.f_bavail * before.f_frsize)
            job['log'] += f"\nReclaimed bytes: {reclaimed}."
            job['status'] = 'succeeded'
        except (ValueError, OSError, RuntimeError, subprocess.SubprocessError):
            job.update(status='failed', log=job['log'] + '\nDisk cleanup failed; subsequent steps were skipped.')
        self.save(job)


def cleanup_commands():
    # Fixed argv only. Preserve volumes, containers, networks and active log files.
    return [
        ('Build cache cleanup', ['/usr/bin/docker', 'builder', 'prune', '--all', '--force']),
        ('Unused image cleanup', ['/usr/bin/docker', 'image', 'prune', '--all', '--force']),
        ('Aged temporary files cleanup', ['/usr/bin/sudo', '-n', '/usr/bin/systemd-tmpfiles', '--clean', '--prefix=/tmp', '--prefix=/var/tmp']),
        ('Archived journal cleanup', ['/usr/bin/sudo', '-n', '/usr/bin/journalctl', '--vacuum-time=7d', '--vacuum-size=100M'])
    ]


def compose_command(environment, project_name=None, include_ssh=False):
    compose = ['/usr/bin/docker', 'compose']
    if project_name:
        compose.extend(['--project-name', project_name])
    compose.extend([
        '--env-file', f'docker/.env.{environment}',
        '-f', 'docker/docker-compose.yml', '-f', f'docker/docker-compose.{environment}.yml'
    ])
    if include_ssh:
        compose.extend(['-f', 'docker/docker-compose.ssh.yml'])
    return compose


def commands(branch, environment, include_ssh=False, project_name=None):
    compose = compose_command(environment, project_name, include_ssh)
    compose.extend(['up', '-d', '--build'])
    return [
        ('Fetch', ['/usr/bin/git', 'fetch', 'origin']),
        ('Checkout', ['/usr/bin/git', 'checkout', branch]),
        ('Pull', ['/usr/bin/git', 'pull', '--ff-only', 'origin', branch]),
        ('Compose', compose)
    ]


def run_step(arguments, directory, timeout):
    with subprocess.Popen(arguments, cwd=directory, stdin=subprocess.DEVNULL, stdout=subprocess.DEVNULL,
                          stderr=subprocess.DEVNULL, env=systemd_environment(), start_new_session=True) as process:
        try:
            return process.wait(timeout=timeout)
        except subprocess.TimeoutExpired:
            try:
                os.killpg(process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
            process.wait()
            raise


def gateway(executor, raw):
    try:
        if len(raw) > 4096:
            raise ValueError('invalid_request')
        request = json.loads(raw)
        if not isinstance(request, dict):
            raise ValueError('invalid_request')
        if request.get('action') == 'submit':
            status = executor.submit(request)
        elif request.get('action') == 'cleanup':
            status = executor.submit_cleanup(request)
        elif request.get('action') == 'status':
            status = executor.status(request.get('id'))
        else:
            raise ValueError('invalid_request')
        return {'success': True, 'deployment': status}
    except DeploymentNotFound:
        return {'success': False, 'error': 'not_found'}
    except (ValueError, TypeError, AttributeError):
        return {'success': False, 'error': 'invalid_request'}
    except (OSError, RuntimeError, subprocess.SubprocessError):
        return {'success': False, 'error': 'executor_unavailable'}


def main():
    os.umask(0o077)
    try:
        executor = Executor(json.loads(CONFIG.read_text()))
        if len(sys.argv) == 3 and sys.argv[1] == 'run':
            executor.run(sys.argv[2])
            return
        if os.environ.get('SSH_ORIGINAL_COMMAND') != 'olivaw-deploy':
            raise ValueError('invalid_request')
        print(json.dumps(gateway(executor, sys.stdin.read(4097))))
    except (OSError, ValueError, KeyError):
        print(json.dumps({'success': False, 'error': 'executor_unavailable'}))


if __name__ == '__main__':
    main()
