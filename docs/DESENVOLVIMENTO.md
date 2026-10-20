# Guia de desenvolvimento

## Abrir e jogar

Use Unity 6000.3.26f1. Abra `Assets/Scenes/Percurso.unity` e pressione Play. A cena contém as duas fases; somente a fase atual fica ativa. O menu contém nome, subtítulo de fuga, **Jogar** (desafio direto) e **Tutorial** (opcional). Após vencer o tutorial, **Continuar a fuga** inicia o desafio. Não há bloqueio por progresso.

WASD ou setas movem, Esc/P pausa e R reinicia a fase atual. O analógico esquerdo ou direcional do controle também move; A confirma os botões e Start pausa. O suporte ao controle físico está implementado, mas não foi testado; o autor decidiu aceitar essa limitação.

## Regras

Um impacto encerra a partida imediatamente. Não há contador de vidas nem invulnerabilidade. Reiniciar reposiciona os inimigos, os obstáculos e o jogador e restaura o relógio. Perder no desafio não obriga a repetir o tutorial.

O tutorial tem 90 segundos e o desafio 38. Esses valores devem ser refinados com jogadores. No tutorial, é possível passar por cima ou por baixo do primeiro bloco móvel. A passagem inferior é mais curta e exige sincronizar o movimento com o bloco; ainda é necessário contornar o topo da parede. Uma barra gira continuamente no primeiro setor; um pêndulo oscila no segundo. Grades laterais conduzem aos vãos dentro do alcance de cada barra: observe o movimento e espere uma janela para atravessar. No braço giratório, avance à direita do eixo, acompanhando o giro. O guarda fica parado num vão de 1,2 unidades, sem espaço para passar ao lado: aproxime-se para atraí-lo, recue e contorne-o antes de atravessar o vão.

O desafio conecta cinco setores alternando passagens superiores e inferiores. Reúne dois obstáculos deslizantes em eixos distintos, barra giratória, pêndulo, três armadilhas cíclicas, dois pisos lentos e três perseguidores. Não há bifurcação segura nem placas explicativas; os estados dos perigos são apresentados pelas cores e pelos símbolos ensinados no tutorial. A vitória depende de chegar à área verde antes do tempo acabar, sem sofrer impacto.

## Onde editar

- `Fase 01 — Tutorial` e `Fase 02 — Desafio`: objetos separados na Hierarchy. Cada raiz possui `CourseLevel`, com duração, posição inicial, metas `threeStarSeconds`/`twoStarSeconds` e referências dos elementos.
- `Player`: `Rigidbody2D`, gravidade zero e rotação travada. Cápsula vertical fixa de 0,46 × 0,82 unidades, centro 0,2 acima da raiz, ajustada ao corpo desenhado. `Speed` controla a velocidade: 4,5 unidades/s (redução de 10% em relação à versão anterior).
- Blocos e pistão: `PatrolHazard`, com pontos A/B, velocidade e deslocamento inicial. Pontos verticais produzem o pistão do desafio.
- Barra giratória e pêndulo: `RotatingHazard`, com ângulo inicial, velocidade angular, opção `Oscillates`, amplitude e período. O Rigidbody2D fica na raiz sem escala; a barra filha contém o trigger. A rotação segue o relógio da fase, portanto pausa e reinício mantêm a sincronização.
- Armadilhas: `CycleTrap`, com tempo apagado, aviso, tempo ativo e deslocamento inicial. O amarelo é aviso e ainda não causa dano. Vermelho causa dano mesmo se o jogador já estiver parado dentro da área. No tutorial, o texto reforça o estado além da cor.
- Pisos: `SlowZone`, com multiplicador de velocidade. Não acumulam penalidades: vale a redução mais forte sob o centro do personagem.
- Inimigos: `ChaserEnemy`, com pontos de patrulha, velocidade de perseguição, raio de detecção e território. Detectam somente dentro do território, com linha de visão livre. Uma cápsula vertical de 0,50 × 0,84 unidades, centro 0,2 acima da raiz, define o contato. O movimento usa CapsuleCast com a forma real do corpo e margem de 0,02. A origem acompanha a posição física, sem atraso da interpolação visual. Ao encostar numa parede, conserva a parte tangencial do movimento para sair e voltar à guarita; não é um navegador geral para labirintos.
- `GameController`: fases, transições, recordes, mensagens e controle do relógio.
- `Interface`: Canvas, HUD, dicas e telas. Cabeçalho com tempo, recorde e pausa; rodapé com metas de estrelas e controles. Menu, resultados e transições não usam o conceito de vidas. Os recordes são locais ao navegador/dispositivo. `CourseLevel.layoutVersion` separa recordes após mudanças de percurso (tutorial v6, desafio v7 após o fechamento dos desvios do pêndulo). Os tempos anteriores do desafio permanecem armazenados, mas não competem com o percurso corrigido.

