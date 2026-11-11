using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum TipoTorreGaucha
{
    Revolver,
    Espingarda
}

/// <summary>
/// Monta a camada jogavel sobre a cena de pixel art existente.
/// A cena continua simples de editar: basta abri-la e apertar Play.
/// </summary>
public sealed class JogoGaucho : MonoBehaviour
{
    sealed class ModeloInimigo
    {
        public GameObject objeto;
        public Sprite[] quadros;
        public bool ehCinza;
    }

    sealed class TextoFlutuante
    {
        public Vector3 posicao;
        public string texto;
        public Color cor;
        public float criadoEm;
    }

    const int TotalOndas = 6;
    const int CustoRevolver = 90;
    const int CustoEspingarda = 130;
    const int EscoltaDoChefe = 10;
    const float RaioDeSelecao = 0.7f;
    const int PontosDoAnel = 48;

    readonly List<Transform> pontos = new List<Transform>();
    Transform[] caminho;
    readonly List<ModeloInimigo> modelosInimigo = new List<ModeloInimigo>();
    readonly HashSet<InimigoGaucho> inimigosAtivos = new HashSet<InimigoGaucho>();
    readonly List<TorreGaucha> torres = new List<TorreGaucha>();
    readonly List<TextoFlutuante> textosFlutuantes = new List<TextoFlutuante>();
    readonly List<SpriteRenderer> obstaculosDeConstrucao = new List<SpriteRenderer>();

    Camera cameraPrincipal;
    Transform grupoInimigos;
    Transform grupoTorres;
    Transform galpao;
    GameObject modeloRevolver;
    GameObject modeloEspingarda;
    GameObject previaTorre;
    TorreGaucha selecionada;
    LineRenderer anelDeAlcance;
    TipoTorreGaucha tipoEmConstrucao;
    bool posicaoPreviaValida;
    bool construindo;
    bool ondaEmAndamento;
    bool geracaoTerminou;
    bool tutorialAberto = true;
    bool jogoEncerrado;
    bool vitoria;
    int vidas = 20;
    // O jogador comeca sem gauchos: 140 de troco + o preco de um revolver e de uma espingarda.
    int pilas = 360;
    int ondaAtual;
    int eliminados;
    int inimigosDaOnda;
    float velocidadeDoJogo = 1f;
    string aviso = "Proteja o galpao!";
    float avisoAte;

    GUIStyle estiloPainel;
    GUIStyle estiloTitulo;
    GUIStyle estiloTexto;
    GUIStyle estiloDestaque;
    GUIStyle estiloBotao;
    GUIStyle estiloBotaoPerigo;
    GUIStyle estiloCentro;
    Texture2D texturaPainel;
    Texture2D texturaBotao;
    Texture2D texturaBotaoHover;
    Texture2D texturaPerigo;
    Material materialParticula;

    public bool PodeCombater => !jogoEncerrado;

