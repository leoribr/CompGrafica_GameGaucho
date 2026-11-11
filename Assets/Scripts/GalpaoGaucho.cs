using UnityEngine;

/// <summary>
/// Area de chegada do galpao: um trigger do tamanho do sprite detecta, pela fisica 2D,
/// quando um animal encosta na base e tira vidas do jogador.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public sealed class GalpaoGaucho : MonoBehaviour
{
    [Tooltip("Fracao do sprite coberta pelo trigger (1 = galpao inteiro)")]
    public float escalaDaArea = 0.8f;

    void Awake()
    {
        BoxCollider2D area = GetComponent<BoxCollider2D>();
        area.isTrigger = true;

        // Usa os limites do sprite em espaco local, para o collider acompanhar a escala do objeto.
        SpriteRenderer renderizador = GetComponent<SpriteRenderer>();
        if (renderizador != null && renderizador.sprite != null)
        {
            Bounds limites = renderizador.sprite.bounds;
            area.offset = limites.center;
            area.size = (Vector2)limites.size * escalaDaArea;
        }

        // Os animais tem corpo Kinematic, e na fisica 2D dois corpos kinematic nao geram contato.
        // Por isso o galpao recebe um corpo Dynamic sem gravidade e travado: nunca se move, mas dispara o trigger.
        Rigidbody2D corpo = GetComponent<Rigidbody2D>();
        if (corpo == null)
            corpo = gameObject.AddComponent<Rigidbody2D>();
        corpo.bodyType = RigidbodyType2D.Dynamic;
        corpo.gravityScale = 0f;
        corpo.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        // Projeteis tambem tocam o trigger, mas nao tem InimigoGaucho e sao ignorados.
        InimigoGaucho inimigo = outro.GetComponent<InimigoGaucho>();
        if (inimigo != null && inimigo.EstaVivo)
            inimigo.ChegarAoGalpao();
    }
}
