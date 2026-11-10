using System.Collections.Generic;
using UnityEngine;

/// <summary>Mira e dispara automaticamente no animal mais avancado dentro do alcance.</summary>
public sealed class TorreGaucha : MonoBehaviour
{
    static Material materialRastro;
    static Projetil moldeProjetil;
    static AudioClip somRevolver;
    static AudioClip somEspingarda;

    JogoGaucho jogo;
    TipoTorreGaucha tipo;
    Transform arma;
    SpriteRenderer armaRenderer;
    AudioSource audioSource;
    InimigoGaucho alvo;
    float alcanceBase;
    float intervaloBase;
    int danoBase;
    float alcance;
    float intervalo;
    float proximoDisparo;
    float proximaBusca;
    int dano;
    bool ehPrevia;

    public int Nivel { get; private set; } = 1;
    public TipoTorreGaucha Tipo => tipo;
    public float Alcance => alcance;
    public int Dano => dano;
    public float Intervalo => intervalo;
    public bool PodeMelhorar => Nivel < MelhoriasTorre.NivelMaximo;

    public void Melhorar()
    {
        if (!PodeMelhorar)
            return;
        Nivel++;
        AtualizarStats();
    }

    void AtualizarStats()
    {
        alcance = MelhoriasTorre.Alcance(alcanceBase, Nivel);
        intervalo = MelhoriasTorre.Intervalo(intervaloBase, Nivel);
        dano = MelhoriasTorre.Dano(danoBase, Nivel);
    }

    public void Configurar(JogoGaucho gerenciador, TipoTorreGaucha novoTipo, bool previa)
    {
        jogo = gerenciador;
        tipo = novoTipo;
        ehPrevia = previa;
        arma = transform.Find("Arma");
        if (arma == null)
        {
            foreach (Transform filho in transform)
            {
                if (filho.name.ToLowerInvariant().Contains("arma"))
                {
                    arma = filho;
                    break;
                }
            }
        }
        armaRenderer = arma != null ? arma.GetComponent<SpriteRenderer>() : null;

        if (tipo == TipoTorreGaucha.Revolver)
        {
            alcanceBase = 4.25f;
            intervaloBase = 0.48f;
            danoBase = 2;
        }
        else
        {
            alcanceBase = 4.9f;
            intervaloBase = 1.05f;
            danoBase = 5;
        }
        Nivel = 1;
        AtualizarStats();

        if (!ehPrevia && audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = tipo == TipoTorreGaucha.Revolver ? 0.12f : 0.16f;
        }
    }

    void Update()
    {
        if (ehPrevia || jogo == null || !jogo.PodeCombater)
            return;

        if (alvo == null || !alvo.EstaVivo || Vector2.Distance(transform.position, alvo.transform.position) > alcance)
            alvo = null;

        if (alvo == null && Time.time >= proximaBusca)
        {
            alvo = jogo.EncontrarAlvo(transform.position, alcance);
            proximaBusca = Time.time + 0.12f;
        }

        if (alvo == null)
            return;

        Mirar(alvo.transform.position);
        if (Time.time >= proximoDisparo)
        {
            Disparar(alvo);
            proximoDisparo = Time.time + intervalo;
        }
    }

    void Mirar(Vector3 posicaoAlvo)
    {
        if (arma == null)
            return;
        Vector2 direcao = posicaoAlvo - arma.position;
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
        arma.rotation = Quaternion.Euler(0f, 0f, angulo);
        if (armaRenderer != null)
            armaRenderer.flipY = direcao.x < 0f;
    }

    void Disparar(InimigoGaucho alvoPrincipal)
    {
        if (alvoPrincipal == null || !alvoPrincipal.EstaVivo)
            return;

        if (tipo == TipoTorreGaucha.Espingarda)
        {
            Vector3 origem = arma != null ? arma.position : transform.position;
            Vector3 destino = alvoPrincipal.transform.position;
            CriarRastro(origem, destino);
            List<InimigoGaucho> atingidos = jogo.InimigosNoRaio(destino, 0.72f);
            for (int i = 0; i < atingidos.Count; i++)
                atingidos[i].ReceberDano(atingidos[i] == alvoPrincipal ? dano : Mathf.Max(1, dano / 2));
        }
        else
        {
            DispararProjetil(alvoPrincipal);
        }

        if (audioSource != null)
        {
            AudioClip clip = tipo == TipoTorreGaucha.Revolver ? ObterSomRevolver() : ObterSomEspingarda();
            audioSource.pitch = Random.Range(0.94f, 1.07f);
            audioSource.PlayOneShot(clip);
        }
    }

    // A bala sai da ponta da arma; o dano so acontece quando ela colide com o inimigo.
    void DispararProjetil(InimigoGaucho alvoPrincipal)
    {
        if (moldeProjetil == null)
            moldeProjetil = Resources.Load<Projetil>("Projetil_Revolver");
        if (moldeProjetil == null)
        {
            Debug.LogError("Game Gaucho: nao encontrei o prefab Resources/Projetil_Revolver.");
            return;
        }

        Vector3 bocaDaArma = arma != null ? arma.position + arma.right * 0.6f : transform.position;
        Projetil projetil = Instantiate(moldeProjetil, bocaDaArma, Quaternion.identity);
        projetil.Configurar(jogo, alvoPrincipal, dano);
    }

    void CriarRastro(Vector3 origem, Vector3 destino)
    {
        GameObject efeito = new GameObject("Rastro do tiro");
        LineRenderer linha = efeito.AddComponent<LineRenderer>();
        if (materialRastro == null)
            materialRastro = new Material(Shader.Find("Sprites/Default"));
        linha.material = materialRastro;
        linha.positionCount = 2;
        origem.z = -0.2f;
        destino.z = -0.2f;
        linha.SetPosition(0, origem);
        linha.SetPosition(1, destino);
        linha.startWidth = tipo == TipoTorreGaucha.Espingarda ? 0.065f : 0.038f;
        linha.endWidth = 0.012f;
        linha.startColor = new Color(1f, 0.92f, 0.34f, 1f);
        linha.endColor = new Color(1f, 0.45f, 0.08f, 0.15f);
        linha.sortingOrder = 25;
        Destroy(efeito, 0.075f);
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
        Gizmos.DrawWireSphere(transform.position, alcance > 0f ? alcance : 4f);
    }
}
