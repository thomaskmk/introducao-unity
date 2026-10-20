# Arte — fuga da prisão

As duas fases usam o mesmo conjunto de pixel art: piso de concreto, grades, prisioneiro, guardas, máquinas e pisos especiais. O layout aprovado em ed4852f foi preservado. Na revisão das dicas e sinalização, a auditoria dos 79 registros contra f9e547c encontrou apenas as alterações previstas: velocidade do jogador de 5 para 4,5 e versão de recordes de 4 para 5. Posições, rotações, escalas, colliders, corpos físicos, tempos dos perigos, patrulhas e composição das fases foram preservados.

## Origem das artes

- Cenário: Modern Interiors Free v2.2, por LimeZu, https://limezu.itch.io/moderninteriors. ZIP obtido na página oficial em 2026-10-20. Uso não comercial conforme LICENSE.txt incluído no pacote. PNGs originais ficam fora do Git público; importação documentada no README.
- Prisioneiro, guardas, braço giratório, bloco/portão, pistão, grades e pisos especiais: gerados pela ferramenta integrada OpenAI imagegen, com transparência. A imagem free_overview.png do pacote foi fornecida como referência de estilo para os três primeiros assets. Grades, pistão e pisos especiais usam Slider.png como referência; o guarda usa Prisoner.png. Não houve uso de API/CLI externa nem de artes extraídas de The Escapists.
- Os arquivos gerados permanecem em Assets/Art/Prison/Generated. Os recortes de sprites são criados pelo PrisonArtBuilder. O Unity aplica filtro Point, sem compressão nem mipmaps, e limita a resolução de importação para aproximar a densidade de pixels do cenário.

## Integração e limites

O concreto vem de Modern Interiors Free. As grades usam módulos repetidos com base sólida. Cada desenho de parede ou máquina se ajusta ao collider existente; a arte não redefine a física. Braço e pêndulo compartilham a peça industrial, com uma tonalidade mais quente no pêndulo. Portões quadrados e pistões retangulares mantêm as dimensões e trajetórias originais.

PrisonCharacterVisual anima prisioneiro e guardas nas quatro direções, com repouso e três quadros de caminhada. A animação acompanha o deslocamento real, pausa com o jogo e reinicia com a tentativa. Os guardas mantêm os indicadores de atenção/perseguição sobre a cabeça. O comportamento de detecção e perseguição continua em ChaserEnemy.

PrisonTrapVisual usa o mesmo relógio da armadilha. Faixas amarelas e pretas delimitam a grade de alta tensão. Sem corrente, há marcas de confirmação verdes; durante o aviso, aparecem exclamações amarelas; quando ativo, o piso fica vermelho e surgem descargas brancas. A pulsação suave das descargas para na pausa. Todos os sinais ficam dentro da área de dano existente. No tutorial, o estado informa “SEM CORRENTE · PODE PASSAR”, “LIGANDO · AGUARDE” ou “ELETRIFICADO · NÃO PISE”. Os sinais complementares usam os sprites geométricos já existentes; não foram geradas novas imagens nesta revisão.

A textura azul representa uma área alagada, com limite visual exatamente no contorno do piso lento. A saída tem concreto verde e marcação de borda dentro da área original. Os nomes dos portões e as placas explicativas fixas foram removidos. TutorialGuide mostra uma explicação por aproximação, aponta para o elemento correspondente, evita cobrir o personagem e desaparece fora da região, na pausa, no menu e no desafio. Não adiciona colliders nem interrompe o relógio.

Os sprites usam filtro Point, sem mipmaps ou compressão. Os personagens gerados são recortados por faixas transparentes; não se presume que a folha tenha células uniformes. O desenho dos personagens ocupa espaço acima dos pés, como em um jogo visto de cima; as colisões originais de 0,7 para o jogador e 0,9 para os guardas permanecem iguais.

## Reconstrução e conferência

O menu Percurso > Reconstruir as duas fases recria a cena a partir de CourseBuilder e aplica PrisonArtBuilder. Esse procedimento sobrescreve alterações manuais na cena.

Para reconstruir e comparar com a cena salva, execute o Unity em batch com `-executeMethod PrisonArtValidation.RebuildPreservingGameplay`. Os arquivos `Logs/gameplay-before.txt` e `Logs/gameplay-after.txt` registram a comparação; diferenças fazem a operação falhar. A comparação exige a cena aprovada previamente salva. Ela não substitui a inspeção visual.