Paredes usam a camada `CourseWalls` (8). Ao adicionar uma parede, use essa camada para os inimigos enxergarem e respeitarem a colisão. Paredes precisam de `BoxCollider2D` sem trigger.

A cena está salva e pode ser editada no Inspector. **Percurso > Reconstruir as duas fases (substitui Percurso)** reconstrói as duas fases e sobrescreve modificações manuais na cena. Guarde suas alterações no Git antes de usar essa ferramenta.

## Organização do código

`RunSession` contém as regras sem depender da cena e distingue `Contact` de `Timeout`. `StarRating` classifica uma fuga concluída; `GameController.EarnedStars` retorna zero fora da vitória. `StarRatingGraphic` desenha as estrelas como geometria da interface, sem depender de uma fonte. `GameController` seleciona a fase e coordena a partida; `CourseLevel` reúne os objetos de cada fase. Os componentes individuais cuidam de movimento, inimigos e obstáculos. `CourseHud` apresenta os estados e `CameraFit` mantém a arena visível. `TutorialGuide` mostra dicas por proximidade, com áreas, alvos, textos e posições definidos em `CourseBuilder.BuildTutorialGuide`; funciona sem triggers físicos. `Editor/CourseBuilder` constrói a cena e gera a versão WebGL. `Editor/PrisonArtBuilder` aplica a arte às duas fases sem alterar os objetos físicos; `PrisonCharacterVisual` anima os personagens e `PrisonTrapVisual` acompanha os estados das armadilhas.

## Testes e build

No Unity: **Window > General > Test Runner**, abas EditMode e PlayMode. Com o editor desse projeto fechado:

```sh
bash scripts/unity.sh test-edit
bash scripts/unity.sh test-play
bash scripts/unity.sh build-web
bash scripts/package-web.sh
```

Relatórios ficam em `TestResults/`. A saída WebGL fica em `Builds/WebGL/` e o pacote em `Builds/percurso-webgl.zip`. Teste em navegador com `python3 -m http.server 8765 --bind 127.0.0.1 --directory Builds/WebGL`.

Os testes exercitam derrota no primeiro impacto, colisões, transição/reinício de fase, estados da armadilha, piso lento, perseguição, limites, pausa, impacto da barra giratória e reversão do pêndulo, bloqueio do vão pelo guarda, percurso de atração e contorno, bloqueio dos quatro desvios laterais, janelas seguras e perigosas das duas barras, e conectividade dos cinco setores do desafio. Os testes de personagens também conferem os quadros de animação, o espaço lateral sem colisão invisível, o contato na cabeça e a distância do guarda às paredes. A avaliação de dificuldade continua dependendo de partidas reais.

No itch.io, use o tipo HTML e marque o ZIP para ser jogado no navegador. O ZIP precisa ter `index.html` na raiz. A publicação depende do acesso à conta do autor.

## Verificação visual

A primeira divisória do tutorial foi reposicionada de x=-6 para x=-6,1, mantendo o topo e a passagem inferior do bloco. Na versão WebGL verificada, a parede voltou a aparecer com o SpriteRenderer e material padrão do projeto; sua colisão acompanha a mesma posição. As duas fases devem continuar sendo verificadas visualmente no navegador após mudanças na cena. A auditoria registra 79 entradas de física e regras (incluindo caixas e cápsulas): `PrisonArtValidation.RebuildPreservingGameplay` reconstrói a cena comparando os registros antes/depois e interrompe em caso de diferença.

## Créditos

Os conceitos de objetos, componentes, física, scripts, triggers e interface seguem o tutorial da disciplina. Código e gráficos geométricos foram produzidos com apoio do Codex/OpenAI. Revise e compreenda os scripts antes da entrega e siga as regras da disciplina. Não foram usados projetos de colegas nem assets externos de jogos comerciais.


## Interface no navegador

O template WebGL mantém o canvas posicionado dentro da área visível da janela, sem o tamanho mínimo implícito do antigo contêiner Grid. Isso permite reduzir a altura ou a largura da janela sem cortar o rodapé; `CanvasScaler` e `CameraFit` continuam ajustando a interface e a arena.

