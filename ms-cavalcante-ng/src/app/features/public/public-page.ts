import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-public-page',
  templateUrl: './public-page.html',
  styleUrl: './public-page.scss',
})
export class PublicPage {
  readonly menuOpen = signal(false);
  readonly category = signal('Todas');
  readonly categories = ['Todas', 'Movimentação de terra', 'Espaços compactos', 'Transporte'];
  readonly machines = [
    {
      name: 'Retroescavadeira',
      category: 'Movimentação de terra',
      image: 'retro.jpg',
      description: 'Escavação, carregamento e preparação de terrenos.',
      tag: 'VERSATILIDADE NA OBRA',
    },
    {
      name: 'Minicarregadeira Bobcat',
      category: 'Espaços compactos',
      image: 'bobcat.jpg',
      description: 'Movimentação de materiais e limpeza em áreas menores.',
      tag: 'COMPACTA E VERSÁTIL',
    },
    {
      name: 'Mini escavadeira',
      category: 'Espaços compactos',
      image: 'mini.jpg',
      description: 'Escavações e valas em locais com acesso limitado.',
      tag: 'PRECISÃO EM CADA ETAPA',
    },
    {
      name: 'Caminhão caçamba',
      category: 'Transporte',
      image: 'truck.jpg',
      description: 'Transporte de terra, materiais e remoção de entulho.',
      tag: 'APOIO DO INÍCIO AO FIM',
    },
    {
      name: 'Vassoura coletora Bobcat',
      category: 'Espaços compactos',
      image: 'sweeper.jpg',
      description: 'Limpeza mecanizada de pátios e áreas pavimentadas.',
      tag: 'ACABAMENTO E LIMPEZA',
    },
  ];
  readonly services = [
    {
      number: '01',
      title: 'Terraplenagem',
      text: 'Cortes, aterros e nivelamento. O primeiro passo para preparar o terreno da sua obra.',
    },
    {
      number: '02',
      title: 'Demolição',
      text: 'Demolição e remoção para abrir espaço para o próximo projeto.',
    },
    {
      number: '03',
      title: 'Perfuração de solo',
      text: 'Perfuração com equipamento específico, conforme a necessidade do terreno.',
    },
    {
      number: '04',
      title: 'Limpeza de terrenos',
      text: 'Limpeza e movimentação de materiais para deixar a área pronta para começar.',
    },
  ];
  readonly faqs = [
    {
      question: 'Não sei qual máquina preciso. Vocês podem ajudar?',
      answer:
        'Sim. Conte o que precisa fazer, informe o bairro e, se possível, envie fotos do terreno pelo WhatsApp. A equipe poderá orientar a escolha conforme a necessidade da obra.',
    },
    {
      question: 'O que preciso informar para pedir um orçamento?',
      answer:
        'Informe o serviço ou máquina, o local da obra, a data desejada e uma breve descrição do trabalho. Fotos e medidas aproximadas ajudam na avaliação. Valores e disponibilidade são confirmados pela equipe.',
    },
    {
      question: 'A locação inclui operador e transporte?',
      answer:
        'Converse com a equipe para confirmar operador, transporte, período de locação e condições do serviço antes de contratar. Essas condições dependem da proposta para sua obra.',
    },
    {
      question: 'Vocês vendem materiais para obra?',
      answer:
        'O site atual divulga terra para aterro, terra preta, brita 1, pedrisco, pó de pedra e saibro. Consulte a equipe sobre materiais, quantidades e entrega no seu endereço.',
    },
  ];
  whatsapp(subject = 'um orçamento para minha obra'): string {
    return (
      'https://wa.me/5541996544458?text=' +
      encodeURIComponent(
        `Olá, Cavalcante! Gostaria de conversar sobre ${subject}. Local da obra: ___. Data desejada: ___.`,
      )
    );
  }
  closeMenu(): void {
    this.menuOpen.set(false);
  }
}
