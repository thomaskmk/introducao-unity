# Teste de arte — saída das celas

O piso e as máquinas do primeiro setor usam pixel art. O prisioneiro e as grades aparecem em todo o tutorial. O desafio mantém a apresentação geométrica. As divisórias principais continuam nas mesmas posições e dimensões; quatro trechos curtos de grade fecham os desvios laterais do braço e do pêndulo. Tempos, velocidades, três guardas do desafio e regras de dano permanecem iguais. A placa “SAÍDA DAS CELAS” foi removida do percurso.

## Origem das artes

- Cenário: Modern Interiors Free v2.2, por LimeZu, https://limezu.itch.io/moderninteriors. ZIP obtido na página oficial em 2026-10-20. Uso não comercial conforme LICENSE.txt incluído no pacote. PNGs originais ficam fora do Git público; importação documentada no README.
- Personagem, braço giratório, bloco/portão e grades: gerados pela ferramenta integrada OpenAI imagegen, com transparência. A imagem free_overview.png do pacote foi fornecida como referência de estilo para os três primeiros assets. A grade usa Slider.png, já gerado, como referência. Não houve uso de API/CLI externa nem de artes extraídas de The Escapists.
- Os arquivos gerados permanecem em Assets/Art/Prison/Generated. Os recortes de sprites são criados pelo PrisonArtBuilder. O Unity aplica filtro Point, sem compressão nem mipmaps, e limita a resolução de importação para aproximar a densidade de pixels do cenário.

## Escopo e limitações

Esta é uma amostra para avaliar a direção visual, não a aplicação da arte ao jogo inteiro. O cenário usa somente uma fração do pacote gratuito. O prisioneiro tem quatro direções e três quadros de caminhada por direção, além do quadro parado. As margens de cada célula são recortadas e os pés alinhados na importação. As animações geradas ainda devem ser avaliadas visualmente pelo autor do jogo.

PrisonerVisual cuida apenas da apresentação. O quadrado original continua sendo a referência de física; sua imagem é ocultada no tutorial e reaparece no desafio. O braço e o portão mantêm seus colliders originais. O recorte do braço fica ajustado à largura e espessura do obstáculo existente. As grades são módulos repetidos, com base escura contínua. Seu desenho usa a posição e a dimensão do BoxCollider2D da parede, evitando manter duas geometrias separadas. Os vazios entre barras são apenas visuais: toda a base bloqueia a passagem.

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


## Verificação deste teste

- WebGL compilado com sucesso no Unity 6000.3.26f1.
- 20 testes PlayMode passaram após a revisão das grades. Incluem bloqueio físico dos quatro desvios laterais e travessias seguras e perigosas do braço e do pêndulo, além de colisões, dano, pausa, reinício, progressão e mecânicas das fases.
- Inspeção manual da revisão: placa removida; grades visíveis; bloqueio da lateral esquerda; travessia do braço pela direita; reinício e pausa. Na integração inicial também foram conferidos as quatro direções do prisioneiro, o primeiro portão, dano, menu e apresentação geométrica do desafio.
- A folha gerada não veio com células uniformes. A importação identifica quatro faixas ocupadas em cada eixo, separadas por transparência, evitando recortar os pés e misturar quadros de direções diferentes.
- A versão final não apresentou erros ou avisos no console do navegador durante essas verificações.
- A amostra valida a integração visual; não representa uma nova avaliação da dificuldade do desafio nem a arte final do jogo inteiro.
