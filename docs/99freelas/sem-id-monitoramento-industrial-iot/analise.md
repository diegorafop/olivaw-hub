# Análise preliminar — monitoramento industrial IoT

## Parecer

O pedido combina integração física com máquinas, coleta de eventos, regras de estado e aplicação de gestão. A descrição permite discutir um piloto, mas ainda não permite orçar a solução inteira com responsabilidade. A principal lacuna é saber se o hardware está pronto e entrega dados confiáveis ou se eletrônica, firmware e instalação também fazem parte da contratação.

A experiência declarada do mantenedor em sistemas, integrações, arquitetura e infraestrutura é pertinente à camada de software. Não há evidência nesta conversa para afirmar experiência específica anterior em eletrônica industrial, CLPs, firmware ou projetos IoT de fábrica. Responder com transparência e avaliar eventual parceiro especializado quando o escopo for conhecido.

## Fatos recebidos

- Coleta automática de pulsos e sinais digitais por hardware IoT.
- Detecção de paradas e registro de motivos em tablet.
- Cadastro de equipamentos, regras configuráveis, banco e relatórios históricos.
- Pedido de MVP e interesse futuro em plataforma comercial.
- Convite para conversa de cerca de 30 minutos; nenhuma reunião marcada.

## Hipótese de recorte para discutir

Piloto com uma máquina, um dispositivo coletor e um tablet, usando hardware fornecido e documentado pelo cliente. Essa quantidade é uma hipótese da Olivaw, não requisito confirmado.

Possível composição: coletor/gateway → serviço de ingestão → persistência de eventos → regras de parada → painel e tablet. A camada web pode seguir ASP.NET Core e Angular da Olivaw; a tecnologia do coletor depende do equipamento e do protocolo. Não prometer que o uso de Python ou uma interface específica resolve a aquisição física dos sinais.

O MVP poderia demonstrar contagem conciliada, estados de operação/parada/desconexão, registro de motivo e relatório simples de produção e duração de paradas. “Sem pulsos” pode significar parada, ausência de ordem, perda de rede ou falha do sensor: não assumir equivalência. Distinguir hora do evento e recebimento, identificar reinícios/contadores zerados e evitar duplicação no reenvio.

A leitura de sinais deve ser distinguida de comando da máquina. O anúncio não solicita ligar/desligar equipamentos ou atuar em funções de segurança. Instalação elétrica, isolamento e validação de sinais exigem responsabilidade técnica e autorização próprias; não estão presumidos na camada de software.

## Pontos para a conversa de 30 minutos

- Hardware existente, sensores/CLPs, documentação, firmware e quem instala/mantém. Existe amostra de mensagem ou simulador?
- Quantidade de máquinas/tablets, tipos de processo, frequência máxima dos pulsos e definição de peça produzida. Há rejeitos, retrabalho ou múltiplos pulsos por peça?
- Interface elétrica e forma de aquisição já resolvidas pelo hardware? Como os dados chegam ao software: rede local, gateway, API ou protocolo industrial?
- Latência aceitável para “tempo real”, conectividade, duração tolerada sem internet e necessidade de armazenamento local.
- Critério de parada por equipamento/turno/produto, paradas planejadas, horários sem produção e correção posterior de motivos.
- Quem opera o tablet, como identifica a máquina e quais regras de aprovação/auditoria dos motivos?
- Quais relatórios e indicadores iniciais? OEE não foi solicitado; disponibilidade, desempenho e qualidade exigem dados e definições adicionais.
- Retenção, usuários, segregação por empresa no produto futuro, requisitos de rede e infraestrutura. Plataforma multiempresa comercial não é consequência automática do piloto.
- Critério de aceite: comparação com contagem conhecida, tolerância, falhas simuladas e acesso a ambiente de homologação. Homologação física deve ser combinada, sem testes remotos em máquinas reais presumidos.

## Orçamento e próximos passos

Primeiro obter respostas à pergunta preparada e discutir um fluxo físico de ponta a ponta. Depois estimar análise, integração, backend, tablet, relatórios, testes, implantação e orientação, com horas por profissional e despesas externas. Não definir preço ou prazo neste registro por desconhecer hardware, acesso e volume.

Separar desenvolvimento do piloto de hardware, instalação, deslocamento, conectividade e operação posterior. A opção de infraestrutura AWS administrada pela Olivaw pode ser discutida com mensalidade dimensionada; ambiente local/offline pode mudar essa solução. Nenhum plano, valor, manutenção ou SLA foi aprovado.

Este registro não seleciona componentes, plugins ou serviços específicos; pesquisa e licenças serão avaliadas após identificar as interfaces reais. Não houve inspeção do sistema, teste de integração, cadastro de Lead ou contato com o anunciante.
