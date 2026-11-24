using UnityEngine;

public sealed class LagoaGaucho
{
    const float PixelsPorUnidade = 16f;

    readonly Vector2 centro;
    readonly Vector2 raios;

    public GameObject Objeto { get; private set; }

    public LagoaGaucho(Vector2 centroDaLagoa, Vector2 raiosDaLagoa, Transform pai)
    {
        centro = centroDaLagoa;
        raios = raiosDaLagoa;
        Objeto = new GameObject("Lagoa");
        Objeto.transform.SetParent(pai, false);
        Objeto.transform.position = new Vector3(centro.x, centro.y, 0f);
        SpriteRenderer renderizador = Objeto.AddComponent<SpriteRenderer>();
        renderizador.sprite = CriarSprite();
        renderizador.sortingOrder = 2;
    }

    public bool Contem(Vector3 posicao, float margem)
    {
        float rx = raios.x - margem;
        float ry = raios.y - margem;
        if (rx <= 0f || ry <= 0f)
            return false;
        float dx = (posicao.x - centro.x) / rx;
        float dy = (posicao.y - centro.y) / ry;
        return dx * dx + dy * dy <= 1f;
    }

    Sprite CriarSprite()
    {
        int largura = Mathf.CeilToInt(raios.x * 2f * PixelsPorUnidade) + 2;
        int altura = Mathf.CeilToInt(raios.y * 2f * PixelsPorUnidade) + 2;
        Texture2D textura = new Texture2D(largura, altura, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Point;
        textura.wrapMode = TextureWrapMode.Clamp;

        Color margem = new Color(0.55f, 0.45f, 0.3f);
        Color rasa = new Color(0.27f, 0.56f, 0.8f);
        Color funda = new Color(0.17f, 0.4f, 0.65f);
        Color brilho = new Color(0.65f, 0.85f, 0.97f);
        Color taboa = new Color(0.3f, 0.5f, 0.2f);

        for (int y = 0; y < altura; y++)
        {
            for (int x = 0; x < largura; x++)
            {
                float nx = (x + 0.5f - largura * 0.5f) / (raios.x * PixelsPorUnidade);
                float ny = (y + 0.5f - altura * 0.5f) / (raios.y * PixelsPorUnidade);
                float distancia = Mathf.Sqrt(nx * nx + ny * ny);
                Color cor = new Color(0f, 0f, 0f, 0f);
                if (distancia <= 1f)
                {
                    if (distancia > 0.9f)
                        cor = (x * 7 + y * 3) % 11 == 0 ? taboa : margem;
                    else if (distancia > 0.55f)
                        cor = rasa;
                    else
                        cor = funda;
                    if (distancia < 0.85f && y % 5 == 2 && (x + y) % 9 < 3)
                        cor = brilho;
                }
                textura.SetPixel(x, y, cor);
            }
        }
        textura.Apply();
        return Sprite.Create(textura, new Rect(0f, 0f, largura, altura), new Vector2(0.5f, 0.5f), PixelsPorUnidade);
    }
}
