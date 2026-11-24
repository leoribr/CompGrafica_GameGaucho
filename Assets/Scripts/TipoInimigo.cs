using UnityEngine;

public enum CategoriaBicho
{
    Pequeno,
    Especial,
    Grande
}

public sealed class DefinicaoInimigo
{
    public string nome;
    public string desenho = "";
    public string descricao = "";
    public Color cor = Color.white;
    public float escala = 1f;
    public int vida = 1;
    public float velocidade = 1f;
    public int recompensa = 1;
    public int danoNaBase = 1;
    public int peso = 1;
    public CategoriaBicho categoria = CategoriaBicho.Pequeno;
    public bool furtivo;
    public bool blindado;
    public bool imuneLentidao;
    public bool chefe;
    public bool coroa;
    public TipoDano[] imunidades = new TipoDano[0];
    public DefinicaoInimigo filho;
    public int quantidadeFilhos;
    public DefinicaoInimigo filho2;
    public int quantidadeFilhos2;
    public bool filhosTeimosos;
    public DefinicaoInimigo soltaAoAndar;
    public float intervaloSoltar = 1f;
    public DefinicaoInimigo soltaAoApanhar;
    public int danoParaSoltar = 10;

    public bool Grande => categoria == CategoriaBicho.Grande;

    public bool ImuneA(TipoDano tipo)
    {
        if (blindado && (tipo == TipoDano.Corte || tipo == TipoDano.Bala))
            return true;
        for (int i = 0; i < imunidades.Length; i++)
        {
            if (imunidades[i] == tipo)
                return true;
        }
        return false;
    }
}

public static class TiposInimigo
{
    public static readonly DefinicaoInimigo Prea = new DefinicaoInimigo
    {
        nome = "Preá", desenho = "prea", vida = 1, velocidade = 1f, recompensa = 1, danoNaBase = 1,
        descricao = "o bicho mais comum do campo"
    };

    public static readonly DefinicaoInimigo Perdiz = new DefinicaoInimigo
    {
        nome = "Perdiz", desenho = "perdiz", vida = 2, velocidade = 1.4f, recompensa = 2, danoNaBase = 1,
        descricao = "corre rente ao chão"
    };

    public static readonly DefinicaoInimigo Lebre = new DefinicaoInimigo
    {
        nome = "Lebre", desenho = "lebre", vida = 3, velocidade = 1.8f, recompensa = 3, danoNaBase = 1,
        descricao = "espécie invasora, ligeira"
    };

    public static readonly DefinicaoInimigo Caturrita = new DefinicaoInimigo
    {
        nome = "Caturrita", desenho = "caturrita", vida = 4, velocidade = 3.2f, recompensa = 4, danoNaBase = 1,
        descricao = "praga das lavouras, muito rápida"
    };

    public static readonly DefinicaoInimigo Ema = new DefinicaoInimigo
    {
        nome = "Ema", desenho = "ema", vida = 5, velocidade = 3.5f, recompensa = 5, danoNaBase = 1,
        descricao = "a corredora do pampa, a mais rápida do bando"
    };

    public static readonly DefinicaoInimigo Graxaim = new DefinicaoInimigo
    {
        nome = "Graxaim", desenho = "graxaim", vida = 6, velocidade = 2.5f, recompensa = 6, danoNaBase = 2,
        peso = 2, categoria = CategoriaBicho.Especial, furtivo = true,
        descricao = "FURTIVO: só quem vê furtivo consegue mirar nele"
    };

    public static readonly DefinicaoInimigo TucoTuco = new DefinicaoInimigo
    {
        nome = "Tuco-tuco", desenho = "tuco", vida = 11, velocidade = 1.8f, recompensa = 7, danoNaBase = 2,
        peso = 2, categoria = CategoriaBicho.Especial, imunidades = new[] { TipoDano.Explosao },
        descricao = "imune a explosão: se enfia na toca"
    };

