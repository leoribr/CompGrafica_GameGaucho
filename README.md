# Game Gaucho - Defesa do Galpao

Tower defense 2D em pixel art inspirado em Bloons TD, ambientado no Rio Grande do Sul.

Trabalho pratico da disciplina CCC300 - Computacao Grafica e Realidade Virtual (UPF, 2026/2).

## Downloads

- **Executavel (Windows):** [GameGaucho_Windows.zip](https://github.com/leoribr/CompGrafica_GameGaucho/releases/latest/download/GameGaucho_Windows.zip) ([pagina da release](https://github.com/leoribr/CompGrafica_GameGaucho/releases/latest)): extraia a pasta e abra `GameGaucho.exe`.
- **Video demonstrativo:** [assistir no YouTube](https://www.youtube.com/watch?v=9MOvQtDeSEA)
- **Roteiro do pitch e guia do codigo:** [PITCH.md](PITCH.md)

## Como abrir e jogar

1. Abra o Unity Hub.
2. Em **Projects > Add**, escolha esta pasta.
3. Use a versao **Unity 6.0 (6000.0.85f2)**.
4. Na janela Project, abra `Assets/Scenes/Menu`.
5. Clique no botao **Play** (triangulo no topo).

No menu da para escolher o nivel (Facil, Normal ou Dificil) e o modo de jogo, ver o ranking, as instrucoes e as opcoes (volume e tela cheia). Tambem e possivel abrir `Assets/Scenes/Teste_PixelArt` direto: a camada jogavel e preparada automaticamente ao iniciar a cena.

## Controles

- **Espaco**: iniciar a proxima onda. Com uma onda em andamento, chama a seguinte antes da hora (as ondas se sobrepoem).
- **A**: ligar ou desligar o inicio automatico das ondas.
- **Tab**: trocar a classe da loja (Peoes, Farrapos, Lendas, Estancia).
- **1 a 7**: contratar o gaucho da classe aberta.
- **Clique esquerdo**: posicionar o gaucho (o anel mostra o alcance) ou selecionar um gaucho ja contratado.
- **Z, X, C**: comprar o proximo nivel do caminho 1, 2 ou 3 do gaucho selecionado.
- **V**: vender o gaucho selecionado (devolve 70% do que foi gasto).
- **P**: trocar a prioridade de alvo (primeiro, ultimo, mais perto, mais forte).
- **Clique direito** ou **Esc**: cancelar a construcao ou tirar a selecao.
- **Esc** (sem nada selecionado) ou botao **PAUSA**: pausar; o painel de pausa tem volume e tela cheia.
- Botao **x1/x2/x3**: alternar a velocidade.
- **L**: depois da vitoria, continuar no jogo livre.
- **R**: reiniciar depois de uma vitoria ou derrota.
- **M**: voltar ao menu depois de uma vitoria ou derrota.

## Objetivo da versao atual

- Defender o galpao por 20 ondas antes do tempo acabar (pilas, vidas e tempo dependem do nivel).
- Cada gaucho tem 3 caminhos de melhoria com 5 niveis; so um caminho passa do nivel 2.
- Os bichos sao da fauna do RS e tem jeitos diferentes: furtivos (so quem ve furtivo mira neles), blindados (corte e bala comum nao furam), imunes a explosao, frio, fogo ou magica, teimosos (regeneram) e bichos grandes que soltam outros quando caem.
- A ultima onda traz o Rei Javaporco: se ele chegar ao galpao, a partida acaba. Ao chegar a 66% e a 33% da vida, ele fica blindado por alguns segundos e chama dois javalis.
- Modos de jogo (menu > MODO): Padrao, So Peoes (a loja so tem a classe Peoes), Metade das Pilas (bichos, bonus e rendas pagam a metade) e Desafio (sem vender e com uma vida so).
- Depois da vitoria, o jogo livre continua com ondas geradas por formula, cada vez mais fortes.
- O mapa tem uma lagoa dentro da primeira curva do caminho: o Pescador e o Lanchao so ficam na agua, e a Iara fica na agua ou na beira.

## Gauchos (26, em 4 classes)

Inspirados no papel das torres do Bloons TD 6, mas com personagens, nomes e melhorias proprios do RS. Nao ha habilidades ativaveis: tudo e passivo ou automatico.

- **Peoes:** Peao da Peixeira, Gaucho da Boleadeira, Fogueteiro, Assador do Churrasco, Gaucho do Minuano, Lacador e Gaucho do Revolver.
- **Farrapos:** Bombeador da Carabina, Pescador da Lagoa, Lanchao Farroupilha, Aviador do Teco-teco, Gaucho a Cavalo, Canhao Farroupilha e Gaucho da Espingarda.
- **Lendas e Causos:** Bruxo da Salamanca, Monarca das Coxilhas, Dancador da Chula, Benzedeira, Guardiao da Araucaria, Iara da Lagoa e Pia da Pandorga.
- **Estancia:** Charqueada (pacotes de charque para clicar), Alambrador das Rosetas, Roda de Chimarrao (fortalece quem esta perto), Ferreiro da Estancia e Peao Domador.

## Bichos

- **Bando do campo:** Prea, Perdiz, Lebre, Caturrita e Ema (do mais lento ao mais rapido).
- **Especiais:** Graxaim (furtivo), Tuco-tuco (imune a explosao), Pinguim (imune a frio), Sapo-cururu (imune a fogo e magica), Tatu (blindado, solta 4 tatuzinhos), Zorrilho, Gamba (solta filhotes) e Capivara (traz 2 gambas nas costas).
- **Bichos grandes:** Javali, Javali Cinza, Javaporco, Jacare (furtivo e blindado) e o chefe final, Rei Javaporco.
- **Chefes de evento:** Nuvem de Gafanhotos (larga gafanhotos pelo caminho) e Enxame de Borrachudos (furtivo, se divide quando apanha).

## Pontuacao e ranking

Ao fim de cada partida (vitoria ou derrota) a pontuacao e salva no ranking (top 10, menu > RANKING):

- 10 pontos por animal eliminado e 100 por onda vencida;
- na vitoria: +500, +20 por vida restante e +2 por segundo que sobrou;
- multiplicador do nivel: Facil x1, Normal x1,5, Dificil x2;
- multiplicador do modo: So Peoes x1,2, Metade das Pilas x1,3, Desafio x1,5.

A tela final mostra os bichos derrubados, as pilas ganhas, as ondas vencidas e os gauchos que mais derrubaram.

## Graficos

- Pixel art de gauchos, bichos, armas e projeteis gerada em codigo (`SpritesGaucho.cs`).
- Iluminacao 2D do URP: luz global, luzes no assado e no galpao e clarao nas explosoes, nos tiros e nos projeteis de fogo e magica.
- Shader proprio (`Assets/Resources/SpriteEfeito.shader`): contorno branco no gaucho selecionado, contorno dourado no nivel 5 e piscar branco quando o bicho apanha.
- Sombras embaixo de bichos e gauchos, recuo da arma no tiro e tremor de camera nas explosoes grandes e na chegada dos chefes.
- O gaucho muda com as melhorias, no estilo do Bloons TD 6: duas armas quando a melhoria da duas (como o Par de Garruchas), cinturao de balas quando o tiro fica mais rapido, pena quando o alcance cresce, oculos quando passa a ver furtivo, arma maior quando o dano dobra e brilho na arma quando ganha fogo, frio ou magica.
- Pelo nivel: faixa no poncho com a cor do caminho no nivel 3, chapeu dessa cor no nivel 4 e, no nivel 5, contorno dourado, lenco dourado e brilhos. Corpos que nao sao gaucho ganham uma bandeirola.

## Efeitos sonoros

Tiros, acertos, morte dos animais, dano no galpao e os jingles de vitoria/derrota sao sintetizados em codigo (`TorreGaucha.cs` e `EfeitosSonoros.cs`), sem arquivos de audio.

## Estrutura principal

- `Assets/Scenes/Menu.unity` + `Assets/Scripts/MenuGaucho.cs`: menu principal (jogar, nivel, ranking, como jogar, sair).
- `Assets/Scenes/Teste_PixelArt.unity`: mapa principal.
- `Assets/Scripts/JogoGaucho.cs`: ondas, economia, construcao, venda, melhorias e filhotes.
- `Assets/Scripts/JogoGauchoCombate.cs`: mira com prioridade, imunidades, explosoes, especiais (rede, zona, laco, armadilha, espantalho), auras de apoio e efeitos visuais.
- `Assets/Scripts/JogoGauchoInterface.cs`: HUD, loja por classe e painel com os 3 caminhos de melhoria.
- `Assets/Scripts/TipoTorre.cs` + `TorresPeoes.cs`, `TorresFarrapos.cs`, `TorresLendas.cs`, `TorresEstancia.cs`: atributos e caminhos de melhoria de cada gaucho.
- `Assets/Scripts/MelhoriasTorre.cs`: custo dos niveis, regra dos caminhos e valor de venda.
- `Assets/Scripts/TipoInimigo.cs` + `OndasGaucho.cs`: bichos (vida, velocidade, imunidades, filhotes) e as 20 ondas.
- `Assets/Scripts/InimigoGaucho.cs`: movimento, lentidao, congelamento, queimadura, empurrao e regeneracao dos bichos.
- `Assets/Scripts/TorreGaucha.cs`: ataques (projetil, radial, pulso, instantaneo, corpo a corpo, bumerangue, ponto fixo), movimento e especiais.
- `Assets/Scripts/SpritesGaucho.cs`: pixel art dos bichos, armas e projeteis gerada em codigo.
- `Assets/Scripts/LagoaGaucho.cs`: a lagoa do mapa (desenho e regra de quem pode ficar na agua).
- `Assets/Scripts/Projetil.cs` + prefabs em `Assets/Resources/`: projeteis com colisao, perfuracao, ricochete, bumerangue e caminho.
- `Assets/Scripts/Ranking.cs`: pontuacao das partidas salva no PlayerPrefs (top 10).
- `Assets/Scripts/ModoDeJogo.cs`: modos de jogo (Padrao, So Peoes, Metade das Pilas e Desafio).
- `Assets/Scripts/OpcoesGaucho.cs`: volume e tela cheia, usados no menu e na pausa.
- `Assets/Resources/SpriteEfeito.shader`: shader de contorno e de piscar branco.
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
