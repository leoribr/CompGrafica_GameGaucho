using UnityEngine;

public class MirarNoAlvo : MonoBehaviour
{
    public float alcance = 4f;
    [Tooltip("Espelha o sprite ao mirar para a esquerda, para a arma não ficar de cabeça para baixo")]
    public bool espelharParaEsquerda = false;

    SpriteRenderer sr;
    Transform alvoAtual;
    float proximaBusca;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Time.time >= proximaBusca || alvoAtual == null)
        {
            alvoAtual = ProcurarAlvo();
            proximaBusca = Time.time + 0.15f;
        }

        Transform alvo = alvoAtual;
        if (alvo == null)
            return;

        Vector2 direcao = alvo.position - transform.position;
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angulo);

        if (espelharParaEsquerda && sr != null)
            sr.flipY = direcao.x < 0f;
    }

    Transform ProcurarAlvo()
    {
        Transform maisProximo = null;
        float menorDistancia = alcance;

        foreach (InimigoGaucho inimigo in FindObjectsByType<InimigoGaucho>(FindObjectsSortMode.None))
        {
            if (!inimigo.EstaVivo)
                continue;
            float distancia = Vector2.Distance(transform.position, inimigo.transform.position);
            if (distancia <= menorDistancia)
            {
                menorDistancia = distancia;
                maisProximo = inimigo.transform;
            }
        }
        return maisProximo;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, alcance);
    }
}
