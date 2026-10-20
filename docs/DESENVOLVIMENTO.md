# Guia da primeira versão

## Abrir e jogar

Use Unity 6000.3.26f1. Abra `Assets/Scenes/Percurso.unity` e pressione Play. A cena começa no menu inicial. O objetivo é alcançar a área verde antes dos 60 segundos acabarem. Os perigos laranja retiram uma vida; o personagem pisca durante 1,5 segundo de proteção para evitar dano repetido instantâneo.

Controles: WASD ou setas movem, Esc/P pausa, R reinicia. O analógico esquerdo ou direcional do controle também move; A confirma os botões e Start pausa. O suporte ao controle deve ser conferido com um dispositivo físico antes da entrega final.

## Onde alterar cada coisa

- `Arena` na Hierarchy: piso, paredes e chegada. Paredes possuem `BoxCollider2D`.
- `Player`: personagem com `Rigidbody2D`, gravidade zero e rotação travada. `Speed` ajusta a velocidade e `Protection Seconds` ajusta a proteção após dano.
- `Perigos`: três patrulhas. `Point A`, `Point B` e `Speed` definem o trajeto.
- `GameController`: limite de tempo, vidas e referências da partida.
- `Interface`: textos, botões e telas. Usa Canvas e componentes UI do Unity.

A cena está salva no projeto e pode ser editada normalmente no Inspector. O menu `Percurso > Criar cena inicial` é uma ferramenta para reconstruir o protótipo; ele substitui a cena `Percurso` e não deve ser usado depois de personalizá-la sem guardar uma versão antes.

## Código

- `RunSession.cs`: regras e estados da partida, sem dependência da cena.
- `GameController.cs`: conecta as regras, os botões e o cronômetro.
- `PlayerController2D.cs`: entrada e movimento por velocidade do Rigidbody2D. A diagonal é limitada para não dar vantagem.
- `PatrolHazard.cs`: percurso de patrulha e contato com o jogador.
- `FinishZone.cs`: detecta a chegada por trigger.
- `CourseHud.cs`: atualiza a interface.
- `CameraFit.cs`: mantém a arena visível em diferentes proporções de tela.
- `Editor/CourseBuilder.cs`: criação inicial da cena e geração de WebGL.

A coleta de itens não é requisito para vencer. Arte final, música, efeitos sonoros, atalho e expansão da fase pertencem às próximas etapas.

## Testar

No Unity: `Window > General > Test Runner`, abas EditMode e PlayMode. Os testes verificam limites do cronômetro, estados finais, vidas, pausa, reinício, colisão com parede, contato com perigo e chegada.

Com o editor desse projeto fechado:

```sh
bash scripts/unity.sh test-edit
bash scripts/unity.sh test-play
```

Os relatórios ficam em `TestResults/`, fora do versionamento. O teste de colisão e os testes de regras não substituem jogar a versão WebGL e avaliar a dificuldade.

## Gerar WebGL

No Unity: `Percurso > Gerar WebGL`. Pela linha de comando, com o projeto fechado:

```sh
bash scripts/unity.sh build-web
```

A saída fica em `Builds/WebGL/`. Compacte o conteúdo dessa pasta, com `index.html` na raiz do ZIP. No itch.io, use o tipo HTML e marque o arquivo para ser jogado no navegador. O template ocupa o espaço disponível e oferece tela cheia. A compressão Gzip tem fallback habilitado para hospedagem estática.

## Créditos e integridade

O tutorial da disciplina orientou os conceitos de GameObjects, componentes, física, scripts, triggers, interface e fim de partida. O código desta primeira versão e os gráficos geométricos foram produzidos com apoio do Codex/OpenAI nesta conversa. Revise e compreenda os scripts antes da entrega e siga as regras de integridade da disciplina. Não foram usados projetos de colegas nem assets externos de jogos comerciais.