    void Awake()
    {
        Application.targetFrameRate = 60;
        Time.timeScale = 1f;

        cameraPrincipal = Camera.main;
        if (cameraPrincipal == null)
        {
            Debug.LogError("Game Gaucho: a cena precisa de uma camera com a tag MainCamera.");
            enabled = false;
            return;
        }

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
        // A chegada dos animais e detectada por colisao com o galpao (trigger), nao pelo ultimo ponto.
        if (baseDoGalpao != null && galpao.GetComponent<GalpaoGaucho>() == null)
            galpao.gameObject.AddComponent<GalpaoGaucho>();
        PrepararObstaculosDeConstrucao();
        ExibirAviso("Clique em INICIAR ONDA quando estiver pronto", 6f);
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

        if (modelosInimigo.Count == 0)
        {
            Debug.LogError("Game Gaucho: nenhum inimigo com SpriteRenderer foi encontrado.");
            return false;
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

    // So mexe na cena depois que todas as validacoes passaram, para nao deixar a cena pela metade.
    void AplicarPreparo()
    {
        foreach (ModeloInimigo modelo in modelosInimigo)
        {
            SeguirCaminho movimentoAntigo = modelo.objeto.GetComponent<SeguirCaminho>();
            if (movimentoAntigo != null)
                movimentoAntigo.enabled = false;
            modelo.objeto.SetActive(false);
        }

        // Os gauchos da cena sao so moldes (como os javalis): o jogador comeca sem nenhum
        // e posiciona todos onde quiser.
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
        // A selecao vem antes da construcao: o clique que posiciona uma torre nao a seleciona no mesmo quadro.
        AtualizarSelecao();
        AtualizarConstrucao();
        // Depois da construcao, para o anel acompanhar a previa no mesmo quadro em que ela se move.
        AtualizarAnelDeAlcance();
        AtualizarTextosFlutuantes();

        if (ondaEmAndamento && geracaoTerminou && inimigosAtivos.Count == 0)
            ConcluirOnda();
    }

    void AtualizarAtalhos()
    {
        Keyboard teclado = Keyboard.current;
        if (teclado == null)
            return;

        if (teclado.uKey.wasPressedThisFrame)
            MelhorarSelecionada();

        if (teclado.escapeKey.wasPressedThisFrame)
        {
            selecionada = null;
            if (construindo)
                CancelarConstrucao();
            else
                tutorialAberto = false;
        }

        if (jogoEncerrado && teclado.rKey.wasPressedThisFrame)
        {
            Reiniciar();
            return;
        }

        if (teclado.spaceKey.wasPressedThisFrame)
            IniciarProximaOnda();
        if (teclado.digit1Key.wasPressedThisFrame)
            SelecionarConstrucao(TipoTorreGaucha.Revolver);
        if (teclado.digit2Key.wasPressedThisFrame)
            SelecionarConstrucao(TipoTorreGaucha.Espingarda);
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

        Vector3 mundo = cameraPrincipal.ScreenToWorldPoint(new Vector3(mouseTela.x, mouseTela.y, -cameraPrincipal.transform.position.z));
        TorreGaucha maisProxima = null;
        float menorDistancia = RaioDeSelecao;
        for (int i = 0; i < torres.Count; i++)
        {
            if (torres[i] == null)
                continue;
            float distancia = Vector2.Distance(mundo, torres[i].transform.position);
            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;
                maisProxima = torres[i];
            }
        }

        selecionada = maisProxima;
    }

    // Mostra o alcance da previa durante a construcao (verde = pode colocar, vermelho = bloqueado)
    // ou, fora dela, o alcance do gaucho selecionado.
    void AtualizarAnelDeAlcance()
    {
        TorreGaucha torreDoAnel = selecionada;
        Color cor = new Color(1f, 0.82f, 0.3f, 0.85f);
        if (construindo && previaTorre != null)
        {
            torreDoAnel = previaTorre.GetComponent<TorreGaucha>();
            cor = posicaoPreviaValida
                ? new Color(0.55f, 1f, 0.55f, 0.9f)
                : new Color(1f, 0.42f, 0.42f, 0.9f);
        }

        if (torreDoAnel == null)
        {
            if (anelDeAlcance != null)
                anelDeAlcance.enabled = false;
            return;
        }

        if (anelDeAlcance == null)
            CriarAnelDeAlcance();

        anelDeAlcance.enabled = true;
        anelDeAlcance.startColor = cor;
        anelDeAlcance.endColor = cor;
        Vector3 centro = torreDoAnel.transform.position;
        float raio = torreDoAnel.Alcance;
        for (int i = 0; i <= PontosDoAnel; i++)
        {
            float angulo = i * Mathf.PI * 2f / PontosDoAnel;
            anelDeAlcance.SetPosition(i, new Vector3(centro.x + Mathf.Cos(angulo) * raio, centro.y + Mathf.Sin(angulo) * raio, -0.2f));
        }
    }

    void CriarAnelDeAlcance()
    {
        GameObject objeto = new GameObject("Anel de alcance");
        objeto.transform.SetParent(transform, false);
        anelDeAlcance = objeto.AddComponent<LineRenderer>();
        if (materialParticula == null)
            materialParticula = new Material(Shader.Find("Sprites/Default"));
        anelDeAlcance.material = materialParticula;
        anelDeAlcance.useWorldSpace = true;
        anelDeAlcance.loop = false;
        anelDeAlcance.positionCount = PontosDoAnel + 1;
        anelDeAlcance.startWidth = 0.045f;
        anelDeAlcance.endWidth = 0.045f;
        anelDeAlcance.sortingOrder = 24;
    }

    int CustoDaMelhoria(TorreGaucha torre)
    {
        return MelhoriasTorre.Custo(Custo(torre.Tipo), torre.Nivel);
    }

    void MelhorarSelecionada()
    {
        if (jogoEncerrado || selecionada == null || !selecionada.PodeMelhorar)
            return;

        int custo = CustoDaMelhoria(selecionada);
        if (pilas < custo)
        {
            ExibirAviso("Faltam pilas para melhorar esse gaucho", 2.5f);
            return;
        }

        pilas -= custo;
        selecionada.Melhorar();
        ExibirAviso("Gaucho melhorado para o nivel " + selecionada.Nivel, 2f);
    }

    void AtualizarConstrucao()
    {
        if (!construindo || previaTorre == null || Mouse.current == null)
            return;

        Vector2 mouseTela = Mouse.current.position.ReadValue();
        Vector3 mundo = cameraPrincipal.ScreenToWorldPoint(new Vector3(mouseTela.x, mouseTela.y, -cameraPrincipal.transform.position.z));
        mundo.x = Mathf.Round(mundo.x * 2f) / 2f;
        mundo.y = Mathf.Round(mundo.y * 2f) / 2f;
        mundo.z = 0f;
        previaTorre.transform.position = mundo;

        posicaoPreviaValida = PosicaoDeTorreValida(mundo) && !PonteiroSobreInterface(mouseTela);
        Color corPrevia = posicaoPreviaValida
            ? new Color(0.55f, 1f, 0.55f, 0.72f)
            : new Color(1f, 0.42f, 0.42f, 0.72f);
        foreach (SpriteRenderer sr in previaTorre.GetComponentsInChildren<SpriteRenderer>(true))
            sr.color = corPrevia;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelarConstrucao();
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && posicaoPreviaValida)
            ConfirmarConstrucao();
    }

