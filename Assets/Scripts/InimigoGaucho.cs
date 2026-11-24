using UnityEngine;

public sealed class InimigoGaucho : MonoBehaviour
{
    const float DistanciaMaximaAlemDoCaminho = 3f;

    JogoGaucho jogo;
    Transform[] pontos;
    Vector2 direcaoFinal = Vector2.right;
    float distanciaAlemDoCaminho;
    Sprite[] quadros;
    SpriteRenderer renderizador;
    int vida;
    int vidaMaxima;
    int proximoPonto;
    int quadroAtual;
    float velocidade;
    float relogioAnimacao;
    float flashAte;
    Color corBase = Color.white;
    bool finalizado;

    float lentidao;
    float lentidaoAte;
    float paradoAte;
    bool congelado;
    int queimaDano;
    float queimaAte;
    float queimaResto;
    float proximaQueima;
    TipoDano tipoQueima;
    TorreGaucha origemQueima;
    float marca;
    float marcaAte;
    float ultimoDano = -10f;
    float proximaRegeneracao;
    int danoAcumulado;
    float proximoSoltar;
    float proximoAvisoImune;

    public DefinicaoInimigo Definicao { get; private set; }
    public bool Furtivo { get; private set; }
    public bool Teimoso { get; private set; }
    public bool CascaGrossa { get; private set; }
    public int BonusPilas { get; set; }
    public int ExplodeAoCair { get; set; }
    public bool Contagia { get; set; }
    public bool EstaVivo => !finalizado && vida > 0;
    public float VidaNormalizada => vidaMaxima > 0 ? Mathf.Clamp01((float)vida / vidaMaxima) : 0f;
    public int Vida => vida;
    public int VidaMaxima => vidaMaxima;
    public float ProgressoCaminho { get; private set; }
    public int ProximoPonto => proximoPonto;
    public bool Grande => Definicao.Grande;
    public int Peso => Definicao.peso;
    public bool EhChefe => Definicao.chefe;
    public float AlturaDoSprite => renderizador != null ? renderizador.bounds.extents.y : 0.4f;
    public float LentidaoAtual => Time.time < lentidaoAte ? lentidao : 0f;
    public float LentidaoRestante => Mathf.Max(0f, lentidaoAte - Time.time);
    public int QueimaAtual => Time.time < queimaAte ? queimaDano : 0;
    public float QueimaRestante => Mathf.Max(0f, queimaAte - Time.time);
    public TipoDano TipoQueima => tipoQueima;
    public float MarcaAtual => Time.time < marcaAte ? marca : 0f;

    public void Configurar(JogoGaucho gerenciador, Transform[] caminho, DefinicaoInimigo definicao, int novaVida, float novaVelocidade,
        Sprite[] novosQuadros, bool furtivo, bool teimoso, bool cascaGrossa, Vector3 posicao, int pontoSeguinte)
    {
        jogo = gerenciador;
        pontos = caminho;
        Definicao = definicao;
        Furtivo = furtivo;
        Teimoso = teimoso;
        CascaGrossa = cascaGrossa;
        vidaMaxima = Mathf.Max(1, novaVida);
        vida = vidaMaxima;
        velocidade = Mathf.Max(0.05f, novaVelocidade);
        quadros = novosQuadros;
        renderizador = GetComponent<SpriteRenderer>();
        proximoPonto = Mathf.Clamp(pontoSeguinte, 1, pontos.Length);
        quadroAtual = 0;
        finalizado = false;
        distanciaAlemDoCaminho = 0f;
        transform.position = posicao;
        proximoSoltar = Time.time + definicao.intervaloSoltar;

        corBase = definicao.cor;
        if (cascaGrossa)
            corBase *= new Color(0.72f, 0.72f, 0.72f, 1f);
        if (teimoso)
            corBase *= new Color(0.85f, 1f, 0.85f, 1f);
        if (renderizador != null && quadros != null && quadros.Length > 0 && quadros[0] != null)
            renderizador.sprite = quadros[0];
        AtualizarProgresso();
        AtualizarCor();
        PrepararColisao();
    }

    void PrepararColisao()
    {
        if (GetComponent<Collider2D>() == null)
            gameObject.AddComponent<CircleCollider2D>().radius = 0.4f;

        Rigidbody2D corpo = GetComponent<Rigidbody2D>();
        if (corpo == null)
            corpo = gameObject.AddComponent<Rigidbody2D>();
        corpo.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (finalizado || jogo == null || !jogo.PodeCombater)
            return;

        AtualizarEstados();
        if (!EstaVivo)
            return;

        float passo = VelocidadeAtual() * Time.deltaTime;
        if (passo > 0f)
            Andar(passo);
        if (finalizado)
            return;

        Animar();
        AtualizarCor();
    }

