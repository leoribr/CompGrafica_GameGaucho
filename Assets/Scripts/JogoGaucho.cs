using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed partial class JogoGaucho : MonoBehaviour
{
    sealed class ModeloInimigo
    {
        public GameObject objeto;
        public Sprite[] quadros;
        public bool ehCinza;
    }

    sealed class EventoDaOnda
    {
        public float tempo;
        public GrupoOnda grupo;
    }

    sealed class OndaAtiva
    {
        public int numero;
        public bool geracaoTerminou;
    }

    const string CenaDoMenu = "Menu";
    const float VelocidadeBase = 1.4f;
    const float RaioDeSelecao = 0.7f;

    readonly List<Transform> pontos = new List<Transform>();
    Transform[] caminho;
    readonly List<ModeloInimigo> modelosInimigo = new List<ModeloInimigo>();
    readonly HashSet<InimigoGaucho> inimigosAtivos = new HashSet<InimigoGaucho>();
    readonly List<TorreGaucha> torres = new List<TorreGaucha>();
    readonly List<SpriteRenderer> obstaculosDeConstrucao = new List<SpriteRenderer>();
    readonly HashSet<DefinicaoInimigo> bichosVistos = new HashSet<DefinicaoInimigo>();
    readonly List<OndaAtiva> ondasAtivas = new List<OndaAtiva>();
    readonly Dictionary<string, int> abatesDeVendidos = new Dictionary<string, int>();

    Camera cameraPrincipal;
    LagoaGaucho lagoa;
    Transform grupoInimigos;
    Transform grupoTorres;
    Transform galpao;
    GameObject modeloRevolver;
    GameObject modeloEspingarda;
    GameObject previaTorre;
    TorreGaucha selecionada;
    TorreGaucha destacada;
    DefinicaoTorre definicaoEmConstrucao;
    bool posicaoPreviaValida;
    bool construindo;
    bool escolhendoAlvo;
    bool tutorialAberto = true;
    bool jogoEncerrado;
    bool vitoria;
    bool pausado;
    bool jogoLivre;
    bool inicioAutomatico;
    float proximoInicioAutomatico = -1f;
    Dificuldade dificuldade;
    ModoDeJogo modo;
    TempoMissao tempo;
    bool derrotaPorTempo;
    int vidas;
    int pilas;
    int ondaAtual;
    int eliminadosNoTotal;
    int ondasConcluidas;
    int pilasGanhas;
    int pontuacaoFinal;
    int posicaoNoRanking;
    float velocidadeDoJogo = 1f;
    string aviso = "Proteja o galpão!";
    float avisoAte;
    float proximosGlobais;

    public bool PodeCombater => !jogoEncerrado;
    public bool OndaEmAndamento => ondasAtivas.Count > 0;
    public bool Pausado => pausado;
    public bool JogoLivre => jogoLivre;
    public ModoDeJogo Modo => modo;
    public int OndasConcluidas => ondasConcluidas;
    public int OndasAtivas => ondasAtivas.Count;
    bool TemProximaOnda => jogoLivre || ondaAtual < TotalOndas;
    public bool JogoEncerrado => jogoEncerrado;
    public bool Vitoria => vitoria;
    public int Pilas => pilas;
    public int Vidas => vidas;
    public int OndaAtual => ondaAtual;
    public int TotalOndas => OndasGaucho.Total;
    public int BichosNoCampo => inimigosAtivos.Count;
    public List<TorreGaucha> Torres => torres;
    public float TempoRestante => tempo != null ? tempo.Restante : 0f;

    void Awake()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1f;

        dificuldade = Dificuldade.Atual;
        modo = ModoDeJogo.Atual;
        vidas = modo.umaVida ? 1 : dificuldade.vidas;
        pilas = dificuldade.pilasIniciais;
        tempo = new TempoMissao(dificuldade.tempoSegundos);
        inicioAutomatico = PlayerPrefs.GetInt("inicioAutomatico", 0) == 1;
        OpcoesGaucho.Carregar();

        cameraPrincipal = Camera.main;
        if (cameraPrincipal == null)
        {
            Debug.LogError("Game Gaucho: a cena precisa de uma camera com a tag MainCamera.");
            enabled = false;
            return;
        }
        posicaoDaCamera = cameraPrincipal.transform.position;

        if (!PrepararCaminho() || !PrepararInimigos() || !PrepararTorres())
        {
            enabled = false;
            return;
        }
        AplicarPreparo();

        GameObject baseDoGalpao = GameObject.Find("Base (galpao)");
        Transform visualDoGalpao = baseDoGalpao != null ? baseDoGalpao.transform.Find("Galpao") : null;
        galpao = visualDoGalpao != null
            ? visualDoGalpao
            : baseDoGalpao != null ? baseDoGalpao.transform : pontos[pontos.Count - 1];
        if (baseDoGalpao != null && galpao.GetComponent<GalpaoGaucho>() == null)
            galpao.gameObject.AddComponent<GalpaoGaucho>();
        PrepararObstaculosDeConstrucao();
        PrepararLagoa();
        PrepararLuzes();
        gameObject.AddComponent<AmbienteGaucho>();
        ExibirAviso("Contrate gaúchos e clique em INICIAR ONDA quando estiver pronto", 6f);
    }

    void PrepararObstaculosDeConstrucao()
    {
        foreach (SpriteRenderer sr in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (sr == null || sr.sortingOrder < 3)
                continue;
            if (grupoTorres != null && sr.transform.IsChildOf(grupoTorres))
                continue;
            if (grupoInimigos != null && sr.transform.IsChildOf(grupoInimigos))
                continue;
            obstaculosDeConstrucao.Add(sr);
        }
    }

    void PrepararLagoa()
    {
        lagoa = new LagoaGaucho(new Vector2(-3f, -0.9f), new Vector2(1.45f, 1.05f), transform);
        foreach (SpriteRenderer sr in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (sr != null && sr.sortingOrder < 2 && sr.gameObject != lagoa.Objeto && lagoa.Contem(sr.transform.position, -0.3f))
                sr.enabled = false;
        }
    }

    bool PrepararCaminho()
    {
        GameObject raiz = GameObject.Find("Caminho (pontos)");
        if (raiz == null)
        {
            Debug.LogError("Game Gaucho: nao encontrei o objeto 'Caminho (pontos)'.");
            return false;
        }

        foreach (Transform filho in raiz.transform)
            pontos.Add(filho);

        pontos.Sort((a, b) => NumeroDoNome(a.name).CompareTo(NumeroDoNome(b.name)));
        if (pontos.Count < 2)
        {
            Debug.LogError("Game Gaucho: o caminho precisa de pelo menos dois pontos.");
            return false;
        }

        caminho = pontos.ToArray();
        return true;
    }

    bool PrepararInimigos()
    {
        GameObject raiz = GameObject.Find("Inimigos");
        if (raiz == null)
        {
            Debug.LogError("Game Gaucho: nao encontrei o grupo 'Inimigos'.");
            return false;
        }

        grupoInimigos = raiz.transform;
        List<GameObject> originais = new List<GameObject>();
        foreach (Transform filho in grupoInimigos)
            originais.Add(filho.gameObject);

        for (int i = 0; i < originais.Count; i++)
        {
            GameObject original = originais[i];
            SeguirCaminho movimentoAntigo = original.GetComponent<SeguirCaminho>();
            SpriteRenderer renderizador = original.GetComponent<SpriteRenderer>();
            if (renderizador == null)
                continue;

            modelosInimigo.Add(new ModeloInimigo
            {
                objeto = original,
                quadros = movimentoAntigo != null ? movimentoAntigo.quadros : new[] { renderizador.sprite },
                ehCinza = original.name.ToLowerInvariant().Contains("cinza")
            });
        }

        if (ModeloDe(true) == null || ModeloDe(false) == null)
        {
            Debug.LogError("Game Gaucho: o grupo 'Inimigos' precisa de um javali comum e de um javali cinza.");
            return false;
        }
        return true;
    }

    bool PrepararTorres()
    {
        GameObject raiz = GameObject.Find("Torres");
        if (raiz == null)
        {
            Debug.LogError("Game Gaucho: nao encontrei o grupo 'Torres'.");
            return false;
        }

        grupoTorres = raiz.transform;
        foreach (Transform torreTransform in grupoTorres)
        {
            if (torreTransform.name.ToLowerInvariant().Contains("espingarda"))
                modeloEspingarda = torreTransform.gameObject;
            else
                modeloRevolver = torreTransform.gameObject;
        }

        if (modeloRevolver == null || modeloEspingarda == null)
        {
            Debug.LogError("Game Gaucho: a cena precisa das torres Gaucho_Revolver e Gaucho_Espingarda.");
            return false;
        }
        return true;
    }

    void AplicarPreparo()
    {
        foreach (ModeloInimigo modelo in modelosInimigo)
        {
            SeguirCaminho movimentoAntigo = modelo.objeto.GetComponent<SeguirCaminho>();
            if (movimentoAntigo != null)
                movimentoAntigo.enabled = false;
            modelo.objeto.SetActive(false);
        }

        foreach (Transform torreTransform in grupoTorres)
        {
            foreach (MirarNoAlvo miraAntiga in torreTransform.GetComponentsInChildren<MirarNoAlvo>(true))
                miraAntiga.enabled = false;
            torreTransform.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        AtualizarAtalhos();
        if (!pausado)
        {
            AtualizarSelecao();
            AtualizarConstrucao();
        }
        AtualizarDestaque();
        AtualizarAnelDeAlcance();
        AtualizarTextosFlutuantes();
        AtualizarEfeitos();
        AtualizarZonas();
        AtualizarArmadilhas();
        AtualizarPacotes();
        AtualizarTremor();
        AtualizarLuzes();

        if (Time.time >= proximosGlobais)
        {
            RecalcularGlobais();
            proximosGlobais = Time.time + 0.5f;
        }

        ConferirOndas();
        ConferirInicioAutomatico();
        AtualizarTempo();
    }

    void AtualizarDestaque()
    {
        TorreGaucha alvo = jogoEncerrado ? null : selecionada;
        if (alvo == destacada)
            return;
        if (destacada != null)
            destacada.Destacar(false);
        if (alvo != null)
            alvo.Destacar(true);
        destacada = alvo;
    }

    public void AlternarPausa()
    {
        if (jogoEncerrado)
            return;
        pausado = !pausado;
        Time.timeScale = pausado ? 0f : velocidadeDoJogo;
        if (pausado)
            CancelarConstrucao();
    }

    public void AlternarInicioAutomatico()
    {
        inicioAutomatico = !inicioAutomatico;
        PlayerPrefs.SetInt("inicioAutomatico", inicioAutomatico ? 1 : 0);
        if (inicioAutomatico && ondaAtual > 0 && !OndaEmAndamento && TemProximaOnda)
            proximoInicioAutomatico = Time.time + 1f;
    }

    void ConferirInicioAutomatico()
    {
        if (proximoInicioAutomatico < 0f || pausado || jogoEncerrado)
            return;
        if (!inicioAutomatico || OndaEmAndamento || !TemProximaOnda)
        {
            proximoInicioAutomatico = -1f;
            return;
        }
        if (Time.time >= proximoInicioAutomatico)
        {
            proximoInicioAutomatico = -1f;
            IniciarProximaOnda();
        }
    }

    int AjustarRenda(int valor)
    {
        return modo.metadeDasPilas ? Mathf.Max(1, Mathf.RoundToInt(valor * 0.5f)) : valor;
    }

    void GanharPilas(int valor)
    {
        pilas += valor;
        pilasGanhas += valor;
    }

    void AtualizarTempo()
    {
        if (jogoEncerrado)
            return;
        tempo.Atualizar(Time.deltaTime);
        if (tempo.Esgotou)
        {
            derrotaPorTempo = true;
            EncerrarDerrota("O tempo acabou!");
        }
    }

    TorreGaucha CriarTorre(DefinicaoTorre definicao, Vector3 posicao, bool previa)
    {
        GameObject modelo = definicao.arma == "espingarda" ? modeloEspingarda : modeloRevolver;
        GameObject objeto = Instantiate(modelo, posicao, Quaternion.identity, grupoTorres);
        objeto.name = previa ? "Previa de torre" : definicao.nome;
        objeto.SetActive(true);
        TorreGaucha torre = objeto.GetComponent<TorreGaucha>();
        if (torre == null)
            torre = objeto.AddComponent<TorreGaucha>();
        torre.Configurar(this, definicao, previa);
        return torre;
    }

    public bool ConstruirEm(DefinicaoTorre definicao, Vector3 posicao)
    {
        if (jogoEncerrado || !PosicaoDeTorreValida(posicao, definicao))
            return false;
        int preco = PrecoNoPonto(definicao, posicao);
        if (pilas < preco)
            return false;
        pilas -= preco;
        TorreGaucha torre = CriarTorre(definicao, posicao, false);
        torre.Gasto = preco;
        torres.Add(torre);
        RecalcularGlobais();
        return true;
    }

    public int PrecoNoPonto(DefinicaoTorre definicao, Vector3 posicao)
    {
        return Mathf.RoundToInt(definicao.preco * (1f - DescontoNoPonto(posicao)));
    }

    public int CustoDaMelhoria(TorreGaucha torre, int caminho)
    {
        int custo = MelhoriasTorre.Custo(torre.Definicao.preco, torre.Niveis[caminho]);
        return Mathf.RoundToInt(custo * (1f - DescontoDeMelhoria(torre)));
    }

    public bool ComprarMelhoria(TorreGaucha torre, int caminho)
    {
        if (jogoEncerrado || torre == null || torre.Ajudante || !torre.PodeMelhorar(caminho))
            return false;
        int custo = CustoDaMelhoria(torre, caminho);
        if (pilas < custo)
        {
            ExibirAviso("Faltam pilas para essa melhoria", 2.5f);
            return false;
        }
        pilas -= custo;
        Melhoria melhoria = torre.Definicao.caminhos[caminho][torre.Niveis[caminho]];
        torre.Melhorar(caminho, custo);
        RecalcularGlobais();
        ExibirAviso(melhoria.nome + "!", 2f);
        return true;
    }

    public void Vender(TorreGaucha torre)
    {
        if (torre == null || torre.Ajudante)
            return;
        if (modo.semVender)
        {
            ExibirAviso("No modo Desafio não dá para vender", 2.5f);
            return;
        }
        int abatesAntes;
        abatesDeVendidos.TryGetValue(torre.Definicao.nome, out abatesAntes);
        abatesDeVendidos[torre.Definicao.nome] = abatesAntes + torre.Abatidos;
        int valor = MelhoriasTorre.ValorDeVenda(torre.Gasto);
        pilas += valor;
        AdicionarTexto(torre.transform.position + Vector3.up * 0.5f, "+" + valor + " pilas", new Color(1f, 0.86f, 0.25f));
        torres.Remove(torre);
        if (selecionada == torre)
            selecionada = null;
        Destroy(torre.gameObject);
        RecalcularGlobais();
    }

    void SelecionarConstrucao(DefinicaoTorre definicao)
    {
        if (jogoEncerrado || definicao == null)
            return;
        if (pilas < definicao.preco)
        {
            ExibirAviso("Faltam pilas para contratar esse gaúcho", 2.5f);
            return;
        }

        selecionada = null;
        escolhendoAlvo = false;
        CancelarConstrucao();
        definicaoEmConstrucao = definicao;
        previaTorre = CriarTorre(definicao, Vector3.zero, true).gameObject;
        construindo = true;
        string onde = definicao.soNaAgua ? "na água" : definicao.aguaOuMargem ? "na água ou na beira" : "na grama";
        ExibirAviso("Clique " + onde + " para posicionar. ESC cancela.", 3f);
    }

    void ConfirmarConstrucao()
    {
        if (previaTorre == null || definicaoEmConstrucao == null)
        {
            CancelarConstrucao();
            return;
        }
        Vector3 posicao = previaTorre.transform.position;
        int preco = PrecoNoPonto(definicaoEmConstrucao, posicao);
        if (pilas < preco)
        {
            CancelarConstrucao();
            return;
        }

        pilas -= preco;
        TorreGaucha torre = previaTorre.GetComponent<TorreGaucha>();
        foreach (SpriteRenderer sr in previaTorre.GetComponentsInChildren<SpriteRenderer>(true))
            sr.color = Color.white;
        previaTorre.name = definicaoEmConstrucao.nome;
        torre.Configurar(this, definicaoEmConstrucao, false);
        torre.Gasto = preco;
        torres.Add(torre);
        previaTorre = null;
        construindo = false;
        selecionada = torre;
        RecalcularGlobais();
        ExibirAviso(definicaoEmConstrucao.nome + " contratado!", 2f);
    }

    void CancelarConstrucao()
    {
        construindo = false;
        posicaoPreviaValida = false;
        if (previaTorre != null)
            Destroy(previaTorre);
        previaTorre = null;
    }

    public bool PosicaoDeTorreValida(Vector3 posicao, DefinicaoTorre definicao)
    {
        Vector3 viewport = cameraPrincipal.WorldToViewportPoint(posicao);
        if (viewport.x < 0.045f || viewport.x > 0.955f || viewport.y < 0.08f || viewport.y > 0.92f)
            return false;

        bool naAgua = NaAgua(posicao, 0.35f);
        bool pertoDaAgua = NaAgua(posicao, -0.75f);
        if (definicao.soNaAgua && !naAgua)
            return false;
        if (definicao.aguaOuMargem && !pertoDaAgua)
            return false;
        if (!definicao.soNaAgua && !definicao.aguaOuMargem && NaAgua(posicao, -0.25f))
            return false;

        for (int i = 0; i < pontos.Count - 1; i++)
        {
            if (DistanciaAteSegmento(posicao, pontos[i].position, pontos[i + 1].position) < 1.05f)
                return false;
        }

        if (galpao != null && Vector2.Distance(posicao, galpao.position) < 1.7f)
            return false;

        if (!naAgua)
        {
            for (int i = 0; i < obstaculosDeConstrucao.Count; i++)
            {
                SpriteRenderer obstaculo = obstaculosDeConstrucao[i];
                if (obstaculo == null || !obstaculo.enabled)
                    continue;
                Vector3 pontoMaisProximo = obstaculo.bounds.ClosestPoint(posicao);
                if (((Vector2)(pontoMaisProximo - posicao)).sqrMagnitude < 0.16f)
                    return false;
            }
        }

        for (int i = torres.Count - 1; i >= 0; i--)
        {
            if (torres[i] == null)
            {
                torres.RemoveAt(i);
                continue;
            }
            if (Vector2.Distance(posicao, torres[i].Casa) < 0.95f)
                return false;
        }
        return true;
    }

    static float DistanciaAteSegmento(Vector2 ponto, Vector2 inicio, Vector2 fim)
    {
        Vector2 segmento = fim - inicio;
        float comprimentoQuadrado = segmento.sqrMagnitude;
        if (comprimentoQuadrado <= 0.0001f)
            return Vector2.Distance(ponto, inicio);
        float t = Mathf.Clamp01(Vector2.Dot(ponto - inicio, segmento) / comprimentoQuadrado);
        return Vector2.Distance(ponto, inicio + segmento * t);
    }

    public void IniciarProximaOnda()
    {
        if (jogoEncerrado || pausado || !TemProximaOnda)
            return;

        tutorialAberto = false;
        CancelarConstrucao();
        bool antecipada = ondasAtivas.Count > 0;
        ondaAtual++;
        if (ondaAtual == 1)
            tempo.Iniciar();
        OndaAtiva onda = new OndaAtiva { numero = ondaAtual };
        ondasAtivas.Add(onda);
        proximoInicioAutomatico = -1f;
        RecalcularGlobais();
        foreach (TorreGaucha torre in torres)
        {
            if (torre != null)
                torre.ComecoDaOnda();
        }
        StartCoroutine(GerarOnda(onda));
        if (!jogoLivre && ondaAtual == TotalOndas)
            ExibirAviso("ÚLTIMA ONDA! O REI JAVAPORCO CHEGOU!", 3f);
        else
            ExibirAviso(antecipada ? "Onda " + ondaAtual + " chamada antes da hora!" : "Onda " + ondaAtual + " começou!", 3f);
    }

    IEnumerator GerarOnda(OndaAtiva onda)
    {
        GrupoOnda[] grupos = OndasGaucho.Onda(onda.numero);
        List<EventoDaOnda> eventos = new List<EventoDaOnda>();
        foreach (GrupoOnda grupo in grupos)
        {
            for (int i = 0; i < grupo.quantidade; i++)
                eventos.Add(new EventoDaOnda { tempo = grupo.inicio + i * grupo.espacamento, grupo = grupo });
        }
        eventos.Sort((a, b) => a.tempo.CompareTo(b.tempo));

        float relogio = 0f;
        foreach (EventoDaOnda evento in eventos)
        {
            if (evento.tempo > relogio)
            {
                yield return new WaitForSeconds(evento.tempo - relogio);
                relogio = evento.tempo;
            }
            if (jogoEncerrado)
                yield break;

            GrupoOnda g = evento.grupo;
            CriarInimigo(g.bicho, pontos[0].position, 1, g.furtivo, g.teimoso, g.cascaGrossa, onda.numero);
            if (g.bicho.chefe)
                TremerCamera(0.12f, 0.45f);
            if (!bichosVistos.Contains(g.bicho))
            {
                bichosVistos.Add(g.bicho);
                if (onda.numero > 1 || g.bicho != TiposInimigo.Prea)
                    ExibirAviso("NOVO BICHO: " + g.bicho.nome + " (" + g.bicho.descricao + ")", 5f);
            }
        }
        onda.geracaoTerminou = true;
    }

    ModeloInimigo ModeloDe(bool cinza)
    {
        for (int i = 0; i < modelosInimigo.Count; i++)
        {
            if (modelosInimigo[i].ehCinza == cinza)
                return modelosInimigo[i];
        }
        return null;
    }

    InimigoGaucho CriarInimigo(DefinicaoInimigo definicao, Vector3 posicao, int pontoSeguinte, bool furtivo, bool teimoso, bool cascaGrossa, int onda)
    {
        ModeloInimigo modelo = ModeloDe(definicao.desenho == "javali_cinza");
        GameObject objeto = Instantiate(modelo.objeto, posicao, Quaternion.identity, grupoInimigos);
        objeto.name = definicao.nome;
        objeto.transform.localScale = modelo.objeto.transform.localScale * definicao.escala * (cascaGrossa ? 1.08f : 1f);

        SeguirCaminho antigo = objeto.GetComponent<SeguirCaminho>();
        if (antigo != null)
            antigo.enabled = false;

        InimigoGaucho inimigo = objeto.GetComponent<InimigoGaucho>();
        if (inimigo == null)
            inimigo = objeto.AddComponent<InimigoGaucho>();

        Sprite[] quadros = definicao.desenho == "javali" || definicao.desenho == "javali_cinza"
            ? modelo.quadros
            : SpritesGaucho.Quadros(definicao.desenho);
        if (quadros == null)
            quadros = modelo.quadros;

        float vidaBase = definicao.vida * dificuldade.multiplicadorVida;
        if (onda > TotalOndas)
            vidaBase *= 1f + 0.15f * (onda - TotalOndas);
        if (cascaGrossa)
            vidaBase *= 2f;
        if (definicao.Grande)
            vidaBase *= 1f - reducaoVidaGrandes;
        int vida = Mathf.Max(1, Mathf.RoundToInt(vidaBase));
        float velocidade = VelocidadeBase * definicao.velocidade * dificuldade.multiplicadorVelocidade;

        objeto.SetActive(true);
        inimigo.Configurar(this, caminho, definicao, vida, velocidade, quadros, furtivo || definicao.furtivo, teimoso, cascaGrossa, posicao, pontoSeguinte);
        inimigo.Onda = onda;
        if (definicao.coroa)
            AdicionarCoroa(objeto);
        inimigosAtivos.Add(inimigo);
        return inimigo;
    }

    void AdicionarCoroa(GameObject objeto)
    {
        GameObject coroa = new GameObject("Coroa");
        coroa.transform.SetParent(objeto.transform, false);
        coroa.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        SpriteRenderer sr = coroa.AddComponent<SpriteRenderer>();
        sr.sprite = SpritesGaucho.Pegar("coroa");
        SpriteRenderer pai = objeto.GetComponent<SpriteRenderer>();
        sr.sortingOrder = pai != null ? pai.sortingOrder + 1 : 6;
    }

    void SoltarFilhotes(InimigoGaucho pai, Vector3 posicao)
    {
        DefinicaoInimigo d = pai.Definicao;
        int total = 0;
        if (d.filho != null)
        {
            for (int i = 0; i < d.quantidadeFilhos; i++)
                CriarFilhote(pai, d.filho, posicao, total++);
        }
        if (d.filho2 != null)
        {
            for (int i = 0; i < d.quantidadeFilhos2; i++)
                CriarFilhote(pai, d.filho2, posicao, total++);
        }
    }

    public void SoltarFilhote(InimigoGaucho pai, DefinicaoInimigo filho)
    {
        if (pai == null || jogoEncerrado)
            return;
        CriarFilhote(pai, filho, pai.transform.position, 0);
    }

    void CriarFilhote(InimigoGaucho pai, DefinicaoInimigo filho, Vector3 posicao, int ordem)
    {
        bool furtivo = pai.Furtivo && !filho.Grande;
        bool teimoso = pai.Teimoso || pai.Definicao.filhosTeimosos;
        InimigoGaucho novo = CriarInimigo(filho, posicao, pai.ProximoPonto, furtivo, teimoso, false, pai.Onda);
        if (ordem > 0)
            novo.Recuar(0.16f * ordem);
        if (pai.Contagia)
        {
            novo.Contagia = true;
            if (pai.LentidaoAtual > 0f)
                novo.AplicarLentidao(pai.LentidaoAtual, pai.LentidaoRestante + 0.5f);
            if (pai.QueimaAtual > 0)
                novo.Queimar(pai.QueimaAtual, pai.QueimaRestante + 0.5f, pai.TipoQueima, null);
        }
    }

    void ConferirOndas()
    {
        for (int i = ondasAtivas.Count - 1; i >= 0 && !jogoEncerrado; i--)
        {
            OndaAtiva onda = ondasAtivas[i];
            if (!onda.geracaoTerminou || TemBichoDaOnda(onda.numero))
                continue;
            ondasAtivas.RemoveAt(i);
            ConcluirOnda(onda.numero);
        }
    }

    bool TemBichoDaOnda(int numero)
    {
        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo != null && inimigo.Onda == numero)
                return true;
        }
        return false;
    }

    void ConcluirOnda(int numero)
    {
        ondasConcluidas++;
        ExpirarArmadilhas();
        if (vidas <= 0)
            return;

        bool campoLimpo = ondasAtivas.Count == 0;
        if (campoLimpo)
        {
            foreach (TorreGaucha torre in torres)
            {
                if (torre != null)
                    torre.FimDaOnda();
            }
        }

        if (!jogoLivre && campoLimpo && ondaAtual >= TotalOndas)
        {
            Vencer();
            return;
        }

        int bonus = AjustarRenda(60 + numero * 12);
        GanharPilas(bonus);
        PagarRendas();
        ExibirAviso("Onda " + numero + " vencida! Bônus de " + bonus + " pilas", 4f);
        if (campoLimpo && inicioAutomatico && TemProximaOnda)
            proximoInicioAutomatico = Time.time + 2f;
    }

    void Vencer()
    {
        jogoEncerrado = true;
        vitoria = true;
        pausado = false;
        tempo.Parar();
        velocidadeDoJogo = 1f;
        Time.timeScale = 1f;
        RegistrarNoRanking();
        EfeitosSonoros.Vitoria();
        ExibirAviso("O galpão está salvo!", 5f);
    }

    public void ContinuarJogoLivre()
    {
        if (!jogoEncerrado || !vitoria || jogoLivre)
            return;
        jogoLivre = true;
        jogoEncerrado = false;
        ExibirAviso("JOGO LIVRE: agora as ondas não acabam mais!", 4f);
    }

    public void FaseDoChefe(InimigoGaucho chefe, int fase)
    {
        if (chefe == null || jogoEncerrado)
            return;
        chefe.Blindar(2.5f);
        Vector3 posicao = chefe.transform.position;
        for (int i = 0; i < 2; i++)
            CriarFilhote(chefe, TiposInimigo.Javali, posicao, i + 1);
        TremerCamera(0.2f, 0.6f);
        CriarAnel(posicao, 2.4f, new Color(1f, 0.4f, 0.2f, 0.9f), 0.5f);
        ExibirAviso(fase == 1 ? "O REI JAVAPORCO CHAMOU REFORÇOS!" : "O REI JAVAPORCO SE ENFURECEU!", 3f);
    }

    void PagarRendas()
    {
        foreach (TorreGaucha torre in torres)
        {
            if (torre == null || torre.Ajudante)
                continue;
            AtributosTorre a = torre.Atributos;
            Vector3 acima = torre.transform.position + Vector3.up * 0.6f;
            if (a.producao > 0)
            {
                int valor = AjustarRenda(Mathf.RoundToInt(a.producao * a.multiplicadorProducao));
                if (a.guaiaca)
                {
                    int guardado = Mathf.RoundToInt(torre.Guardado * (1f + a.rendimento)) + valor;
                    torre.Guardado = Mathf.Min(a.limiteGuardado, guardado);
                    AdicionarTexto(acima, "guaiaca: $" + torre.Guardado, new Color(1f, 0.86f, 0.25f));
                }
                else if (a.coletaAutomatica)
                {
                    GanharPilas(valor);
                    AdicionarTexto(acima, "+" + valor + " pilas", new Color(1f, 0.86f, 0.25f));
                }
                else
                    CriarPacotes(torre, valor);
            }
            if (a.pilasPorOnda > 0)
            {
                int extra = AjustarRenda(a.pilasPorOnda);
                GanharPilas(extra);
                AdicionarTexto(acima + Vector3.up * 0.3f, "+" + extra + " pilas", new Color(1f, 0.86f, 0.25f));
            }
            if (a.vidasPorOnda > 0)
            {
                vidas += a.vidasPorOnda;
                AdicionarTexto(acima + Vector3.up * 0.6f, "+" + a.vidasPorOnda + " vida", new Color(0.5f, 1f, 0.5f));
            }
        }
    }

    public void SacarGuaiaca(TorreGaucha torre)
    {
        if (torre == null || torre.Guardado <= 0)
            return;
        GanharPilas(torre.Guardado);
        AdicionarTexto(torre.transform.position + Vector3.up * 0.6f, "+" + torre.Guardado + " pilas", new Color(1f, 0.86f, 0.25f));
        torre.Guardado = 0;
    }

    public void InimigoEliminado(InimigoGaucho inimigo, TorreGaucha origem)
    {
        if (!inimigosAtivos.Remove(inimigo))
            return;

        Vector3 posicao = inimigo.transform.position;
        int recompensa = inimigo.Definicao.recompensa + inimigo.BonusPilas;
        float bonus = bonusPilasGlobal + BonusDePilasNoPonto(posicao);
        recompensa = AjustarRenda(Mathf.RoundToInt(recompensa * (1f + bonus)));
        GanharPilas(recompensa);
        eliminadosNoTotal++;
        if (origem != null)
            origem.Abatidos++;

        EfeitosSonoros.Morte(inimigo.EhChefe);
        if (recompensa >= 3 || inimigo.Grande)
            AdicionarTexto(posicao + Vector3.up * 0.45f, "+" + recompensa, new Color(1f, 0.86f, 0.25f));
        CriarEstouro(posicao, new Color(1f, 0.65f, 0.18f), inimigo.Grande ? 18 : 8, inimigo.Grande ? 0.5f : 0.12f);

        if (!jogoEncerrado)
        {
            SoltarFilhotes(inimigo, posicao);
            if (inimigo.ExplodeAoCair > 0)
                Explodir(posicao, 1.3f, AtaqueDeExplosaoAoCair(), inimigo.ExplodeAoCair, origem);
            LiberarAlmas(posicao, inimigo.ProximoPonto);
        }
    }

    public void InimigoChegouAoGalpao(InimigoGaucho inimigo, int dano, Vector3 posicao)
    {
        if (!inimigosAtivos.Remove(inimigo))
            return;
        vidas = Mathf.Max(0, vidas - dano);
        EfeitosSonoros.DanoNoGalpao();
        if (dano >= 10)
            TremerCamera(0.12f, 0.3f);
        AdicionarTexto(posicao + Vector3.up * 0.5f, "-" + dano + " vida", new Color(1f, 0.3f, 0.25f));
        CriarEstouro(posicao, new Color(0.85f, 0.22f, 0.16f), 9, 0.12f);

        if (vidas == 0)
            EncerrarDerrota(inimigo.EhChefe ? "O " + inimigo.Definicao.nome + " invadiu o galpão..." : "O galpão foi invadido...");
    }

    void EncerrarDerrota(string mensagem)
    {
        if (jogoEncerrado)
            return;
        jogoEncerrado = true;
        vitoria = false;
        pausado = false;
        ondasAtivas.Clear();
        tempo.Parar();
        velocidadeDoJogo = 1f;
        Time.timeScale = 1f;
        StopAllCoroutines();
        CancelarConstrucao();
        if (!jogoLivre)
            RegistrarNoRanking();
        EfeitosSonoros.Derrota();
        ExibirAviso(mensagem, 5f);
    }

    int CalcularPontuacao()
    {
        int ondasVencidas = vitoria ? TotalOndas : ondasConcluidas;
        int total = eliminadosNoTotal * 10 + ondasVencidas * 100;
        if (vitoria)
            total += 500 + vidas * 20 + Mathf.CeilToInt(tempo.Restante) * 2;
        return Mathf.RoundToInt(total * dificuldade.multiplicadorPontuacao * modo.multiplicadorPontuacao);
    }

    void RegistrarNoRanking()
    {
        pontuacaoFinal = CalcularPontuacao();
        posicaoNoRanking = Ranking.Registrar(new RegistroRanking
        {
            pontos = pontuacaoFinal,
            nivel = dificuldade.nome,
            modo = this.modo.nome,
            vitoria = vitoria,
            ondas = vitoria ? TotalOndas : ondasConcluidas,
            data = System.DateTime.Now.ToString("dd/MM HH:mm")
        });
    }

    public void DescartarInimigo(InimigoGaucho inimigo)
    {
        inimigosAtivos.Remove(inimigo);
    }

    void ExibirAviso(string mensagem, float duracao)
    {
        aviso = mensagem;
        avisoAte = Time.unscaledTime + duracao;
    }

    void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void VoltarAoMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(CenaDoMenu);
    }

    void AlternarVelocidade()
    {
        if (jogoEncerrado)
            return;
        velocidadeDoJogo = velocidadeDoJogo >= 2.5f ? 1f : velocidadeDoJogo + 1f;
        if (!pausado)
            Time.timeScale = velocidadeDoJogo;
    }

    static int NumeroDoNome(string nome)
    {
        int inicio = nome.Length;
        while (inicio > 0 && char.IsDigit(nome[inicio - 1]))
            inicio--;
        int numero;
        return int.TryParse(nome.Substring(inicio), out numero) ? numero : 0;
    }

    void OnDestroy()
    {
        Time.timeScale = 1f;
        LimparInterface();
        LimparEfeitos();
    }
}

static class JogoGauchoBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar()
    {
        SceneManager.sceneLoaded -= AoCarregarCena;
        SceneManager.sceneLoaded += AoCarregarCena;
        CriarGerenciador();
    }

    static void AoCarregarCena(Scene cena, LoadSceneMode modo)
    {
        CriarGerenciador();
    }

    static void CriarGerenciador()
    {
        if (GameObject.Find("Caminho (pontos)") == null)
            return;
        if (Object.FindFirstObjectByType<JogoGaucho>() != null)
            return;

        GameObject gerenciador = new GameObject("Jogo Gaucho - Gerenciador");
        gerenciador.AddComponent<JogoGaucho>();
    }
}