## Prompts finais

### prisoner

Use case: stylized-concept. Asset type: transparent pixel-art character spritesheet for a Unity top-down prison escape game. Reference image 1 is a STYLE REFERENCE ONLY: match its restrained muted palette, dark desaturated violet outlines, chibi proportions and 3/4 overhead RPG perspective. Generate an ORIGINAL male adult prisoner with short dark brown hair, medium skin, an orange prison jumpsuit and dark shoes. Exactly 16 full-body sprites in a perfectly regular 4 columns by 4 rows square grid, identical cell dimensions. Row 1 faces SOUTH toward viewer, row 2 faces WEST left, row 3 faces EAST right, row 4 faces NORTH away. Each row has idle, left step, idle, right step. Every sprite has precisely the same scale and the feet on the same relative baseline. Think of each cell as 32x32 logical pixels with a character 14 pixels wide and 26 tall; draw using a VERY coarse crisp pixel grid, whole pixels, very few colors, nearest-neighbor enlarged display. No scenery, NO cell borders, NO words, NO labels, NO shadow blob, no glow, no antialiasing, no outlines around whole cells. Transparent background and transparent margins in every cell. This is a practical animation asset, not a presentation board. Keep each character entirely within its cell.

Transparência: true. Referência: Modern tiles_Free/free_overview.png.

### rotor

Use case: stylized-concept. Asset type: one transparent game sprite of an industrial rotating security arm. Reference image is STYLE REFERENCE ONLY for matching muted crisp minimal RPG pixel art, slate lavender outlines and low pixel density. Create a straight horizontal slim mechanical hazard bar, seen directly from above. Full silhouette: a rectangular steel beam with yellow and charcoal diagonal hazard stripes along its two ends, small rivets, and a small circular dark steel axle centered exactly in its middle. Beam extends equally to left and right, no perspective foreshortening. Width-to-height of the entire object about 9:1; center axle same height as the bar so the silhouette stays thin. At most 64 by 8 logical pixels, enlarged with hard square pixels. Bar centered on transparent square canvas with generous empty space above and below. The object will be rotated by Unity. No cast shadow, no environment, no text, no glow, no blur, no attached floor or pedestal. Restrained clean details readable at tiny size.

Transparência: true. Referência: Modern tiles_Free/free_overview.png.

### slider

Use case: stylized-concept. Asset type: one transparent sprite for a square sliding security gate obstacle in a top-down prison game. Reference image is STYLE REFERENCE ONLY for matching muted crisp minimal RPG pixel art, slate violet outlines, few colors and a 3/4 overhead perspective. One compact square industrial steel gate housing/piston block with three vertical recessed metal slats, yellow-and-charcoal striped top and bottom edges, a tiny red status lamp at the top right and small corner rivets. Entire square silhouette, width and height equal. Steel grey, off-white highlights, muted dark outlines, safety mustard yellow. Extremely low-resolution pixel art, approximately 24 by 24 logical pixels enlarged by nearest neighbor, hard square pixel edges. Center the object, use a transparent background, no floor, no attached rails, no scene, no text, no drop shadow, no glow, no antialiasing. Practical game sprite with generous transparent margins.

Transparência: true. Referência: Modern tiles_Free/free_overview.png.


### cell-bars

Use case: stylized-concept. Asset type: one modular horizontal prison-cell grille tile for a top-down pixel-art Unity game. Input image is STYLE REFERENCE ONLY: match the existing sprite's muted slate-violet outlines, grey steel, restrained off-white highlights and crisp coarse pixel-art grid. Create a NEW static wall segment, not a moving obstacle. A wide horizontal rectangular steel grille panel, width-to-height ratio 3:1. Exactly six thin vertical iron bars, equally spaced. Continuous dark steel top and bottom rails. A slightly thicker slate concrete footing at the bottom clearly signals a solid impassable wall. Simple rectangular silhouette, no perspective foreshortening, no gate handle or hinges, no diagonal stripes, no yellow, no red light. Flat orthographic stylized view suitable for rotating by 90 degrees in an overhead game. Small metal rivets on the rails, square pixel shading. Intended logical resolution about 48 by 16 pixels, enlarged with large hard square pixels and very few colors. Object isolated and centered on truly transparent square canvas, with transparent gaps between bars and generous transparent margins above and below. No setting, no floor, no extra objects, no cast shadow, no text, no watermark, no glow, no smoothing. The side edges must align so multiple identical panels can repeat along a wall without changing the thickness. Output one panel only.

