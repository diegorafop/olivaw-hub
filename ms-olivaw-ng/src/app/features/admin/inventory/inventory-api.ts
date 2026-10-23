import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpContext, HttpParams } from '@angular/common/http';
import { timeout } from 'rxjs';
import { RuntimeConfig } from '../../../core/config/runtime-config';
import { LOCAL_ERROR, SKIP_LOADING } from '../../../core/http/http-context';
export const resourceTypes: Record<string, string> = {
  compute: 'Computação',
  volume: 'Volume / armazenamento',
  'database-instance': 'Instância de banco',
  database: 'Banco lógico',
  'message-broker': 'Broker de mensagens',
  'message-namespace': 'Namespace de mensagens',
  'cache-instance': 'Instância de cache',
  'cache-namespace': 'Namespace de cache',
  domain: 'Domínio',
  subdomain: 'Subdomínio',
  'public-ip': 'IP público',
  'dns-zone': 'Zona DNS',
  'dns-record': 'Registro DNS',
  proxy: 'Proxy / entrada',
  certificate: 'Certificado',
  backup: 'Destino de backup',
  observability: 'Observabilidade',
  registry: 'Registro de imagens',
  'external-service': 'Serviço externo',
  other: 'Outro',
};
export const resourceStatuses: Record<string, string> = {
  reported: 'Informado',
  verified: 'Conferido',
  retired: 'Desativado',
};
export const environments: Record<string, string> = {
  development: 'Desenvolvimento',
  quality: 'Qualidade',
  release: 'Release',
  staging: 'Homologação',
  production: 'Produção',
};
export const deploymentStatuses: Record<string, string> = {
  planned: 'Planejada',
  active: 'Ativa',
  closed: 'Encerrada',
};
export const relationships: Record<string, string> = {
  'part-of': 'Faz parte de',
  'runs-on': 'Executa em',
  'stored-on': 'Armazena em',
  'exposed-by': 'Exposto por',
  'depends-on': 'Depende de',
};
export interface ResourceInput {
  name: string;
  type: string;
  purpose: string;
  provider: string;
  accountReference: string;
  region: string;
  providerResourceId: string;
  owner: string;
  payer: string;
  operator: string;
  scope: string;
  status: string;
  source: string;
  monitoring: string;
  referenceUrl: string;
  cpuCapacity: number | null;
  memoryCapacityGiB: number | null;
  storageCapacityGiB: number | null;
}
export interface ResourceOrigin {
  kind: string;
  fromResourceExternalId: string | null;
  fromResourceName: string;
  deploymentExternalId: string | null;
  serviceName: string;
  environment: string;
  relationshipKind: string;
  purpose: string;
}
export interface ResourceItem {
  externalId: string;
  technicalId: string;
  name: string;
  type: string;
  status: string;
  provider: string;
  customerCount: number;
  originCount: number;
  origins: ResourceOrigin[];
}
export interface ResourceDetail extends ResourceItem, ResourceInput {
  revision: number;
  providerIdentityLocked: boolean;
  verifiedAt: string | null;
  customers: { externalId: string; business: string }[];
  reservedCpu: number | null;
  reservedMemoryGiB: number | null;
  reservedStorageGiB: number | null;
  incompleteReservations: number;
  history: (ResourceInput & { revision: number; actorName: string; createdAt: string })[];
}
export interface Page<T> {
  page: number;
  pageSize: number;
  total: number;
  items: T[];
}
export interface ResourceFilters {
  search?: string;
  type?: string;
  provider?: string;
  status?: string;
  environment?: string;
  customerExternalId?: string;
  shared?: string;
}
export interface DeploymentInput {
  name: string;
  location: string;
  endpoint: string;
  status: string;
}
export interface DeploymentItem {
  externalId: string;
  serviceExternalId: string;
  serviceName: string;
  name: string;
  environment: string;
  status: string;
  endedAt: string | null;
}
export interface DeploymentDetail extends DeploymentItem, DeploymentInput {
  revision: number;
  history: (DeploymentInput & { revision: number; createdAt: string; actorName: string })[];
}
export type LinkKind = 'customer' | 'dependency' | 'deployment';
export interface LinkInput {
  resourceExternalId: string;
  targetExternalId: string;
  purpose: string;
  kind?: string;
  reservedCpu?: number | null;
  reservedMemoryGiB?: number | null;
  reservedStorageGiB?: number | null;
}
export interface InventoryLink {
  externalId: string;
  resourceExternalId: string;
  resourceName: string;
  targetExternalId: string;
  targetName: string;
  relationshipKind: string;
  environment: string;
  purpose: string;
  revision: number;
  startedAt: string;
  endedAt: string | null;
  startedByName: string;
  endedByName: string | null;
  reservedCpu: number | null;
  reservedMemoryGiB: number | null;
  reservedStorageGiB: number | null;
}
export function inventoryError(error: unknown): string {
  const value = error as { error?: { detail?: string; errors?: Record<string, string[]> } };
  const problem = value?.error;
  if (problem?.errors) return Object.values(problem.errors).flat().join(' ');
  return (
    problem?.detail ??
    'Não foi possível concluir a operação. Os campos foram mantidos; confira os dados antes de tentar novamente.'
  );
}
@Injectable({ providedIn: 'root' })
export class InventoryApi {
  private readonly http = inject(HttpClient);
  private readonly config = inject(RuntimeConfig);
  private options(params?: HttpParams) {
    return { context: new HttpContext().set(LOCAL_ERROR, true).set(SKIP_LOADING, true), params };
  }
  private params(values: Record<string, string | number | boolean | undefined>) {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(values))
      if (value !== undefined && value !== '') params = params.set(key, value);
    return params;
  }
  resources(page: number, filters: ResourceFilters = {}) {
    return this.http
      .get<Page<ResourceItem>>(
        this.config.endpoint('resources'),
        this.options(this.params({ page, pageSize: 20, ...filters })),
      )
      .pipe(timeout(15000));
  }
  resource(id: string) {
    return this.http
      .get<ResourceDetail>(
        this.config.endpoint(`resources/${encodeURIComponent(id)}`),
        this.options(),
      )
      .pipe(timeout(15000));
  }
  createResource(technicalId: string, input: ResourceInput) {
    return this.http
      .post<{ externalId: string; revision: number }>(
        this.config.endpoint('resources'),
        { technicalId, ...input },
        this.options(),
      )
      .pipe(timeout(15000));
  }
  updateResource(id: string, revision: number, input: ResourceInput) {
    return this.http
      .put<{ externalId: string; revision: number }>(
        this.config.endpoint(`resources/${encodeURIComponent(id)}`),
        { ...input, revision },
        this.options(),
      )
      .pipe(timeout(15000));
  }
  deployments(page: number, serviceExternalId = '', environment = '') {
    return this.http
      .get<Page<DeploymentItem>>(
        this.config.endpoint('deployments'),
        this.options(this.params({ page, pageSize: 20, serviceExternalId, environment })),
      )
      .pipe(timeout(15000));
  }
  deployment(id: string) {
    return this.http
      .get<DeploymentDetail>(
        this.config.endpoint(`deployments/${encodeURIComponent(id)}`),
        this.options(),
      )
      .pipe(timeout(15000));
  }
  createDeployment(serviceExternalId: string, environment: string, input: DeploymentInput) {
    return this.http
      .post<{ externalId: string; revision: number }>(
        this.config.endpoint('deployments'),
        { serviceExternalId, environment, ...input },
        this.options(),
      )
      .pipe(timeout(15000));
  }
  updateDeployment(id: string, revision: number, input: DeploymentInput) {
    return this.http
      .put<{ externalId: string; revision: number }>(
        this.config.endpoint(`deployments/${encodeURIComponent(id)}`),
        { ...input, revision },
        this.options(),
      )
      .pipe(timeout(15000));
  }
  links(
    page: number,
    kind: LinkKind,
    anchor: { resourceExternalId?: string; deploymentExternalId?: string },
    includeEnded: boolean,
  ) {
    return this.http
      .get<Page<InventoryLink>>(
        this.config.endpoint('inventory-links'),
        this.options(this.params({ page, pageSize: 20, kind, ...anchor, includeEnded })),
      )
      .pipe(timeout(15000));
  }
  private linkRoute(kind: LinkKind) {
    return {
      customer: 'resource-customers',
      dependency: 'resource-dependencies',
      deployment: 'deployment-resources',
    }[kind];
  }
  createLink(kind: LinkKind, input: LinkInput) {
    return this.http
      .post<{ externalId: string; revision: number }>(
        this.config.endpoint(this.linkRoute(kind)),
        input,
        this.options(),
      )
      .pipe(timeout(15000));
  }
  closeLink(kind: LinkKind, id: string, revision: number) {
    return this.http
      .put<{ externalId: string; revision: number }>(
        this.config.endpoint(`${this.linkRoute(kind)}/${encodeURIComponent(id)}`),
        { revision },
        this.options(),
      )
      .pipe(timeout(15000));
  }
}
