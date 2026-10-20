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

O primeiro setor do tutorial tem um teste de pixel art com tema de fuga da prisão: piso da LimeZu, prisioneiro animado e máquinas com artes geradas. As paredes do tutorial usam grades geradas, com colisão sólida. O restante do cenário mantém a arte geométrica provisória. Música, efeitos sonoros e refinamento de dificuldade ficam para as próximas etapas. Os limites de tempo são valores iniciais para testes, não uma conclusão de todos os critérios da entrega final.

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
4. Para o teste visual, importe o pacote gratuito conforme as instruções abaixo.
5. Aguarde a importação e abra `Assets/Scenes/Percurso.unity`.
6. Pressione Play e, no menu, **Começar**.

A cena já está pronta e pode ser modificada pela Hierarchy e pelo Inspector. Veja [o guia de desenvolvimento](docs/DESENVOLVIMENTO.md) para localizar cada componente e suas configurações.

## Artes do teste visual

Baixe **Modern_Interiors_Free_v2.2.zip** em [Modern Interiors, por LimeZu](https://limezu.itch.io/moderninteriors), usando o download gratuito. Importe no projeto:

```sh
python3 scripts/import-modern-interiors.py ~/Downloads/Modern_Interiors_Free_v2.2.zip
```

Os PNGs da LimeZu ficam somente na cópia local, em `Assets/Art/Licensed/ModernInteriors`, e são ignorados pelo Git. Seus `.meta` e os recortes usados pela cena são versionados para restaurar as referências após a importação. Um clone novo precisa dessa importação antes de abrir a cena ou gerar o build com arte. A licença gratuita permite uso e edição em **projetos não comerciais**.

As quatro imagens geradas estão em `Assets/Art/Prison/Generated`. Os prompts e a descrição do teste estão em [docs/ARTE.md](docs/ARTE.md). A reconstrução pelo menu **Percurso > Reconstruir as duas fases** aplica automaticamente o teste quando os arquivos necessários estão presentes; sem o pacote, reconstrói o protótipo geométrico.

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
- Cenário do teste visual: **[LimeZu — Modern Interiors Free v2.2](https://limezu.itch.io/moderninteriors)**. Uso não comercial; licença original em `Assets/Art/Licensed/ModernInteriors/LICENSE.txt`. Os créditos também aparecem no tutorial jogável.
- Prisioneiro, duas máquinas e grades: imagens geradas com **OpenAI imagegen** usando o pacote como referência visual; preparação e integração no Unity com apoio do Codex.
- Regras de exclusão baseadas no [Unity.gitignore do GitHub](https://github.com/github/gitignore/blob/main/Unity.gitignore).

The Escapists foi uma referência de direção visual; nenhum arquivo do jogo foi extraído ou incorporado. Novos recursos de arte e áudio deverão receber créditos quando forem incorporados.
