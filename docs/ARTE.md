# Teste de arte — saída das celas

O teste ocupa o primeiro setor do tutorial, à esquerda da primeira divisória. O prisioneiro usa a arte nova durante todo o tutorial. O desafio mantém a apresentação geométrica. A geometria física, os tempos, as velocidades, os três guardas do desafio e as regras de dano não foram alterados.

## Origem das artes

- Cenário: Modern Interiors Free v2.2, por LimeZu, https://limezu.itch.io/moderninteriors. ZIP obtido na página oficial em 2026-10-20. Uso não comercial conforme LICENSE.txt incluído no pacote. PNGs originais ficam fora do Git público; importação documentada no README.
- Personagem, braço giratório e bloco/portão: gerados pela ferramenta integrada OpenAI imagegen, com transparência. A imagem free_overview.png do pacote foi fornecida como referência de estilo. Não houve uso de API/CLI externa nem de artes extraídas de The Escapists.
- Os arquivos gerados permanecem em Assets/Art/Prison/Generated. Os recortes de sprites são criados pelo PrisonArtBuilder. O Unity aplica filtro Point, sem compressão nem mipmaps, e limita a resolução de importação para aproximar a densidade de pixels do cenário.

## Escopo e limitações

Esta é uma amostra para avaliar a direção visual, não a aplicação da arte ao jogo inteiro. O cenário usa somente uma fração do pacote gratuito. O prisioneiro tem quatro direções e três quadros de caminhada por direção, além do quadro parado. As margens de cada célula são recortadas e os pés alinhados na importação. As animações geradas ainda devem ser avaliadas visualmente pelo autor do jogo.

PrisonerVisual cuida apenas da apresentação. O quadrado original continua sendo a referência de física; sua imagem é ocultada no tutorial e reaparece no desafio. O braço e o portão mantêm seus colliders originais. O recorte do braço fica ajustado à largura e espessura do obstáculo existente.

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


## Verificação deste teste

- WebGL compilado com sucesso no Unity 6000.3.26f1.
- 18 testes PlayMode existentes passaram após a integração da arte; cobrem colisões, dano, pausa, reinício, progressão e mecânicas das fases.
- Inspeção manual no navegador: quatro direções do prisioneiro; reinício; passagem superior do primeiro portão; dano do braço giratório; pausa; retorno ao menu; desafio com o quadrado e a arte geométrica originais.
- A folha gerada não veio com células uniformes. A importação identifica quatro faixas ocupadas em cada eixo, separadas por transparência, evitando recortar os pés e misturar quadros de direções diferentes.
- A versão final não apresentou erros ou avisos no console do navegador durante essas verificações.
- A amostra valida a integração visual; não representa uma nova avaliação da dificuldade do desafio nem a arte final do jogo inteiro.
