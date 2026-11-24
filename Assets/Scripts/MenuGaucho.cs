using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Menu principal (cena Menu): iniciar o jogo, escolher a dificuldade, ver o ranking e as instrucoes.
/// Desenhado com OnGUI, com as mesmas cores do HUD do JogoGaucho.
/// </summary>
public sealed class MenuGaucho : MonoBehaviour
{
    public const string CenaDoJogo = "Teste_PixelArt";

    enum Painel
    {
        Principal,
        Ranking,
        ComoJogar,
        Opcoes
    }

    Painel painelAberto = Painel.Principal;
    // Lido do PlayerPrefs so ao abrir o painel, nao a cada OnGUI.
    System.Collections.Generic.List<RegistroRanking> ranking;
    bool confirmarLimpeza;

    GUIStyle estiloPainel;
    GUIStyle estiloTitulo;
    GUIStyle estiloSubtitulo;
    GUIStyle estiloTexto;
    GUIStyle estiloBotao;
    Texture2D texturaPainel;
    Texture2D texturaBotao;
    Texture2D texturaBotaoHover;

    void Awake()
    {
        // Volta ao normal caso o jogo tenha sido deixado no x2.
        Time.timeScale = 1f;
    }

    void Update()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null)
            return;

        if (teclado.escapeKey.wasPressedThisFrame)
            painelAberto = Painel.Principal;
        else if (painelAberto == Painel.Principal && teclado.enterKey.wasPressedThisFrame)
            Jogar();
    }

    void Jogar()
    {
        SceneManager.LoadScene(CenaDoJogo);
    }

    void Sair()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnGUI()
    {
        PrepararEstilos();
        float e = EscalaUi();

        if (painelAberto == Painel.Ranking)
            DesenharRanking(e);
        else if (painelAberto == Painel.ComoJogar)
            DesenharComoJogar(e);
        else if (painelAberto == Painel.Opcoes)
            DesenharOpcoes(e);
        else
            DesenharPrincipal(e);
    }

    void DesenharOpcoes(float e)
    {
        Rect painel = PainelGrande(e, "OPÇÕES");
        OpcoesGaucho.Desenhar(new Rect(painel.x + 120f * e, painel.y + 150f * e, painel.width - 240f * e, 0f), estiloTexto, estiloBotao, e);
        BotaoVoltar(painel, e);
    }

    void DesenharPrincipal(float e)
    {
        float largura = 520f * e;
        float altura = 780f * e;
        Rect painel = new Rect((Screen.width - largura) * 0.5f, (Screen.height - altura) * 0.5f - 40f * e, largura, altura);
        GUI.Box(painel, GUIContent.none, estiloPainel);

        float y = painel.y + 30f * e;
        GUI.Label(new Rect(painel.x, y, largura, 70f * e), "GAME GAUCHO", estiloTitulo);
        y += 70f * e;
        GUI.Label(new Rect(painel.x, y, largura, 36f * e), "Defesa do Galpao", estiloSubtitulo);
        y += 60f * e;

        float x = painel.x + 50f * e;
        float w = largura - 100f * e;
        if (Botao(x, ref y, w, e, "JOGAR  [ENTER]"))
            Jogar();

        Dificuldade atual = Dificuldade.Atual;
        if (Botao(x, ref y, w, e, "NIVEL: " + atual.nome.ToUpperInvariant()))
            Dificuldade.Alternar();
        GUI.Label(new Rect(x, y - 14f * e, w, 30f * e), DescreverDificuldade(Dificuldade.Atual), estiloSubtitulo);
        y += 22f * e;

        if (Botao(x, ref y, w, e, "MODO: " + ModoDeJogo.Atual.nome.ToUpperInvariant()))
            ModoDeJogo.Alternar();
        GUI.Label(new Rect(x, y - 14f * e, w, 30f * e), ModoDeJogo.Atual.descricao, estiloSubtitulo);
        y += 22f * e;

        if (Botao(x, ref y, w, e, "RANKING"))
        {
            painelAberto = Painel.Ranking;
            ranking = null;
            confirmarLimpeza = false;
        }
        if (Botao(x, ref y, w, e, "COMO JOGAR"))
            painelAberto = Painel.ComoJogar;
        if (Botao(x, ref y, w, e, "OPÇÕES"))
            painelAberto = Painel.Opcoes;
        if (Botao(x, ref y, w, e, "SAIR"))
            Sair();
    }

    void DesenharRanking(float e)
    {
        Rect painel = PainelGrande(e, "RANKING");
        if (ranking == null)
            ranking = Ranking.Carregar();

        if (ranking.Count == 0)
        {
            GUI.Label(new Rect(painel.x + 50f * e, painel.y + 120f * e, painel.width - 100f * e, 60f * e),
                "Nenhuma partida registrada ainda.", estiloSubtitulo);
        }
        else
        {
            float x = painel.x + 60f * e;
            float y = painel.y + 100f * e;
            float linha = 38f * e;
            LinhaRanking(x, y, e, "#", "PONTOS", "NIVEL / MODO", "RESULTADO", "DATA");
            for (int i = 0; i < ranking.Count; i++)
            {
                RegistroRanking r = ranking[i];
                string resultado = r.vitoria ? "Vitoria" : "Derrota (onda " + (r.ondas + 1) + ")";
                string nivel = string.IsNullOrEmpty(r.modo) || r.modo == ModoDeJogo.Padrao.nome ? r.nivel : r.nivel + " / " + r.modo;
                LinhaRanking(x, y + linha * (i + 1), e, (i + 1) + "o", r.pontos.ToString(), nivel, resultado, r.data);
            }
        }

        // Duas confirmacoes para nao apagar o ranking sem querer.
        float largura = 220f * e;
        Rect botaoLimpar = new Rect(painel.xMax - largura - 30f * e, painel.yMax - 90f * e, largura, 62f * e);
        if (ranking.Count > 0 && GUI.Button(botaoLimpar, confirmarLimpeza ? "CONFIRMAR?" : "LIMPAR", estiloBotao))
        {
            if (confirmarLimpeza)
            {
                Ranking.Limpar();
                ranking = null;
            }
            confirmarLimpeza = !confirmarLimpeza;
        }
        BotaoVoltar(painel, e);
    }

    void LinhaRanking(float x, float y, float e, string posicao, string pontos, string nivel, string resultado, string data)
    {
        float altura = 34f * e;
        GUI.Label(new Rect(x, y, 50f * e, altura), posicao, estiloTexto);
        GUI.Label(new Rect(x + 55f * e, y, 100f * e, altura), pontos, estiloTexto);
        GUI.Label(new Rect(x + 160f * e, y, 245f * e, altura), nivel, estiloTexto);
        GUI.Label(new Rect(x + 410f * e, y, 200f * e, altura), resultado, estiloTexto);
        GUI.Label(new Rect(x + 615f * e, y, 110f * e, altura), data, estiloTexto);
    }

    void DesenharComoJogar(float e)
    {
        Rect painel = PainelGrande(e, "COMO JOGAR");
        GUI.Label(new Rect(painel.x + 50f * e, painel.y + 100f * e, painel.width - 100f * e, painel.height - 200f * e),
            "A bicharada quer chegar ao galpão. Contrate gaúchos e segure as 20 ondas antes do tempo acabar.\n\n" +
            "TAB: troca a classe da loja (Peões, Farrapos, Lendas, Estância)\n" +
            "1 a 7: contrata o gaúcho da classe   |   Clique: posiciona   |   ESC: cancela\n" +
            "Clique no gaúcho: Z, X e C melhoram os 3 caminhos   |   V vende   |   P troca o alvo\n" +
            "Espaço: próxima onda (durante a onda, chama a seguinte antes da hora)   |   A: início automático\n" +
            "ESC: pausa   |   x1, x2 e x3: velocidade   |   Depois da vitória, L abre o jogo livre\n" +
            "Modos: Só Peões, Metade das Pilas e Desafio (sem vender, com uma vida só) valem mais pontos.\n\n" +
            "Cada gaúcho tem 3 caminhos de 5 níveis, e só um caminho passa do nível 2.\n" +
            "Furtivos só aparecem para quem vê furtivo. Blindados não sentem corte nem bala.\n" +
            "Bichos grandes soltam outros quando caem. Clique no charque para pegar as pilas.\n\n" +
            "O nível muda a vida e a velocidade dos bichos, as pilas, as vidas e o tempo da missão.\n" +
            "Pontos: 10 por bicho, 100 por onda e bônus de vitória. Normal x1,5, Difícil x2.",
            estiloTexto);
        GUI.Label(new Rect(painel.x + 50f * e, painel.yMax - 150f * e, painel.width - 100f * e, 52f * e),
            "Musica: \"Las Dos Fronteras\", de Patrick de Arteaga (patrickdearteaga.com) - CC-BY 4.0", estiloSubtitulo);
        BotaoVoltar(painel, e);
    }

    Rect PainelGrande(float e, string titulo)
    {
        float largura = 820f * e;
        float altura = 640f * e;
        Rect painel = new Rect((Screen.width - largura) * 0.5f, (Screen.height - altura) * 0.5f - 40f * e, largura, altura);
        GUI.Box(painel, GUIContent.none, estiloPainel);
        GUI.Label(new Rect(painel.x, painel.y + 24f * e, largura, 70f * e), titulo, estiloTitulo);
        return painel;
    }

    void BotaoVoltar(Rect painel, float e)
    {
        float largura = 260f * e;
        if (GUI.Button(new Rect(painel.x + (painel.width - largura) * 0.5f, painel.yMax - 90f * e, largura, 62f * e), "VOLTAR  [ESC]", estiloBotao))
            painelAberto = Painel.Principal;
    }

    bool Botao(float x, ref float y, float largura, float e, string texto)
    {
        bool clicou = GUI.Button(new Rect(x, y, largura, 62f * e), texto, estiloBotao);
        y += 78f * e;
        return clicou;
    }

    static string DescreverDificuldade(Dificuldade d)
    {
        int segundos = Mathf.CeilToInt(d.tempoSegundos);
        return d.vidas + " vidas  |  $" + d.pilasIniciais + " pilas  |  " + (segundos / 60) + ":" + (segundos % 60).ToString("00") + " de missao";
    }

    void PrepararEstilos()
    {
        float e = EscalaUi();
        if (estiloPainel == null)
        {
            texturaPainel = TexturaSolida(new Color(0.16f, 0.09f, 0.06f, 0.94f));
            texturaBotao = TexturaSolida(new Color(0.55f, 0.24f, 0.08f, 0.97f));
            texturaBotaoHover = TexturaSolida(new Color(0.78f, 0.39f, 0.10f, 1f));

            estiloPainel = new GUIStyle(GUI.skin.box) { normal = { background = texturaPainel } };
            estiloTitulo = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(1f, 0.82f, 0.38f) }
            };
            estiloSubtitulo = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = new Color(1f, 0.94f, 0.82f) }
            };
            estiloTexto = new GUIStyle(estiloSubtitulo) { alignment = TextAnchor.UpperLeft };
            estiloBotao = new GUIStyle(GUI.skin.button)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { background = texturaBotao, textColor = Color.white },
                hover = { background = texturaBotaoHover, textColor = Color.white },
                active = { background = texturaBotaoHover, textColor = Color.white }
            };
        }

        // A escala acompanha o tamanho da janela, como no HUD do jogo.
        estiloTitulo.fontSize = Mathf.RoundToInt(48f * e);
        estiloSubtitulo.fontSize = Mathf.RoundToInt(20f * e);
        estiloTexto.fontSize = Mathf.RoundToInt(20f * e);
        estiloBotao.fontSize = Mathf.RoundToInt(22f * e);
    }

    static Texture2D TexturaSolida(Color cor)
    {
        Texture2D textura = new Texture2D(1, 1);
        textura.hideFlags = HideFlags.HideAndDontSave;
        textura.SetPixel(0, 0, cor);
        textura.Apply();
        return textura;
    }

    static float EscalaUi()
    {
        return Mathf.Clamp(Screen.height / 1080f, 0.62f, 1.35f);
    }

    void OnDestroy()
    {
        if (texturaPainel != null)
            Destroy(texturaPainel);
        if (texturaBotao != null)
            Destroy(texturaBotao);
        if (texturaBotaoHover != null)
            Destroy(texturaBotaoHover);
    }
}
