using System.Collections.Generic;
using UnityEngine;

public static class SpritesGaucho
{
    const float PixelsPorUnidade = 16f;

    static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    static readonly Dictionary<string, Sprite[]> quadros = new Dictionary<string, Sprite[]>();
    static Material materialSelecao;
    static Material materialDourado;
    static Material materialBranco;
    static Shader shaderDeEfeito;
    static bool shaderProcurado;

    static readonly string[] Armas =
    {
        "peixeira", "boleadeira", "rojao", "espeto", "laco", "carabina", "arpao", "lanca", "cano",
        "cajado", "relho", "martelo", "garrafa", "pinha", "arame", "roseta"
    };

    static readonly string[] DesenhoGaucho =
    {
        "....KKKKKKKK....",
        "...KKKKKKKKKK...",
        ".KKKK112211KKKK.",
        "KKKKK111111KKKKK",
        "KK21K333333K12KK",
        "KK211111111112KK",
        "KKK2222222222KKK",
        ".KKK44544454KKK.",
        "..KK66566656KK..",
        "..KKK664446KKK..",
        ".KKK78933987KKK.",
        "KKK7787777877KKK",
        "KK663808808366KK",
        "KK66K888888K66KK",
        "KKKKK88KK88KKKKK",
        ".KKKK99KK99KKKK."
    };

    public static Sprite Pegar(string nome)
    {
        Sprite sprite;
        if (sprites.TryGetValue(nome, out sprite))
            return sprite;

        string[] desenho = Desenho(nome);
        if (desenho == null)
            return null;

        Vector2 pivo = System.Array.IndexOf(Armas, nome) >= 0 ? new Vector2(0.15f, 0.5f) : new Vector2(0.5f, 0.5f);
        sprite = Criar(desenho, nome, pivo, Color.clear, Color.clear);
        sprites[nome] = sprite;
        return sprite;
    }

    public static Sprite[] Quadros(string nome)
    {
        Sprite[] lista;
        if (quadros.TryGetValue(nome, out lista))
            return lista;

        string[][] desenhos = DesenhosDeBicho(nome);
        if (desenhos == null)
            return null;

        lista = new Sprite[desenhos.Length];
        for (int i = 0; i < desenhos.Length; i++)
            lista[i] = Criar(desenhos[i], nome + i, new Vector2(0.5f, 0.5f), Color.clear, Color.clear);
        quadros[nome] = lista;
        return lista;
    }

    public static Sprite Gaucho(Color poncho, Color lenco)
    {
        return Gaucho(poncho, lenco, Color.clear);
    }

    public static Sprite Gaucho(Color poncho, Color lenco, Color faixa)
    {
        string chave = "gaucho" + ColorUtility.ToHtmlStringRGB(poncho) + ColorUtility.ToHtmlStringRGB(lenco);
        if (faixa.a > 0f)
            chave += ColorUtility.ToHtmlStringRGB(faixa);
        Sprite sprite;
        if (sprites.TryGetValue(chave, out sprite))
            return sprite;
        string[] desenho = DesenhoGaucho;
        if (faixa.a > 0f)
        {
            desenho = (string[])DesenhoGaucho.Clone();
            desenho[11] = desenho[11].Replace('7', 'X');
        }
        sprite = Criar(desenho, chave, new Vector2(0.5f, 0.5f), poncho, lenco, faixa);
        sprites[chave] = sprite;
        return sprite;
    }

    static string[] Camada(int primeira, params string[] linhas)
    {
        string[] desenho = new string[16];
        for (int i = 0; i < 16; i++)
            desenho[i] = i >= primeira && i - primeira < linhas.Length ? linhas[i - primeira] : "................";
        return desenho;
    }

    static string[] CamadaLarga(params string[] linhas)
    {
        string[] desenho = new string[20];
        for (int i = 0; i < 20; i++)
            desenho[i] = i < linhas.Length ? linhas[i] : "....................";
        return desenho;
    }