    void AtualizarEstados()
    {
        float agora = Time.time;
        if (queimaDano > 0 && agora < queimaAte && agora >= proximaQueima)
        {
            proximaQueima = agora + 0.5f;
            queimaResto += queimaDano * 0.5f;
            int dano = Mathf.FloorToInt(queimaResto);
            if (dano > 0)
            {
                queimaResto -= dano;
                ReceberDano(dano, origemQueima);
                if (!EstaVivo)
                    return;
            }
        }

        if (Teimoso && vida < vidaMaxima && agora - ultimoDano > 2f && agora >= proximaRegeneracao)
        {
            vida++;
            proximaRegeneracao = agora + 1f;
        }

        if (Definicao.soltaAoAndar != null && agora >= proximoSoltar)
        {
            proximoSoltar = agora + Definicao.intervaloSoltar;
            jogo.SoltarFilhote(this, Definicao.soltaAoAndar);
        }
    }

    float VelocidadeAtual()
    {
        if (Time.time < paradoAte)
            return 0f;
        if (Definicao.imuneLentidao)
            return velocidade;

        float valor = Mathf.Max(LentidaoAtual, jogo.LentidaoGlobal);
        if (Grande)
            valor *= 0.5f;
        return velocidade * (1f - Mathf.Clamp(valor, 0f, 0.9f));
    }

    void Andar(float passo)
    {
        int voltas = 0;
        while (passo > 0f && voltas < 8)
        {
            voltas++;
            if (proximoPonto >= pontos.Length)
            {
                AvancarAlemDoCaminho(passo);
                return;
            }

            Vector3 antes = transform.position;
            Vector3 alvo = pontos[proximoPonto].position;
            float distancia = Vector2.Distance(antes, alvo);
            if (distancia > passo)
            {
                transform.position = Vector3.MoveTowards(antes, alvo, passo);
                passo = 0f;
            }
            else
            {
                transform.position = alvo;
                passo -= distancia;
                proximoPonto++;
                if (proximoPonto >= pontos.Length)
                {
                    direcaoFinal = ((Vector2)(alvo - pontos[proximoPonto - 2].position)).normalized;
                    if (direcaoFinal == Vector2.zero)
                        direcaoFinal = Vector2.right;
                }
            }

            float deslocamentoX = transform.position.x - antes.x;
            if (renderizador != null && Mathf.Abs(deslocamentoX) > 0.0001f)
                renderizador.flipX = deslocamentoX < 0f;
        }
        AtualizarProgresso();
    }

    void AvancarAlemDoCaminho(float passo)
    {
        transform.position += (Vector3)(direcaoFinal * passo);
        distanciaAlemDoCaminho += passo;
        ProgressoCaminho = pontos.Length - 1 + distanciaAlemDoCaminho * 0.1f;

        if (distanciaAlemDoCaminho > DistanciaMaximaAlemDoCaminho)
            ChegarAoGalpao();
    }

    void AtualizarProgresso()
    {
        if (proximoPonto >= pontos.Length)
        {
            ProgressoCaminho = pontos.Length - 1 + distanciaAlemDoCaminho * 0.1f;
            return;
        }
        Vector3 inicio = pontos[proximoPonto - 1].position;
        float comprimento = Vector2.Distance(inicio, pontos[proximoPonto].position);
        float percorrido = comprimento > 0.001f ? Vector2.Distance(inicio, transform.position) / comprimento : 1f;
        ProgressoCaminho = proximoPonto - 1 + Mathf.Clamp01(percorrido);
    }

    public void ChegarAoGalpao()
    {
        if (!EstaVivo)
            return;
        finalizado = true;
        jogo.InimigoChegouAoGalpao(this, Definicao.danoNaBase, transform.position);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (jogo != null)
            jogo.DescartarInimigo(this);
    }

    void Animar()
    {
        if (renderizador == null || quadros == null || quadros.Length < 2)
            return;
        if (Time.time < paradoAte)
            return;
        relogioAnimacao += Time.deltaTime;
        if (relogioAnimacao < 0.15f)
            return;
        relogioAnimacao -= 0.15f;
        quadroAtual = (quadroAtual + 1) % quadros.Length;
        if (quadros[quadroAtual] != null)
            renderizador.sprite = quadros[quadroAtual];
    }

