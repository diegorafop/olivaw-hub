class UpdateSkillHandler:
  def __init__(self, review):
    self.review = review

  def handle(self):
    return self.review.update()