Transparência: true. Ferramenta integrada imagegen. Referência de estilo: Assets/Art/Prison/Generated/Slider.png. Arquivo final: Assets/Art/Prison/Generated/CellBars.png.


### Guard

Use case: stylized-concept. Asset type: transparent pixel-art 4x4 character spritesheet for a top-down prison escape game. Reference image is STYLE REFERENCE ONLY, match the exact chibi scale, large head/small body proportions, coarse square pixels, muted slate-violet outlines and 3/4 overhead viewpoint of this prisoner. Generate a NEW adult prison guard wearing a dark navy blue uniform, navy peaked cap with a small gold badge, medium skin, dark boots and a utility belt. No weapon held, no baton extended, hands close to body. Exactly 16 separated full-body sprites in 4 columns and 4 rows with wide empty transparent gutters and aligned feet in each row. Row 1 faces SOUTH toward viewer, row 2 WEST left, row 3 EAST right, row 4 NORTH away. Each row: idle, left step, idle, right step. Keep every character same height, width, outfit and proportions. Think 32x32 logical-pixel cells and a character 14 pixels wide, 26 high. Very coarse crisp pixel art, few colors, no soft edges, no blur, no glow. Transparent canvas and margins, no environment, no separate shadows, no grid borders, no labels, no text, no watermarks. Practical animation spritesheet, not a concept board.

Ferramenta: imagegen integrado. Referência de estilo: Assets/Art/Prison/Generated/Prisoner.png. Transparência: true. Arquivo final: Assets/Art/Prison/Generated/Guard.png.

### FloorTrap

Use case: stylized-concept. Asset type: ONE square modular metal floor hazard tile for an overhead pixel-art prison escape game. Reference is STYLE REFERENCE ONLY: use the same muted slate violet dark outlines, steel grey off-white highlights and coarse crisp square pixel grid. Draw a square steel floor grate, flat orthographic view directly from above. One solid rectangular silhouette, no perspective, four corner bolts, dark recessed parallel vent slots inside, a thin metal frame around all four edges. A few neutral white inset status strips along the upper and lower edges: these will be colored by the game to indicate safe, warning, active. Entire sprite neutral desaturated grey, no yellow, no red, no green. Logical resolution about 24x24 pixels enlarged with hard square pixels. Make design legible at small size. Single isolated square plate centered with transparent margin. No floor outside plate, no machine outside plate, no smoke, no beams outside plate, no glow or shadows, no text, no letters, no labels, no anti-aliasing. One tile only. It will repeat in a horizontal strip as an industrial security floor.

Ferramenta: imagegen integrado. Referência de estilo: Assets/Art/Prison/Generated/Slider.png. Transparência: true. Arquivo final: Assets/Art/Prison/Generated/FloorTrap.png.

### SlowFloor

Use case: stylized-concept. Asset type: ONE seamless square floor texture tile for a pixel-art overhead prison escape game, a flooded/slippery maintenance floor that slows movement. Reference is STYLE REFERENCE ONLY for coarse square pixels, muted restrained palette and dark slate outlines. Orthographic view directly from above. Fill the entire square canvas edge to edge with dark desaturated blue-grey water over rough concrete, with a few sparse lighter muted blue short horizontal ripple pixels. Subtle uneven oily puddle patches and a few sparse dark square speckles. Quiet low-contrast texture, approximately 24x24 logical pixels, crisp nearest-neighbor square blocks, 5 colors maximum. All four edges must tile seamlessly. No enclosing frame, no object, no scenery, no text, no icons, no transparency, no perspective, no gradient, no smooth reflections, no glow. The water should be darker and bluer than the grey concrete surrounding it.

Ferramenta: imagegen integrado. Referência de estilo: Assets/Art/Prison/Generated/Slider.png. Transparência: false. Arquivo final: Assets/Art/Prison/Generated/SlowFloor.png.

### Piston