    public static Sprite Circulo()
    {
        Sprite sprite;
        if (sprites.TryGetValue("circulo", out sprite))
            return sprite;

        const int tamanho = 32;
        Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);
        textura.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                float dx = (x + 0.5f) / tamanho - 0.5f;
                float dy = (y + 0.5f) / tamanho - 0.5f;
                float distancia = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
                float alfa = Mathf.Clamp01((1f - distancia) * 4f);
                textura.SetPixel(x, y, new Color(1f, 1f, 1f, alfa));
            }
        }
        textura.Apply();
        sprite = Sprite.Create(textura, new Rect(0f, 0f, tamanho, tamanho), new Vector2(0.5f, 0.5f), tamanho);
        sprites["circulo"] = sprite;
        return sprite;
    }

    static Sprite Criar(string[] desenho, string nome, Vector2 pivo, Color poncho, Color lenco, Color extra = default)
    {
        int altura = desenho.Length;
        int largura = 0;
        for (int i = 0; i < altura; i++)
            largura = Mathf.Max(largura, desenho[i].Length);

        Texture2D textura = new Texture2D(largura + 2, altura + 2, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Point;
        textura.wrapMode = TextureWrapMode.Clamp;
        textura.name = nome;
        textura.SetPixels(new Color[(largura + 2) * (altura + 2)]);
        for (int y = 0; y < altura; y++)
        {
            string linha = desenho[altura - 1 - y];
            for (int x = 0; x < largura; x++)
            {
                char c = x < linha.Length ? linha[x] : '.';
                Color cor = Cor(c);
                if (c == '7' && poncho.a > 0f)
                    cor = poncho;
                if (c == '8' && poncho.a > 0f)
                    cor = new Color(poncho.r * 0.5f, poncho.g * 0.5f, poncho.b * 0.5f);
                if (c == '3' && lenco.a > 0f)
                    cor = lenco;
                if (c == 'X')
                    cor = extra;
                textura.SetPixel(x + 1, y + 1, cor);
            }
        }
        textura.Apply();
        Vector2 pivoComBorda = new Vector2((pivo.x * largura + 1f) / (largura + 2f), (pivo.y * altura + 1f) / (altura + 2f));
        return Sprite.Create(textura, new Rect(0f, 0f, largura + 2, altura + 2), pivoComBorda, PixelsPorUnidade, 0, SpriteMeshType.FullRect);
    }

    public static Material MaterialContorno(bool dourado)
    {
        if (ShaderDeEfeito() == null)
            return null;
        if (dourado)
        {
            if (materialDourado == null)
                materialDourado = NovoContorno(new Color(1f, 0.78f, 0.2f, 1f));
            return materialDourado;
        }
        if (materialSelecao == null)
            materialSelecao = NovoContorno(Color.white);
        return materialSelecao;
    }

    static Material NovoContorno(Color cor)
    {
        Material material = new Material(shaderDeEfeito);
        material.SetFloat("_Contorno", 1f);
        material.SetColor("_CorContorno", cor);
        return material;
    }

    public static Material MaterialBranco()
    {
        if (materialBranco == null && ShaderDeEfeito() != null)
        {
            materialBranco = new Material(shaderDeEfeito);
            materialBranco.SetFloat("_Branco", 1f);
        }
        return materialBranco;
    }

    static Shader ShaderDeEfeito()
    {
        if (!shaderProcurado)
        {
            shaderProcurado = true;
            shaderDeEfeito = Resources.Load<Shader>("SpriteEfeito");
            if (shaderDeEfeito != null && !shaderDeEfeito.isSupported)
                shaderDeEfeito = null;
        }
        return shaderDeEfeito;
    }

    static Color Hex(int rgb)
    {
        return new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);
    }

    static Color Cor(char c)
    {
        switch (c)
        {
            case 'k': return Hex(0x141b1b);
            case 'K': return Hex(0x3f2631);
            case 'w': return Hex(0xf2eaf1);
            case 'W': return Hex(0xc0cbdc);
            case 'g': return Hex(0x8b9bb4);
            case 'G': return Hex(0x52607c);
            case 'm': return Hex(0xdadada);
            case 'M': return Hex(0x8a8a8a);
            case 'n': return Hex(0x515151);
            case 'b': return Hex(0x8f5a3c);
            case 'B': return Hex(0x5a3a22);
            case 'c': return Hex(0xd9a878);
            case 'C': return Hex(0xf2d3a8);
            case 'o': return Hex(0xe07a2a);
            case 'O': return Hex(0xb8541a);
            case 'y': return Hex(0xf2d14a);
            case 'Y': return Hex(0xc9a227);
            case 'r': return Hex(0xd14b34);
            case 'R': return Hex(0x8f2a24);
            case 'p': return Hex(0xf2a1a1);
            case 'P': return Hex(0xc96b7a);
            case 'v': return Hex(0x6fbf4a);
            case 'V': return Hex(0x3e7a2e);
            case 'l': return Hex(0xa8d35a);
            case 'a': return Hex(0x4a90d9);
            case 'A': return Hex(0x2c5a9e);
            case 'i': return Hex(0xbfe8ff);
            case 'u': return Hex(0x8e5cc2);
            case 'U': return Hex(0x5a3a8a);
            case 's': return Hex(0xf2b98a);
            case 'h': return Hex(0x2a2a2a);
            case 'f': return Hex(0xffb02e);
            case 'F': return Hex(0xff5a1f);
            case 'z': return Hex(0xe3d08a);
            case 'Z': return Hex(0x8c7a45);
            case 't': return Hex(0x3fae9a);
            case 'T': return Hex(0x2a7a6c);
            case 'e': return Hex(0xe6e0c8);
            case '1': return Hex(0xcf8254);
            case '2': return Hex(0xfec99c);
            case '3': return Hex(0xc34b35);
            case '4': return Hex(0xe19a65);
            case '5': return Hex(0x262b44);
            case '6': return Hex(0xf7c282);
            case '7': return Hex(0xf28462);
            case '8': return Hex(0x52607c);
            case '9': return Hex(0xbd6c4a);
            case '0': return Hex(0x8c9cb5);
            default: return new Color(0f, 0f, 0f, 0f);
        }
    }

    static string[] Juntar(string[] corpo, params string[] pernas)
    {
        string[] resultado = new string[corpo.Length + pernas.Length];
        corpo.CopyTo(resultado, 0);
        pernas.CopyTo(resultado, corpo.Length);
        return resultado;
    }

    static string[][] DesenhosDeBicho(string nome)
    {
        switch (nome)
        {
            case "prea":
            {
                string[] corpo =
                {
                    "...kkkk....",
                    "..kbbbbkBk.",
                    ".kbbbbbbbbk",
                    "kbbbbbbbkbk",
                    "kbcccbbbbbp",
                    ".kkkkkkkkk."
                };
                return new[] { Juntar(corpo, ".kk...kk..."), Juntar(corpo, "..kk.kk....") };
            }
            case "perdiz":
            {
                string[] corpo =
                {
                    "......kkk..",
                    ".....kbbkk.",
                    "..kkkkbkbby",
                    ".kbcbcbbbk.",
                    "kBbbcbcbbk.",
                    ".kbbbbbbk..",
                    "..kkkkkk..."
                };
                return new[] { Juntar(corpo, "...k..k...."), Juntar(corpo, "....kk.....") };
            }
            case "lebre":
            {
                string[] corpo =
                {
                    ".........B.B.",
                    ".........B.B.",
                    "........kbkbk",
                    "...kkkkkbbbbk",
                    ".kkbbbbbbbkbk",
                    "kwbbbbbbbbbbp",
                    "kwbcccccbbbk.",
                    ".kkkkkkkkkk.."
                };
                return new[] { Juntar(corpo, "..kk....kk..."), Juntar(corpo, "...kk..kk....") };
            }
            case "caturrita":
                return new[]
                {
                    new[]
                    {
                        "..kk.......",
                        "..kvk......",
                        "...kvk.kkk.",
                        "kkkkvvkWWWk",
                        "kvvvvvvvWky",
                        ".kvvvvvvvk.",
                        "..kAkkkkk.."
                    },
                    new[]
                    {
                        "...........",
                        ".......kkk.",
                        "kkkkkkkWWWk",
                        "kvvvvvvvWky",
                        ".kvvvkvvvk.",
                        "..kvvk.kk..",
                        "...kAk....."
                    }
                };
            case "ema":
            {
                string[] corpo =
                {
                    "........kk.",
                    ".......kWWk",
                    ".......kWkW",
                    "........kGk",
                    "........kGk",
                    "........kGk",
                    "...kkkkkkGk",
                    ".kkgggggggk",
                    "kgggWggggGk",
                    "kgggWWgggk.",
                    ".kgggggggk.",
                    "..kkkkkkk.."
                };
                return new[]
                {
                    Juntar(corpo, "....k..k...", "....k...k..", "...kk...kk."),
                    Juntar(corpo, "....k..k...", "...k...k...", "..kk..kk...")
                };
            }
            case "graxaim":
            {
                string[] corpo =
                {
                    "...........k.k.",
                    "..........kgkgk",
                    "kk........kgggk",
                    "kgk.kkkkkkggkgW",
                    "kggkgggggggggWk",
                    ".kgkgggggggggk.",
                    "..kkgWWWWggk...",
                    "...kkkkkkkk...."
                };
                return new[] { Juntar(corpo, "....kk...kk...."), Juntar(corpo, ".....kk.kk.....") };
            }
            case "tuco":
            {
                string[] corpo =
                {
                    ".kkkkkk..",
                    "kBBBBBBk.",
                    "kBBBBBkBk",
                    "kBbbbBBBo",
                    ".kkkkkkk."
                };
                return new[] { Juntar(corpo, ".k...k..."), Juntar(corpo, "..k.k....") };
            }
            case "pinguim":
            {
                string[] corpo =
                {
                    "..kkkk..",
                    ".kkkkkk.",
                    ".kkkwkko",
                    ".kkwwwk.",
                    "kkkwwwk.",
                    "kkkwwwwk",
                    "kkkwwwwk",
                    "kkkwwwwk",
                    ".kkwwwk.",
                    "..kkkk.."
                };
                return new[] { Juntar(corpo, "..oo.oo."), Juntar(corpo, ".oo..oo.") };
            }
            case "sapo":
                return new[]
                {
                    new[]
                    {
                        "...kk..kk..",
                        "..kyk.kyk..",
                        ".kZZZkZZZk.",
                        "kZzZZZZzZZk",
                        "kZZZzZZZZzk",
                        ".kZZZZZZZk.",
                        "kk.kk.kk.kk"
                    },
                    new[]
                    {
                        "...........",
                        "...kk..kk..",
                        "..kyk.kyk..",
                        ".kZZZkZZZk.",
                        "kZzZZZZzZZk",
                        "kZZZzZZZZzk",
                        ".kkkkkkkkk."
                    }
                };
            case "tatu":
            {
                string[] corpo =
                {
                    "....kkkkk...",
                    "..kkgGgGgk..",
                    ".kgGgGgGgGkk",
                    "kkgGgGgGgGcc",
                    ".kkkkkkkkkck"
                };
                return new[] { Juntar(corpo, "..kk...kk..."), Juntar(corpo, "...kk.kk....") };
            }
            case "zorrilho":
            {
                string[] corpo =
                {
                    "kk...........",
                    "kwk..........",
                    "kwwk..kkkkk..",
                    ".kwkkkwwwwkk.",
                    ".kkkkkkkkkkwk",
                    "..kkkkkkkkkkp",
                    "..kkkkkkkkk.."
                };
                return new[] { Juntar(corpo, "...kk...kk..."), Juntar(corpo, "....kk.kk....") };
            }
            case "gamba":
            {
                string[] corpo =
                {
                    "....kk.kk....",
                    "...kgkkgk....",
                    "..kkkkkkkkkk.",
                    "pkWWWWWWWWwwk",
                    "p.kWWgWWWwwkw",
                    "...kWWWWWwwwp",
                    "...kkkkkkkkk."
                };
                return new[] { Juntar(corpo, "....kk..kk..."), Juntar(corpo, ".....kkkk....") };
            }
            case "capivara":
            {
                string[] corpo =
                {
                    "..........kk.kk.",
                    "...kkkkkkkBkkBk.",
                    "..kbbbbbbbbbbbbk",
                    ".kbbbbbbbbbbbkbk",
                    "kbbbbbbbbbbbbbbk",
                    "kbbcbbbbbbbbbbBk",
                    "kbccccccbbbbbbk.",
                    ".kkkkkkkkkkkkk.."
                };
                return new[] { Juntar(corpo, "..kk..kk..kk.kk."), Juntar(corpo, "...kk.kk.kk.kk..") };
            }
            case "jacare":
            {
                string[] corpo =
                {
                    "........kkkkkk......",
                    "...kkkkkvVvVvVkkkk..",
                    "kkkVVVVVVVVVVVVVkkkk",
                    "..kkVVVVVVVVVVVyyyyk",
                    "....kkkkkkkkkkkkkkk."
                };
                return new[] { Juntar(corpo, ".....kk...kk........"), Juntar(corpo, "......kk.kk.........") };
            }
            case "gafanhoto":
            {
                string[] corpo =
                {
                    "..kk...",
                    ".kvvkk.",
                    "kvvvvvk",
                    ".kvvvk."
                };
                return new[] { Juntar(corpo, "k.k.k.k"), Juntar(corpo, ".k.k.k.") };
            }
            case "borrachudo":
                return new[]
                {
                    new[] { ".k...k..", "...k...k", "k.k..k..", "..k.k..k", ".k...k.." },
                    new[] { "..k...k.", "k...k...", ".k..k.k.", "k..k...k", "..k..k.." }
                };
            case "enxame":
                return new[]
                {
                    new[]
                    {
                        "...g.k..g.k...",
                        ".k..gk.k..g.k.",
                        "..gk.kgk.kg...",
                        "k.k.gkk.gk.k.k",
                        ".gk.kgk.kgk.g.",
                        "k..gk.kgk.k..k",
                        ".k.k.gk.kgk.k.",
                        "..gk.k.gk.k...",
                        ".k..g.k..g.k..",
                        "....k...k....."
                    },
                    new[]
                    {
                        "..k.g..k.g....",
                        "k..gk..gk..k..",
                        ".gk.kgk.kgk.g.",
                        "..k.gk.kgk.k.k",
                        "k.gk.kgk.kg.k.",
                        ".k.gk.kgkk.g..",
                        "k.k.kgk.gk.k.k",
                        ".gk.k.gk.k.g..",
                        "..k.g..k.g..k.",
                        ".....k...k...."
                    }
                };
            case "nuvem":
                return new[]
                {
                    new[]
                    {
                        "......g.v.g.V.......",
                        "...v.gvBgv.Vg.v.g...",
                        ".g.VgvgVgBvgvgVg.v..",
                        "v.gvBgvgvgVgBvgvgv.g",
                        ".VgvgvVgBgvgvgVgBgv.",
                        "g.vgBgvgvgVgvgvBgvg.",
                        ".vgVgvBgvgvgBgvgVg.v",
                        "..gvgvgVgvBgvgvgv.g.",
                        "...v.gBgv.gVgv.g....",
                        "......v...g...v....."
                    },
                    new[]
                    {
                        ".....v.g.V.g.v......",
                        "..g.vgVgv.Bgv.gv....",
                        "..VgvgBgvgVgvgBg.g..",
                        "g.vgVgvgBgvgvgVgvg.v",
                        ".gvBgvgVgvgBgvgvgVg.",
                        "v.gvgVgBgvgvgVgvgBg.",
                        ".gVgvgvgVgBgvgvgvg.g",
                        "..vgBgvgvgvgVgvBg.v.",
                        "....g.vgVg.vgBg.v...",
                        ".......g...v...g...."
                    }
                };
            case "cusco":
            {
                string[] corpo =
                {
                    ".......kk.",
                    "k.....kbbk",
                    "bk.kkkkbkb",
                    ".kbcccccbk",
                    "..kcccccck",
                    "..kkkkkkk."
                };
                return new[] { Juntar(corpo, "..k.k..k.k"), Juntar(corpo, "...k.k.k.k") };
            }
            case "cavalo":
            {
                string[] corpo =
                {
                    ".............kk.....",
                    "............kbbk....",
                    "...........kBbbbk...",
                    "..........kBbbkbbk..",
                    "..........kBbbbbbbk.",
                    ".kkkkkkkkkkBbbkkkk..",
                    "kBkbbbbbbbbbbbk.....",
                    "kBkbbbbbbbbbbbk.....",
                    ".kBbbbbbbbbbbbk.....",
                    "..kbbbbbbbbbbk......",
                    "..kbkk.....kbk......"
                };
                return new[]
                {
                    Juntar(corpo, "..kbk......kbk......", "..kbk......kbk......", "..kkk......kkk......"),
                    Juntar(corpo, "...kbk....kbk.......", "....kbk..kbk........", "....kkk..kkk........")
                };
            }
            default:
                return null;
        }
    }

    static string[] Desenho(string nome)
    {
        switch (nome)
        {
            case "peixeira": return new[] { "...kkkkkk.", "kbbkmmmmmk", "...kkkkk.." };
            case "boleadeira": return new[] { "kk......", "kBk.....", ".kcc..kk", "...cccBk", ".kcc..kk", "kBk.....", "kk......" };
            case "rojao": return new[] { ".kkkkkkkk.", "bkrrrrrryk", ".kkkkkkkk." };
            case "espeto": return new[] { "....kk.kk...", "bbmmrrmrrmmk", "....kk.kk..." };
            case "laco": return new[] { ".kkkkkk..", "kcbbbbck.", "kc....ck.", "kc....cbb", "kc....ck.", "kcbbbbck.", ".kkkkkk.." };
            case "carabina": return new[] { "...........kk.....", "kbbbbkkkkkkkkkkkkk", "kbbbkMMMMMMMMMMMMk", ".kk.kkkk.........." };
            case "arpao": return new[] { "...........k..", "bbbbbbbbbbbmmk", "...........k.." };
            case "lanca": return new[] { ".............r..k.", "bbbbbbbbbbbbbbbmmk", "................k." };
            case "cano": return new[] { ".kkkkkkkkkk.", "kMMMMMMMMMmk", "kMnnnnnnnnmk", "kMMMMMMMMMmk", ".kkkkkkkkkk." };
            case "cajado": return new[] { "..........k..", ".........kuk.", "BBBBBBBBBkuUk", ".........kuk.", "..........k.." };
            case "relho": return new[] { "..........kkk.", "BBBkbbbbbbb..k", "BBBk.........." };
            case "martelo": return new[] { "......kkk", "......kMk", "bbbbbbkMk", "......kMk", "......kkk" };
            case "garrafa": return new[] { ".kbk.", ".kVk.", "kVVVk", "kVwVk", "kVwVk", "kVVVk", "kVVVk", ".kkk." };
            case "pinha": return new[] { "..kk..", ".kbbk.", "kbBbBk", "kBbBbk", "kbBbBk", ".kBbk.", "..kk.." };
            case "arame": return new[] { "..kkkk..", ".kMmmMk.", "kMmkkmMk", "kmk..kmk", "kmk..kmk", "kMmkkmMk", ".kMmmMk.", "..kkkk.." };
            case "roseta": return new[] { "..m..", ".mMm.", "mMnMm", ".mMm.", "..m.." };
            case "faca": return new[] { ".kkkk.", "bmmmmk", ".kkkk." };
            case "facao": return new[] { "....kkkkkkkk..", "bbkkmmmmmmmmkk", "bbkmmmmmmmmmmk", "bbkkmmmmmmmmk.", "....kkkkkkk..." };
            case "bolas": return new[] { "..kk...", ".kBBk..", "..kckkk", "...ccBk", "..kckkk", ".kBBk..", "..kk..." };
            case "foguete": return new[] { "..kkkk.", "FfkrrRk", "..kkkk." };
            case "espetinho": return new[] { "mmrrmrrk" };
            case "balacanhao": return new[] { ".kk.", "knMk", "knnk", ".kk." };
            case "gelo": return new[] { "...kii.", "kiiiiik", "...kii." };
            case "faisca": return new[] { "..u..", ".uiu.", "uiwiu", ".uiu.", "..u.." };
            case "estalo": return new[] { "..y..", ".ywy.", "ywwwy", ".ywy.", "..y.." };
            case "pinhao": return new[] { ".b.", "bBb", "bBb", "bBb", ".b." };
            case "gota": return new[] { ".a..", ".aa.", "aiaa", "aaaa", ".aa." };
            case "vento": return new[] { ".WWW..", "W...W.", ".WW..W", "....W." };
            case "fogo": return new[] { "..F...", ".FfF..", "FfyfF.", "FfyyfF", ".FffF.", "..FF.." };
            case "raio": return new[] { "...yy.", "..yy..", ".yyyy.", "...yy.", "..yy..", ".yy..." };
            case "fardo": return new[] { "kkkkkkk", "kzZzZzk", "kzzzzzk", "kzZzZzk", "kkkkkkk" };
            case "fantasma": return new[] { "..www..", ".wwwww.", "wwkwkww", "wwwwwww", "wwwwwww", "wwwwwww", "w.w.w.w" };
            case "lacada": return new[] { ".cccc.", "c....c", "c....c", "c....c", ".cccc." };
            case "ave": return new[] { "k.......k", "Bk.....kB", ".BBkkkBB.", "...BwoY..", "....kk..." };
            case "touro":
                return new[]
                {
                    "..........e..e..",
                    "...........kk...",
                    "..kkkkkkkkkBBk..",
                    ".kBBBBBBBBBBBkBk",
                    "kBBBBBBBBBBBBBBk",
                    "kBBBBBBBBBBBBBpk",
                    ".kBBBBBBBBBBBk..",
                    "..kBk.....kBk...",
                    "..kBk.....kBk...",
                    "..kkk.....kkk..."
                };
            case "boitata":
                return new[]
                {
                    "..................ff",
                    "...FF....FF....FFyk.",
                    ".FFffF..FffF..FffffF",
                    "FfyyfFFFfyyfFFfyyfF.",
                    ".FffF..FffF..FffF...",
                    "..FF....FF....FF...."
                };
            case "rede":
                return new[]
                {
                    "..cccccccc..",
                    ".c.c.c.c.c.c",
                    "cc.c.c.c.c.c",
                    "c.c.c.c.c.cc",
                    "cc.c.c.c.c.c",
                    "c.c.c.c.c.cc",
                    "cc.c.c.c.c.c",
                    "c.c.c.c.c.cc",
                    "cc.c.c.c.c.c",
                    "c.c.c.c.c.cc",
                    ".c.c.c.c.c.c",
                    "..cccccccc.."
                };
            case "redemoinho":
                return new[]
                {
                    "...WWWW...",
                    ".WW....W..",
                    "W...WW..W.",
                    "W..W..W.W.",
                    "W..W.W..W.",
                    "W...W..W..",
                    ".W....W...",
                    "..WWWW...."
                };
            case "rosetas": return new[] { "..k..k...", ".kBk.Bk.k", "kBcBkcBkB", ".kBkBkBk.", "..k..k..." };
            case "aramechao": return new[] { "k.k.k.k.k.", "MkMkMkMkMk", "kMkMkMkMkM", "MkMkMkMkMk", "k.k.k.k.k." };
            case "arapuca": return new[] { "....k....", "...kbk...", "..kbkbk..", ".kbkbkbk.", "kbkbkbkbk", "bbbbbbbbb" };
            case "charque": return new[] { ".kkkkk.", "krRrRrk", "kRrwrRk", "krRrRrk", ".kkkkk." };
            case "coroa": return new[] { "y.y.y.y", "yYyYyYy", "yYYYYYy", "yyyyyyy" };
            case "chapeu": return Camada(0, "....wwwwwwww....", "...wwwwwwwwww...");
            case "pena": return CamadaLarga("................r...", "...............rw...", "..............rw....", ".............rw.....");
            case "bandeira": return new[] { "kwwww", "kwwww", "kwww.", "kww..", "k....", "k....", "k...." };
            case "oculos": return Camada(7, "....kyayyyayk...");
            case "cinturao": return Camada(10, "....By..........", "......yBy.......", ".........ByB....");
            case "canoa": return new[] { "k................k", "kbkkkkkkkkkkkkkkbk", ".kbbbbbbbbbbbbbbk.", "..kBBBBBBBBBBBBk..", "...kkkkkkkkkkkk..." };
            case "pandorga":
                return new[]
                {
                    "....k....",
                    "...krk...",
                    "..krrrk..",
                    ".kyyryyk.",
                    "kvvvkvvvk",
                    ".kyyryyk.",
                    "..krrrk..",
                    "...krk...",
                    "....k....",
                    "....k....",
                    "...r.....",
                    "....y...."
                };
            case "aviao":
                return new[]
                {
                    "......kkk.......",
                    "......kwk.......",
                    "......kwk.......",
                    "......kwk.......",
                    "......kwk.......",
                    ".kk...kwk.......",
                    ".kwkkkkwkkkkkk..",
                    ".kwyyyyyyyyyyyk.",
                    ".kwyyyyyyyyyyykk",
                    ".kwkkkkwkkkkkk..",
                    ".kk...kwk.......",
                    "......kwk.......",
                    "......kwk.......",
                    "......kwk.......",
                    "......kwk.......",
                    "......kkk......."
                };
            case "lanchao":
                return new[]
                {
                    "...........ryv..........",
                    "...........k............",
                    "...........kwww.........",
                    "...........kwwww........",
                    "...........kwwwww.......",
                    "...........kwwwwww......",
                    "...........kwwwwwww.....",
                    "...........k............",
                    "kkkkkkkkkkkkkkkkkkkkkkkk",
                    ".kbbbbbbbbbbbbbbbbbbbbk.",
                    "..kbBbbbBbbbBbbbBbbbbk..",
                    "...kbbbbbbbbbbbbbbbbk...",
                    "....kkkkkkkkkkkkkkkk...."
                };
            case "canhao":
                return new[]
                {
                    "...kkkkkkkk...",
                    "..kbbbbbbbbk..",
                    "..kbbbbbbbbk..",
                    ".kkkk....kkkk.",
                    "kBbBk....kBbBk",
                    "kbkbk....kbkbk",
                    "kBbBk....kBbBk",
                    ".kkk......kkk."
                };
            case "benzedeira":
                return new[]
                {
                    ".....kkkkkk.....",
                    "....kuuuuuuk....",
                    "...kuuuuuuuuk...",
                    "...kWssssssWk...",
                    "...kWsksskssk...",
                    "...kWssssssWk...",
                    "....kssppssk....",
                    ".....kssssk.....",
                    "...kkUUUUUUkk...",
                    "..kUUUwwwwUUUk..",
                    "..kUsUwwwwUsUk..",
                    "..kUUUwwwwUUUk..",
                    "..kUUUwwwwUUUk..",
                    "..kUUUUUUUUUUk..",
                    "...kUUUUUUUUk...",
                    "....kkk..kkk...."
                };
            case "iara":
                return new[]
                {
                    ".....kkkkk......",
                    "....khhhhhk.....",
                    "...khssssshk....",
                    "...khskssksk....",
                    "...khsssssshk...",
                    "...khhsppshhk...",
                    "..khhhssssshhk..",
                    "..khhaaaaaahhk..",
                    "..khsaaaaaashk..",
                    "...kkssssssk....",
                    "....kttttttk....",
                    ".....kttTttk....",
                    "......kttTtk....",
                    ".......kttttk...",
                    "......kTtkkTtk..",
                    ".....kTk....kTk."
                };
            case "charqueada":
                return new[]
                {
                    "................",
                    ".kk.........kk..",
                    ".kBkkkkkkkkkkBk.",
                    ".kB.k..k..k..Bk.",
                    ".kBkrkkrkkrk.Bk.",
                    ".kBkrRkrRkrRkBk.",
                    ".kBkrRkrRkrRkBk.",
                    ".kBkRrkRrkRrkBk.",
                    ".kB.kk.kk.kk.Bk.",
                    ".kB..........Bk.",
                    ".kB..........Bk.",
                    ".kB...kkk....Bk.",
                    ".kB..kwwwk...Bk.",
                    ".kB.kwwwwwk..Bk.",
                    "kkBkwwwwwwwkkBkk",
                    "kkkkkkkkkkkkkkkk"
                };
            case "roda":
                return new[]
                {
                    "................",
                    "..kkkkkkkkkkkk..",
                    "..k....k.....k..",
                    "..k...kkk....k..",
                    "..k..kMMMk...k..",
                    "..k.kMnnnMk..k..",
                    "..k.kMMMMMkk.k..",
                    "..k..kkkkk...k..",
                    "..k...F.F....k..",
                    "..k..FfFfF...k..",
                    "..k.FfyfyfF..k..",
                    "..kkBBBBBBBBkkY.",
                    "....BkBkBkB..kVk",
                    "............kvVk",
                    ".............kk.",
                    "................"
                };
            case "espantalho":
                return new[]
                {
                    "....kkkk....",
                    "...kBBBBk...",
                    "..kBBBBBBk..",
                    "....kzzk....",
                    "....kzkz....",
                    "....kzzk....",
                    "kkkkkrrkkkkk",
                    "kzrrrrrrrrzk",
                    "kkkkkrrkkkkk",
                    ".....rr.....",
                    ".....BB.....",
                    ".....BB.....",
                    ".....BB.....",
                    ".....BB.....",
                    "....kBBk....",
                    "...kkkkkk..."
                };
            default:
                return null;
        }
    }
}
