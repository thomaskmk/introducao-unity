# O Escapista

*Uma fuga da prisão contra o tempo.*

Protótipo de um jogo 2D visto de cima, desenvolvido para a disciplina de Jogos e Interação do Insper com **Unity 6.3 LTS (6000.3.26f1)** e o modelo Universal 2D.

Chegue à área verde antes de acabar o tempo. Contorne as paredes e evite os obstáculos móveis e o perseguidor. Encostar em um perigo encerra a tentativa.

## Duas fases

- **Tutorial (90 segundos):** começa com três explicações: movimentação, saída e tempo/estrelas. O personagem, a saída e o cabeçalho são destacados, com o restante da tela escurecido. Clique em Próximo ou pressione Enter / A; Começar inicia o relógio. Reiniciar uma tentativa não repete a apresentação. Depois, apresenta blocos móveis, barra giratória, pêndulo, armadilha cíclica, piso lento e um guarda. Dicas explicativas aparecem por proximidade e apontam para o obstáculo, uma de cada vez. No primeiro bloco, passar por cima dá mais margem; por baixo encurta o caminho e exige esperar a abertura. O guarda ocupa a única saída do último setor: atraia-o e contorne-o.
- **Desafio (38 segundos):** cinco setores em sequência ocupam toda a arena. O percurso reúne bloco deslizante, pistão vertical, barra giratória, pêndulo, três armadilhas, dois pisos lentos e **três inimigos**. Não há rota segura alternativa nem placas explicativas.
- Velocidade do personagem: **4,5 unidades/s**, 10% menor que na versão anterior. O tutorial mantém os recordes v6. O desafio usa v7, após o fechamento dos desvios laterais do pêndulo.
- **Um impacto encerra a tentativa.** Não há contador de vidas. Reiniciar restaura o tempo, os obstáculos e os inimigos da fase atual.
- O botão **Jogar** abre o desafio diretamente. O botão menor **Tutorial** oferece uma introdução opcional; ao terminá-la, é possível seguir para o desafio. Recordes são separados por fase e versão do percurso.
- Menu, pausa, vitória, derrota por tempo ou dano, reinício e retorno ao menu. Entrada de teclado e controle. O suporte a controle físico não foi testado; essa limitação foi aceita pelo autor.
- Build WebGL com carregamento e botão de tela cheia.

As duas fases usam pixel art com tema de fuga da prisão: piso de concreto da LimeZu, grades, prisioneiro e guardas animados, braços mecânicos, portões, pistões, piso de segurança e áreas alagadas. A arte acompanha as colisões e o percurso aprovados. Há música de fundo e efeitos distintos para ações do menu, captura, vitória e avisos. O refinamento de dificuldade continua sujeito a partidas de teste. Os limites de tempo são valores iniciais para testes, não uma conclusão de todos os critérios da entrega final.

## Estrelas por fase

| Fase | 3 estrelas | 2 estrelas | 1 estrela |
| --- | --- | --- | --- |
| Tutorial | até 25 s | acima de 25 s e até 40 s | concluir após 40 s, antes dos 90 s |
| Desafio | até 24 s | acima de 24 s e até 31 s | concluir após 31 s, antes dos 38 s |

Derrotas não rendem estrelas. Os limites são inclusivos. O cabeçalho mostra o score da tentativa como três estrelas, depois duas e uma conforme o tempo ultrapassa cada limite. Na pausa, a classificação fica congelada; uma derrota zera as estrelas e reiniciar restaura as três. Os limites também ficam visíveis durante a partida e na vitória. A classificação do recorde é calculada pelo melhor tempo salvo por fase; uma tentativa mais lenta não apaga a melhor classificação. Os limites são valores iniciais para balanceamento. A avaliação usa somente tempo e estrelas, sem pontuação numérica.

Pausa compacta, sem mensagem. Derrota por contato: “Você foi pego durante a fuga. Tente novamente.” Derrota por tempo: “Você não alcançou a saída a tempo.” O tutorial termina com “Primeiro setor superado”; a fase final, com “Você escapou!”.

## Controles

| Ação | Teclado | Controle |
| --- | --- | --- |
| Mover | WASD ou setas | Analógico esquerdo ou direcional |
| Confirmar botão | Enter / clique | A |
| Pausar e continuar | Esc ou P | Start |
| Reiniciar | R ou botão na tela | Botão na tela |

Ao perder o foco, a partida pausa. O recorde fica salvo somente no navegador ou dispositivo usado.

## Abrir no Unity

1. Instale o Unity Editor **6000.3.26f1** pelo Unity Hub.
2. Instale o módulo **WebGL Build Support** para gerar a versão de navegador.
3. Clone o repositório e selecione sua pasta em **Add project from disk** no Hub.
4. Para o cenário, importe o pacote gratuito conforme as instruções abaixo.
5. Aguarde a importação e abra `Assets/Scenes/Percurso.unity`.
6. Pressione Play e, no menu, **Jogar** para o desafio ou **Tutorial** para aprender.

A cena já está pronta e pode ser modificada pela Hierarchy e pelo Inspector. Veja [o guia de desenvolvimento](docs/DESENVOLVIMENTO.md) para localizar cada componente e suas configurações.

## Artes do jogo

