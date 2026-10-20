# Guia de desenvolvimento

## Abrir e jogar

Use Unity 6000.3.26f1. Abra `Assets/Scenes/Percurso.unity` e pressione Play. A cena contém as duas fases; somente a fase atual fica ativa. O menu inicia o tutorial. Após vencê-lo, **Próxima fase** inicia o desafio e o menu passa a oferecer acesso direto a ele.

WASD ou setas movem, Esc/P pausa e R reinicia a fase atual. O analógico esquerdo ou direcional do controle também move; A confirma os botões e Start pausa. O suporte ao controle deve ser conferido com dispositivo físico antes da entrega final.

## Regras

Cada tentativa começa com uma vida. Um impacto encerra a partida imediatamente; não há vida extra nem invulnerabilidade. Reiniciar reposiciona os inimigos, os obstáculos e o jogador, restaura o relógio e devolve uma vida. Perder no desafio não obriga a repetir o tutorial.

O tutorial tem 90 segundos e o desafio 38. Esses valores devem ser refinados com jogadores. No tutorial, é possível passar por cima ou por baixo do primeiro bloco móvel. A passagem inferior é mais curta e exige sincronizar o movimento com o bloco; ainda é necessário contornar o topo da parede. Uma barra gira continuamente no primeiro setor; um pêndulo oscila no segundo. Grades laterais conduzem aos vãos dentro do alcance de cada barra: observe o movimento e espere uma janela para atravessar. No braço giratório, avance à direita do eixo, acompanhando o giro. O guarda fica parado num vão de 1,2 unidades, sem espaço para passar ao lado: aproxime-se para atraí-lo, recue e contorne-o antes de atravessar o vão.

O desafio conecta cinco setores alternando passagens superiores e inferiores. Reúne dois obstáculos deslizantes em eixos distintos, barra giratória, pêndulo, três armadilhas cíclicas, dois pisos lentos e três perseguidores. Não há bifurcação segura nem placas explicativas; os estados dos perigos são apresentados pelas cores e pelos símbolos ensinados no tutorial. A vitória depende de chegar à área verde antes do tempo acabar, sem sofrer impacto.

## Onde editar

- `Fase 01 — Tutorial` e `Fase 02 — Desafio`: objetos separados na Hierarchy. Cada raiz possui `CourseLevel`, com duração, posição inicial e referências dos elementos.
- `Player`: `Rigidbody2D`, gravidade zero e rotação travada. `Speed` controla a velocidade: 4,5 unidades/s (redução de 10% em relação à versão anterior).
- Blocos e pistão: `PatrolHazard`, com pontos A/B, velocidade e deslocamento inicial. Pontos verticais produzem o pistão do desafio.
- Barra giratória e pêndulo: `RotatingHazard`, com ângulo inicial, velocidade angular, opção `Oscillates`, amplitude e período. O Rigidbody2D fica na raiz sem escala; a barra filha contém o trigger. A rotação segue o relógio da fase, portanto pausa e reinício mantêm a sincronização.
- Armadilhas: `CycleTrap`, com tempo apagado, aviso, tempo ativo e deslocamento inicial. O amarelo é aviso e ainda não causa dano. Vermelho causa dano mesmo se o jogador já estiver parado dentro da área. No tutorial, o texto reforça o estado além da cor.
- Pisos: `SlowZone`, com multiplicador de velocidade. Não acumulam penalidades: vale a redução mais forte sob o centro do personagem.
- Inimigos: `ChaserEnemy`, com pontos de patrulha, velocidade de perseguição, raio de detecção e território. Detectam somente dentro do território, com linha de visão livre. Um teste de colisão antes do movimento impede atravessar paredes. Não fazem busca de caminho: use áreas abertas; paredes internas complexas podem prendê-los.
- `GameController`: fases, transições, recordes, mensagens e controle do relógio.
- `Interface`: Canvas, HUD, dicas e telas. Os recordes e a liberação do desafio são locais ao navegador/dispositivo. `CourseLevel.layoutVersion` separa recordes após mudanças de percurso (tutorial v5, desafio v5, devido à redução de velocidade), preservando o desbloqueio do desafio.

Paredes usam a camada `CourseWalls` (8). Ao adicionar uma parede, use essa camada para os inimigos enxergarem e respeitarem a colisão. Paredes precisam de `BoxCollider2D` sem trigger.

A cena está salva e pode ser editada no Inspector. **Percurso > Reconstruir as duas fases (substitui Percurso)** reconstrói as duas fases e sobrescreve modificações manuais na cena. Guarde suas alterações no Git antes de usar essa ferramenta.

## Organização do código

`RunSession` contém as regras sem depender da cena. `GameController` seleciona a fase e coordena a partida; `CourseLevel` reúne os objetos de cada fase. Os componentes individuais cuidam de movimento, inimigos e obstáculos. `CourseHud` apresenta os estados e `CameraFit` mantém a arena visível. `TutorialGuide` mostra dicas por proximidade, com áreas, alvos, textos e posições definidos em `CourseBuilder.BuildTutorialGuide`; funciona sem triggers físicos. `Editor/CourseBuilder` constrói a cena e gera a versão WebGL. `Editor/PrisonArtBuilder` aplica a arte às duas fases sem alterar os objetos físicos; `PrisonCharacterVisual` anima os personagens e `PrisonTrapVisual` acompanha os estados das armadilhas.

## Testes e build

No Unity: **Window > General > Test Runner**, abas EditMode e PlayMode. Com o editor desse projeto fechado:

```sh
bash scripts/unity.sh test-edit
bash scripts/unity.sh test-play
bash scripts/unity.sh build-web
bash scripts/package-web.sh
```

Relatórios ficam em `TestResults/`. A saída WebGL fica em `Builds/WebGL/` e o pacote em `Builds/percurso-webgl.zip`. Teste em navegador com `python3 -m http.server 8765 --bind 127.0.0.1 --directory Builds/WebGL`.

Os testes exercitam derrota no primeiro impacto, colisões, transição/reinício de fase, estados da armadilha, piso lento, perseguição, limites, pausa, impacto da barra giratória e reversão do pêndulo, bloqueio do vão pelo guarda, percurso de atração e contorno, bloqueio dos quatro desvios laterais, janelas seguras e perigosas das duas barras, e conectividade dos cinco setores do desafio. A avaliação de dificuldade continua dependendo de partidas reais.

No itch.io, use o tipo HTML e marque o ZIP para ser jogado no navegador. O ZIP precisa ter `index.html` na raiz. A publicação depende do acesso à conta do autor.

## Verificação visual

A primeira divisória do tutorial foi reposicionada de x=-6 para x=-6,1, mantendo o topo e a passagem inferior do bloco. Na versão WebGL verificada, a parede voltou a aparecer com o SpriteRenderer e material padrão do projeto; sua colisão acompanha a mesma posição. As duas fases devem continuar sendo verificadas visualmente no navegador após mudanças na cena. A ampliação da arte mantém os 79 registros de física e regras da versão aprovada: `PrisonArtValidation.RebuildPreservingGameplay` reconstrói a cena comparando os registros antes/depois e interrompe em caso de diferença.

## Créditos

Os conceitos de objetos, componentes, física, scripts, triggers e interface seguem o tutorial da disciplina. Código e gráficos geométricos foram produzidos com apoio do Codex/OpenAI. Revise e compreenda os scripts antes da entrega e siga as regras da disciplina. Não foram usados projetos de colegas nem assets externos de jogos comerciais.
