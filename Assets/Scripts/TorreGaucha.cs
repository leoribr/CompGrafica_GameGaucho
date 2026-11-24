using System.Collections.Generic;
using UnityEngine;

public sealed class TorreGaucha : MonoBehaviour
{
    static AudioClip somRevolver;
    static AudioClip somEspingarda;
    static AudioClip somArremesso;

    JogoGaucho jogo;
    Transform arma;
    SpriteRenderer armaRenderer;
    SpriteRenderer corpoRenderer;
    SpriteRenderer gineteRenderer;
    Transform cusco;
    SpriteRenderer cuscoRenderer;
    Vector3 cuscoBase;
    Transform pandorga;
    Vector3 pandorgaBase;
    AudioSource audioSource;
    InimigoGaucho alvo;
    readonly List<InimigoGaucho> alvos = new List<InimigoGaucho>();
    readonly int[] niveis = new int[3];
    readonly Dictionary<TipoEspecial, float> proximoEspecial = new Dictionary<TipoEspecial, float>();
    readonly List<SpriteRenderer> seguidores = new List<SpriteRenderer>();
    Sprite[] quadrosCavalo;
    Sprite[] quadrosCusco;
    float proximoDisparo;
    float proximoEfetivo;
    float proximoEfeitoDeAlcance;
    int tirosDaRajada;
    int contadorCritico;
    float anguloCirculo;
    int indicePatrulha = 1;
    int sentidoPatrulha = 1;
    float buffDano;
    float buffAlcance;
    float buffCadencia;
    float buffAte;
    bool buffDaOnda;
    float mordidaAte;
    Vector3 mordidaAlvo;
    float relogioAnimacao;
    int quadroAtual;
    ModoMovimento movimento;

    public DefinicaoTorre Definicao { get; private set; }
    public AtributosTorre Atributos { get; private set; }
    public AtributosTorre Efetivo { get; private set; }
    public Prioridade Prioridade { get; set; }
    public Vector3 Casa { get; set; }
    public int Gasto { get; set; }
    public int Guardado { get; set; }
    public Vector3 AlvoFixo { get; set; }
    public bool Ajudante { get; private set; }
    public float ExpiraEm { get; private set; }
    public TorreGaucha Dono { get; private set; }
    public int Abatidos { get; set; }
    public bool EhPrevia { get; private set; }
    public int BonusPermanente { get; private set; }
    public bool JaFoiAfiada { get; private set; }
    public int AlmasAtivas { get; set; }
    public int ArmadilhasAtivas { get; set; }
    public int AjudantesAtivos { get; set; }
    public float Alcance => Efetivo != null ? Efetivo.alcance : 2f;
    public int[] Niveis => niveis;
    public bool Fortalecida => buffDaOnda || Time.time < buffAte;

    public float FatorDeCadencia
    {
        get
        {
            if (Efetivo == null || Atributos == null || Efetivo.intervalo <= 0f)
                return 1f;
            return Mathf.Max(0.2f, Atributos.intervalo / Efetivo.intervalo);
        }
    }

    public ModoMovimento Movimento
    {
        get { return movimento; }
        set
        {
            movimento = value;
            if (movimento == ModoMovimento.Patrulhar && jogo != null)
                indicePatrulha = jogo.IndiceMaisProximoDoCaminho(transform.position);
        }
    }

    public bool PodeMelhorar(int caminho)
    {
        return MelhoriasTorre.PodeMelhorar(niveis, caminho);
    }

    public void Configurar(JogoGaucho gerenciador, DefinicaoTorre definicao, bool previa)
    {
        jogo = gerenciador;
        Definicao = definicao;
        EhPrevia = previa;
        PrepararVisual();
        Recalcular();
        Movimento = Atributos.movimento;
        Prioridade = Prioridade.Primeiro;
        Casa = transform.position;
        AlvoFixo = jogo.PontoMaisProximoDoCaminho(transform.position);

        if (!previa && audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.09f;
        }
    }

    public void ConfigurarAjudante(JogoGaucho gerenciador, DefinicaoTorre definicao, TorreGaucha dono, AtributosTorre ataque, float duracao)
    {
        Configurar(gerenciador, definicao, false);
        Ajudante = true;
        Dono = dono;
        ExpiraEm = Time.time + duracao;
        Atributos = ataque.Copiar();
        Efetivo = Atributos.Copiar();
    }

