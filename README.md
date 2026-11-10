# Game Gaucho - Defesa do Galpao

Tower defense 2D em pixel art inspirado em Bloons TD, ambientado no Rio Grande do Sul.

## Como abrir e jogar

1. Abra o Unity Hub.
2. Em **Projects > Add**, escolha esta pasta.
3. Use a versao **Unity 6.0 (6000.0.85f2)**.
4. Na janela Project, abra `Assets/Scenes/Teste_PixelArt`.
5. Clique no botao **Play** (triangulo no topo).

Nao e necessario montar objetos ou configurar componentes: a camada jogavel e preparada automaticamente ao iniciar a cena.

## Controles

- **Espaco**: iniciar a proxima onda.
- **1**: contratar gaucho de revolver (rapido).
- **2**: contratar gaucho de espingarda (chumbo em leque: forte e acerta varios).
- **Clique esquerdo**: posicionar o gaucho na grama.
- **Clique direito** ou **Esc**: cancelar a construcao.
- Botao **x1/x2**: alternar a velocidade.
- **R**: reiniciar depois de uma vitoria ou derrota.

## Objetivo da versao atual

- Comece sem gauchos: contrate com **1** ou **2** e posicione onde quiser (360 pilas iniciais).
- Defender 20 vidas do galpao por 6 ondas.
- Animais eliminados rendem pilas.
- Use as pilas para reforcar a defesa.
- Javalis cinza sao mais resistentes e causam 2 de dano ao galpao.

## Estrutura principal

- `Assets/Scenes/Teste_PixelArt.unity`: mapa principal.
- `Assets/Scripts/JogoGaucho.cs`: ondas, economia, HUD e construcao.
- `Assets/Scripts/InimigoGaucho.cs`: vida e movimento dos inimigos.
- `Assets/Scripts/TorreGaucha.cs`: mira, tiros e dano das torres.
- `Assets/Scripts/Projetil.cs` + prefabs em `Assets/Resources/`: projeteis que voam ate o inimigo e causam dano na colisao (bala do revolver e chumbo em leque da espingarda).
