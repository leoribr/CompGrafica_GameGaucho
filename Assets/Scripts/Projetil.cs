using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class Projetil : MonoBehaviour
{
    public float velocidade = 12f;
    [Tooltip("Segundos ate a bala sumir, caso nao acerte nada")]
    public float tempoDeVida = 2f;

    const int Reto = 0;
    const int Bumerangue = 1;
    const int Caminho = 2;
    const int Destino = 3;

    JogoGaucho jogo;
    TorreGaucha origem;
    AtributosTorre ataque;
    InimigoGaucho alvo;
    Rigidbody2D corpo;
    SpriteRenderer renderizador;
    Camera cameraPrincipal;
    Vector2 direcao = Vector2.right;
    int dano;
    int perfuracaoRestante;
    int ricochetesRestantes;
    readonly HashSet<InimigoGaucho> atingidos = new HashSet<InimigoGaucho>();
    bool acabou;
    float morreEm;
    int modo = Reto;
    Vector2 centro;
    float raio;
    float angulo;
    float percorrido;
    Transform[] pontos;
    int indice;
    Vector3 destino;

    public bool EhAlma { get; set; }

    void Awake()
    {
        corpo = GetComponent<Rigidbody2D>();
        renderizador = GetComponent<SpriteRenderer>();
        cameraPrincipal = Camera.main;
    }

    public void Configurar(JogoGaucho gerenciador, TorreGaucha torre, AtributosTorre novoAtaque, int novoDano,
        InimigoGaucho novoAlvo, Vector2 novaDirecao, float alcanceMaximo)
    {
        jogo = gerenciador;
        origem = torre;
        ataque = novoAtaque;
        dano = novoDano;
        alvo = novoAlvo;
        perfuracaoRestante = Mathf.Max(1, ataque.perfuracao);
        ricochetesRestantes = ataque.ricochetes;
        direcao = novaDirecao.sqrMagnitude > 0.0001f ? novaDirecao.normalized : Vector2.right;
        velocidade = Mathf.Max(1f, ataque.velocidadeProjetil);
        morreEm = Time.time + Mathf.Max(0.25f, alcanceMaximo / velocidade);
        transform.localScale = Vector3.one * ataque.escalaProjetil;

        if (renderizador != null)
        {
            Sprite sprite = string.IsNullOrEmpty(ataque.desenhoProjetil) ? null : SpritesGaucho.Pegar(ataque.desenhoProjetil);
            if (sprite != null)
                renderizador.sprite = sprite;
            renderizador.color = ataque.corProjetil;
        }
        Girar();
    }

    public void ConfigurarBumerangue(Vector3 alvoDoArremesso)
    {
        modo = Bumerangue;
        Vector2 inicio = transform.position;
        Vector2 ate = (Vector2)alvoDoArremesso - inicio;
        float distancia = Mathf.Max(0.8f, ate.magnitude);
        raio = distancia * 0.55f;
        centro = inicio + ate.normalized * raio;
        angulo = Mathf.Atan2(inicio.y - centro.y, inicio.x - centro.x);
        percorrido = 0f;
        morreEm = Time.time + Mathf.PI * 2f * raio / velocidade + 0.5f;
    }

    public void ConfigurarCaminho(Transform[] caminho, int indiceInicial)
    {
        modo = Caminho;
        pontos = caminho;
        indice = indiceInicial;
        float comprimento = 0f;
        for (int i = 1; i < caminho.Length; i++)
            comprimento += Vector2.Distance(caminho[i - 1].position, caminho[i].position);
        morreEm = Time.time + comprimento / velocidade + 1f;
    }

    public void ConfigurarDestino(Vector3 ponto)
    {
        modo = Destino;
        destino = ponto;
        morreEm = Time.time + Vector2.Distance(transform.position, ponto) / velocidade + 1f;
    }

    void FixedUpdate()
    {
        if (acabou)
            return;

        float dt = Time.fixedDeltaTime;
        Vector2 posicao = corpo.position;

        if (modo == Bumerangue)
        {
            float passo = velocidade / raio * dt;
            angulo += passo;
            percorrido += passo;
            Vector2 proxima = centro + new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * raio;
            direcao = (proxima - posicao).normalized;
            corpo.linearVelocity = (proxima - posicao) / dt;
            if (percorrido >= Mathf.PI * 2f)
                Acabar();
        }
        else if (modo == Caminho)
        {
            if (indice < 0)
            {
                Fim(transform.position);
                return;
            }
            Vector2 ponto = pontos[indice].position;
            Vector2 falta = ponto - posicao;
            if (falta.magnitude <= velocidade * dt)
            {
                indice--;
                corpo.linearVelocity = falta / dt;
            }
            else
            {
                direcao = falta.normalized;
                corpo.linearVelocity = direcao * velocidade;
            }
        }
        else if (modo == Destino)
        {
            Vector2 falta = (Vector2)destino - posicao;
            if (falta.magnitude <= velocidade * dt)
            {
                corpo.position = destino;
                if (ataque.raioExplosao > 0f)
                    jogo.Explodir(destino, ataque.raioExplosao, ataque, dano, origem);
                Fim(destino);
                return;
            }
            direcao = falta.normalized;
            corpo.linearVelocity = direcao * velocidade;
        }
        else
        {
            if (alvo != null && alvo.EstaVivo)
            {
                Vector2 paraOAlvo = (Vector2)alvo.transform.position - posicao;
                if (paraOAlvo.sqrMagnitude > 0.0001f)
                    direcao = paraOAlvo.normalized;
            }
            corpo.linearVelocity = direcao * velocidade;
        }
        Girar();
    }

    void Girar()
    {
        if (ataque != null && ataque.giraProjetil)
        {
            transform.Rotate(0f, 0f, -720f * Time.deltaTime);
            return;
        }
        if (renderizador != null && ataque != null && (ataque.desenhoProjetil == "touro" || ataque.desenhoProjetil == "fantasma"))
        {
            renderizador.flipX = direcao.x < 0f;
            return;
        }
        float graus = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, graus);
    }

    void Update()
    {
        if (acabou)
            return;
        if ((jogo != null && !jogo.PodeCombater) || Time.time >= morreEm || (modo == Reto && SaiuDaTela()))
            Acabar();
    }

    bool SaiuDaTela()
    {
        if (cameraPrincipal == null)
            return false;
        Vector3 viewport = cameraPrincipal.WorldToViewportPoint(transform.position);
        return viewport.x < -0.05f || viewport.x > 1.05f || viewport.y < -0.05f || viewport.y > 1.05f;
    }

    public void IgnorarInimigo(InimigoGaucho inimigo)
    {
        if (inimigo != null)
            atingidos.Add(inimigo);
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (acabou || modo == Destino || jogo == null)
            return;

        InimigoGaucho inimigo = outro.GetComponent<InimigoGaucho>();
        if (inimigo == null || !inimigo.EstaVivo || atingidos.Contains(inimigo))
            return;

        atingidos.Add(inimigo);
        if (!jogo.PodeFerir(inimigo, ataque))
        {
            jogo.AvisarImune(inimigo);
            return;
        }

        Vector3 ponto = inimigo.transform.position;
        if (ataque.raioExplosao > 0f)
        {
            jogo.Explodir(ponto, ataque.raioExplosao, ataque, dano, origem);
            Fim(ponto);
            return;
        }

        jogo.AplicarAcerto(inimigo, ataque, dano, origem);
        EfeitosSonoros.Acerto();
        perfuracaoRestante--;

        if (ricochetesRestantes > 0)
        {
            InimigoGaucho proximo = jogo.ProximoParaRicochete(ponto, 2.6f, atingidos, ataque);
            if (proximo != null)
            {
                ricochetesRestantes--;
                alvo = proximo;
                modo = Reto;
                perfuracaoRestante = Mathf.Max(perfuracaoRestante, 1);
                morreEm = Mathf.Max(morreEm, Time.time + 1.2f);
                return;
            }
        }

        if (modo == Reto && alvo == inimigo)
            alvo = null;
        if (perfuracaoRestante <= 0)
            Fim(ponto);
    }

    void Fim(Vector3 ponto)
    {
        if (acabou)
            return;
        if (ataque.estilhacos > 0)
            jogo.SoltarEstilhacos(ponto, ataque, dano, origem, atingidos);
        if (ataque.zonaDano > 0)
            jogo.CriarZonaDoAtaque(ponto, ataque, origem);
        Acabar();
    }

    void Acabar()
    {
        if (acabou)
            return;
        acabou = true;
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (EhAlma && origem != null)
            origem.AlmasAtivas--;
    }
}