    void PrepararVisual()
    {
        corpoRenderer = GetComponent<SpriteRenderer>();
        arma = transform.Find("Arma");
        armaRenderer = arma != null ? arma.GetComponent<SpriteRenderer>() : null;
        if (Definicao.usaModeloDaCena)
            return;

        int ordem = corpoRenderer != null ? corpoRenderer.sortingOrder : 6;
        string corpo = Definicao.corpo;
        if (corpoRenderer != null)
        {
            if (corpo == "gaucho" || corpo == "pescador" || corpo == "domador" || corpo == "pia")
                corpoRenderer.sprite = SpritesGaucho.Gaucho(Definicao.corPoncho, Definicao.corLenco);
            else if (corpo == "cavalo")
            {
                quadrosCavalo = SpritesGaucho.Quadros("cavalo");
                corpoRenderer.sprite = quadrosCavalo[0];
            }
            else
                corpoRenderer.sprite = SpritesGaucho.Pegar(corpo);
            if (corpo == "aviao")
                corpoRenderer.sortingOrder = 25;
        }
        transform.localScale = Vector3.one * Definicao.escalaCorpo;

        if (armaRenderer != null)
        {
            Sprite sprite = string.IsNullOrEmpty(Definicao.arma) ? null : SpritesGaucho.Pegar(Definicao.arma);
            armaRenderer.sprite = sprite;
            armaRenderer.enabled = sprite != null;
        }

        if (corpo == "cavalo")
        {
            gineteRenderer = CriarFilho("Ginete", SpritesGaucho.Gaucho(Definicao.corPoncho, Definicao.corLenco), new Vector3(0.05f, 0.55f, 0f), 0.75f, ordem + 1);
            if (arma != null)
            {
                arma.localPosition = new Vector3(0.25f, 0.5f, 0f);
                armaRenderer.sortingOrder = ordem + 2;
            }
        }
        else if (corpo == "pescador")
            CriarFilho("Canoa", SpritesGaucho.Pegar("canoa"), new Vector3(0f, -0.42f, 0f), 1f, ordem - 1);
        else if (corpo == "domador")
        {
            quadrosCusco = SpritesGaucho.Quadros("cusco");
            cuscoRenderer = CriarFilho("Cusco", quadrosCusco[0], new Vector3(0.62f, -0.32f, 0f), 1f, ordem);
            cusco = cuscoRenderer.transform;
            cuscoBase = cusco.localPosition;
        }
        else if (corpo == "pia")
        {
            pandorga = CriarFilho("Pandorga", SpritesGaucho.Pegar("pandorga"), new Vector3(0.6f, 1f, 0f), 1f, 27).transform;
            pandorgaBase = pandorga.localPosition;
        }
    }

    SpriteRenderer CriarFilho(string nome, Sprite sprite, Vector3 posicaoLocal, float escala, int ordem)
    {
        Transform existente = transform.Find(nome);
        GameObject objeto = existente != null ? existente.gameObject : new GameObject(nome);
        objeto.transform.SetParent(transform, false);
        objeto.transform.localPosition = posicaoLocal;
        objeto.transform.localScale = Vector3.one * escala;
        SpriteRenderer renderizador = objeto.GetComponent<SpriteRenderer>();
        if (renderizador == null)
            renderizador = objeto.AddComponent<SpriteRenderer>();
        renderizador.sprite = sprite;
        renderizador.sortingOrder = ordem;
        return renderizador;
    }

    public void Recalcular()
    {
        AtributosTorre a = Definicao.CriarBasico();
        for (int c = 0; c < 3; c++)
        {
            for (int n = 0; n < niveis[c]; n++)
                Definicao.caminhos[c][n].aplicar(a);
        }
        a.dano += BonusPermanente;
        Atributos = a;
        AtualizarEfetivo();
        AtualizarSeguidores();
    }

    public void Melhorar(int caminho, int custo)
    {
        if (!PodeMelhorar(caminho))
            return;
        niveis[caminho]++;
        Gasto += custo;
        Recalcular();
    }

    void AtualizarEfetivo()
    {
        AtributosTorre e = Atributos.Copiar();
        if (jogo != null && !EhPrevia)
            jogo.AplicarAuras(this, e);
        if (Fortalecida)
        {
            e.dano = Mathf.RoundToInt(e.dano * (1f + buffDano));
            e.alcance *= 1f + buffAlcance;
            e.intervalo /= 1f + buffCadencia;
        }
        if (e.vigiaGalpao && jogo != null && jogo.TemBichoPertoDoGalpao)
            e.intervalo /= e.multiplicadorVigia;
        Efetivo = e;
    }