Baixe **Modern_Interiors_Free_v2.2.zip** em [Modern Interiors, por LimeZu](https://limezu.itch.io/moderninteriors), usando o download gratuito. Importe no projeto:

```sh
python3 scripts/import-modern-interiors.py ~/Downloads/Modern_Interiors_Free_v2.2.zip
```

Os PNGs da LimeZu ficam somente na cópia local, em `Assets/Art/Licensed/ModernInteriors`, e são ignorados pelo Git. Seus `.meta` e os recortes usados pela cena são versionados para restaurar as referências após a importação. Um clone novo precisa dessa importação antes de abrir a cena ou gerar o build com arte. A licença gratuita permite uso e edição em **projetos não comerciais**.

As oito imagens geradas estão em `Assets/Art/Prison/Generated`. Os prompts e a descrição da integração estão em [docs/ARTE.md](docs/ARTE.md). A reconstrução pelo menu **Percurso > Reconstruir as duas fases** aplica automaticamente a arte às duas fases quando os arquivos necessários estão presentes; sem o pacote, reconstrói o protótipo geométrico.

## Testes e build

No Unity, execute os testes em **Window > General > Test Runner**. Para gerar o jogo, use **Percurso > Gerar WebGL**.

No macOS, com o editor desse projeto fechado:

```sh
bash scripts/unity.sh test-edit
bash scripts/unity.sh test-play
bash scripts/unity.sh build-web
bash scripts/package-web.sh
```

Para outra instalação, defina `UNITY_EDITOR` com o caminho do executável. Os relatórios ficam em `TestResults/` e a versão WebGL em `Builds/WebGL/`. Não há automação de Local Environment do Codex configurada; esses comandos são o fluxo documentado do projeto.

Os testes cobrem regras da partida, derrota no primeiro impacto, progressão entre fases, reinício, pausa, ciclo das armadilhas, piso lento, perseguição, dano da barra em movimento, reversão do pêndulo, passagem inferior do tutorial, atração do guarda, conectividade do desafio e colisões reais. Os relatórios do Test Runner indicam os resultados de cada execução.

## Publicação

O comando `bash scripts/package-web.sh` gera `Builds/percurso-webgl.zip`, com `index.html` na raiz. Para testar localmente, execute `python3 -m http.server 8765 --bind 127.0.0.1 --directory Builds/WebGL` e abra `http://127.0.0.1:8765`. No itch.io, selecione **HTML**, envie o ZIP e marque **This file will be played in the browser**. Use uma área de 1280 × 720 e permita tela cheia. A publicação inicial ainda depende do acesso à conta do autor.

O código está no [repositório público](https://github.com/thomaskmk/introducao-unity). O desenvolvimento individual usa commits diretamente na `main`, que é a branch padrão. Crie outra branch somente quando houver necessidade; pull requests não são obrigatórios. As mensagens de commit seguem o formato `tipo: mensagem`, por exemplo `feat: adiciona um obstáculo` ou `fix: corrige a colisão com a parede`.

`Library`, `Temp`, `Logs`, `UserSettings`, relatórios e builds não são versionados. Os arquivos `.meta` dos assets devem ser preservados.

## Referências e créditos

- Autor do projeto: **thomaskmk**; substituir pelo nome completo na página de entrega.
- [Tutorial da disciplina: Introdução Rápida](https://slender-ceder-1da.notion.site/Tutorial-Introdu-o-R-pida-1a6d1ae1773780a7a59cf823a8b599ec): conceitos de objetos, componentes, física, scripts, triggers, interface e fim de partida.
- [Enunciado do projeto](https://docs.google.com/document/d/1fy-NeFFVT3tUe1lplrUzKLuoy-goAoKWvLZV1D5I__Y/edit).
- Código do protótipo e gráficos geométricos: produzidos com apoio do **Codex/OpenAI**. O responsável pelo projeto deve revisar e compreender os scripts.
- Cena e recursos iniciais, fonte interna e pacotes: **Unity Technologies**, modelo Universal 2D.
- Piso do cenário: **[LimeZu — Modern Interiors Free v2.2](https://limezu.itch.io/moderninteriors)**. Uso não comercial; licença original em `Assets/Art/Licensed/ModernInteriors/LICENSE.txt`. Os créditos também aparecem no tutorial jogável.
- Prisioneiro, guardas, máquinas, grades e pisos especiais: imagens geradas com **OpenAI imagegen** usando o pacote como referência visual; preparação e integração no Unity com apoio do Codex.
- Música: **polosik**, CC0; efeitos: **Kenney Interface Sounds**, CC0. Fontes e licenças em [Assets/Audio/README.md](Assets/Audio/README.md).
- Regras de exclusão baseadas no [Unity.gitignore do GitHub](https://github.com/github/gitignore/blob/main/Unity.gitignore).

The Escapists foi uma referência de direção visual; nenhum arquivo do jogo foi extraído ou incorporado. Novos recursos de arte e áudio deverão receber créditos quando forem incorporados.

## Conceitos do tutorial aplicados

Este jogo adapta os conceitos do tutorial de introdução a um percurso de fuga, cuja vitória é alcançar a saída a tempo.

| Conceito | Aplicação no projeto |
| --- | --- |
| GameObjects, sprites e componentes | Prisioneiro, guardas, grades, portões e pisos |
| Rigidbody2D e colisões | Movimento do jogador, paredes e obstáculos |
| Scripts e ciclo de execução | Inicialização, atualização da interface e movimentação física |
| Movimento e normalização diagonal | Teclado, analógico e direcional com velocidade limitada |
| Triggers e reação a interações | Captura, piso elétrico, piso lento e chegada |
| Interface, menu e fim de partida | Jogar, tutorial opcional, pausa, derrota, vitória e reinício |
| Áudio e AudioSource | Música em loop, efeitos e sincronização com pausa e resultados |
| Estado compartilhado | GameController e RunSession coordenam cada tentativa |

Não há coleta como objetivo nem exigência de manter os mesmos sprites ou arena do exercício. Os objetos são construídos por `CourseBuilder`, sem assets de prefab nesta versão. Essa é uma diferença de organização em relação à etapa de prefabs do tutorial; os créditos e a origem dos conceitos estão indicados acima.