Use case: stylized-concept. Asset type: ONE narrow vertical rectangular moving piston/block game sprite, width to height ratio 1:2. Reference image is STYLE REFERENCE ONLY, match muted slate-violet outlines, steel grey, off-white highlights, coarse crisp pixel blocks, mustard yellow and charcoal caution stripes. Orthographic view directly from above, zero perspective. Compact rectangular hydraulic steel carriage: two short inset parallel silver rods running vertically through a central dark housing, sturdy metal caps across top and bottom, bolts at the four corners. Mustard yellow/charcoal diagonal caution strip across top and bottom caps. No rods or pieces outside the rectangular silhouette. No pedestal, no wall, no attached rail. Approximate logical resolution 16x32 pixels, very coarse crisp pixel art enlarged with hard squares and few colors. One isolated object centered in a transparent canvas with generous transparent margins. No environment, no cast shadow, no text, no glow, no blur, no anti-aliasing. It must remain recognizable when rotated by 90 degrees.

Ferramenta: imagegen integrado. Referência de estilo: Assets/Art/Prison/Generated/Slider.png. Transparência: true. Arquivo final: Assets/Art/Prison/Generated/Piston.png.

## Verificação da integração completa (f9e547c)

- Comparação da cena com a versão aprovada ed4852f: 79 registros de física e regras idênticos. A comparação foi repetida após o ajuste das placas.
- 22 testes PlayMode aprovados: mecânicas anteriores e novos testes de animação, perseguição, pausa, reinício, troca de fase e cores dos três estados do piso de segurança.
- WebGL compilado no Unity 6000.3.26f1. Inspeção no navegador das duas fases, movimento do prisioneiro, patrulha animada dos três guardas do desafio, pisos, máquinas e placas do tutorial.
- Os três estados do piso de segurança também foram conferidos visualmente no navegador; nenhum erro ou aviso apareceu no console durante a inspeção.
- Os limites de tempo, o número de inimigos, as regras de dano e os recordes da versão 4 foram preservados.


## Revisão das dicas e da grade elétrica

- Velocidade do prisioneiro: 4,5 unidades/s, redução de 10%. Tempos das fases e dos perigos mantidos; recordes separados na versão 5, sem apagar o desbloqueio do desafio.
- Auditoria contra f9e547c: 79 registros, com diferenças somente na velocidade do jogador e na versão dos recordes. Nova reconstrução após os ajustes de apresentação: 79 registros idênticos.
- 23 testes PlayMode aprovados. Incluem sete regiões de dicas, apontamento do alvo, caixa fora do personagem, saída da região, pausa, reinício e troca de fase; também verificam símbolos e texto dos três estados, descargas pausadas e travessias dos portões na velocidade real do personagem.
- A grade mantém as mesmas durações e a mesma regra de dano: o aviso amarelo ainda não causa dano; a corrente vermelha causa dano inclusive ao personagem já dentro da área.
- Conferência WebGL final: primeiro portão atravessado com teclado, dica seguinte reposicionada para manter o portão e a grade elétrica visíveis, estados seguro/aviso/ativo presentes simultaneamente no desafio e console sem erros ou avisos. O teste de dicas foi repetido após o ajuste de posição e antecipação do alerta.


## Limpeza da interface

O cabeçalho agora exibe tempo, recorde e pausa. Rodapé apenas com controles; não há mensagens de objetivo ou contador de vidas em menus, resultados ou durante a partida. O conceito foi removido de RunSession: um contato perigoso encerra a tentativa diretamente, mantendo os valores de pontuação anteriores.

O tutorial usa seis dicas: o primeiro portão explica a consequência de um impacto; o guarda é descrito por proximidade, perseguição e captura. O bloco móvel da passagem inferior não tem popup. O template WebGL ocupa exatamente a janela disponível, evitando o corte de rodapé causado pelo tamanho mínimo do antigo Grid.

Validação desta revisão: 8 testes EditMode e 23 PlayMode aprovados; 79 registros de física/configuração iguais aos da cena anterior. No navegador, a falha foi reproduzida em 1280×560 (canvas de 720 pixels de altura) e corrigida: o canvas passou a medir exatamente 1280×560. Conferência adicional em 900×700, tamanho padrão e tela cheia, com rodapé completo e console sem erros/avisos.