    public void AplicarBuff(float dano, float alcance, float cadencia, float duracao, bool ateOFimDaOnda)
    {
        if (!Fortalecida)
        {
            buffDano = 0f;
            buffAlcance = 0f;
            buffCadencia = 0f;
        }
        buffDano = Mathf.Max(buffDano, dano);
        buffAlcance = Mathf.Max(buffAlcance, alcance);
        buffCadencia = Mathf.Max(buffCadencia, cadencia);
        if (ateOFimDaOnda)
            buffDaOnda = true;
        else
            buffAte = Mathf.Max(buffAte, Time.time + duracao);
        AtualizarEfetivo();
    }

    public void AfiarParaSempre()
    {
        if (JaFoiAfiada)
            return;
        JaFoiAfiada = true;
        BonusPermanente++;
        Recalcular();
    }

    public void FimDaOnda()
    {
        buffDaOnda = false;
        AtualizarEfetivo();
    }

    public void ComecoDaOnda()
    {
        if (EhPrevia || Ajudante)
            return;
        foreach (Especial especial in Atributos.especiais)
        {
            if (especial.porOnda)
                jogo.UsarEspecial(this, especial);
        }
    }

    void Update()
    {
        if (EhPrevia || jogo == null || !jogo.PodeCombater)
            return;

        if (Ajudante && Time.time >= ExpiraEm)
        {
            if (Dono != null)
                Dono.AjudantesAtivos--;
            Destroy(gameObject);
            return;
        }

        if (Time.time >= proximoEfetivo)
        {
            AtualizarEfetivo();
            proximoEfetivo = Time.time + 0.25f;
        }

        Mover();

        if (Time.time >= proximoEfeitoDeAlcance)
        {
            jogo.AplicarEfeitosDeAlcance(this, Efetivo);
            proximoEfeitoDeAlcance = Time.time + 0.2f;
        }

        Atacar();
        if (!Ajudante)
            RodarEspeciais();
        AnimarExtras();
    }

    void Atacar()
    {
        AtributosTorre e = Efetivo;
        if (e.modo == ModoAtaque.Nenhum)
            return;

        if (Time.time < proximoDisparo)
        {
            if (alvo != null && alvo.EstaVivo)
                MirarEm(alvo.transform.position);
            return;
        }

        if (e.modo == ModoAtaque.PontoFixo)
        {
            if (!jogo.OndaEmAndamento)
                return;
            MirarEm(AlvoFixo);
            int tiros = Mathf.Max(1, e.quantidade);
            for (int i = 0; i < tiros; i++)
            {
                Vector2 erro = Random.insideUnitCircle * e.imprecisao;
                jogo.CriarProjetilAoPonto(this, e, DanoDoTiro(e), BocaDaArma(), AlvoFixo + (Vector3)erro);
            }
            TocarSom(e);
            Agendar(e);
            return;
        }

        if (Prioridade == Prioridade.Mouse && e.miraNoMouse)
        {
            if (!jogo.OndaEmAndamento)
                return;
            Vector3 mouse = jogo.PosicaoDoMouse();
            MirarEm(mouse);
            DispararProjeteis(e, null, mouse);
            TocarSom(e);
            Agendar(e);
            return;
        }

        if (e.modo == ModoAtaque.Pulso || e.modo == ModoAtaque.Radial)
        {
            if (jogo.EncontrarAlvo(transform.position, e.alcance, Prioridade.MaisPerto, e) == null)
                return;
            if (e.modo == ModoAtaque.Pulso)
                jogo.AfetarArea(transform.position, e.alcance, e, DanoDoTiro(e), this, JogoGaucho.CorDoTipo(e.tipoDano), true);
            else
                DispararRadial(e);
            TocarSom(e);
            Agendar(e);
            return;
        }

        float alcance = e.modo == ModoAtaque.Instantaneo && e.alcance > 50f ? 999f : e.alcance;
        int quantos = e.modo == ModoAtaque.CorpoACorpo ? Mathf.Max(1, e.quantidade) : Mathf.Max(1, e.alvos);
        int achados = jogo.EncontrarAlvos(alvos, transform.position, alcance, Prioridade, e, quantos);
        if (achados == 0)
        {
            alvo = null;
            return;
        }

        alvo = alvos[0];
        MirarEm(alvo.transform.position);

        if (e.modo == ModoAtaque.Instantaneo)
        {
            for (int i = 0; i < achados; i++)
                DispararInstantaneo(e, alvos[i]);
        }
        else if (e.modo == ModoAtaque.CorpoACorpo)
            GolpearCorpoACorpo(e, achados);
        else if (e.modo == ModoAtaque.Bumerangue)
        {
            for (int i = 0; i < achados; i++)
                ArremessarBumerangue(e, alvos[i].transform.position);
        }
        else
        {
            for (int i = 0; i < achados; i++)
                DispararProjeteis(e, alvos[i], alvos[i].transform.position);
        }
        TocarSom(e);
        Agendar(e);
    }

