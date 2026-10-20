# Introdução ao Unity — percurso de obstáculos

Projeto individual da disciplina de Jogos e Interação do Insper, desenvolvido com **Unity 6.3 LTS (6000.3.26f1)** e o modelo **Universal 2D**.

## Estado atual

Repositório inicial com a cena padrão do Unity. Movimentação, percurso e regras da partida ainda serão implementados. A versão jogável para navegador será publicada no itch.io durante o desenvolvimento.

## Proposta do jogo

Um percurso 2D visto de cima: alcançar a chegada antes de o tempo acabar, desviando de obstáculos e inimigos. A coleta de itens não é condição de vitória.

Escopo inicial planejado, sujeito a ajustes após os testes:

- Uma fase curta com início e chegada.
- Contagem regressiva de 60 segundos e três vidas.
- Barreiras móveis, inimigos patrulhando e um atalho arriscado.
- Menu inicial, vitória, derrota e opção de reiniciar.
- Pontuação, tempo e vidas visíveis; suporte a teclado e controle.
- Música, efeitos sonoros e arte coerente com um tema a definir.

## Como abrir o projeto

1. Instale o Unity Hub e o Unity Editor **6000.3.26f1**.
2. Para gerar a entrega para navegador, instale também **WebGL Build Support**.
3. Clone este repositório ou baixe e extraia seu ZIP.
4. No Unity Hub, use **Add / Add project from disk** e selecione a pasta que contém `Assets`, `Packages` e `ProjectSettings`.
5. Abra o projeto e aguarde a importação dos arquivos e a restauração dos pacotes.
6. Abra a cena `Assets/Scenes/SampleScene.unity`.

## Organização

- `Assets/`: cenas, imagens, sons e scripts, incluindo seus arquivos `.meta`.
- `Packages/`: dependências do projeto e versões fixadas.
- `ProjectSettings/`: configurações e versão do Unity.

`Library`, `Temp`, `Logs`, `UserSettings` e as pastas de builds são ignoradas pelo Git. Os arquivos `.meta` dos assets fazem parte do projeto e devem ser preservados.

O desenvolvimento usa a branch `develop`. Para registrar progresso, salve as alterações no Unity, revise os arquivos modificados, faça um commit e envie-o ao GitHub. Mudanças no computador só aparecem no GitHub depois desse envio.

## Referências e créditos

- [Tutorial: Introdução Rápida — disciplina de Jogos e Interação](https://slender-ceder-1da.notion.site/Tutorial-Introdu-o-R-pida-1a6d1ae1773780a7a59cf823a8b599ec).
- [Enunciado do projeto](https://docs.google.com/document/d/1fy-NeFFVT3tUe1lplrUzKLuoy-goAoKWvLZV1D5I__Y/edit).
- Cena e recursos iniciais: modelo Universal 2D da Unity Technologies.
- Regras de exclusão baseadas no [modelo Unity.gitignore do GitHub](https://github.com/github/gitignore/blob/main/Unity.gitignore).

Os créditos e as licenças de novos assets, músicas, efeitos e trechos de código serão acrescentados conforme forem utilizados.
