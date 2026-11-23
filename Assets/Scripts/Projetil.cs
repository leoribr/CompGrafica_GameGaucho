using UnityEngine;

/// <summary>
/// Bala (revolver) ou chumbo (espingarda) disparado pelo gaucho: voa em direcao ao alvo e causa
/// dano ao colidir com um inimigo. Se o alvo morrer no caminho, segue reto ate acertar outro
/// animal, sair da tela ou acabar o tempo de vida.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class Projetil : MonoBehaviour
{
    public float velocidade = 12f;
    [Tooltip("Segundos ate a bala sumir, caso nao acerte nada")]
    public float tempoDeVida = 2f;

    JogoGaucho jogo;
    InimigoGaucho alvo;
    Rigidbody2D corpo;
    Camera cameraPrincipal;
    Vector2 direcao = Vector2.right;
    int dano;
    bool acertou;

    void Awake()
    {
        corpo = GetComponent<Rigidbody2D>();
        cameraPrincipal = Camera.main;
    }

    void Start()
    {
        Destroy(gameObject, tempoDeVida);
    }

    public void Configurar(JogoGaucho gerenciador, InimigoGaucho novoAlvo, int novoDano, float desvioEmGraus = 0f)
    {
        jogo = gerenciador;
        dano = novoDano;
        if (novoAlvo != null)
            direcao = ((Vector2)(novoAlvo.transform.position - transform.position)).normalized;

        // Com desvio (chumbo lateral da espingarda) o projetil nao persegue: segue reto na direcao desviada.
        if (desvioEmGraus != 0f)
            direcao = Quaternion.Euler(0f, 0f, desvioEmGraus) * direcao;
        else
            alvo = novoAlvo;
    }

    void FixedUpdate()
    {
        // Persegue o alvo enquanto ele estiver vivo; depois mantem a ultima direcao.
        if (alvo != null && alvo.EstaVivo)
        {
            Vector2 paraOAlvo = alvo.transform.position - transform.position;
            if (paraOAlvo.sqrMagnitude > 0.0001f)
                direcao = paraOAlvo.normalized;
        }

        corpo.linearVelocity = direcao * velocidade;
        corpo.MoveRotation(Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg);
    }

    void Update()
    {
        if ((jogo != null && !jogo.PodeCombater) || SaiuDaTela())
            Destroy(gameObject);
    }

    bool SaiuDaTela()
    {
        if (cameraPrincipal == null)
            return false;
        Vector3 viewport = cameraPrincipal.WorldToViewportPoint(transform.position);
        return viewport.x < -0.05f || viewport.x > 1.05f || viewport.y < -0.05f || viewport.y > 1.05f;
    }

    // Impacto detectado pela fisica 2D: o colisor da bala e um trigger e o inimigo tem colisor proprio.
    void OnTriggerEnter2D(Collider2D outro)
    {
        if (acertou)
            return;

        InimigoGaucho inimigo = outro.GetComponent<InimigoGaucho>();
        if (inimigo == null || !inimigo.EstaVivo)
            return;

        acertou = true;
        EfeitosSonoros.Acerto();
        inimigo.ReceberDano(dano);
        Destroy(gameObject);
    }
}