O tutorial tem seis dicas. A primeira explica que tocar em obstáculos móveis encerra a tentativa; o guarda é descrito pelo comportamento de perseguição, sem instruir a manobra para passar. O segundo bloco deslizante, junto à passagem inferior, não mostra popup. A avaliação usa apenas tempo e estrelas; a fórmula de pontos do protótipo foi removida.


## Estrelas e telas de resultado

Metas iniciais: tutorial 25 s para três estrelas e 40 s para duas; desafio 24 s e 31 s. Qualquer vitória mais lenta recebe uma estrela. Derrota não recebe estrelas. A comparação usa o tempo real da partida; os tempos de resultado e recorde são exibidos com duas casas decimais, arredondadas para cima para não disfarçar um excesso sobre o limite.

`BestStars` é derivado do melhor tempo já salvo, sem uma segunda chave de persistência que possa ficar desatualizada. Na revisão que introduziu as estrelas, a versão dos recordes permaneceu 6. A revisão posterior das passagens usa v7 apenas no desafio. Pausar e reiniciar não concedem estrelas. Repetir uma fase com resultado inferior preserva o recorde anterior.

A pausa usa cartão de 640 × 300, apenas título e botões. Derrotas usam 880 × 520, com texto específico para contato ou tempo. Vitória usa 880 × 690 com título, mensagem, três estrelas preenchidas conforme a classificação, tempo, metas e ações. O menu inicial foi simplificado na revisão descrita abaixo. Os subtítulos antigos foram removidos da pausa e dos resultados.

Validação da revisão: 14 testes EditMode e 31 PlayMode aprovados, incluindo limites inclusivos, causas de derrota, pausa compacta, três classificações, persistência após recarregar a cena e recordes separados por fase. Auditoria: os 79 registros mantêm física e configurações anteriores, com adição somente das metas de estrelas; reconstrução final idêntica. No navegador, tutorial concluído com teclado em 19,45 s (3 estrelas) e 44,34 s (1 estrela), mantendo o recorde anterior. Tela de vitória conferida em 1280 × 720 e 1280 × 560.

A conferência visual também detectou o corte da segunda linha dos títulos de derrota. A fonte foi ajustada e os testes passaram a verificar a altura necessária do título nas telas de resultado. Os nove testes de integração da interface passaram novamente após o ajuste.

Foi corrigido um caso anterior do piso elétrico: o corpo físico parado podia dormir e deixar de emitir eventos contínuos de trigger. CycleTrap agora verifica a interseção real dos colliders durante a fase ativa, respeitando pausa e colliders desativados. Um teste coloca explicitamente o jogador em repouso físico antes de ligar a corrente e exige derrota por contato, sem estrelas.

A compilação final também foi conferida no navegador: permanecer imóvel no piso elétrico encerrou a tentativa por contato em 3,42 s; aguardar no início do desafio encerrou por tempo em 38 s. Ambas as telas exibiram o título completo, a mensagem correspondente e nenhuma estrela. Console sem erros ou avisos.

## Revisão do menu e das passagens

O menu inicial usa cartão de 780 × 550, com apenas nome do jogo, subtítulo e dois botões: Jogar e Tutorial. O fundo opaco oculta o cenário, cabeçalho, rodapé e instruções enquanto o menu está aberto. Pausa e resultados mantêm seus respectivos formatos. A pontuação numérica foi removida das regras, da interface e do registro de partidas; permanecem tempo, estrelas e recorde.

O desafio tem grades de 1,1 × 0,35 em x=-2,15 e x=2,15, ambas em y=-0,3. Elas fecham as bordas fora do alcance do pêndulo, deixando um vão central de 3,2 unidades. O alcance, período, amplitude, hitbox e posição do pêndulo permanecem iguais. Os limites de estrelas e de tempo também permanecem iguais, sujeitos a novas partidas de balanceamento.

O travamento do guarda na saída diagonal da guarita foi reproduzido por teste antes da correção. A movimentação preserva o deslocamento paralelo à parede e usa a posição física atual para as consultas de colisão. Os testes cobrem a saída diagonal, o retorno pelos dois lados, respeito às paredes e a manobra completa para concluir o tutorial.

Validação final: 14 testes EditMode e 35 PlayMode aprovados. A auditoria compara 81 registros de física e regras; em relação à versão anterior, apenas duas grades foram acrescentadas e a versão do recorde do desafio passou para 7. A reconstrução mantém os 81 registros idênticos. No WebGL, o tutorial foi concluído em 18,59 s após atrair o guarda na diagonal e contorná-lo; a vitória mostrou tempo e três estrelas, sem pontos. Ambos os botões do menu foram conferidos, assim como as grades do desafio. Console sem erros ou avisos.

## Passagem superior e score durante a partida

