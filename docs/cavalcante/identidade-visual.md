# Cores e identidade visual

## Direção

**Força de máquina, clareza de atendimento.** Amarelo quente como destaque, grafite para estrutura e fundo mineral claro para leitura. A combinação remete visualmente a equipamentos de construção e ao ambiente de obra; essa é uma hipótese de reconhecimento de categoria. Não há evidência neste estudo de que amarelo, verde ou laranja aumentem vendas da Cavalcante.

As fotos existentes já trazem amarelo, terra, cinza e vegetação. A interface deve organizar esses elementos, sem competir com todos eles. Evitar textura de terra atrás de texto, várias cores de botões, sombras pesadas, faixas animadas e carrossel automático.

## Paleta escolhida

| Token | Cor | Uso |
| --- | --- | --- |
| `--cv-yellow` | `#FFCB45` | Ação principal, pequenos destaques e avatar |
| `--cv-ink` | `#252824` | Texto principal, rodapé e seção de serviços |
| `--cv-paper` | `#F7F7F2` | Fundo dominante e áreas de leitura |
| `--cv-muted` | `#5D635A` | Texto secundário em fundo claro |
| `--cv-line` | `#DCE0D5` | Separadores decorativos |
| `--cv-green` | `#35483C` | Reserva para evolução de apoio; não multiplica CTAs |

Os tokens estão no [tema do cliente](../../ms-cavalcante-ng/src/theme/_brand.scss). O Material herdado usa paletas públicas laranja/amarelo; a landing usa os tokens próprios. A fonte de sistema Arial dispensa download de fontes e mantém a primeira renderização independente de terceiros.

## Comparação de alternativas

| Direção | Vantagem de projeto | Limite |
| --- | --- | --- |
| Amarelo + grafite + mineral | Proximidade visual com máquinas; destaque localizado | Amarelo com branco tem pouco contraste; usar texto escuro |
| Laranja + carvão + areia | Tom de terra e energia | Pode competir com fotos já quentes |
| Verde + areia + grafite | Aproximação com terreno e paisagem | Não sugerir certificação ou compromisso ambiental sem comprovação |
| Azul + cinza | Aparência técnica convencional | Menor relação com a proposta de máquina amarela desta versão |

Recomendação: começar com amarelo/grafite e testar reconhecimento e legibilidade com o cliente. “Mais atrativa” depende de preferência, contexto e tarefa; a recomendação não se baseia em psicologia de cores determinista.

## Contraste e acessibilidade

Usar grafite sobre amarelo e sobre mineral; texto secundário somente em superfícies claras. Branco fica reservado ao grafite. O separador claro é decorativo e não pode ser a única indicação de estado. Botões usam rótulos e estado pressionado; links e foco não dependem só da cor.

O relatório de [validação](validacao.md) registra contrastes calculados pela fórmula sRGB da WCAG. Metas: 4,5:1 em texto comum, 3:1 em texto grande e elementos gráficos essenciais, além de foco visível. Sobre fotos, o overlay precisa ser conferido no recorte real: o cálculo de cores sólidas não comprova esse contraste.

Fonte normativa: [W3C — WCAG 2.2](https://www.w3.org/TR/WCAG22/).

## Tratorzinho

Avatar original vetorial, estilo emoji: corpo amarelo, contorno grafite, cabine clara, pneus arredondados e silhueta curta. É um símbolo de marca; não promete locação de trator agrícola. O catálogo apresenta as máquinas de fato divulgadas no site atual.

- [Avatar SVG](../../ms-cavalcante-ng/public/tractor.svg), `viewBox` de 128 × 128, fundo amarelo com cantos arredondados.
- [Favicon SVG](../../ms-cavalcante-ng/public/favicon.svg), mesma identidade, referenciado no HTML.
- Sem letras no desenho; não depende de emoji do sistema, fontes ou CDN.
- Conferir em 16, 32, 48 e 128 px. Em 16 px, priorizar reconhecimento de cabine/rodas, sem exigir detalhes.

Produzido diretamente em SVG para permitir manutenção simples e nitidez em tamanhos pequenos. Não houve uso de imagens geradas por IA. Uma versão PNG para redes sociais pode ser exportada depois a partir do vetor aprovado.
