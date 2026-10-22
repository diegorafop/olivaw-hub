# Mini Olivaws

Coleção de gestos e expressões do avatar para mensagens e interações da Olivaw. As reações complementam a marca; não substituem o logotipo.

## Primeira reação: sua mensagem é bem-vinda

`olivaw-message-welcome-v1.png` mostra o Olivaw segurando um envelope e acenando. Usado no rodapé do contato, junto à frase “Pode mandar sua ideia. A Olivaw entra em contato com você.” A imagem acolhe o visitante; não indica confirmação de envio.

- Original PNG transparente nesta pasta, para futuras derivações.
- Versão WebP de 256 × 256 em [`public/assets/brand/reactions`](../../../public/assets/brand/reactions/olivaw-message-welcome-v1.webp), exibida a 80 × 80 no site.
- Referência de identidade: [`olivaw-avatar-web-v2.png`](../../../public/assets/brand/olivaw-avatar-web-v2.png).
- Gerado com a ferramenta integrada de imagens em 21/10/2026. O WebP é uma redução do original, mantendo transparência.

## Coordenador da operação

`olivaw-operations-coordinator-v1.png` mostra o mini Olivaw acompanhando um tablet com uma lista de tarefas. É a primeira versão do coordenador. A expressão acolhedora permanece; o gesto representa organização e acompanhamento.

- Original PNG transparente nesta pasta.
- Versão WebP de 384 × 384 em [`public/assets/brand/reactions`](../../../public/assets/brand/reactions/olivaw-operations-coordinator-v1.webp), exibida a 180 × 180 no dashboard.
- Criado com a ferramenta integrada `imagegen` a partir da referência de identidade, preservando os avatares anteriores.
- Prompt: “Create a compact Olivaw operations coordinator with pearly white shell, navy face, smiling cyan eyes and its characteristic white orbit. Small robotic arms hold a navy tablet and point to a cyan checklist. Friendly confident posture, premium soft 3D rendering, simple shapes readable at 180px, genuinely transparent background. Preserve the reference identity. No clothing, hat, envelope, text or watermark.”

### Versão escolhida para a Visão geral

`olivaw-operations-coordinator-v2.png` afina a órbita lateral e amplia o espaço transparente entre ela e o personagem. Selecionada pelo mantenedor após comparação. A Visão geral usa [`olivaw-operations-coordinator-v2.webp`](../../../public/assets/brand/reactions/olivaw-operations-coordinator-v2.webp), com 384 × 384, exibido a 180 × 180. A versão 1 permanece como referência.

Prompt de refinamento, usando a versão 1 como referência: “Change only the white outer orbital frame: make its lateral band approximately 20 percent thinner by retracting its inner edge outward, preserving the outer silhouette. Increase transparent negative space between the robot and the lateral frame. Preserve character, face, pose, tablet, materials, cyan lamp and transparent background.” Ferramenta integrada `imagegen`.

## Novas reações

Ideias: pensando, comemorando, ouvindo e dando risada. Crie um arquivo por reação, com nome em inglês e versão: `olivaw-<acao>-v1`. Preserve originais nesta pasta e publique somente as versões otimizadas em `public/assets/brand/reactions`.

Preserve a carcaça branca perolada, o visor azul-marinho, os olhos ciano e a órbita característica. Use gestos simples legíveis em tamanho pequeno, fundo transparente e nenhum texto dentro da imagem. Não sobrescreva os avatares ou logos existentes. No site, mantenha texto acompanhando a imagem; quando decorativa, use `alt=""`. Possíveis animações futuras devem ser suaves e respeitar movimento reduzido.

Estas imagens ainda não constituem um pacote de figurinhas ou avatares instalado em aplicativos de mensagens. Cada canal precisará de uma exportação e integração próprias.

## Prompt da primeira reação

```text
Use case: identity-preserve. Create a single charming Olivaw brand character reaction sticker for a website contact form footer. Input reference: preserve the EXACT recognizable robot identity from the reference: pearly white rounded shell, deep navy glass face with two smiling cyan crescent eyes, sculptural oval white orbit enclosing the head, cyan circular light at top right of orbit. Adapt this same character into a compact friendly miniature with small rounded white robotic arms. One hand gently holds a tiny ivory envelope with a subtle cyan seal just below the face, the other hand makes a relaxed welcoming wave just outside the orbit. Feeling: 'your message is welcome, I am here to listen'. Premium polished soft 3D rendering, approachable and elegant, not a generic emoji or cartoon replacement. Rounded simple readable shapes suitable at 72px, very limited detail, full figure centered with generous safe margin, no cut-off parts. Preserve navy, pearl white, restrained cyan highlights. Actual transparent background, no colored backdrop, no ground plane, no text, no watermark, no sticker outline, no extra props, no extra characters. Create one image only.
```

## Família visual com órbita fina

A versão fina foi estendida ao símbolo principal e ao gesto da carta a pedido do mantenedor. Originais anteriores permanecem como referência; os pontos de uso atuais referenciam as novas versões.

- `olivaw-avatar-thin-orbit-v3.png`: original do símbolo principal; versão web de 768 × 768 em `public/assets/brand/olivaw-avatar-web-v3.png`.
- `olivaw-message-welcome-v2.png`: carta e aceno preservados, com a órbita mais fina; WebP de 256 × 256 em `public/assets/brand/reactions/olivaw-message-welcome-v2.webp`.
- Favicon PNG de 48 × 48, Apple Touch de 180 × 180 e `public/favicon.ico` (16/32/48) derivados do símbolo principal.
- Compartilhamento: `public/assets/brand/olivaw-share-v2.jpg`, gerado pelo script `scripts/generate-social-preview.mjs`.
- Geração dos dois originais: ferramenta integrada `imagegen`. Prompt: “Change only the white outer orbital frame to match the approved coordinator v2 reference: approximately 20 percent thinner lateral bands, increased transparent separation. Preserve the target character, face, expression, pose, pearl white/navy/cyan materials, circular lamp and genuinely transparent background. For the base icon, preserve head only; for the contact reaction, preserve waving hand and envelope. Do not introduce tablet, text or watermark.”