O primeiro bloco móvel do desafio tinha topo em y=5,2, deixando só 0,8 unidade até o teto (y=6), menos que a altura física de 0,82 do jogador. A trajetória foi baixada de y=4,7 para y=4,5, abrindo uma folga de 1 unidade. A passagem inferior continua dependendo de o bloco sair da abertura. As demais paredes e hitboxes permanecem iguais, e os recordes v6/v7 são preservados.

`GameController.CurrentStars` fornece o score da tentativa: 3, 2 ou 1 em jogo, pausa e vitória, usando os mesmos limites da avaliação final. Menu e derrota retornam zero. `EarnedStars` continua reservando a concessão de estrelas para uma fuga concluída; a interface não grava uma classificação provisória como recorde. O cabeçalho mostra três estrelas desenhadas por `StarRatingGraphic`, atualizadas conforme o tempo cruza os limites de cada fase.


## Áudio livre

`CourseAudio`, no mesmo objeto do `GameController`, acompanha a sessão em `LateUpdate`. Usa duas fontes de áudio: música em loop e efeitos. O menu inicial é silencioso; jogar inicia a música, pausar suspende e retomar continua. Reiniciar interrompe o áudio anterior e inicia uma única reprodução; derrota e vitória interrompem a música e tocam efeitos diferentes. Um aviso toca ao entrar nos últimos dez segundos ou quando um piso próximo começa a armar (distância até o collider menor que 2,5 unidades, intervalo mínimo de 0,8 s entre avisos).

Os cinco arquivos são CC0: música de polosik e efeitos Kenney Interface Sounds. Os arquivos e créditos estão em `Assets/Audio`. Volume inicial: música 0,20 e efeitos 0,55. Não há exigência de instalar pacotes adicionais.


Validação desta revisão: 14 testes EditMode e 39 PlayMode aprovados. O teste da passagem superior falhou antes da correção e passou depois, incluindo passagem inferior aberta e bloqueada. Os testes de áudio cobrem referências, pausa, retomada, reinício sem duplicação, resultados e aviso de tempo; os testes de estrelas cobrem ambas as fases e os limites exatos. A auditoria mantém 81 registros, com apenas três registros alterados em relação à versão anterior, todos referentes à altura/trajetória do primeiro bloco do desafio. A reconstrução da cena mantém os registros idênticos.

No navegador, a passagem superior foi atravessada com teclado e o cabeçalho exibiu 3, 2 e 1 estrela conforme a tentativa ultrapassou 24 e 31 segundos. Pausa e retorno ao menu foram conferidos. Console sem erros ou avisos. A reprodução e as transições do áudio foram verificadas pelos testes Unity; o volume percebido fica sujeito à configuração de som do dispositivo. O controle físico permanece sem teste, conforme decisão do autor.


## Introdução guiada do tutorial

Ao selecionar Tutorial no menu, `RunSession.Introduction` mantém tempo, movimento e obstáculos parados durante três etapas: personagem/movimentação, saída/objetivo e cabeçalho/tempo-estrelas. `TutorialIntroduction` projeta os alvos pela câmera e recalcula os recortes quando a janela muda. `SpotlightGraphic` desenha o escurecimento fora do alvo e intercepta cliques no HUD, inclusive dentro da área transparente. O cartão mantém o estilo das dicas contextuais e aceita clique ou a ação Submit da interface (Enter / A).

A terceira confirmação inicia a partida, com 90 segundos e música; as seis dicas de proximidade continuam iguais. Reinício começa diretamente; voltar ao menu e escolher Tutorial reapresenta as três etapas. O Desafio continua iniciando diretamente. Metas, velocidades, colisões, paredes e regras de vitória/derrota não mudam.

Durante a introdução, a interpolação visual dos corpos é suspensa e o desenho é alinhado à posição física já restaurada. Ao começar, os modos de interpolação originais são restabelecidos. Isso evita mostrar a pose da tentativa anterior ao reabrir o tutorial com o tempo parado; o teste de reentrada reproduziu o deslocamento de 1,5 unidade antes da correção.

Validação final da introdução: 14 testes EditMode e 41 PlayMode aprovados, incluindo os três destaques, relógio/física parados, texto sem corte, foco do botão, avanço único por quadro, início de áudio/dicas, desafio direto e reentrada após pausa. A auditoria preserva os mesmos 81 registros de física e regras do commit anterior. No navegador, avanço por clique e Enter, bloqueio dos controles ao fundo, início normal e reentrada alinhada foram confirmados; a terceira etapa também foi conferida em 1280 × 560. Console sem erros ou avisos.
