using System.Collections.Generic;

public sealed class GrupoOnda
{
    public DefinicaoInimigo bicho;
    public int quantidade;
    public float espacamento;
    public float inicio;
    public bool furtivo;
    public bool teimoso;
    public bool cascaGrossa;
}

public static class OndasGaucho
{
    static List<GrupoOnda[]> ondas;

    public static int Total
    {
        get
        {
            Montar();
            return ondas.Count;
        }
    }

    public static GrupoOnda[] Onda(int numero)
    {
        Montar();
        return ondas[numero - 1];
    }

    static GrupoOnda G(DefinicaoInimigo bicho, int quantidade, float espacamento, float inicio = 0f, string modificadores = "")
    {
        return new GrupoOnda
        {
            bicho = bicho,
            quantidade = quantidade,
            espacamento = espacamento,
            inicio = inicio,
            furtivo = modificadores.Contains("f"),
            teimoso = modificadores.Contains("t"),
            cascaGrossa = modificadores.Contains("c")
        };
    }

    static void Montar()
    {
        if (ondas != null)
            return;

        ondas = new List<GrupoOnda[]>();
        ondas.Add(new[] { G(TiposInimigo.Prea, 20, 0.9f) });
        ondas.Add(new[] { G(TiposInimigo.Prea, 32, 0.6f) });
        ondas.Add(new[] { G(TiposInimigo.Prea, 20, 0.5f), G(TiposInimigo.Perdiz, 12, 0.9f, 4f) });
        ondas.Add(new[] { G(TiposInimigo.Perdiz, 25, 0.55f), G(TiposInimigo.Prea, 20, 0.4f, 3f) });
        ondas.Add(new[] { G(TiposInimigo.Lebre, 12, 1f), G(TiposInimigo.Perdiz, 20, 0.5f, 2f) });
        ondas.Add(new[] { G(TiposInimigo.Lebre, 22, 0.6f), G(TiposInimigo.Graxaim, 4, 2f, 6f) });
        ondas.Add(new[] { G(TiposInimigo.Caturrita, 10, 0.8f), G(TiposInimigo.Lebre, 20, 0.5f, 2f), G(TiposInimigo.Perdiz, 20, 0.3f, 8f) });
        ondas.Add(new[] { G(TiposInimigo.Tatu, 3, 3f), G(TiposInimigo.Caturrita, 14, 0.6f, 2f) });
        ondas.Add(new[] { G(TiposInimigo.Ema, 12, 0.8f), G(TiposInimigo.TucoTuco, 5, 1.4f, 3f), G(TiposInimigo.Graxaim, 5, 1.2f, 6f) });
        ondas.Add(new[] { G(TiposInimigo.Capivara, 2, 4f), G(TiposInimigo.Pinguim, 6, 1.2f, 2f), G(TiposInimigo.Ema, 12, 0.6f, 5f) });
        ondas.Add(new[] { G(TiposInimigo.Gamba, 5, 1.6f), G(TiposInimigo.Sapo, 6, 1.1f, 3f), G(TiposInimigo.Tatu, 4, 1.6f, 7f) });
        ondas.Add(new[] { G(TiposInimigo.Zorrilho, 6, 1.4f), G(TiposInimigo.Graxaim, 10, 0.7f, 2f), G(TiposInimigo.EnxameBorrachudos, 1, 1f, 8f) });
        ondas.Add(new[] { G(TiposInimigo.Capivara, 5, 1.8f), G(TiposInimigo.Gamba, 5, 1.4f, 4f), G(TiposInimigo.Ema, 16, 0.4f, 8f, "t") });
        ondas.Add(new[] { G(TiposInimigo.Capivara, 8, 1.3f, 0f, "t"), G(TiposInimigo.Tatu, 8, 1.1f, 3f, "c"), G(TiposInimigo.TucoTuco, 10, 0.6f, 6f) });
        ondas.Add(new[] { G(TiposInimigo.Javali, 1, 1f), G(TiposInimigo.Capivara, 8, 1.1f, 4f), G(TiposInimigo.NuvemGafanhotos, 1, 1f, 9f) });
        ondas.Add(new[] { G(TiposInimigo.Javali, 2, 6f), G(TiposInimigo.Gamba, 12, 0.8f, 2f, "t"), G(TiposInimigo.Zorrilho, 8, 0.9f, 6f) });
        ondas.Add(new[] { G(TiposInimigo.Jacare, 1, 1f), G(TiposInimigo.Capivara, 12, 0.9f, 3f, "f"), G(TiposInimigo.Javali, 2, 5f, 8f) });
        ondas.Add(new[] { G(TiposInimigo.Javali, 5, 3.2f), G(TiposInimigo.Capivara, 12, 0.7f, 4f, "c"), G(TiposInimigo.Jacare, 2, 6f, 10f) });
        ondas.Add(new[] { G(TiposInimigo.JavaliCinza, 1, 1f), G(TiposInimigo.Graxaim, 16, 0.45f, 4f), G(TiposInimigo.Javali, 5, 2.8f, 6f), G(TiposInimigo.Jacare, 2, 5f, 14f) });
        ondas.Add(new[] { G(TiposInimigo.ReiJavaporco, 1, 1f), G(TiposInimigo.Ema, 30, 0.25f, 2f, "f"), G(TiposInimigo.Javali, 4, 3.5f, 5f) });
    }
}