    void AtualizarCor()
    {
        if (renderizador == null)
            return;

        Color cor = corBase;
        float agora = Time.time;
        if (agora < flashAte)
            cor = new Color(1f, 0.48f, 0.38f);
        else if (agora < paradoAte)
            cor *= congelado ? new Color(0.6f, 0.85f, 1f) : new Color(1f, 1f, 0.55f);
        else if (agora < queimaAte && queimaDano > 0)
            cor = Color.Lerp(cor, new Color(1f, 0.55f, 0.2f), 0.35f + Mathf.PingPong(agora * 4f, 0.3f));
        else if (LentidaoAtual > 0f)
            cor *= new Color(0.85f, 0.85f, 1f);

        cor.a = Furtivo ? 0.42f : 1f;
        renderizador.color = cor;
    }

    public bool PodeAvisarImune()
    {
        if (Time.time < proximoAvisoImune)
            return false;
        proximoAvisoImune = Time.time + 0.8f;
        return true;
    }

    public void ReceberDano(int dano, TorreGaucha origem)
    {
        if (!EstaVivo || dano <= 0)
            return;

        vida -= dano;
        ultimoDano = Time.time;
        flashAte = Time.time + 0.06f;

        if (Definicao.soltaAoApanhar != null && vida > 0)
        {
            danoAcumulado += dano;
            int soltos = 0;
            while (danoAcumulado >= Definicao.danoParaSoltar && soltos < 5)
            {
                danoAcumulado -= Definicao.danoParaSoltar;
                soltos++;
                jogo.SoltarFilhote(this, Definicao.soltaAoApanhar);
            }
        }

        if (vida > 0)
            return;
        finalizado = true;
        jogo.InimigoEliminado(this, origem);
        Destroy(gameObject);
    }

    public void Matar(TorreGaucha origem)
    {
        ReceberDano(Mathf.Max(1, vida), origem);
    }

    public void AplicarLentidao(float valor, float duracao)
    {
        if (Definicao.imuneLentidao || valor <= 0f)
            return;
        if (Time.time >= lentidaoAte || valor >= lentidao)
            lentidao = valor;
        lentidaoAte = Mathf.Max(lentidaoAte, Time.time + duracao);
    }

    public void Parar(float duracao, bool porFrio)
    {
        if (duracao <= 0f || Definicao.imuneLentidao)
            return;
        if (Grande)
            duracao *= 0.5f;
        if (Time.time + duracao > paradoAte)
        {
            paradoAte = Time.time + duracao;
            congelado = porFrio;
        }
    }

    public void Queimar(int dps, float duracao, TipoDano tipo, TorreGaucha origem)
    {
        if (dps <= 0)
            return;
        if (Time.time >= queimaAte || dps >= queimaDano)
        {
            queimaDano = dps;
            tipoQueima = tipo;
            origemQueima = origem;
        }
        queimaAte = Mathf.Max(queimaAte, Time.time + duracao);
    }

    public void Marcar(float valor, float duracao)
    {
        if (Time.time >= marcaAte || valor >= marca)
            marca = valor;
        marcaAte = Mathf.Max(marcaAte, Time.time + duracao);
    }

    public void Revelar()
    {
        Furtivo = false;
    }

    public void Recuar(float distancia)
    {
        if (!EstaVivo || distancia <= 0f || Definicao.imuneLentidao)
            return;

        if (proximoPonto >= pontos.Length)
        {
            proximoPonto = pontos.Length - 1;
            distanciaAlemDoCaminho = 0f;
        }

        float resta = distancia;
        int voltas = 0;
        while (resta > 0f && voltas < 10)
        {
            voltas++;
            Vector3 anterior = pontos[proximoPonto - 1].position;
            float ate = Vector2.Distance(transform.position, anterior);
            if (ate >= resta)
            {
                transform.position = Vector3.MoveTowards(transform.position, anterior, resta);
                resta = 0f;
            }
            else
            {
                transform.position = anterior;
                resta -= ate;
                if (proximoPonto <= 1)
                    break;
                proximoPonto--;
            }
        }
        AtualizarProgresso();
    }

    public void VoltarAoComeco()
    {
        if (!EstaVivo)
            return;
        transform.position = pontos[0].position;
        proximoPonto = 1;
        distanciaAlemDoCaminho = 0f;
        AtualizarProgresso();
    }
}