    public static readonly DefinicaoInimigo Pinguim = new DefinicaoInimigo
    {
        nome = "Pinguim", desenho = "pinguim", vida = 11, velocidade = 2f, recompensa = 7, danoNaBase = 2,
        peso = 2, categoria = CategoriaBicho.Especial, imunidades = new[] { TipoDano.Frio },
        descricao = "imune ao frio: o Minuano não assusta ele"
    };

    public static readonly DefinicaoInimigo Sapo = new DefinicaoInimigo
    {
        nome = "Sapo-cururu", desenho = "sapo", vida = 11, velocidade = 3f, recompensa = 7, danoNaBase = 2,
        peso = 2, categoria = CategoriaBicho.Especial, imunidades = new[] { TipoDano.Fogo, TipoDano.Magica },
        descricao = "imune a fogo e mágica"
    };

    public static readonly DefinicaoInimigo Tatuzinho = new DefinicaoInimigo
    {
        nome = "Tatuzinho", desenho = "tatu", cor = new Color(1f, 0.92f, 0.85f), escala = 0.65f,
        vida = 3, velocidade = 1.4f, recompensa = 3, danoNaBase = 1,
        descricao = "filhote de tatu, ainda de casco mole"
    };

    public static readonly DefinicaoInimigo Tatu = new DefinicaoInimigo
    {
        nome = "Tatu", desenho = "tatu", vida = 8, velocidade = 1f, recompensa = 6, danoNaBase = 2,
        peso = 2, categoria = CategoriaBicho.Especial, blindado = true, filho = Tatuzinho, quantidadeFilhos = 4,
        descricao = "BLINDADO: corte e bala comum não furam o casco"
    };

    public static readonly DefinicaoInimigo Zorrilho = new DefinicaoInimigo
    {
        nome = "Zorrilho", desenho = "zorrilho", vida = 23, velocidade = 1.8f, recompensa = 12, danoNaBase = 2,
        peso = 2, categoria = CategoriaBicho.Especial, imunidades = new[] { TipoDano.Explosao, TipoDano.Frio },
        descricao = "imune a explosão e frio"
    };

    public static readonly DefinicaoInimigo FilhoteGamba = new DefinicaoInimigo
    {
        nome = "Filhote de gambá", desenho = "gamba", escala = 0.6f, vida = 6, velocidade = 2.5f, recompensa = 4, danoNaBase = 1,
        descricao = "desce das costas da mãe"
    };

    public static readonly DefinicaoInimigo Gamba = new DefinicaoInimigo
    {
        nome = "Gambá", desenho = "gamba", vida = 12, velocidade = 2.2f, recompensa = 9, danoNaBase = 2,
        peso = 2, categoria = CategoriaBicho.Especial, filho = FilhoteGamba, quantidadeFilhos = 4,
        descricao = "solta 4 filhotes quando cai"
    };

    public static readonly DefinicaoInimigo Capivara = new DefinicaoInimigo
    {
        nome = "Capivara", desenho = "capivara", vida = 14, velocidade = 1.6f, recompensa = 12, danoNaBase = 3,
        peso = 2, categoria = CategoriaBicho.Especial, filho = Gamba, quantidadeFilhos = 2,
        descricao = "amiga de todo mundo: traz 2 gambás nas costas"
    };

    public static readonly DefinicaoInimigo Javali = new DefinicaoInimigo
    {
        nome = "Javali", desenho = "javali", escala = 1.25f, vida = 120, velocidade = 1f, recompensa = 40, danoNaBase = 10,
        peso = 3, categoria = CategoriaBicho.Grande, filho = Capivara, quantidadeFilhos = 4,
        descricao = "BICHO GRANDE: muita vida, solta 4 capivaras"
    };

    public static readonly DefinicaoInimigo JavaliCinza = new DefinicaoInimigo
    {
        nome = "Javali Cinza", desenho = "javali_cinza", escala = 1.55f, vida = 400, velocidade = 0.3f, recompensa = 80, danoNaBase = 15,
        peso = 4, categoria = CategoriaBicho.Grande, filho = Javali, quantidadeFilhos = 4,
        descricao = "a porca velha que lidera a vara: solta 4 javalis"
    };