    bool PonteiroSobreInterface(Vector2 mouseTela)
    {
        float escala = EscalaUi();
        float yGui = Screen.height - mouseTela.y;
        if (yGui < 92f * escala)
            return true;
        if (yGui > Screen.height - 125f * escala && mouseTela.x < 650f * escala)
            return true;
        if (yGui > Screen.height - 150f * escala && mouseTela.x > Screen.width - 410f * escala)
            return true;
        if (tutorialAberto && mouseTela.x < 455f * escala && yGui < 390f * escala)
            return true;
        if (selecionada != null && RectPainelMelhoria(escala).Contains(new Vector2(mouseTela.x, yGui)))
            return true;
        return false;
    }

    bool PosicaoDeTorreValida(Vector3 posicao)
    {
        Vector3 viewport = cameraPrincipal.WorldToViewportPoint(posicao);
        if (viewport.x < 0.045f || viewport.x > 0.955f || viewport.y < 0.08f || viewport.y > 0.92f)
            return false;

        for (int i = 0; i < pontos.Count - 1; i++)
        {
            if (DistanciaAteSegmento(posicao, pontos[i].position, pontos[i + 1].position) < 1.05f)
                return false;
        }

        if (galpao != null && Vector2.Distance(posicao, galpao.position) < 1.7f)
            return false;

        for (int i = 0; i < obstaculosDeConstrucao.Count; i++)
        {
            SpriteRenderer obstaculo = obstaculosDeConstrucao[i];
            if (obstaculo == null || !obstaculo.enabled)
                continue;
            Vector3 pontoMaisProximo = obstaculo.bounds.ClosestPoint(posicao);
            if (((Vector2)(pontoMaisProximo - posicao)).sqrMagnitude < 0.16f)
                return false;
        }

        for (int i = torres.Count - 1; i >= 0; i--)
        {
            if (torres[i] == null)
            {
                torres.RemoveAt(i);
                continue;
            }
            if (Vector2.Distance(posicao, torres[i].transform.position) < 1.25f)
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

    void SelecionarConstrucao(TipoTorreGaucha tipo)
    {
        if (jogoEncerrado)
            return;

        int custo = Custo(tipo);
        if (pilas < custo)
        {
            ExibirAviso("Faltam pilas para contratar esse gaucho", 2.5f);
            return;
        }

        selecionada = null;
        CancelarConstrucao();
        tipoEmConstrucao = tipo;
        GameObject modelo = tipo == TipoTorreGaucha.Revolver ? modeloRevolver : modeloEspingarda;
        previaTorre = Instantiate(modelo, grupoTorres);
        previaTorre.name = "Previa de torre";
        previaTorre.SetActive(true);

        TorreGaucha torre = previaTorre.GetComponent<TorreGaucha>();
        if (torre == null)
            torre = previaTorre.AddComponent<TorreGaucha>();
        torre.Configurar(this, tipo, true);
        construindo = true;
        ExibirAviso("Clique na grama para posicionar. ESC cancela.", 3f);
    }

    void ConfirmarConstrucao()
    {
        int custo = Custo(tipoEmConstrucao);
        if (pilas < custo || previaTorre == null)
        {
            CancelarConstrucao();
            return;
        }

        pilas -= custo;
        TorreGaucha torre = previaTorre.GetComponent<TorreGaucha>();
        torre.Configurar(this, tipoEmConstrucao, false);
        foreach (SpriteRenderer sr in previaTorre.GetComponentsInChildren<SpriteRenderer>(true))
            sr.color = Color.white;
        previaTorre.name = tipoEmConstrucao == TipoTorreGaucha.Revolver
            ? "Gaucho_Revolver_Contratado"
            : "Gaucho_Espingarda_Contratado";
        torres.Add(torre);
        previaTorre = null;
        construindo = false;
        ExibirAviso("Gaucho contratado!", 2f);
    }

    void CancelarConstrucao()
    {
        construindo = false;
        posicaoPreviaValida = false;
        if (previaTorre != null)
            Destroy(previaTorre);
        previaTorre = null;
    }

    int Custo(TipoTorreGaucha tipo)
    {
        return tipo == TipoTorreGaucha.Revolver ? CustoRevolver : CustoEspingarda;
    }

    public void IniciarProximaOnda()
    {
        if (jogoEncerrado || ondaEmAndamento || ondaAtual >= TotalOndas)
            return;

        tutorialAberto = false;
        CancelarConstrucao();
        ondaAtual++;
        ondaEmAndamento = true;
        geracaoTerminou = false;
        eliminados = 0;
        bool ondaDoChefe = ondaAtual == TotalOndas;
        inimigosDaOnda = ondaDoChefe ? 1 + EscoltaDoChefe : 5 + ondaAtual * 2;
        StartCoroutine(GerarOnda(inimigosDaOnda));
        if (ondaDoChefe)
            ExibirAviso("O CHEFE CHEGOU!", 4f);
        else
            ExibirAviso("Onda " + ondaAtual + " comecou!", 2f);
    }

    IEnumerator GerarOnda(int quantidade)
    {
        int[] contagem = new int[TiposInimigo.Todos.Length];
        for (int i = 0; i < quantidade; i++)
        {
            if (jogoEncerrado)
                yield break;

            // Na onda final o chefe sai primeiro; como e lento, a escolta o ultrapassa.
            DefinicaoInimigo definicao = ondaAtual == TotalOndas && i == 0
                ? TiposInimigo.Chefe
                : TiposInimigo.Sortear(ondaAtual);
            contagem[System.Array.IndexOf(TiposInimigo.Todos, definicao)]++;
            ModeloInimigo modelo = ModeloDe(definicao.origemCinza);
            GameObject objeto = Instantiate(modelo.objeto, pontos[0].position, Quaternion.identity, grupoInimigos);
            objeto.name = definicao.nome;
            objeto.transform.localScale *= definicao.escala;

            SeguirCaminho antigo = objeto.GetComponent<SeguirCaminho>();
            if (antigo != null)
                antigo.enabled = false;

            InimigoGaucho inimigo = objeto.GetComponent<InimigoGaucho>();
            if (inimigo == null)
                inimigo = objeto.AddComponent<InimigoGaucho>();

            int vida = Mathf.Max(1, Mathf.RoundToInt((5 + ondaAtual * 4) * definicao.multiplicadorVida));
            float velocidade = (1.45f + ondaAtual * 0.08f) * definicao.multiplicadorVelocidade;
            int recompensa = 7 + ondaAtual + definicao.bonusRecompensa;

            objeto.SetActive(true);
            inimigo.Configurar(this, caminho, vida, velocidade, recompensa, definicao.danoNaBase, modelo.quadros, definicao.cor, definicao == TiposInimigo.Chefe);
            inimigosAtivos.Add(inimigo);
            yield return new WaitForSeconds(Mathf.Max(0.48f, 0.88f - ondaAtual * 0.045f));
        }

        System.Text.StringBuilder resumo = new System.Text.StringBuilder("Game Gaucho: onda " + ondaAtual + " ->");
        for (int i = 0; i < contagem.Length; i++)
        {
            if (contagem[i] > 0)
                resumo.Append(' ').Append(TiposInimigo.Todos[i].nome).Append(" x").Append(contagem[i]);
        }
        Debug.Log(resumo.ToString());

        geracaoTerminou = true;
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

    void ConcluirOnda()
    {
        ondaEmAndamento = false;
        geracaoTerminou = false;

        if (vidas <= 0)
            return;

        if (ondaAtual >= TotalOndas)
        {
            jogoEncerrado = true;
            vitoria = true;
            velocidadeDoJogo = 1f;
            Time.timeScale = 1f;
            ExibirAviso("O galpao esta salvo!", 5f);
            return;
        }

        int bonus = 30 + ondaAtual * 8;
        pilas += bonus;
        ExibirAviso("Onda vencida! Bonus de " + bonus + " pilas", 4f);
    }

    public InimigoGaucho EncontrarAlvo(Vector3 origem, float alcance)
    {
        InimigoGaucho melhor = null;
        float melhorProgresso = -1f;
        float alcanceQuadrado = alcance * alcance;

        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo == null || !inimigo.EstaVivo)
                continue;
            if (((Vector2)(inimigo.transform.position - origem)).sqrMagnitude > alcanceQuadrado)
                continue;
            if (inimigo.ProgressoCaminho > melhorProgresso)
            {
                melhor = inimigo;
                melhorProgresso = inimigo.ProgressoCaminho;
            }
        }

        return melhor;
    }

    public void InimigoEliminado(InimigoGaucho inimigo, int recompensa, Vector3 posicao)
    {
        if (!inimigosAtivos.Remove(inimigo))
            return;
        pilas += recompensa;
        eliminados++;
        AdicionarTexto(posicao + Vector3.up * 0.45f, "+" + recompensa + " pilas", new Color(1f, 0.86f, 0.25f));
        CriarEstouro(posicao, new Color(1f, 0.65f, 0.18f));
    }

    public void InimigoChegouAoGalpao(InimigoGaucho inimigo, int dano, Vector3 posicao)
    {
        if (!inimigosAtivos.Remove(inimigo))
            return;
        vidas = Mathf.Max(0, vidas - dano);
        AdicionarTexto(posicao + Vector3.up * 0.5f, "-" + dano + " vida", new Color(1f, 0.3f, 0.25f));
        CriarEstouro(posicao, new Color(0.85f, 0.22f, 0.16f));

        if (vidas == 0)
        {
            jogoEncerrado = true;
            vitoria = false;
            ondaEmAndamento = false;
            velocidadeDoJogo = 1f;
            Time.timeScale = 1f;
            StopAllCoroutines();
            CancelarConstrucao();
            ExibirAviso("O galpao foi invadido...", 5f);
        }
    }

    public void DescartarInimigo(InimigoGaucho inimigo)
    {
        inimigosAtivos.Remove(inimigo);
    }

    void CriarEstouro(Vector3 posicao, Color cor)
    {
        GameObject efeito = new GameObject("Estouro");
        efeito.transform.position = posicao;
        ParticleSystem particulas = efeito.AddComponent<ParticleSystem>();
        particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule principal = particulas.main;
        principal.duration = 0.12f;
        principal.loop = false;
        principal.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.42f);
        principal.startSpeed = new ParticleSystem.MinMaxCurve(0.7f, 1.8f);
        principal.startSize = new ParticleSystem.MinMaxCurve(0.055f, 0.12f);
        principal.startColor = new ParticleSystem.MinMaxGradient(cor, Color.white);
        principal.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emissao = particulas.emission;
        emissao.rateOverTime = 0f;
        emissao.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)9) });

        ParticleSystem.ShapeModule forma = particulas.shape;
        forma.shapeType = ParticleSystemShapeType.Circle;
        forma.radius = 0.12f;

        ParticleSystem.ColorOverLifetimeModule corAoLongoDaVida = particulas.colorOverLifetime;
        corAoLongoDaVida.enabled = true;
        Gradient gradiente = new Gradient();
        gradiente.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(cor, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        corAoLongoDaVida.color = gradiente;

        ParticleSystemRenderer renderizador = efeito.GetComponent<ParticleSystemRenderer>();
        if (materialParticula == null)
            materialParticula = new Material(Shader.Find("Sprites/Default"));
        renderizador.material = materialParticula;
        renderizador.sortingOrder = 30;
        particulas.Play();
        Destroy(efeito, 0.8f);
    }

    void AdicionarTexto(Vector3 posicao, string texto, Color cor)
    {
        textosFlutuantes.Add(new TextoFlutuante
        {
            posicao = posicao,
            texto = texto,
            cor = cor,
            criadoEm = Time.unscaledTime
        });
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

    void AlternarVelocidade()
    {
        if (jogoEncerrado)
            return;
        velocidadeDoJogo = velocidadeDoJogo < 1.5f ? 2f : 1f;
        Time.timeScale = velocidadeDoJogo;
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
        GUI.Box(new Rect(0f, 0f, Screen.width, 86f * e), GUIContent.none, estiloPainel);
        GUI.Label(new Rect(22f * e, 12f * e, 340f * e, 62f * e), "GAME GAUCHO", estiloTitulo);
        GUI.Label(new Rect(390f * e, 17f * e, 220f * e, 52f * e), "VIDAS  " + vidas, estiloDestaque);
        GUI.Label(new Rect(625f * e, 17f * e, 260f * e, 52f * e), "PILAS  $" + pilas, estiloDestaque);
        GUI.Label(new Rect(900f * e, 17f * e, 260f * e, 52f * e), "ONDA  " + ondaAtual + "/" + TotalOndas, estiloDestaque);

        if (GUI.Button(new Rect(Screen.width - 130f * e, 16f * e, 104f * e, 50f * e), "x" + velocidadeDoJogo.ToString("0"), estiloBotao))
            AlternarVelocidade();

        float rodapeY = Screen.height - 104f * e;
        GUI.Box(new Rect(18f * e, rodapeY - 10f * e, 606f * e, 96f * e), GUIContent.none, estiloPainel);
        GUI.Label(new Rect(32f * e, rodapeY - 1f * e, 160f * e, 32f * e), "CONTRATAR", estiloTexto);
        if (GUI.Button(new Rect(175f * e, rodapeY, 205f * e, 58f * e), "[1] REVOLVER $" + CustoRevolver, estiloBotao))
            SelecionarConstrucao(TipoTorreGaucha.Revolver);
        if (GUI.Button(new Rect(392f * e, rodapeY, 216f * e, 58f * e), "[2] ESPING. $" + CustoEspingarda, estiloBotao))
            SelecionarConstrucao(TipoTorreGaucha.Espingarda);

        if (selecionada != null && !jogoEncerrado)
            DesenharPainelMelhoria(e);

        if (!ondaEmAndamento && !jogoEncerrado)
        {
            string rotulo = ondaAtual == 0 ? "INICIAR ONDA 1" : "PROXIMA ONDA  " + (ondaAtual + 1);
            if (GUI.Button(new Rect(Screen.width - 390f * e, Screen.height - 118f * e, 365f * e, 88f * e), rotulo + "   [ESPACO]", estiloBotao))
                IniciarProximaOnda();
        }
        else if (ondaEmAndamento)
        {
            GUI.Box(new Rect(Screen.width - 390f * e, Screen.height - 118f * e, 365f * e, 88f * e), GUIContent.none, estiloPainel);
            GUI.Label(new Rect(Screen.width - 375f * e, Screen.height - 105f * e, 335f * e, 62f * e),
                "ONDA EM CURSO\nEliminados: " + eliminados + "/" + inimigosDaOnda, estiloCentro);
        }

        if (construindo)
        {
            string estado = posicaoPreviaValida ? "POSICAO LIVRE" : "POSICAO BLOQUEADA";
            GUI.Label(new Rect(Screen.width * 0.5f - 220f * e, Screen.height - 72f * e, 440f * e, 46f * e),
                estado + "  |  clique para colocar  |  ESC cancela", estiloCentro);
        }

        if (Time.unscaledTime < avisoAte)
            GUI.Label(new Rect(Screen.width * 0.5f - 370f * e, 102f * e, 740f * e, 54f * e), aviso, estiloCentro);

        if (tutorialAberto && !jogoEncerrado)
            DesenharTutorial(e);
        if (jogoEncerrado)
            DesenharFimDeJogo(e);
    }

    // Fica logo acima do painel de contratacao; PonteiroSobreInterface usa o mesmo retangulo.
    Rect RectPainelMelhoria(float e)
    {
        float altura = 140f * e;
        return new Rect(18f * e, Screen.height - 114f * e - altura - 6f * e, 606f * e, altura);
    }

    void DesenharPainelMelhoria(float e)
    {
        Rect painel = RectPainelMelhoria(e);
        GUI.Box(painel, GUIContent.none, estiloPainel);

        string nome = selecionada.Tipo == TipoTorreGaucha.Revolver ? "GAUCHO REVOLVER" : "GAUCHO ESPINGARDA";
        GUI.Label(new Rect(painel.x + 16f * e, painel.y + 8f * e, painel.width - 32f * e, 36f * e),
            nome + "   Nv " + selecionada.Nivel + "/" + MelhoriasTorre.NivelMaximo, estiloDestaque);
        GUI.Label(new Rect(painel.x + 16f * e, painel.y + 46f * e, painel.width - 32f * e, 32f * e),
            "Dano " + selecionada.Dano + "   Alcance " + selecionada.Alcance.ToString("0.00")
            + "   Tiro a cada " + selecionada.Intervalo.ToString("0.00") + "s", estiloTexto);

        bool podeMelhorar = selecionada.PodeMelhorar;
        string rotulo = podeMelhorar ? "MELHORAR [U]  $" + CustoDaMelhoria(selecionada) : "NIVEL MAXIMO";
        bool habilitadoAntes = GUI.enabled;
        GUI.enabled = podeMelhorar;
        if (GUI.Button(new Rect(painel.x + 16f * e, painel.y + 84f * e, painel.width - 32f * e, 46f * e), rotulo, estiloBotao))
            MelhorarSelecionada();
        GUI.enabled = habilitadoAntes;
    }

    void DesenharNiveisDeTorre()
    {
        GUI.color = new Color(1f, 0.82f, 0.25f, 1f);
        for (int i = 0; i < torres.Count; i++)
        {
            TorreGaucha torre = torres[i];
            if (torre == null || torre.Nivel <= 1)
                continue;
            Vector3 tela = cameraPrincipal.WorldToScreenPoint(torre.transform.position + Vector3.up * 0.75f);
            if (tela.z <= 0f)
                continue;
            int marcas = torre.Nivel - 1;
            float x = tela.x - marcas * 4f;
            float y = Screen.height - tela.y;
            for (int m = 0; m < marcas; m++)
                GUI.DrawTexture(new Rect(x + m * 8f, y, 6f, 6f), Texture2D.whiteTexture);
        }
        GUI.color = Color.white;
    }

    void DesenharTutorial(float e)
    {
        Rect painel = new Rect(22f * e, 108f * e, 430f * e, 270f * e);
        GUI.Box(painel, GUIContent.none, estiloPainel);
        GUI.Label(new Rect(42f * e, 124f * e, 390f * e, 44f * e), "COMO JOGAR", estiloDestaque);
        GUI.Label(new Rect(42f * e, 172f * e, 380f * e, 124f * e),
            "1. Contrate gauchos e coloque na grama.\n2. Inicie a onda.\n3. Os gauchos atiram sozinhos.\n4. Nao deixe os animais chegarem ao galpao.", estiloTexto);
        if (GUI.Button(new Rect(42f * e, 307f * e, 165f * e, 48f * e), "ENTENDI", estiloBotao))
            tutorialAberto = false;
    }

    void DesenharFimDeJogo(float e)
    {
        Color antiga = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.72f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = antiga;

        float largura = 660f * e;
        float altura = 330f * e;
        Rect painel = new Rect(Screen.width * 0.5f - largura * 0.5f, Screen.height * 0.5f - altura * 0.5f, largura, altura);
        GUI.Box(painel, GUIContent.none, estiloPainel);
        GUI.Label(new Rect(painel.x + 30f * e, painel.y + 34f * e, painel.width - 60f * e, 75f * e),
            vitoria ? "VITORIA NO PAMPA!" : "O GALPAO CAIU", estiloTitulo);
        GUI.Label(new Rect(painel.x + 45f * e, painel.y + 120f * e, painel.width - 90f * e, 80f * e),
            vitoria
                ? "Voce segurou todas as ondas e salvou a estancia."
                : "Os animais atravessaram a defesa. Reposicione os gauchos e tente novamente.", estiloCentro);
        if (GUI.Button(new Rect(painel.x + 145f * e, painel.y + 225f * e, painel.width - 290f * e, 66f * e),
            "JOGAR DE NOVO  [R]", vitoria ? estiloBotao : estiloBotaoPerigo))
            Reiniciar();
    }

    void DesenharBarrasDeVida()
    {
        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo == null || !inimigo.EstaVivo || inimigo.VidaNormalizada >= 0.999f)
                continue;
            // O chefe tem barra mais larga e mais alta, por causa da escala maior.
            float largura = inimigo.EhChefe ? 72f : 36f;
            float altura = inimigo.EhChefe ? 1.1f : 0.62f;
            Vector3 tela = cameraPrincipal.WorldToScreenPoint(inimigo.transform.position + Vector3.up * altura);
            if (tela.z <= 0f)
                continue;
            float x = tela.x - largura * 0.5f;
            float y = Screen.height - tela.y;
            GUI.color = new Color(0.12f, 0.08f, 0.07f, 0.9f);
            GUI.DrawTexture(new Rect(x, y, largura, 5f), Texture2D.whiteTexture);
            GUI.color = new Color(0.36f, 0.86f, 0.28f, 1f);
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
        texturaBotao = TexturaSolida(new Color(0.55f, 0.24f, 0.08f, 0.97f));
        texturaBotaoHover = TexturaSolida(new Color(0.78f, 0.39f, 0.10f, 1f));
        texturaPerigo = TexturaSolida(new Color(0.55f, 0.10f, 0.07f, 1f));

        estiloPainel = new GUIStyle(GUI.skin.box) { normal = { background = texturaPainel } };
        estiloTitulo = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(32f * EscalaUi()),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(1f, 0.82f, 0.38f) }
        };
        estiloTexto = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(19f * EscalaUi()),
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,
            normal = { textColor = new Color(1f, 0.94f, 0.82f) }
        };
        estiloDestaque = new GUIStyle(estiloTexto)
        {
            fontSize = Mathf.RoundToInt(23f * EscalaUi()),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        estiloCentro = new GUIStyle(estiloTexto)
        {
            fontSize = Mathf.RoundToInt(20f * EscalaUi()),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        estiloBotao = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.RoundToInt(18f * EscalaUi()),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { background = texturaBotao, textColor = Color.white },
            hover = { background = texturaBotaoHover, textColor = Color.white },
            active = { background = texturaBotaoHover, textColor = Color.white }
        };
        estiloBotaoPerigo = new GUIStyle(estiloBotao);
        estiloBotaoPerigo.normal.background = texturaPerigo;
        AtualizarTamanhoDasFontes();
    }

    void AtualizarTamanhoDasFontes()
    {
        float e = EscalaUi();
        estiloTitulo.fontSize = Mathf.RoundToInt(32f * e);
        estiloTexto.fontSize = Mathf.RoundToInt(19f * e);
        estiloDestaque.fontSize = Mathf.RoundToInt(23f * e);
        estiloCentro.fontSize = Mathf.RoundToInt(20f * e);
        estiloBotao.fontSize = Mathf.RoundToInt(18f * e);
        estiloBotaoPerigo.fontSize = estiloBotao.fontSize;
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

    float EscalaUi()
    {
        return Mathf.Clamp(Screen.height / 1080f, 0.62f, 1.35f);
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
        if (materialParticula != null)
            Destroy(materialParticula);
        if (texturaPainel != null)
            Destroy(texturaPainel);
        if (texturaBotao != null)
            Destroy(texturaBotao);
        if (texturaBotaoHover != null)
            Destroy(texturaBotaoHover);
        if (texturaPerigo != null)
            Destroy(texturaPerigo);
    }
}

static class JogoGauchoBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar()
    {
        // O hook roda uma vez so; reiniciar recarrega a cena e destroi o gerenciador, entao recriamos a cada carga.
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
        if (SceneManager.GetActiveScene().name != "Teste_PixelArt")
            return;
        if (Object.FindFirstObjectByType<JogoGaucho>() != null)
            return;

        GameObject gerenciador = new GameObject("Jogo Gaucho - Gerenciador");
        gerenciador.AddComponent<JogoGaucho>();
    }
}
