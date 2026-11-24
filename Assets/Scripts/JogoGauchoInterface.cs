using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed partial class JogoGaucho
{
    sealed class TextoFlutuante
    {
        public Vector3 posicao;
        public string texto;
        public Color cor;
        public float criadoEm;
    }

    const int PontosDoAnel = 48;
    const int MaximoDeTextos = 40;

    static readonly ClasseTorre[] Classes = { ClasseTorre.Peoes, ClasseTorre.Farrapos, ClasseTorre.Lendas, ClasseTorre.Estancia };
    static readonly string[] TeclasDosCaminhos = { "Z", "X", "C" };

    readonly List<TextoFlutuante> textosFlutuantes = new List<TextoFlutuante>();
    readonly Dictionary<ClasseTorre, List<DefinicaoTorre>> loja = new Dictionary<ClasseTorre, List<DefinicaoTorre>>();
    ClasseTorre classeDaLoja = ClasseTorre.Peoes;
    LineRenderer anelDeAlcance;
    LineRenderer anelDoAlvo;

    GUIStyle estiloPainel;
    GUIStyle estiloCaixa;
    GUIStyle estiloTitulo;
    GUIStyle estiloTexto;
    GUIStyle estiloPequeno;
    GUIStyle estiloMini;
    GUIStyle estiloDestaque;
    GUIStyle estiloBotao;
    GUIStyle estiloBotaoPequeno;
    GUIStyle estiloAbaAtiva;
    GUIStyle estiloBotaoPerigo;
    GUIStyle estiloCentro;
    GUIStyle estiloPequenoCentro;
    Texture2D texturaPainel;
    Texture2D texturaCaixa;
    Texture2D texturaBotao;
    Texture2D texturaBotaoHover;
    Texture2D texturaPerigo;
    Texture2D texturaAba;

    List<DefinicaoTorre> LojaDaClasse(ClasseTorre classe)
    {
        List<DefinicaoTorre> lista;
        if (!loja.TryGetValue(classe, out lista))
        {
            lista = TiposTorre.DaClasse(classe);
            loja[classe] = lista;
        }
        return lista;
    }

    void AtualizarAtalhos()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null)
            return;

        if (teclado.escapeKey.wasPressedThisFrame)
        {
            if (pausado)
                AlternarPausa();
            else if (escolhendoAlvo)
                escolhendoAlvo = false;
            else if (construindo)
                CancelarConstrucao();
            else if (selecionada != null)
                selecionada = null;
            else if (tutorialAberto)
                tutorialAberto = false;
            else if (!jogoEncerrado)
                AlternarPausa();
        }

        if (jogoEncerrado)
        {
            if (teclado.rKey.wasPressedThisFrame)
                Reiniciar();
            else if (teclado.mKey.wasPressedThisFrame)
                VoltarAoMenu();
            else if (teclado.lKey.wasPressedThisFrame)
                ContinuarJogoLivre();
            return;
        }
        if (pausado)
            return;

        if (teclado.spaceKey.wasPressedThisFrame)
            IniciarProximaOnda();
        if (teclado.aKey.wasPressedThisFrame)
            AlternarInicioAutomatico();
        if (teclado.tabKey.wasPressedThisFrame)
            TrocarClasse();

        Key[] numeros = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7 };
        List<DefinicaoTorre> atual = LojaDaClasse(classeDaLoja);
        for (int i = 0; i < numeros.Length && i < atual.Count; i++)
        {
            if (teclado[numeros[i]].wasPressedThisFrame)
                SelecionarConstrucao(atual[i]);
        }

        if (selecionada != null)
        {
            if (teclado.zKey.wasPressedThisFrame)
                ComprarMelhoria(selecionada, 0);
            if (teclado.xKey.wasPressedThisFrame)
                ComprarMelhoria(selecionada, 1);
            if (teclado.cKey.wasPressedThisFrame)
                ComprarMelhoria(selecionada, 2);
            if (teclado.vKey.wasPressedThisFrame)
                Vender(selecionada);
            else if (teclado.pKey.wasPressedThisFrame)
                CiclarPrioridade(selecionada);
        }
    }

    ClasseTorre[] ClassesDisponiveis()
    {
        return modo.soPeoes ? new[] { ClasseTorre.Peoes } : Classes;
    }

    void TrocarClasse()
    {
        ClasseTorre[] disponiveis = ClassesDisponiveis();
        int indice = System.Array.IndexOf(disponiveis, classeDaLoja);
        classeDaLoja = disponiveis[(indice + 1) % disponiveis.Length];
    }

    void CiclarPrioridade(TorreGaucha torre)
    {
        if (torre == null)
            return;
        int maximo = torre.Atributos.miraNoMouse ? 5 : 4;
        torre.Prioridade = (Prioridade)(((int)torre.Prioridade + 1) % maximo);
    }

    void CiclarMovimento(TorreGaucha torre)
    {
        if (torre.Movimento == ModoMovimento.Seguir)
            torre.Movimento = ModoMovimento.Patrulhar;
        else if (torre.Movimento == ModoMovimento.Patrulhar)
            torre.Movimento = ModoMovimento.Parado;
        else
            torre.Movimento = ModoMovimento.Seguir;
    }

    static string NomeDaPrioridade(Prioridade p)
    {
        switch (p)
        {
            case Prioridade.Ultimo: return "ÚLTIMO";
            case Prioridade.MaisPerto: return "MAIS PERTO";
            case Prioridade.MaisForte: return "MAIS FORTE";
            case Prioridade.Mouse: return "MOUSE";
            default: return "PRIMEIRO";
        }
    }

    static string NomeDoMovimento(ModoMovimento m)
    {
        switch (m)
        {
            case ModoMovimento.Patrulhar: return "PATRULHAR";
            case ModoMovimento.Parado: return "PARADO";
            default: return "SEGUIR";
        }
    }

    void AtualizarSelecao()
    {
        if (jogoEncerrado)
        {
            selecionada = null;
            return;
        }
        if (construindo || Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mouseTela = Mouse.current.position.ReadValue();
        if (PonteiroSobreInterface(mouseTela))
            return;

        Vector3 mundo = PosicaoDoMouse();
        if (escolhendoAlvo && selecionada != null)
        {
            selecionada.AlvoFixo = mundo;
            escolhendoAlvo = false;
            ExibirAviso("Alvo do canhão definido", 1.5f);
            return;
        }

        if (ColetarPacoteEm(mundo))
            return;

        TorreGaucha maisProxima = null;
        float menorDistancia = RaioDeSelecao;
        for (int i = 0; i < torres.Count; i++)
        {
            if (torres[i] == null || torres[i].Ajudante)
                continue;
            float distancia = Vector2.Distance(mundo, torres[i].transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                maisProxima = torres[i];
            }
        }
        selecionada = maisProxima;
        escolhendoAlvo = false;
    }

    void AtualizarConstrucao()
    {
        if (!construindo || previaTorre == null || Mouse.current == null)
            return;

        Vector2 mouseTela = Mouse.current.position.ReadValue();
        Vector3 mundo = PosicaoDoMouse();
        mundo.x = Mathf.Round(mundo.x * 2f) / 2f;
        mundo.y = Mathf.Round(mundo.y * 2f) / 2f;
        previaTorre.transform.position = mundo;

        posicaoPreviaValida = PosicaoDeTorreValida(mundo, definicaoEmConstrucao) && !PonteiroSobreInterface(mouseTela);
        Color corPrevia = posicaoPreviaValida ? new Color(0.55f, 1f, 0.55f, 0.72f) : new Color(1f, 0.42f, 0.42f, 0.72f);
        foreach (SpriteRenderer sr in previaTorre.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.name != "Sombra")
                sr.color = corPrevia;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelarConstrucao();
            return;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame && posicaoPreviaValida)
            ConfirmarConstrucao();
    }

    void AtualizarAnelDeAlcance()
    {
        TorreGaucha torreDoAnel = selecionada;
        Color cor = new Color(1f, 0.82f, 0.3f, 0.85f);
        if (construindo && previaTorre != null)
        {
            torreDoAnel = previaTorre.GetComponent<TorreGaucha>();
            cor = posicaoPreviaValida ? new Color(0.55f, 1f, 0.55f, 0.9f) : new Color(1f, 0.42f, 0.42f, 0.9f);
        }

        anelDeAlcance = DesenharCirculo(anelDeAlcance, torreDoAnel != null && torreDoAnel.Alcance < 30f,
            torreDoAnel != null ? torreDoAnel.transform.position : Vector3.zero, torreDoAnel != null ? torreDoAnel.Alcance : 0f, cor);

        bool mostrarAlvo = selecionada != null && !construindo && selecionada.Atributos.modo == ModoAtaque.PontoFixo;
        anelDoAlvo = DesenharCirculo(anelDoAlvo, mostrarAlvo, mostrarAlvo ? selecionada.AlvoFixo : Vector3.zero,
            mostrarAlvo ? Mathf.Max(0.3f, selecionada.Atributos.raioExplosao) : 0f, new Color(1f, 0.35f, 0.25f, 0.9f));
    }

    LineRenderer DesenharCirculo(LineRenderer linha, bool visivel, Vector3 centro, float raio, Color cor)
    {
        if (!visivel)
        {
            if (linha != null)
                linha.enabled = false;
            return linha;
        }
        if (linha == null)
        {
            GameObject objeto = new GameObject("Anel de alcance");
            objeto.transform.SetParent(transform, false);
            linha = objeto.AddComponent<LineRenderer>();
            linha.material = MaterialDeEfeito();
            linha.useWorldSpace = true;
            linha.loop = false;
            linha.positionCount = PontosDoAnel + 1;
            linha.startWidth = 0.045f;
            linha.endWidth = 0.045f;
            linha.sortingOrder = 24;
        }
        linha.enabled = true;
        linha.startColor = cor;
        linha.endColor = cor;
        for (int i = 0; i <= PontosDoAnel; i++)
        {
            float angulo = i * Mathf.PI * 2f / PontosDoAnel;
            linha.SetPosition(i, new Vector3(centro.x + Mathf.Cos(angulo) * raio, centro.y + Mathf.Sin(angulo) * raio, -0.2f));
        }
        return linha;
    }

    void AdicionarTexto(Vector3 posicao, string texto, Color cor)
    {
        if (textosFlutuantes.Count >= MaximoDeTextos)
            textosFlutuantes.RemoveAt(0);
        textosFlutuantes.Add(new TextoFlutuante { posicao = posicao, texto = texto, cor = cor, criadoEm = Time.unscaledTime });
    }

    void AtualizarTextosFlutuantes()
    {
        for (int i = textosFlutuantes.Count - 1; i >= 0; i--)
        {
            TextoFlutuante item = textosFlutuantes[i];
            item.posicao += Vector3.up * (0.32f * Time.unscaledDeltaTime);
            if (Time.unscaledTime - item.criadoEm > 1.15f)
                textosFlutuantes.RemoveAt(i);
        }
    }

    float EscalaUi()
    {
        return Mathf.Clamp(Screen.height / 1080f, 0.62f, 1.35f);
    }

    Rect RectBarraSuperior(float e)
    {
        return new Rect(0f, 0f, Screen.width, 86f * e);
    }

    Rect RectBotaoOnda(float e)
    {
        return new Rect(Screen.width - 390f * e, Screen.height - 118f * e, 365f * e, 88f * e);
    }

    Rect RectLoja(float e)
    {
        float largura = Mathf.Min(1124f * e, Screen.width - 430f * e);
        float altura = 158f * e;
        return new Rect(18f * e, Screen.height - altura - 10f * e, largura, altura);
    }

    Rect RectPainelMelhoria(float e)
    {
        Rect lojaRect = RectLoja(e);
        float altura = 250f * e;
        return new Rect(lojaRect.x, lojaRect.y - altura - 8f * e, lojaRect.width, altura);
    }

    Rect RectTutorial(float e)
    {
        return new Rect(22f * e, 108f * e, 470f * e, 300f * e);
    }

    Rect RectBotaoAutomatico(float e)
    {
        Rect onda = RectBotaoOnda(e);
        return new Rect(onda.x, onda.y - 46f * e, onda.width, 40f * e);
    }

    bool PonteiroSobreInterface(Vector2 mouseTela)
    {
        float e = EscalaUi();
        Vector2 gui = new Vector2(mouseTela.x, Screen.height - mouseTela.y);
        if (jogoEncerrado || pausado)
            return true;
        if (RectBarraSuperior(e).Contains(gui) || RectLoja(e).Contains(gui) || RectBotaoOnda(e).Contains(gui) || RectBotaoAutomatico(e).Contains(gui))
            return true;
        if (tutorialAberto && RectTutorial(e).Contains(gui))
            return true;
        if (selecionada != null && RectPainelMelhoria(e).Contains(gui))
            return true;
        return false;
    }

    void OnGUI()
    {
        if (!enabled || cameraPrincipal == null)
            return;

        PrepararEstilos();
        DesenharBarrasDeVida();
        DesenharNiveisDeTorre();
        DesenharTextosFlutuantes();
        DesenharHud();
    }

    void DesenharHud()
    {
        float e = EscalaUi();
        GUI.Box(RectBarraSuperior(e), GUIContent.none, estiloPainel);
        GUI.Label(new Rect(22f * e, 12f * e, 340f * e, 62f * e), "GAME GAÚCHO", estiloTitulo);
        GUI.Label(new Rect(390f * e, 17f * e, 220f * e, 52f * e), "VIDAS  " + vidas, estiloDestaque);
        GUI.Label(new Rect(625f * e, 17f * e, 260f * e, 52f * e), "PILAS  $" + pilas, estiloDestaque);
        string textoDaOnda = jogoLivre ? "ONDA  " + ondaAtual + " (LIVRE)" : "ONDA  " + ondaAtual + "/" + TotalOndas;
        GUI.Label(new Rect(900f * e, 17f * e, 260f * e, 52f * e), textoDaOnda, estiloDestaque);
        GUI.Label(new Rect(Screen.width - 460f * e, 17f * e, 185f * e, 52f * e), dificuldade.nome.ToUpperInvariant(), estiloDestaque);

        Color corAntes = estiloDestaque.normal.textColor;
        if (tempo.Ativo && tempo.Restante <= 30f)
            estiloDestaque.normal.textColor = new Color(1f, 0.35f, 0.3f);
        GUI.Label(new Rect(1175f * e, 17f * e, 220f * e, 52f * e), "TEMPO  " + tempo.Formatado, estiloDestaque);
        estiloDestaque.normal.textColor = corAntes;

        if (GUI.Button(new Rect(Screen.width - 262f * e, 16f * e, 120f * e, 50f * e), pausado ? "SEGUIR" : "PAUSA", estiloBotao))
            AlternarPausa();
        if (GUI.Button(new Rect(Screen.width - 130f * e, 16f * e, 104f * e, 50f * e), "x" + velocidadeDoJogo.ToString("0"), estiloBotao))
            AlternarVelocidade();

        bool habilitadoAntes = GUI.enabled;
        GUI.enabled = habilitadoAntes && !pausado;
        DesenharLoja(e);
        if (selecionada != null && !jogoEncerrado)
            DesenharPainelMelhoria(e);
        DesenharBotaoDaOnda(e);
        GUI.enabled = habilitadoAntes;

        if (construindo)
        {
            string estado = posicaoPreviaValida ? "POSIÇÃO LIVRE" : "POSIÇÃO BLOQUEADA";
            int preco = previaTorre != null && definicaoEmConstrucao != null ? PrecoNoPonto(definicaoEmConstrucao, previaTorre.transform.position) : 0;
            GUI.Label(new Rect(Screen.width * 0.5f - 300f * e, 150f * e, 600f * e, 46f * e),
                estado + "  |  $" + preco + "  |  ESC cancela", estiloCentro);
        }
        else if (escolhendoAlvo)
            GUI.Label(new Rect(Screen.width * 0.5f - 300f * e, 150f * e, 600f * e, 46f * e), "CLIQUE NO MAPA PARA ESCOLHER O ALVO DO CANHÃO", estiloCentro);

        if (Time.unscaledTime < avisoAte)
            GUI.Label(new Rect(Screen.width * 0.5f - 420f * e, 102f * e, 840f * e, 54f * e), aviso, estiloCentro);

        if (tutorialAberto && !jogoEncerrado && !pausado)
            DesenharTutorial(e);
        if (pausado)
            DesenharPausa(e);
        if (jogoEncerrado)
            DesenharFimDeJogo(e);

        if (!string.IsNullOrEmpty(GUI.tooltip))
            DesenharDica(e, GUI.tooltip);
    }

    void DesenharBotaoDaOnda(float e)
    {
        if (jogoEncerrado)
            return;
        Rect botaoOnda = RectBotaoOnda(e);
        if (GUI.Button(RectBotaoAutomatico(e), "INÍCIO AUTOMÁTICO: " + (inicioAutomatico ? "SIM" : "NÃO") + "  [A]", estiloBotaoPequeno))
            AlternarInicioAutomatico();

        if (OndaEmAndamento)
        {
            if (TemProximaOnda)
            {
                if (GUI.Button(botaoOnda, "CHAMAR ONDA " + (ondaAtual + 1) + "  [ESPAÇO]\nBichos no campo: " + inimigosAtivos.Count, estiloBotao))
                    IniciarProximaOnda();
            }
            else
            {
                GUI.Box(botaoOnda, GUIContent.none, estiloPainel);
                GUI.Label(new Rect(botaoOnda.x + 15f * e, botaoOnda.y + 13f * e, botaoOnda.width - 30f * e, 62f * e),
                    "ÚLTIMA ONDA EM CURSO\nBichos no campo: " + inimigosAtivos.Count, estiloCentro);
            }
        }
        else if (TemProximaOnda)
        {
            string rotulo = ondaAtual == 0 ? "INICIAR ONDA 1" : "PRÓXIMA ONDA  " + (ondaAtual + 1);
            if (GUI.Button(botaoOnda, rotulo + "   [ESPAÇO]", estiloBotao))
                IniciarProximaOnda();
        }
    }

    void DesenharPausa(float e)
    {
        Color antiga = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.45f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = antiga;

        float largura = 520f * e;
        float altura = 470f * e;
        Rect painel = new Rect((Screen.width - largura) * 0.5f, (Screen.height - altura) * 0.5f, largura, altura);
        GUI.Box(painel, GUIContent.none, estiloPainel);
        float x = painel.x + 30f * e;
        float w = painel.width - 60f * e;
        GUI.Label(new Rect(x, painel.y + 18f * e, w, 60f * e), "PAUSADO", estiloTitulo);
        GUI.Label(new Rect(x, painel.y + 76f * e, w, 30f * e), dificuldade.nome + "  |  Modo " + modo.nome + "  |  Onda " + ondaAtual, estiloPequeno);
        float y = painel.y + 118f * e;
        y += OpcoesGaucho.Desenhar(new Rect(x, y, w, 0f), estiloPequeno, estiloBotaoPequeno, e);
        if (GUI.Button(new Rect(x, y, w, 56f * e), "CONTINUAR  [ESC]", estiloBotao))
            AlternarPausa();
        y += 66f * e;
        float metade = (w - 10f * e) * 0.5f;
        if (GUI.Button(new Rect(x, y, metade, 50f * e), "REINICIAR", estiloBotaoPerigo))
            Reiniciar();
        if (GUI.Button(new Rect(x + metade + 10f * e, y, metade, 50f * e), "MENU", estiloBotao))
            VoltarAoMenu();
    }

    void DesenharLoja(float e)
    {
        Rect painel = RectLoja(e);
        GUI.Box(painel, GUIContent.none, estiloPainel);

        ClasseTorre[] disponiveis = ClassesDisponiveis();
        float abaLargura = (painel.width - 20f * e) / Classes.Length;
        for (int i = 0; i < disponiveis.Length; i++)
        {
            Rect aba = new Rect(painel.x + 10f * e + i * abaLargura, painel.y + 8f * e, abaLargura - 6f * e, 34f * e);
            bool ativa = disponiveis[i] == classeDaLoja;
            if (GUI.Button(aba, TiposTorre.NomeDaClasse(disponiveis[i]) + (ativa ? "  [TAB]" : ""), ativa ? estiloAbaAtiva : estiloBotaoPequeno))
                classeDaLoja = disponiveis[i];
        }

        List<DefinicaoTorre> lista = LojaDaClasse(classeDaLoja);
        float largura = (painel.width - 20f * e) / 7f;
        for (int i = 0; i < lista.Count; i++)
        {
            DefinicaoTorre d = lista[i];
            Rect botao = new Rect(painel.x + 10f * e + i * largura, painel.y + 50f * e, largura - 6f * e, painel.height - 60f * e);
            bool habilitadoAntes = GUI.enabled;
            GUI.enabled = habilitadoAntes && !jogoEncerrado && pilas >= d.preco;
            string texto = "[" + (i + 1) + "]\n" + d.nome + "\n$" + d.preco;
            if (GUI.Button(botao, new GUIContent(texto, d.nome + ": " + d.descricao), estiloBotaoPequeno))
                SelecionarConstrucao(d);
            GUI.enabled = habilitadoAntes;
        }
    }

    void DesenharDica(float e, string texto)
    {
        Rect lojaRect = RectLoja(e);
        Rect caixa = new Rect(lojaRect.x, lojaRect.y - 70f * e, lojaRect.width, 62f * e);
        if (selecionada != null)
            caixa.y = RectPainelMelhoria(e).y - 70f * e;
        GUI.Box(caixa, GUIContent.none, estiloPainel);
        GUI.Label(new Rect(caixa.x + 12f * e, caixa.y + 6f * e, caixa.width - 24f * e, caixa.height - 12f * e), texto, estiloPequeno);
    }

    string DescreverAtributos(TorreGaucha torre)
    {
        AtributosTorre a = torre.Efetivo;
        if (a.modo == ModoAtaque.Nenhum && a.Tem(TipoEspecial.Armadilha))
        {
            Especial rosetas = a.Pegar(TipoEspecial.Armadilha);
            string armadilha = "Espalha rosetas: dano " + rosetas.ataque.dano + ", cada monte fura " + rosetas.ataque.perfuracao
                + " bichos, a cada " + (rosetas.intervalo / torre.FatorDeCadencia).ToString("0.0") + "s";
            if (rosetas.ataque.furaBlindagem)
                armadilha += "   [fura blindagem]";
            return armadilha;
        }
        if (a.modo == ModoAtaque.Nenhum)
        {
            string apoio = "Não ataca.";
            if (a.producao > 0)
                apoio += "  Produz $" + Mathf.RoundToInt(a.producao * a.multiplicadorProducao) + " por onda.";
            if (a.guaiaca)
                apoio += "  Guaiaca: $" + torre.Guardado + " (rende " + Mathf.RoundToInt(a.rendimento * 100f) + "%).";
            if (a.EhApoio())
                apoio += "  Fortalece os gaúchos no alcance.";
            if (a.lentidaoGlobal > 0f)
                apoio += "  Bichos " + Mathf.RoundToInt(a.lentidaoGlobal * 100f) + "% mais lentos.";
            return apoio;
        }

        string texto = "Dano " + a.dano + " (" + NomeDoTipo(a.tipoDano) + ")";
        texto += "   Alcance " + (a.alcance > 30f ? "mapa todo" : a.alcance.ToString("0.0"));
        texto += "   A cada " + a.intervalo.ToString("0.00") + "s";
        if (a.perfuracao > 1)
            texto += "   Perfura " + a.perfuracao;
        if (a.veFurtivo)
            texto += "   [vê furtivo]";
        if (a.furaBlindagem)
            texto += "   [fura blindagem]";
        if (a.ignoraImunidades)
            texto += "   [ignora imunidades]";
        return texto;
    }

    static string NomeDoTipo(TipoDano tipo)
    {
        switch (tipo)
        {
            case TipoDano.Corte: return "corte";
            case TipoDano.Bala: return "bala";
            case TipoDano.Impacto: return "impacto";
            case TipoDano.Explosao: return "explosão";
            case TipoDano.Fogo: return "fogo";
            case TipoDano.Frio: return "frio";
            default: return "mágica";
        }
    }

    void DesenharPainelMelhoria(float e)
    {
        Rect painel = RectPainelMelhoria(e);
        GUI.Box(painel, GUIContent.none, estiloPainel);
        TorreGaucha t = selecionada;
        DefinicaoTorre d = t.Definicao;

        float x = painel.x + 14f * e;
        float y = painel.y + 8f * e;
        float w = painel.width - 28f * e;
        int venda = MelhoriasTorre.ValorDeVenda(t.Gasto);
        GUI.Label(new Rect(x, y, w * 0.55f, 30f * e), d.nome.ToUpperInvariant() + "  (" + TiposTorre.NomeDaClasse(d.classe) + ")", estiloDestaque);
        GUI.Label(new Rect(x + w * 0.55f, y + 4f * e, w * 0.45f, 26f * e), "Bichos derrubados: " + t.Abatidos, estiloPequeno);
        y += 32f * e;
        GUI.Label(new Rect(x, y, w, 26f * e), DescreverAtributos(t), estiloPequeno);
        y += 28f * e;

        float colunaLargura = (w - 20f * e) / 3f;
        for (int c = 0; c < 3; c++)
            DesenharCaminho(new Rect(x + c * (colunaLargura + 10f * e), y, colunaLargura, 128f * e), t, c, e);
        y += 134f * e;

        float botaoLargura = (w - 20f * e) / 3f;
        Rect b1 = new Rect(x, y, botaoLargura, 38f * e);
        Rect b2 = new Rect(x + botaoLargura + 10f * e, y, botaoLargura, 38f * e);
        Rect b3 = new Rect(x + 2f * (botaoLargura + 10f * e), y, botaoLargura, 38f * e);

        bool habilitado = GUI.enabled;
        GUI.enabled = habilitado && !modo.semVender;
        bool vender = GUI.Button(b1, modo.semVender ? "SEM VENDA NO DESAFIO" : "VENDER  $" + venda + "  [V]", estiloBotaoPerigo);
        GUI.enabled = habilitado;
        if (vender)
        {
            Vender(t);
            return;
        }

        if (t.Atributos.modo != ModoAtaque.Nenhum && t.Atributos.modo != ModoAtaque.PontoFixo && t.Atributos.modo != ModoAtaque.Pulso)
        {
            if (GUI.Button(b2, "ALVO: " + NomeDaPrioridade(t.Prioridade) + "  [P]", estiloBotaoPequeno))
                CiclarPrioridade(t);
        }

        if (t.Atributos.movimento == ModoMovimento.Seguir)
        {
            if (GUI.Button(b3, "MODO: " + NomeDoMovimento(t.Movimento), estiloBotaoPequeno))
                CiclarMovimento(t);
        }
        else if (t.Atributos.modo == ModoAtaque.PontoFixo)
        {
            if (GUI.Button(b3, escolhendoAlvo ? "CLIQUE NO MAPA..." : "MUDAR ALVO", estiloBotaoPequeno))
                escolhendoAlvo = !escolhendoAlvo;
        }
        else if (t.Atributos.guaiaca)
        {
            bool habilitadoAntes = GUI.enabled;
            GUI.enabled = habilitadoAntes && t.Guardado > 0;
            if (GUI.Button(b3, "SACAR  $" + t.Guardado, estiloBotaoPequeno))
                SacarGuaiaca(t);
            GUI.enabled = habilitadoAntes;
        }
    }

    void DesenharCaminho(Rect r, TorreGaucha t, int c, float e)
    {
        GUI.Box(r, GUIContent.none, estiloCaixa);
        DefinicaoTorre d = t.Definicao;
        int nivel = t.Niveis[c];
        string pips = "";
        for (int i = 0; i < MelhoriasTorre.NivelMaximo; i++)
            pips += i < nivel ? "■" : "□";

        GUI.Label(new Rect(r.x + 8f * e, r.y + 4f * e, r.width - 16f * e, 24f * e), d.nomesCaminhos[c] + "  " + pips, estiloPequeno);
        if (nivel < MelhoriasTorre.NivelMaximo)
        {
            Melhoria m = d.caminhos[c][nivel];
            GUI.Label(new Rect(r.x + 8f * e, r.y + 28f * e, r.width - 16f * e, 22f * e), m.nome, estiloMini);
            GUI.Label(new Rect(r.x + 8f * e, r.y + 48f * e, r.width - 16f * e, 40f * e), m.descricao, estiloMini);
        }
        else
            GUI.Label(new Rect(r.x + 8f * e, r.y + 28f * e, r.width - 16f * e, 22f * e), "Caminho completo!", estiloMini);

        bool pode = t.PodeMelhorar(c);
        int custo = pode ? CustoDaMelhoria(t, c) : 0;
        string rotulo = pode ? "[" + TeclasDosCaminhos[c] + "]  $" + custo : MelhoriasTorre.Bloqueio(t.Niveis, c);
        bool habilitadoAntes = GUI.enabled;
        GUI.enabled = habilitadoAntes && pode && pilas >= custo;
        if (GUI.Button(new Rect(r.x + 8f * e, r.yMax - 36f * e, r.width - 16f * e, 30f * e), rotulo, estiloBotaoPequeno))
            ComprarMelhoria(t, c);
        GUI.enabled = habilitadoAntes;
    }

    void DesenharNiveisDeTorre()
    {
        for (int i = 0; i < torres.Count; i++)
        {
            TorreGaucha torre = torres[i];
            if (torre == null || torre.Ajudante)
                continue;
            int[] n = torre.Niveis;
            if (n[0] + n[1] + n[2] == 0)
                continue;
            Vector3 tela = cameraPrincipal.WorldToScreenPoint(torre.transform.position + Vector3.up * 0.72f);
            if (tela.z <= 0f)
                continue;
            GUI.color = torre.Fortalecida ? new Color(1f, 1f, 0.5f) : new Color(1f, 0.86f, 0.4f);
            GUI.Label(new Rect(tela.x - 40f, Screen.height - tela.y - 10f, 80f, 20f), n[0] + "-" + n[1] + "-" + n[2], estiloMini);
        }
        GUI.color = Color.white;
    }

    void DesenharTutorial(float e)
    {
        Rect painel = RectTutorial(e);
        GUI.Box(painel, GUIContent.none, estiloPainel);
        GUI.Label(new Rect(painel.x + 20f * e, painel.y + 14f * e, painel.width - 40f * e, 40f * e), "COMO JOGAR", estiloDestaque);
        GUI.Label(new Rect(painel.x + 20f * e, painel.y + 56f * e, painel.width - 40f * e, 170f * e),
            "1. Escolha a classe (TAB) e contrate (1 a 7).\n" +
            "2. Clique no gaúcho: 3 caminhos de melhoria (Z, X, C), vender (V) e alvo (P).\n" +
            "3. ESPAÇO inicia a onda e, durante ela, chama a próxima antes da hora. A liga o início automático.\n" +
            "4. ESC pausa. Furtivos só aparecem para quem vê furtivo; blindados não sentem corte nem bala.",
            estiloPequeno);
        if (GUI.Button(new Rect(painel.x + 20f * e, painel.yMax - 64f * e, 170f * e, 48f * e), "ENTENDI", estiloBotao))
            tutorialAberto = false;

        bool habilitadoAntes = GUI.enabled;
        GUI.enabled = ondaAtual == 0;
        if (GUI.Button(new Rect(painel.x + 205f * e, painel.yMax - 64f * e, 240f * e, 48f * e), "NÍVEL: " + dificuldade.nome.ToUpperInvariant(), estiloBotao))
        {
            Dificuldade.Alternar();
            Reiniciar();
        }
        GUI.enabled = habilitadoAntes;
    }

    void DesenharFimDeJogo(float e)
    {
        Color antiga = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.72f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = antiga;

        float largura = 780f * e;
        float altura = 440f * e;
        Rect painel = new Rect(Screen.width * 0.5f - largura * 0.5f, Screen.height * 0.5f - altura * 0.5f, largura, altura);
        GUI.Box(painel, GUIContent.none, estiloPainel);
        string titulo = jogoLivre ? "FIM DO JOGO LIVRE" : vitoria ? "VITÓRIA NO PAMPA!" : derrotaPorTempo ? "O TEMPO ACABOU" : "O GALPÃO CAIU";
        GUI.Label(new Rect(painel.x + 30f * e, painel.y + 24f * e, painel.width - 60f * e, 70f * e), titulo, estiloTitulo);
        string mensagem;
        if (jogoLivre)
            mensagem = "Os bichos venceram na onda " + ondaAtual + ", " + Mathf.Max(0, ondasConcluidas - TotalOndas) + " ondas além da campanha.";
        else if (vitoria)
            mensagem = "Você segurou todas as ondas e salvou a estância, com " + tempo.Formatado + " de sobra.";
        else if (derrotaPorTempo)
            mensagem = "As ondas não terminaram a tempo. Derrube os bichos mais rápido e tente de novo.";
        else
            mensagem = "Os bichos atravessaram a defesa. Reposicione os gaúchos e tente de novo.";
        GUI.Label(new Rect(painel.x + 45f * e, painel.y + 92f * e, painel.width - 90f * e, 62f * e), mensagem, estiloCentro);
        string pontuacao = "PONTUAÇÃO: " + pontuacaoFinal + "  (" + dificuldade.nome + ", " + modo.nome + ")";
        if (posicaoNoRanking > 0)
            pontuacao += "  |  " + posicaoNoRanking + "º lugar no ranking!";
        GUI.color = new Color(1f, 0.86f, 0.35f);
        GUI.Label(new Rect(painel.x + 45f * e, painel.y + 156f * e, painel.width - 90f * e, 40f * e), pontuacao, estiloCentro);
        GUI.color = antiga;
        GUI.Label(new Rect(painel.x + 45f * e, painel.y + 204f * e, painel.width - 90f * e, 92f * e), TextoDasEstatisticas(), estiloPequenoCentro);

        float y = painel.y + 330f * e;
        bool podeLivre = vitoria && !jogoLivre;
        float x = painel.x + 40f * e;
        float w = painel.width - 80f * e;
        float botao = podeLivre ? (w - 20f * e) / 3f : (w - 10f * e) / 2f;
        if (podeLivre)
        {
            if (GUI.Button(new Rect(x, y, botao, 66f * e), "JOGO LIVRE  [L]", estiloBotao))
                ContinuarJogoLivre();
            x += botao + 10f * e;
        }
        if (GUI.Button(new Rect(x, y, botao, 66f * e), "JOGAR DE NOVO  [R]", vitoria ? estiloBotao : estiloBotaoPerigo))
            Reiniciar();
        x += botao + 10f * e;
        if (GUI.Button(new Rect(x, y, botao, 66f * e), "MENU  [M]", estiloBotao))
            VoltarAoMenu();
    }

    string TextoDasEstatisticas()
    {
        Dictionary<string, int> abates = new Dictionary<string, int>(abatesDeVendidos);
        foreach (TorreGaucha t in torres)
        {
            if (t == null || t.Ajudante)
                continue;
            int atual;
            abates.TryGetValue(t.Definicao.nome, out atual);
            abates[t.Definicao.nome] = atual + t.Abatidos;
        }
        List<KeyValuePair<string, int>> lista = new List<KeyValuePair<string, int>>(abates);
        lista.Sort((a, b) => b.Value.CompareTo(a.Value));
        string destaques = "";
        for (int i = 0; i < lista.Count && i < 3; i++)
        {
            if (lista[i].Value <= 0)
                break;
            destaques += (destaques.Length > 0 ? ", " : "") + lista[i].Key + " (" + lista[i].Value + ")";
        }
        string texto = "Bichos derrubados: " + eliminadosNoTotal + "   |   Pilas ganhas: $" + pilasGanhas + "   |   Ondas vencidas: " + ondasConcluidas;
        if (destaques.Length > 0)
            texto += "\nQuem mais derrubou: " + destaques;
        return texto;
    }

    void DesenharBarrasDeVida()
    {
        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo == null || !inimigo.EstaVivo || inimigo.VidaMaxima <= 5 || inimigo.VidaNormalizada >= 0.999f)
                continue;
            float largura = inimigo.Grande ? 72f : 34f;
            Vector3 tela = cameraPrincipal.WorldToScreenPoint(inimigo.transform.position + Vector3.up * (inimigo.AlturaDoSprite + 0.12f));
            if (tela.z <= 0f)
                continue;
            float x = tela.x - largura * 0.5f;
            float y = Screen.height - tela.y;
            GUI.color = new Color(0.12f, 0.08f, 0.07f, 0.9f);
            GUI.DrawTexture(new Rect(x, y, largura, 5f), Texture2D.whiteTexture);
            GUI.color = inimigo.Grande ? new Color(0.95f, 0.35f, 0.25f, 1f) : new Color(0.36f, 0.86f, 0.28f, 1f);
            GUI.DrawTexture(new Rect(x + 1f, y + 1f, (largura - 2f) * inimigo.VidaNormalizada, 3f), Texture2D.whiteTexture);
        }
        GUI.color = Color.white;
    }

    void DesenharTextosFlutuantes()
    {
        for (int i = 0; i < textosFlutuantes.Count; i++)
        {
            TextoFlutuante item = textosFlutuantes[i];
            Vector3 tela = cameraPrincipal.WorldToScreenPoint(item.posicao);
            if (tela.z <= 0f)
                continue;
            float idade = Time.unscaledTime - item.criadoEm;
            Color cor = item.cor;
            cor.a = 1f - Mathf.Clamp01(idade / 1.15f);
            GUI.color = cor;
            GUI.Label(new Rect(tela.x - 90f, Screen.height - tela.y - 16f, 180f, 32f), item.texto, estiloCentro);
        }
        GUI.color = Color.white;
    }

    void PrepararEstilos()
    {
        if (estiloPainel != null)
        {
            AtualizarTamanhoDasFontes();
            return;
        }

        texturaPainel = TexturaSolida(new Color(0.16f, 0.09f, 0.06f, 0.94f));
        texturaCaixa = TexturaSolida(new Color(0.26f, 0.15f, 0.09f, 0.95f));
        texturaBotao = TexturaSolida(new Color(0.55f, 0.24f, 0.08f, 0.97f));
        texturaBotaoHover = TexturaSolida(new Color(0.78f, 0.39f, 0.10f, 1f));
        texturaPerigo = TexturaSolida(new Color(0.55f, 0.10f, 0.07f, 1f));
        texturaAba = TexturaSolida(new Color(0.85f, 0.55f, 0.15f, 1f));

        estiloPainel = new GUIStyle(GUI.skin.box) { normal = { background = texturaPainel } };
        estiloCaixa = new GUIStyle(GUI.skin.box) { normal = { background = texturaCaixa } };
        estiloTitulo = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(1f, 0.82f, 0.38f) }
        };
        estiloTexto = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,
            normal = { textColor = new Color(1f, 0.94f, 0.82f) }
        };
        estiloPequeno = new GUIStyle(estiloTexto);
        estiloMini = new GUIStyle(estiloTexto);
        estiloDestaque = new GUIStyle(estiloTexto) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
        estiloCentro = new GUIStyle(estiloTexto) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        estiloPequenoCentro = new GUIStyle(estiloTexto) { alignment = TextAnchor.UpperCenter };
        estiloBotao = new GUIStyle(GUI.skin.button)
        {
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal = { background = texturaBotao, textColor = Color.white },
            hover = { background = texturaBotaoHover, textColor = Color.white },
            active = { background = texturaBotaoHover, textColor = Color.white }
        };
        estiloBotaoPequeno = new GUIStyle(estiloBotao);
        estiloAbaAtiva = new GUIStyle(estiloBotao);
        estiloAbaAtiva.normal.background = texturaAba;
        estiloAbaAtiva.normal.textColor = new Color(0.2f, 0.1f, 0.05f);
        estiloBotaoPerigo = new GUIStyle(estiloBotao);
        estiloBotaoPerigo.normal.background = texturaPerigo;
        AtualizarTamanhoDasFontes();
    }

    void AtualizarTamanhoDasFontes()
    {
        float e = EscalaUi();
        estiloTitulo.fontSize = Mathf.RoundToInt(32f * e);
        estiloTexto.fontSize = Mathf.RoundToInt(19f * e);
        estiloPequeno.fontSize = Mathf.RoundToInt(16f * e);
        estiloMini.fontSize = Mathf.RoundToInt(14f * e);
        estiloDestaque.fontSize = Mathf.RoundToInt(23f * e);
        estiloCentro.fontSize = Mathf.RoundToInt(20f * e);
        estiloPequenoCentro.fontSize = Mathf.RoundToInt(17f * e);
        estiloBotao.fontSize = Mathf.RoundToInt(18f * e);
        estiloBotaoPequeno.fontSize = Mathf.RoundToInt(15f * e);
        estiloAbaAtiva.fontSize = estiloBotaoPequeno.fontSize;
        estiloBotaoPerigo.fontSize = estiloBotaoPequeno.fontSize;
    }

    Texture2D TexturaSolida(Color cor)
    {
        Texture2D textura = new Texture2D(1, 1);
        textura.name = "UI Game Gaucho";
        textura.hideFlags = HideFlags.HideAndDontSave;
        textura.SetPixel(0, 0, cor);
        textura.Apply();
        return textura;
    }

    void LimparInterface()
    {
        Texture2D[] texturas = { texturaPainel, texturaCaixa, texturaBotao, texturaBotaoHover, texturaPerigo, texturaAba };
        foreach (Texture2D textura in texturas)
        {
            if (textura != null)
                Destroy(textura);
        }
    }
}
