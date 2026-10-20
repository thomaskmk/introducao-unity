# Guia de desenvolvimento

## Abrir e jogar

Use Unity 6000.3.26f1. Abra `Assets/Scenes/Percurso.unity` e pressione Play. A cena contém as duas fases; somente a fase atual fica ativa. O menu inicia o tutorial. Após vencê-lo, **Próxima fase** inicia o desafio e o menu passa a oferecer acesso direto a ele.

WASD ou setas movem, Esc/P pausa e R reinicia a fase atual. O analógico esquerdo ou direcional do controle também move; A confirma os botões e Start pausa. O suporte ao controle deve ser conferido com dispositivo físico antes da entrega final.

## Regras

Cada tentativa começa com uma vida. A cruz rosa é uma vida extra opcional, consumida uma vez por tentativa. Após a coleta, o primeiro impacto deixa uma vida e concede 1,5 segundo de proteção, sinalizada pelo personagem piscando. O personagem não volta ao início nem perde velocidade por causa do impacto. Um novo dano após a proteção encerra a partida.

A vida extra não é uma chave para a chegada. Pode ser ignorada ou usada para economizar tempo atravessando uma armadilha ativa. A proteção não elimina o efeito do piso lento. Reiniciar recupera o item, reposiciona os inimigos, reseta as armadilhas e devolve apenas a vida inicial. Perder no desafio não obriga a repetir o tutorial.

O tutorial tem 90 segundos e o desafio 20. Esses valores devem ser refinados com jogadores. No tutorial, é possível passar por cima ou por baixo do primeiro bloco móvel. A passagem inferior é mais curta e exige sincronizar o movimento com o bloco; ainda é necessário contornar o topo da parede.

O desafio tem duas rotas horizontais: corredor superior lento, com menos risco, e atalho central com barra giratória, pêndulo e perseguidor. A vida extra fica em um nicho abaixo do atalho, acessível somente pela armadilha cíclica. O retorno pelo mesmo nicho também exige observar o ciclo (ou gastar a vida recém-coletada). As duas rotas terminam em um pistão vertical antes da chegada. A vitória final é chegar à área verde antes de acabar o tempo.

## Onde editar

- `Fase 01 — Tutorial` e `Fase 02 — Desafio`: objetos separados na Hierarchy. Cada raiz possui `CourseLevel`, com duração, posição inicial e referências dos elementos.
- `Player`: `Rigidbody2D`, gravidade zero e rotação travada. `Speed` controla a velocidade e `Protection Seconds` a invulnerabilidade.
- Blocos e pistão: `PatrolHazard`, com pontos A/B, velocidade e deslocamento inicial. Pontos verticais produzem o pistão do desafio.
- Barra giratória e pêndulo: `RotatingHazard`, com ângulo inicial, velocidade angular, opção `Oscillates`, amplitude e período. O Rigidbody2D fica na raiz sem escala; a barra filha contém o trigger. A rotação segue o relógio da fase, portanto pausa e reinício mantêm a sincronização.
- Armadilhas: `CycleTrap`, com tempo apagado, aviso, tempo ativo e deslocamento inicial. O amarelo é aviso e ainda não causa dano. Vermelho causa dano mesmo se o jogador já estiver parado dentro da área. O texto reforça o estado além da cor.
- Pisos: `SlowZone`, com multiplicador de velocidade. Não acumulam penalidades: vale a redução mais forte sob o centro do personagem.
- Inimigos: `ChaserEnemy`, com pontos de patrulha, velocidade de perseguição, raio de detecção e território. Detectam somente dentro do território, com linha de visão livre. Um teste de colisão antes do movimento impede atravessar paredes. Não fazem busca de caminho: use áreas abertas; paredes internas complexas podem prendê-los.
- Item: `ExtraLife`, com trigger de coleta. O símbolo de cruz diferencia o item dos inimigos rosas.
- `GameController`: fases, transições, recordes, mensagens e controle do relógio.
- `Interface`: Canvas, HUD, dicas e telas. Os recordes e a liberação do desafio são locais ao navegador/dispositivo. `CourseLevel.layoutVersion` separa recordes após mudanças de percurso (tutorial v2, desafio v3), preservando o desbloqueio do desafio.

Paredes usam a camada `CourseWalls` (8). Ao adicionar uma parede, use essa camada para os inimigos enxergarem e respeitarem a colisão. Paredes precisam de `BoxCollider2D` sem trigger.

A cena está salva e pode ser editada no Inspector. **Percurso > Reconstruir as duas fases (substitui Percurso)** reconstrói as duas fases e sobrescreve modificações manuais na cena. Guarde suas alterações no Git antes de usar essa ferramenta.

## Organização do código

`RunSession` contém as regras sem depender da cena. `GameController` seleciona a fase e coordena a partida; `CourseLevel` reúne os objetos de cada fase. Os componentes individuais cuidam de movimento, inimigos, obstáculos e item. `CourseHud` apresenta os estados e `CameraFit` mantém a arena visível. `Editor/CourseBuilder` constrói a cena e gera a versão WebGL.

## Testes e build

No Unity: **Window > General > Test Runner**, abas EditMode e PlayMode. Com o editor desse projeto fechado:

```sh
bash scripts/unity.sh test-edit
bash scripts/unity.sh test-play
bash scripts/unity.sh build-web
bash scripts/package-web.sh
```

Relatórios ficam em `TestResults/`. A saída WebGL fica em `Builds/WebGL/` e o pacote em `Builds/percurso-webgl.zip`. Teste em navegador com `python3 -m http.server 8765 --bind 127.0.0.1 --directory Builds/WebGL`.

Os testes exercitam regras de dano e coleta, expiração da proteção, colisões, transição/reinício de fase, estados da armadilha, piso lento, perseguição, limites, pausa, impacto da barra giratória e reversão do pêndulo. A avaliação de dificuldade continua dependendo de partidas reais.

No itch.io, use o tipo HTML e marque o ZIP para ser jogado no navegador. O ZIP precisa ter `index.html` na raiz. A publicação depende do acesso à conta do autor.

## Limitação visual observada

Na prévia WebGL do navegador integrado, a primeira parede do tutorial não aparece, embora sua colisão continue funcionando. O mesmo defeito aparece na captura da versão anterior a esta revisão. O problema persistiu após reconstrução sem cache e tentativas de trocar a representação visual; essas tentativas foram descartadas. A passagem inferior foi validada com física real nos testes, mas a causa da ausência visual permanece pendente.

## Créditos

Os conceitos de objetos, componentes, física, scripts, triggers e interface seguem o tutorial da disciplina. Código e gráficos geométricos foram produzidos com apoio do Codex/OpenAI. Revise e compreenda os scripts antes da entrega e siga as regras da disciplina. Não foram usados projetos de colegas nem assets externos de jogos comerciais.