    void Agendar(AtributosTorre e)
    {
        if (e.rajada > 1)
        {
            tirosDaRajada++;
            if (tirosDaRajada < e.rajada)
            {
                proximoDisparo = Time.time + 0.08f;
                return;
            }
            tirosDaRajada = 0;
        }
        proximoDisparo = Time.time + e.intervalo;
    }

    int DanoDoTiro(AtributosTorre e)
    {
        contadorCritico++;
        if (e.criticoACada > 0 && contadorCritico % e.criticoACada == 0)
            return e.dano * Mathf.Max(1, e.multiplicadorCritico);
        return e.dano;
    }

    void DispararProjeteis(AtributosTorre e, InimigoGaucho principal, Vector3 pontoMira)
    {
        int dano = DanoDoTiro(e);
        if (e.rolaPeloCaminho)
        {
            jogo.CriarProjetilNoCaminho(this, e, dano, transform.position);
            return;
        }

        Vector3 boca = BocaDaArma();
        Vector2 direcao = ((Vector2)(pontoMira - boca)).normalized;
        if (direcao == Vector2.zero)
            direcao = Vector2.right;
        Vector2 lado = new Vector2(-direcao.y, direcao.x);
        float alcanceDoTiro = principal == null ? 25f : e.alcance * 1.6f + 0.5f;
        int quantidade = Mathf.Max(1, e.quantidade);

        for (int i = 0; i < quantidade; i++)
        {
            float posicaoNoLeque = i - (quantidade - 1) * 0.5f;
            float desvio = posicaoNoLeque * e.abertura;
            bool central = Mathf.Abs(desvio) < 0.01f;
            int danoDeste = e.lateraisMetade && !central ? Mathf.Max(1, dano / 2) : dano;
            Vector2 dir = Quaternion.Euler(0f, 0f, desvio) * direcao;
            Vector3 saida = boca;
            if (e.abertura <= 0.01f && quantidade > 1)
                saida += (Vector3)(lado * (posicaoNoLeque * 0.14f));
            InimigoGaucho guiado = principal != null && (central || e.teleguiado) ? principal : null;
            jogo.CriarProjetil(this, e, danoDeste, saida, dir, guiado, alcanceDoTiro);
        }

        if (e.tiroTraseiro)
            jogo.CriarProjetil(this, e, dano, boca, -direcao, null, alcanceDoTiro);
    }

