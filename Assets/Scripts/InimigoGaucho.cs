using UnityEngine;

/// <summary>Vida, animacao e movimento de um animal pelo caminho.</summary>
public sealed class InimigoGaucho : MonoBehaviour
{
    const float DistanciaMaximaAlemDoCaminho = 3f;

    JogoGaucho jogo;
    Transform[] pontos;
    Vector2 direcaoFinal;
    float distanciaAlemDoCaminho;
    Sprite[] quadros;
    SpriteRenderer renderizador;
    int vida;
    int vidaMaxima;
    int recompensa;
    int danoNaBase;
    int proximoPonto;
    int quadroAtual;
    float velocidade;
    float relogioAnimacao;
    float flashAte;
    Color corBase = Color.white;
    bool finalizado;

    public bool EstaVivo => !finalizado && vida > 0;
    public float VidaNormalizada => vidaMaxima > 0 ? Mathf.Clamp01((float)vida / vidaMaxima) : 0f;
    public float ProgressoCaminho { get; private set; }
    public bool EhChefe { get; private set; }

    public void Configurar(
        JogoGaucho gerenciador,
        Transform[] caminho,
        int novaVida,
        float novaVelocidade,
        int novaRecompensa,
        int novoDanoNaBase,
        Sprite[] novosQuadros,
        Color novaCorBase,
        bool ehChefe)
    {
        EhChefe = ehChefe;
        jogo = gerenciador;
        pontos = caminho;
        vidaMaxima = Mathf.Max(1, novaVida);
        vida = vidaMaxima;
        velocidade = Mathf.Max(0.1f, novaVelocidade);
        recompensa = Mathf.Max(0, novaRecompensa);
        danoNaBase = Mathf.Max(1, novoDanoNaBase);
        quadros = novosQuadros;
        renderizador = GetComponent<SpriteRenderer>();
        corBase = novaCorBase;
        if (renderizador != null)
            renderizador.color = corBase;
        proximoPonto = 1;
        quadroAtual = 0;
        finalizado = false;
        ProgressoCaminho = 0f;
        distanciaAlemDoCaminho = 0f;

        if (pontos != null && pontos.Length > 0 && pontos[0] != null)
            transform.position = pontos[0].position;
        if (renderizador != null && quadros != null && quadros.Length > 0 && quadros[0] != null)
            renderizador.sprite = quadros[0];
        PrepararColisao();
    }

    // Colisor + corpo kinematic: o movimento continua pelo Transform, mas os projeteis detectam o impacto.
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
        if (pontos != null && proximoPonto >= pontos.Length && pontos.Length >= 2)
        {
            AvancarAlemDoCaminho();
            return;
        }
        if (pontos == null || pontos.Length < 2 || proximoPonto >= pontos.Length || pontos[proximoPonto] == null)
        {
            Debug.LogWarning("Inimigo sem caminho valido: " + name);
            finalizado = true;
            jogo.DescartarInimigo(this);
            Destroy(gameObject);
            return;
        }

        Vector3 antes = transform.position;
        Vector3 alvo = pontos[proximoPonto].position;
        transform.position = Vector3.MoveTowards(antes, alvo, velocidade * Time.deltaTime);

        float deslocamentoX = transform.position.x - antes.x;
        if (renderizador != null && Mathf.Abs(deslocamentoX) > 0.0001f)
            renderizador.flipX = deslocamentoX < 0f;

        Vector3 inicioDoTrecho = pontos[proximoPonto - 1].position;
        float comprimento = Vector2.Distance(inicioDoTrecho, alvo);
        float percorrido = comprimento > 0.001f
            ? Vector2.Distance(inicioDoTrecho, transform.position) / comprimento
            : 1f;
        ProgressoCaminho = (proximoPonto - 1) + Mathf.Clamp01(percorrido);

        if ((transform.position - alvo).sqrMagnitude < 0.0001f)
        {
            proximoPonto++;
            if (proximoPonto >= pontos.Length)
            {
                // Fim do caminho: segue em frente ate o trigger do galpao detectar a chegada.
                direcaoFinal = ((Vector2)(alvo - inicioDoTrecho)).normalized;
                if (direcaoFinal == Vector2.zero)
                    direcaoFinal = Vector2.right;
            }
        }

        Animar();
        AtualizarFlash();
    }

    void AvancarAlemDoCaminho()
    {
        float passo = velocidade * Time.deltaTime;
        transform.position += (Vector3)(direcaoFinal * passo);
        distanciaAlemDoCaminho += passo;

        // Seguranca: se nenhum galpao com collider for tocado, a onda nao pode travar.
        if (distanciaAlemDoCaminho > DistanciaMaximaAlemDoCaminho)
        {
            Debug.LogWarning("Game Gaucho: " + name + " passou do caminho sem tocar no galpao.");
            ChegarAoGalpao();
            return;
        }

        Animar();
        AtualizarFlash();
    }

    /// <summary>Chamado pelo GalpaoGaucho quando o collider do animal entra no trigger da base.</summary>
    public void ChegarAoGalpao()
    {
        if (!EstaVivo)
            return;
        finalizado = true;
        jogo.InimigoChegouAoGalpao(this, danoNaBase, transform.position);
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Garante que a onda possa terminar mesmo se o inimigo for destruido por fora do fluxo normal.
        if (jogo != null)
            jogo.DescartarInimigo(this);
    }

    void Animar()
    {
        if (renderizador == null || quadros == null || quadros.Length < 2)
            return;
        relogioAnimacao += Time.deltaTime;
        if (relogioAnimacao < 0.15f)
            return;
        relogioAnimacao -= 0.15f;
        quadroAtual = (quadroAtual + 1) % quadros.Length;
        if (quadros[quadroAtual] != null)
            renderizador.sprite = quadros[quadroAtual];
    }

    void AtualizarFlash()
    {
        if (renderizador != null && flashAte > 0f && Time.time >= flashAte)
        {
            renderizador.color = corBase;
            flashAte = 0f;
        }
    }

    public void ReceberDano(int dano)
    {
        if (!EstaVivo)
            return;
        vida -= Mathf.Max(1, dano);
        if (renderizador != null)
        {
            renderizador.color = new Color(1f, 0.48f, 0.38f);
            flashAte = Time.time + 0.07f;
        }

        if (vida > 0)
            return;
        finalizado = true;
        jogo.InimigoEliminado(this, recompensa, transform.position);
        Destroy(gameObject);
    }
}
