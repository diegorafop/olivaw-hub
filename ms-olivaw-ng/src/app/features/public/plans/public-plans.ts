import { Component, computed, inject, effect, DestroyRef } from '@angular/core';
import { CurrencyPipe, DOCUMENT } from '@angular/common';
import { Meta } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { Breadcrumb } from '../../../shared/breadcrumb';
import {
  planOfferings,
  projectScopes,
  creationOfferTerms,
  improvementPointRules,
  improvementExamples,
  improvementAllowanceLabel,
} from './plan-offerings';
@Component({
  selector: 'app-public-plans',
  imports: [CurrencyPipe, RouterLink, Breadcrumb],
  templateUrl: './public-plans.html',
  styleUrl: './public-plans.scss',
})
export class PublicPlans {
  private readonly params = toSignal(inject(ActivatedRoute).paramMap);
  readonly plans = planOfferings;
  readonly scopes = projectScopes;
  readonly terms = creationOfferTerms;
  readonly pointRules = improvementPointRules;
  readonly pointExamples = improvementExamples;
  readonly allowanceLabel = improvementAllowanceLabel;
  readonly isDetail = computed(() => !!this.params()?.get('planId'));
  readonly selected = computed(() =>
    this.plans.find((plan) => plan.id === this.params()?.get('planId')),
  );
  readonly breadcrumbs = computed(() => [
    { label: 'Início', url: '/' },
    { label: 'Planos', url: '/planos' },
    ...(this.isDetail()
      ? [
          {
            label: this.selected()?.name || 'Plano não encontrado',
            url: '/planos/' + this.params()?.get('planId'),
          },
        ]
      : []),
  ]);
  constructor() {
    const doc = inject(DOCUMENT);
    const meta = inject(Meta);
    const destroy = inject(DestroyRef);
    const canonical = doc.querySelector<HTMLLinkElement>('link[rel="canonical"]');
    const oldCanonical = canonical?.getAttribute('href');
    const selectors = [
      'name="description"',
      'property="og:title"',
      'property="og:description"',
      'property="og:url"',
      'name="robots"',
    ];
    const originals = selectors.map((selector) => ({
      selector,
      content: meta.getTag(selector)?.content,
    }));
    effect(() => {
      const plan = this.selected();
      const missing = this.isDetail() && !plan;
      const url = 'https://olivaw.com.br/planos' + (plan ? '/' + plan.id : '');
      const title = 'Olivaw - ' + (plan?.name || 'Planos');
      const description = plan
        ? plan.text + ' ' + this.allowanceLabel(plan.id) + '.'
        : 'Conheça os planos Olivaw, a criação incluída em projetos elegíveis e uma forma transparente de evoluir seu produto.';
      canonical?.setAttribute('href', url);
      meta.updateTag({ name: 'description', content: description });
      meta.updateTag({ property: 'og:title', content: title });
      meta.updateTag({ property: 'og:description', content: description });
      meta.updateTag({ property: 'og:url', content: url });
      meta.updateTag({ name: 'robots', content: missing ? 'noindex,follow' : 'index,follow' });
    });
    destroy.onDestroy(() => {
      if (canonical && oldCanonical) canonical.setAttribute('href', oldCanonical);
      for (const item of originals) {
        if (item.content === undefined) meta.removeTag(item.selector);
        else {
          const key = item.selector.startsWith('name=') ? 'name' : 'property';
          const value = item.selector.split('"')[1];
          meta.updateTag({ [key]: value, content: item.content }, item.selector);
        }
      }
    });
  }
}