    public static readonly DefinicaoInimigo Javaporco = new DefinicaoInimigo
    {
        nome = "Javaporco", desenho = "javali", cor = new Color(1f, 0.72f, 0.78f), escala = 1.9f, vida = 1500, velocidade = 0.22f,
        recompensa = 150, danoNaBase = 20, peso = 5, categoria = CategoriaBicho.Grande, filho = JavaliCinza, quantidadeFilhos = 2,
        descricao = "javali cruzado com porco: enorme"
    };

    public static readonly DefinicaoInimigo Jacare = new DefinicaoInimigo
    {
        nome = "Jacaré", desenho = "jacare", vida = 250, velocidade = 2.6f, recompensa = 50, danoNaBase = 10,
        peso = 3, categoria = CategoriaBicho.Grande, furtivo = true, blindado = true, imunidades = new[] { TipoDano.Explosao },
        filho = Capivara, quantidadeFilhos = 6, filhosTeimosos = true,
        descricao = "FURTIVO e BLINDADO, rápido: solta 6 capivaras teimosas"
    };

    public static readonly DefinicaoInimigo ReiJavaporco = new DefinicaoInimigo
    {
        nome = "Rei Javaporco", desenho = "javali", cor = new Color(0.9f, 0.5f, 0.45f), escala = 2.4f, vida = 4000, velocidade = 0.18f,
        recompensa = 500, danoNaBase = 999, peso = 99, categoria = CategoriaBicho.Grande, chefe = true, coroa = true, imuneLentidao = true,
        filho = Javaporco, quantidadeFilhos = 1, filho2 = Jacare, quantidadeFilhos2 = 2,
        descricao = "O CHEFE: se chegar ao galpão, acabou"
    };

    public static readonly DefinicaoInimigo Gafanhoto = new DefinicaoInimigo
    {
        nome = "Gafanhoto", desenho = "gafanhoto", escala = 0.75f, vida = 1, velocidade = 2.2f, recompensa = 1, danoNaBase = 1,
        descricao = "cai da nuvem"
    };

    public static readonly DefinicaoInimigo NuvemGafanhotos = new DefinicaoInimigo
    {
        nome = "Nuvem de Gafanhotos", desenho = "nuvem", escala = 1.4f, vida = 300, velocidade = 0.5f, recompensa = 60, danoNaBase = 8,
        peso = 4, categoria = CategoriaBicho.Grande, chefe = true, soltaAoAndar = Gafanhoto, intervaloSoltar = 0.7f,
        descricao = "CHEFE: vai largando gafanhotos pelo caminho"
    };

    public static readonly DefinicaoInimigo Borrachudo = new DefinicaoInimigo
    {
        nome = "Borrachudo", desenho = "borrachudo", escala = 0.7f, vida = 2, velocidade = 2f, recompensa = 2, danoNaBase = 1,
        furtivo = true, descricao = "pequeno demais para ver"
    };

    public static readonly DefinicaoInimigo EnxameBorrachudos = new DefinicaoInimigo
    {
        nome = "Enxame de Borrachudos", desenho = "enxame", escala = 1.3f, vida = 120, velocidade = 0.9f, recompensa = 45, danoNaBase = 5,
        peso = 3, categoria = CategoriaBicho.Grande, chefe = true, furtivo = true, soltaAoApanhar = Borrachudo, danoParaSoltar = 10,
        descricao = "CHEFE FURTIVO: se divide quando apanha"
    };

    public static readonly DefinicaoInimigo[] Todos =
    {
        Prea, Perdiz, Lebre, Caturrita, Ema, Graxaim, TucoTuco, Pinguim, Sapo, Tatuzinho, Tatu, Zorrilho,
        FilhoteGamba, Gamba, Capivara, Javali, JavaliCinza, Javaporco, Jacare, ReiJavaporco,
        Gafanhoto, NuvemGafanhotos, Borrachudo, EnxameBorrachudos
    };
}
