using UnityEngine;

public class SeguirCaminho : MonoBehaviour
{
    [Tooltip("Pontos do caminho, na ordem em que o inimigo passa")]
    public Transform[] pontos;
    public float velocidade = 2f;
    [Tooltip("Segundos de espera antes de começar a andar")]
    public float atraso = 0f;

    [Header("Animação quadro a quadro (opcional)")]
    public Sprite[] quadros;
    public float tempoPorQuadro = 0.15f;

    SpriteRenderer sr;
    int proximo = 1;
    float tempoAnimacao;
    int quadroAtual;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        transform.position = pontos[0].position;
    }

    void Update()
    {
        if (atraso > 0f)
        {
            atraso -= Time.deltaTime;
            return;
        }

        Vector3 antes = transform.position;
        Vector3 alvo = pontos[proximo].position;
        transform.position = Vector3.MoveTowards(antes, alvo, velocidade * Time.deltaTime);

        // Os sprites olham para a direita: espelha quando anda para a esquerda
        float dx = transform.position.x - antes.x;
        if (dx != 0f)
            sr.flipX = dx < 0f;

        if (transform.position == alvo)
        {
            proximo++;
            if (proximo >= pontos.Length)
            {
                // Chegou ao fim: por enquanto só volta para o início (depois vai causar dano à base)
                transform.position = pontos[0].position;
                proximo = 1;
            }
        }

        Animar();
    }

    void Animar()
    {
        if (quadros == null || quadros.Length < 2)
            return;

        tempoAnimacao += Time.deltaTime;
        if (tempoAnimacao >= tempoPorQuadro)
        {
            tempoAnimacao -= tempoPorQuadro;
            quadroAtual = (quadroAtual + 1) % quadros.Length;
            sr.sprite = quadros[quadroAtual];
        }
    }

    // Desenha o caminho na janela Scene para facilitar a edição
    void OnDrawGizmos()
    {
        if (pontos == null)
            return;

        Gizmos.color = Color.red;
        for (int i = 0; i < pontos.Length - 1; i++)
        {
            if (pontos[i] != null && pontos[i + 1] != null)
                Gizmos.DrawLine(pontos[i].position, pontos[i + 1].position);
        }
    }
}