    void DispararRadial(AtributosTorre e)
    {
        int quantidade = Mathf.Max(1, e.quantidade);
        int dano = DanoDoTiro(e);
        InimigoGaucho guiado = e.teleguiado ? jogo.EncontrarAlvo(transform.position, e.alcance * 1.5f, Prioridade.MaisPerto, e) : null;
        for (int i = 0; i < quantidade; i++)
        {
            float angulo = i * Mathf.PI * 2f / quantidade;
            Vector2 direcao = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo));
            jogo.CriarProjetil(this, e, dano, transform.position, direcao, guiado, e.alcance);
        }
    }

    void DispararInstantaneo(AtributosTorre e, InimigoGaucho atingido)
    {
        if (atingido == null || !atingido.EstaVivo)
            return;
        Vector3 ponto = atingido.transform.position;
        jogo.Rastro(BocaDaArma(), ponto, new Color(1f, 0.95f, 0.6f, 0.9f));
        int dano = DanoDoTiro(e);
        jogo.AplicarAcerto(atingido, e, dano, this);
        if (e.estilhacos > 0)
            jogo.SoltarEstilhacos(ponto, e, dano, this, null);
    }

    void GolpearCorpoACorpo(AtributosTorre e, int achados)
    {
        int dano = DanoDoTiro(e);
        Vector3 boca = BocaDaArma();
        for (int i = 0; i < achados; i++)
        {
            InimigoGaucho atingido = alvos[i];
            if (atingido == null || !atingido.EstaVivo)
                continue;
            Vector3 ponto = atingido.transform.position;
            if (i == 0)
                mordidaAlvo = ponto;
            jogo.Golpe(boca, ponto);
            jogo.AplicarAcerto(atingido, e, dano, this);
        }
        mordidaAte = Time.time + 0.16f;
    }

    void ArremessarBumerangue(AtributosTorre e, Vector3 ponto)
    {
        int quantidade = Mathf.Max(1, e.quantidade);
        Vector3 boca = BocaDaArma();
        Vector3 ate = ponto - transform.position;
        for (int i = 0; i < quantidade; i++)
        {
            float desvio = (i - (quantidade - 1) * 0.5f) * e.abertura;
            Vector3 destino = transform.position + Quaternion.Euler(0f, 0f, desvio) * ate;
            jogo.CriarBumerangue(this, e, DanoDoTiro(e), boca, destino);
        }
    }

    void RodarEspeciais()
    {
        List<Especial> lista = Efetivo.especiais;
        for (int i = 0; i < lista.Count; i++)
        {
            Especial especial = lista[i];
            if (especial.porOnda || especial.tipo == TipoEspecial.Almas)
                continue;

            float proximo;
            if (!proximoEspecial.TryGetValue(especial.tipo, out proximo))
            {
                proximoEspecial[especial.tipo] = Time.time + especial.intervalo * 0.4f;
                continue;
            }
            if (Time.time < proximo || !jogo.OndaEmAndamento)
                continue;

            bool usou = jogo.UsarEspecial(this, especial);
            proximoEspecial[especial.tipo] = Time.time + (usou ? especial.intervalo / FatorDeCadencia : 0.35f);
        }
    }

    void Mover()
    {
        AtributosTorre e = Efetivo;
        Vector3 atual = transform.position;
        float velocidade = e.velocidadeMovimento;

        if (Movimento == ModoMovimento.Circular)
        {
            float raio = Mathf.Max(0.5f, e.raioCirculo);
            anguloCirculo += velocidade / raio * Time.deltaTime;
            Vector3 novo = Casa + new Vector3(Mathf.Cos(anguloCirculo), Mathf.Sin(anguloCirculo), 0f) * raio;
            Vector3 tangente = novo - atual;
            transform.position = novo;
            if (tangente.sqrMagnitude > 0.000001f)
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(tangente.y, tangente.x) * Mathf.Rad2Deg);
            return;
        }

        if (Atributos.movimento == ModoMovimento.Parado)
            return;

        Vector3 destino = Casa;
        if (Movimento == ModoMovimento.Seguir)
        {
            InimigoGaucho perseguido = jogo.EncontrarAlvo(atual, 99f, Prioridade == Prioridade.Mouse ? Prioridade.Primeiro : Prioridade, e);
            if (perseguido != null)
            {
                Vector3 ponto = perseguido.transform.position;
                destino = Vector2.Distance(atual, ponto) > Mathf.Max(0.3f, e.alcance * 0.6f) ? ponto : atual;
            }
        }
        else if (Movimento == ModoMovimento.Patrulhar)
        {
            Vector3 ponto = jogo.PontoDoCaminho(indicePatrulha);
            if (Vector2.Distance(atual, ponto) < 0.1f)
            {
                int ultimo = jogo.QuantidadeDePontos - 1;
                if (indicePatrulha + sentidoPatrulha > ultimo || indicePatrulha + sentidoPatrulha < 1)
                    sentidoPatrulha = -sentidoPatrulha;
                indicePatrulha = Mathf.Clamp(indicePatrulha + sentidoPatrulha, 1, ultimo);
            }
            destino = jogo.PontoDoCaminho(indicePatrulha);
        }

        Vector3 proximo = Vector3.MoveTowards(atual, destino, velocidade * Time.deltaTime);
        float dx = proximo.x - atual.x;
        transform.position = proximo;
        bool andando = (proximo - atual).sqrMagnitude > 0.0000001f;
        if (andando && Mathf.Abs(dx) > 0.0001f && corpoRenderer != null)
        {
            corpoRenderer.flipX = dx < 0f;
            if (gineteRenderer != null)
                gineteRenderer.flipX = dx < 0f;
        }

        if (quadrosCavalo != null && andando)
        {
            relogioAnimacao += Time.deltaTime;
            if (relogioAnimacao >= 0.12f)
            {
                relogioAnimacao = 0f;
                quadroAtual = (quadroAtual + 1) % quadrosCavalo.Length;
                corpoRenderer.sprite = quadrosCavalo[quadroAtual];
            }
        }
    }

    void AnimarExtras()
    {
        if (pandorga != null)
            pandorga.localPosition = pandorgaBase + new Vector3(Mathf.Sin(Time.time * 2f) * 0.1f, Mathf.Cos(Time.time * 1.6f) * 0.07f, 0f);

        if (cusco != null)
        {
            if (Time.time < mordidaAte)
            {
                Vector3 local = transform.InverseTransformPoint(mordidaAlvo);
                cusco.localPosition = Vector3.Lerp(cuscoBase, local, 0.6f);
                cuscoRenderer.flipX = local.x < cuscoBase.x;
                cuscoRenderer.sprite = quadrosCusco[1];
            }
            else
            {
                cusco.localPosition = cuscoBase;
                cuscoRenderer.sprite = quadrosCusco[0];
            }
        }

        for (int i = 0; i < seguidores.Count; i++)
        {
            Transform t = seguidores[i].transform;
            Vector3 p = t.localPosition;
            p.y = SeguidorY(i) + Mathf.Abs(Mathf.Sin(Time.time * 6f + i)) * 0.05f;
            t.localPosition = p;
        }
    }

    float SeguidorY(int i)
    {
        return -0.35f - 0.18f * (i % 2);
    }

    void AtualizarSeguidores()
    {
        if (EhPrevia || Atributos == null)
            return;
        int quer = Atributos.ajudantes;
        while (seguidores.Count < quer)
        {
            int i = seguidores.Count;
            bool cavalo = Definicao.corpo == "cavalo";
            Sprite sprite = cavalo ? SpritesGaucho.Quadros("cavalo")[0] : SpritesGaucho.Quadros("cusco")[0];
            SpriteRenderer sr = CriarFilho("Seguidor" + i, sprite, new Vector3(-0.55f - 0.4f * (i / 2), SeguidorY(i), 0f), cavalo ? 0.55f : 0.9f, corpoRenderer != null ? corpoRenderer.sortingOrder - 1 : 5);
            seguidores.Add(sr);
        }
    }

    void MirarEm(Vector3 ponto)
    {
        if (arma == null || armaRenderer == null || !armaRenderer.enabled || Movimento == ModoMovimento.Circular)
            return;
        Vector2 direcao = ponto - arma.position;
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        arma.rotation = Quaternion.Euler(0f, 0f, angulo);
        armaRenderer.flipY = direcao.x < 0f;
    }

    public Vector3 BocaDaArma()
    {
        if (arma != null && armaRenderer != null && armaRenderer.enabled)
            return arma.position + arma.right * Definicao.comprimentoArma;
        return transform.position;
    }

    void TocarSom(AtributosTorre e)
    {
        if (audioSource == null)
            return;
        AudioClip clip;
        if (e.tipoDano == TipoDano.Bala)
            clip = Definicao.nome.Contains("Espingarda") || e.quantidade >= 3 ? ObterSomEspingarda() : ObterSomRevolver();
        else
            clip = ObterSomArremesso();
        audioSource.pitch = Random.Range(0.92f, 1.08f);
        audioSource.PlayOneShot(clip);
    }

    static AudioClip ObterSomRevolver()
    {
        if (somRevolver == null)
            somRevolver = CriarSom("Tiro de revolver", 0.075f, 240f, 0.42f);
        return somRevolver;
    }

    static AudioClip ObterSomEspingarda()
    {
        if (somEspingarda == null)
            somEspingarda = CriarSom("Tiro de espingarda", 0.13f, 125f, 0.72f);
        return somEspingarda;
    }

    static AudioClip ObterSomArremesso()
    {
        if (somArremesso == null)
            somArremesso = CriarSom("Arremesso", 0.06f, 520f, 0.85f);
        return somArremesso;
    }

    static AudioClip CriarSom(string nome, float duracao, float frequencia, float ruido)
    {
        const int taxa = 22050;
        int amostras = Mathf.CeilToInt(taxa * duracao);
        float[] dados = new float[amostras];
        for (int i = 0; i < amostras; i++)
        {
            float t = (float)i / taxa;
            float envelope = Mathf.Pow(1f - (float)i / amostras, 3f);
            float tom = Mathf.Sin(2f * Mathf.PI * frequencia * t) * (1f - ruido);
            float estalo = Random.Range(-1f, 1f) * ruido;
            dados[i] = (tom + estalo) * envelope * 0.48f;
        }
        AudioClip clip = AudioClip.Create(nome, amostras, 1, taxa, false);
        clip.SetData(dados, 0);
        return clip;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.76f, 0.18f, 0.75f);
        Gizmos.DrawWireSphere(transform.position, Alcance);
    }
}
