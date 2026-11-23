# Game Gaucho - Defesa do Galpao

Tower defense 2D em pixel art inspirado em Bloons TD, ambientado no Rio Grande do Sul.

Trabalho pratico da disciplina CCC300 - Computacao Grafica e Realidade Virtual (UPF, 2026/2).

## Downloads

- **Executavel (Windows):** _[link do executavel aqui]_
- **Video demonstrativo:** _[link do video aqui]_

## Como abrir e jogar

1. Abra o Unity Hub.
2. Em **Projects > Add**, escolha esta pasta.
3. Use a versao **Unity 6.0 (6000.0.85f2)**.
4. Na janela Project, abra `Assets/Scenes/Menu`.
5. Clique no botao **Play** (triangulo no topo).

No menu da para escolher o nivel (Facil, Normal ou Dificil), ver o ranking e as instrucoes. Tambem e possivel abrir `Assets/Scenes/Teste_PixelArt` direto: a camada jogavel e preparada automaticamente ao iniciar a cena.

## Controles

- **Espaco**: iniciar a proxima onda.
- **1**: contratar gaucho de revolver (rapido).
- **2**: contratar gaucho de espingarda (chumbo em leque: forte e acerta varios).
- **Clique esquerdo**: posicionar o gaucho na grama.
- **Clique direito** ou **Esc**: cancelar a construcao.
- Botao **x1/x2**: alternar a velocidade.
- **R**: reiniciar depois de uma vitoria ou derrota.
- **M**: voltar ao menu depois de uma vitoria ou derrota.

## Objetivo da versao atual

- Comece sem gauchos: contrate com **1** ou **2** e posicione onde quiser.
- Defender o galpao por 6 ondas antes do tempo acabar (pilas, vidas e tempo dependem do nivel).
- Animais eliminados rendem pilas.
- Use as pilas para reforcar a defesa.
- Javalis cinza sao mais resistentes e causam 2 de dano ao galpao.

## Pontuacao e ranking

Ao fim de cada partida (vitoria ou derrota) a pontuacao e salva no ranking (top 10, menu > RANKING):

- 10 pontos por animal eliminado e 100 por onda vencida;
- na vitoria: +500, +20 por vida restante e +2 por segundo que sobrou;
- multiplicador do nivel: Facil x1, Normal x1,5, Dificil x2.

## Efeitos sonoros

Tiros, acertos, morte dos animais, dano no galpao e os jingles de vitoria/derrota sao sintetizados em codigo (`TorreGaucha.cs` e `EfeitosSonoros.cs`), sem arquivos de audio.

## Estrutura principal

- `Assets/Scenes/Menu.unity` + `Assets/Scripts/MenuGaucho.cs`: menu principal (jogar, nivel, ranking, como jogar, sair).
- `Assets/Scenes/Teste_PixelArt.unity`: mapa principal.
- `Assets/Scripts/JogoGaucho.cs`: ondas, economia, HUD e construcao.
- `Assets/Scripts/InimigoGaucho.cs`: vida e movimento dos inimigos.
- `Assets/Scripts/TorreGaucha.cs`: mira, tiros e dano das torres.
- `Assets/Scripts/Projetil.cs` + prefabs em `Assets/Resources/`: projeteis que voam ate o inimigo e causam dano na colisao (bala do revolver e chumbo em leque da espingarda).
- `Assets/Scripts/Ranking.cs`: pontuacao das partidas salva no PlayerPrefs (top 10).
- `Assets/Scripts/EfeitosSonoros.cs`: sons de acerto, morte, dano no galpao, vitoria e derrota.
- `Assets/Scripts/AmbienteGaucho.cs`: vida no cenario (fumaca e brasas do assado, sombras de nuvens, bandos de quero-quero com piado e ovelhas pastando), tudo gerado em codigo.
- `Assets/Scripts/MusicaDeFundo.cs` + `Assets/Resources/Musica/`: trilha sonora em loop, que continua tocando entre o menu e a fase.

## Creditos

| Asset | Autor | Licenca |
|---|---|---|
| Trilha sonora ["Las Dos Fronteras"](https://opengameart.org/content/mml-las-dos-fronteras) | Patrick de Arteaga ([patrickdearteaga.com](https://patrickdearteaga.com)) | CC-BY 4.0 |
| [Tiny Town](https://kenney.nl/assets/tiny-town) e [Tiny Farm](https://opengameart.org/content/tiny-farm) | Kenney | CC0 |
| [Ninja Adventure](https://pixel-boy.itch.io/ninja-adventure-asset-pack) (javalis) | Pixel-Boy e AAA | CC0 |
| [Tiny gun icons (16x16)](https://opengameart.org/content/tiny-gun-icons-16x16) (revolver e espingarda) | congusbongus | CC0 |
| [Argentinian Assets Pack](https://kwyjibo.itch.io/argentinian-assets-pack) (mate e assado) | kwyjibo | Gratuito, com credito ao autor |
