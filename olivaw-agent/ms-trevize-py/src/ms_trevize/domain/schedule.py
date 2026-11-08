"""Agenda em memória, criada exclusivamente por um start do operador."""
class Schedule:
  def __init__(self, hours, now, repeat=True):
    self.interval = hours * 3600
    self.repeat = repeat
    self.next_due = now
    self.completed = False

  def due(self, now):
    return (self.repeat or not self.completed) and now >= self.next_due

  def finish(self, now):
    self.completed = True
    self.next_due = now + self.interval
