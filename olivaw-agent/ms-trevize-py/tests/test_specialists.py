from concurrent.futures import Future, ThreadPoolExecutor
from threading import Event
import unittest

from ms_trevize.worker import Specialists


class SpecialistsTests(unittest.TestCase):
  def test_blocked_collection_does_not_block_contact_and_mail_read(self):
    blocked, contact, read = Event(), Event(), Event()
    executor = ThreadPoolExecutor(max_workers=3)
    self.addCleanup(executor.shutdown)
    self.addCleanup(blocked.set)
    jobs = {'prospecting':(lambda:blocked.wait(5),86400,True),
      'first-contact':(lambda:contact.set(),60,True),'follow-up':(lambda:read.set(),900,True)}
    specialist = Specialists(executor,jobs)
    specialist.tick()
    self.assertTrue(contact.wait(1))
    self.assertTrue(read.wait(1))
    self.assertFalse(specialist.pending['prospecting'].done())
    blocked.set()

  def test_independent_deadlines_never_overlap_and_do_not_reset_after_slow_job(self):
    class Executor:
      def __init__(self): self.calls = []
      def submit(self, operation):
        future = Future(); self.calls.append((operation,future)); return future
    executor = Executor(); clock = [0]
    operations = [lambda:None for _ in range(3)]
    jobs = dict(zip(('prospecting','first-contact','follow-up'),zip(operations,(86400,60,900),(True,True,True))))
    specialist = Specialists(executor,jobs,clock=lambda:clock[0])
    specialist.tick(); self.assertEqual(3,len(executor.calls))
    clock[0]=900
    specialist.tick(); self.assertEqual(3,len(executor.calls))  # tarefas em curso não sobrepõem.
    executor.calls[2][1].set_result({'read':1})
    specialist.tick(); self.assertEqual(4,len(executor.calls))
    self.assertEqual(operations[2],executor.calls[3][0])
    self.assertEqual(86400,specialist.next_due['prospecting'])
    self.assertEqual(1800,specialist.next_due['follow-up'])

  def test_manual_single_collection_and_disabled_commercial_jobs_stay_idle(self):
    class Executor:
      def __init__(self): self.calls = []
      def submit(self, operation):
        future = Future(); future.set_result({}); self.calls.append(operation); return future
    executor=Executor();clock=[0]
    jobs={'prospecting':(lambda:None,86400,True),'follow-up':(lambda:None,900,False)}
    specialist=Specialists(executor,jobs,clock=lambda:clock[0],once=('prospecting',))
    specialist.tick();clock[0]=999999;specialist.tick()
    self.assertEqual(1,len(executor.calls))
