import { CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { planOfferings, improvementAllowanceLabel } from '../plans/plan-offerings';
import { Component, inject, input } from '@angular/core';
import { ProjectDraft } from './project-draft';
@Component({
  selector: 'app-project-plans',
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './project-plans.html',
  styleUrl: './project-plans.scss',
})
export class ProjectPlans {
  readonly active = input(false);
  readonly draft = inject(ProjectDraft);
  readonly plans = planOfferings;
  readonly allowanceLabel = improvementAllowanceLabel;
}
