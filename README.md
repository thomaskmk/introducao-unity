# Percurso Contra o Relógio

Protótipo de um jogo 2D visto de cima, desenvolvido para a disciplina de Jogos e Interação do Insper com **Unity 6.3 LTS (6000.3.26f1)** e o modelo Universal 2D.

Chegue à área verde antes de acabar o tempo. Contorne as paredes e evite os obstáculos móveis e o perseguidor. Cada tentativa tem uma vida, sem itens de recuperação.

## Duas fases

- **Tutorial (90 segundos):** apresenta blocos móveis, barra giratória, pêndulo, armadilha cíclica, piso lento e um guarda. Dicas mudam conforme o trecho. No primeiro bloco, passar por cima dá mais margem; por baixo encurta o caminho e exige esperar a abertura. O guarda ocupa a única saída do último setor: atraia-o e contorne-o.
- **Desafio (38 segundos):** cinco setores em sequência ocupam toda a arena. O percurso reúne bloco deslizante, pistão vertical, barra giratória, pêndulo, três armadilhas, dois pisos lentos e **três inimigos**. Não há rota segura alternativa nem placas explicativas.
- **Uma vida por tentativa, sem vida extra.** Um impacto encerra a partida. Reiniciar restaura o tempo, os obstáculos e os inimigos da fase atual.
- Terminar o tutorial libera o desafio, inclusive pelo menu em visitas seguintes no mesmo navegador. Recordes são separados por fase e versão do percurso, preservando o desbloqueio.
- Menu, pausa, vitória, derrota por tempo ou dano, reinício e retorno ao menu. Entrada de teclado e controle; o controle ainda precisa de teste com dispositivo físico.
- Build WebGL com carregamento e botão de tela cheia.

A arte permanece geométrica e provisória. Arte temática, música, efeitos sonoros e refinamento de dificuldade ficam para as próximas etapas. Os limites de tempo são valores iniciais para testes, não uma conclusão de todos os critérios da entrega final.

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
4. Aguarde a importação e abra `Assets/Scenes/Percurso.unity`.
5. Pressione Play e, no menu, **Começar**.

A cena já está pronta e pode ser modificada pela Hierarchy e pelo Inspector. Veja [o guia de desenvolvimento](docs/DESENVOLVIMENTO.md) para localizar cada componente e suas configurações.

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
- Regras de exclusão baseadas no [Unity.gitignore do GitHub](https://github.com/github/gitignore/blob/main/Unity.gitignore).

Não foram usados assets externos de jogos comerciais. Novos recursos de arte e áudio deverão receber créditos quando forem incorporados.
