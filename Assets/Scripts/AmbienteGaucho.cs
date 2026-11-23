using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vida no cenario, sem ocupar espaco de construcao: fumaca e brasas do assado, sombras de nuvens
/// passando, bandos de quero-quero cruzando o ceu e ovelhas pastando.
/// E adicionado pelo JogoGaucho depois de mapear os obstaculos, entao nada daqui bloqueia os gauchos.
/// Todos os sprites e texturas sao gerados em codigo.
/// </summary>
public sealed class AmbienteGaucho : MonoBehaviour
{
    const int OrdemSombras = 20;
    const int OrdemPassaros = 26;

    sealed class Nuvem
    {
        public Transform transform;
        public float velocidade;
    }

    sealed class Passaro
    {
        public Transform transform;
        public SpriteRenderer renderizador;
        public float fase;
    }

    sealed class Ovelha
    {
        public Transform transform;
        public SpriteRenderer renderizador;
        public Vector3 origem;
        public Vector3 destino;
        public float esperaAte;
    }

    readonly List<Nuvem> nuvens = new List<Nuvem>();
    readonly List<Passaro> bando = new List<Passaro>();
    readonly List<Ovelha> ovelhas = new List<Ovelha>();
    readonly List<Object> criados = new List<Object>();

    Camera cameraPrincipal;
    Material materialParticula;
    Sprite asaCima;
    Sprite asaBaixo;
    Vector3 velocidadeDoBando;
    float proximoBando;
    bool bandoVoando;
    AudioSource fonteDoPiado;
    AudioClip piado;

    void Start()
    {
        cameraPrincipal = Camera.main;
        materialParticula = new Material(Shader.Find("Sprites/Default"));
        criados.Add(materialParticula);

        CriarFumacaDoAssado();
        CriarNuvens();
        PrepararPassaros();
        PrepararOvelhas();
        proximoBando = Time.time + Random.Range(4f, 8f);
    }

    void Update()
    {
        AtualizarNuvens();
        AtualizarBando();
        AtualizarOvelhas();
    }

    // ---------- Fumaca do assado ----------

    void CriarFumacaDoAssado()
    {
        GameObject assado = GameObject.Find("Assado");
        Vector3 posicao = assado != null ? assado.transform.position : new Vector3(8.6f, -3.6f, 0f);

        ParticleSystem fumaca = CriarParticulas("Fumaca do assado", posicao + new Vector3(0f, 0.25f, 0f));
        ParticleSystem.MainModule principal = fumaca.main;
        principal.loop = true;
        principal.startLifetime = new ParticleSystem.MinMaxCurve(1.6f, 2.4f);
        principal.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.45f);
        principal.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
        principal.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 0.75f, 0.75f, 0.55f), new Color(0.9f, 0.9f, 0.9f, 0.4f));
        principal.maxParticles = 60;
        ParticleSystem.EmissionModule emissao = fumaca.emission;
        emissao.rateOverTime = 7f;
        ParticleSystem.ShapeModule forma = fumaca.shape;
        forma.shapeType = ParticleSystemShapeType.Cone;
        forma.angle = 12f;
        forma.radius = 0.08f;
        forma.rotation = new Vector3(-90f, 0f, 0f);
        // O vento do pampa leva a fumaca para o lado enquanto ela sobe.
        ParticleSystem.VelocityOverLifetimeModule vento = fumaca.velocityOverLifetime;
        vento.enabled = true;
        vento.space = ParticleSystemSimulationSpace.World;
        vento.x = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        vento.y = new ParticleSystem.MinMaxCurve(0f, 0f);
        vento.z = new ParticleSystem.MinMaxCurve(0f, 0f);
        ParticleSystem.SizeOverLifetimeModule tamanho = fumaca.sizeOverLifetime;
        tamanho.enabled = true;
        tamanho.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 2.2f));
        ParticleSystem.ColorOverLifetimeModule cor = fumaca.colorOverLifetime;
        cor.enabled = true;
        Gradient desvanecer = new Gradient();
        desvanecer.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0f, 1f) });
        cor.color = desvanecer;
        fumaca.Play();

        ParticleSystem brasas = CriarParticulas("Brasas do assado", posicao + new Vector3(0f, 0.1f, 0f));
        ParticleSystem.MainModule principalBrasas = brasas.main;
        principalBrasas.loop = true;
        principalBrasas.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        principalBrasas.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
        principalBrasas.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.06f);
        principalBrasas.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.55f, 0.1f), new Color(1f, 0.85f, 0.3f));
        principalBrasas.maxParticles = 20;
        ParticleSystem.EmissionModule emissaoBrasas = brasas.emission;
        emissaoBrasas.rateOverTime = 5f;
        ParticleSystem.ShapeModule formaBrasas = brasas.shape;
        formaBrasas.shapeType = ParticleSystemShapeType.Cone;
        formaBrasas.angle = 25f;
        formaBrasas.radius = 0.1f;
        formaBrasas.rotation = new Vector3(-90f, 0f, 0f);
        ParticleSystem.ColorOverLifetimeModule corBrasas = brasas.colorOverLifetime;
        corBrasas.enabled = true;
        Gradient apagar = new Gradient();
        apagar.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(0.6f, 0.2f, 0.1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        corBrasas.color = apagar;
        brasas.Play();
    }

    ParticleSystem CriarParticulas(string nome, Vector3 posicao)
    {
        GameObject objeto = new GameObject(nome);
        objeto.transform.SetParent(transform, false);
        objeto.transform.position = posicao;
        ParticleSystem sistema = objeto.AddComponent<ParticleSystem>();
        sistema.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule principal = sistema.main;
        principal.simulationSpace = ParticleSystemSimulationSpace.World;
        principal.gravityModifier = 0f;
        ParticleSystemRenderer renderizador = objeto.GetComponent<ParticleSystemRenderer>();
        renderizador.material = materialParticula;
        renderizador.sortingOrder = 12;
        return sistema;
    }

    // ---------- Sombras de nuvens ----------

    void CriarNuvens()
    {
        Texture2D textura = TexturaDeSombra(64, 32);
        Sprite sprite = Sprite.Create(textura, new Rect(0f, 0f, textura.width, textura.height), new Vector2(0.5f, 0.5f), 16f);
        criados.Add(sprite);

        for (int i = 0; i < 3; i++)
        {
            GameObject objeto = new GameObject("Sombra de nuvem");
            objeto.transform.SetParent(transform, false);
            SpriteRenderer renderizador = objeto.AddComponent<SpriteRenderer>();
            renderizador.sprite = sprite;
            renderizador.color = new Color(0f, 0.02f, 0.05f, Random.Range(0.10f, 0.15f));
            renderizador.sortingOrder = OrdemSombras;
            float escala = Random.Range(1.4f, 2.4f);
            objeto.transform.localScale = new Vector3(escala, escala * Random.Range(0.8f, 1.1f), 1f);
            // Comecam espalhadas pela tela, para nao chegarem todas juntas.
            objeto.transform.position = new Vector3(Random.Range(-12f, 12f), Random.Range(-4.5f, 4.5f), 0f);
            nuvens.Add(new Nuvem { transform = objeto.transform, velocidade = Random.Range(0.25f, 0.45f) });
        }
    }

    void AtualizarNuvens()
    {
        float limite = LarguraVisivel() * 0.5f + 4f;
        foreach (Nuvem nuvem in nuvens)
        {
            Vector3 p = nuvem.transform.position;
            p.x += nuvem.velocidade * Time.deltaTime;
            if (p.x > limite)
            {
                p.x = -limite;
                p.y = Random.Range(-4.5f, 4.5f);
            }
            nuvem.transform.position = p;
        }
    }

    Texture2D TexturaDeSombra(int largura, int altura)
    {
        Texture2D textura = new Texture2D(largura, altura, TextureFormat.RGBA32, false);
        textura.wrapMode = TextureWrapMode.Clamp;
        criados.Add(textura);
        // Tres elipses sobrepostas formam o contorno de uma nuvem, com borda suave.
        Vector2[] centros = { new Vector2(0.3f, 0.45f), new Vector2(0.55f, 0.58f), new Vector2(0.72f, 0.42f) };
        float[] raios = { 0.26f, 0.3f, 0.24f };
        for (int y = 0; y < altura; y++)
        {
            for (int x = 0; x < largura; x++)
            {
                Vector2 uv = new Vector2((x + 0.5f) / largura, (y + 0.5f) / altura);
                float alfa = 0f;
                for (int i = 0; i < centros.Length; i++)
                {
                    Vector2 d = uv - centros[i];
                    d.y *= 2f;
                    float distancia = d.magnitude / raios[i];
                    alfa = Mathf.Max(alfa, Mathf.Clamp01((1f - distancia) * 3f));
                }
                textura.SetPixel(x, y, new Color(1f, 1f, 1f, alfa));
            }
        }
        textura.Apply();
        return textura;
    }

    // ---------- Quero-queros ----------

    void PrepararPassaros()
    {
        asaCima = SpriteDePassaro(true);
        asaBaixo = SpriteDePassaro(false);

        fonteDoPiado = gameObject.AddComponent<AudioSource>();
        fonteDoPiado.playOnAwake = false;
        fonteDoPiado.spatialBlend = 0f;
        piado = CriarPiado();
        criados.Add(piado);
    }

    void AtualizarBando()
    {
        if (!bandoVoando)
        {
            if (Time.time >= proximoBando)
                SoltarBando();
            return;
        }

        float limite = LarguraVisivel() * 0.5f + 2f;
        bool algumNaTela = false;
        foreach (Passaro passaro in bando)
        {
            passaro.transform.position += velocidadeDoBando * Time.deltaTime;
            passaro.fase += Time.deltaTime * 9f;
            passaro.renderizador.sprite = Mathf.Sin(passaro.fase) > 0f ? asaCima : asaBaixo;
            if (Mathf.Abs(passaro.transform.position.x) < limite)
                algumNaTela = true;
        }

        if (!algumNaTela)
        {
            foreach (Passaro passaro in bando)
                Destroy(passaro.transform.gameObject);
            bando.Clear();
            bandoVoando = false;
            proximoBando = Time.time + Random.Range(14f, 24f);
        }
    }

    void SoltarBando()
    {
        bool paraDireita = Random.value > 0.5f;
        float inicioX = (LarguraVisivel() * 0.5f + 1.5f) * (paraDireita ? -1f : 1f);
        float inicioY = Random.Range(-3.5f, 4f);
        float subida = Random.Range(-0.4f, 0.4f);
        velocidadeDoBando = new Vector3(paraDireita ? 2.6f : -2.6f, subida, 0f);

        // Formacao em V: o lider na frente e os outros atras, em diagonal.
        int quantidade = Random.Range(3, 6);
        for (int i = 0; i < quantidade; i++)
        {
            int fileira = (i + 1) / 2;
            float lado = i % 2 == 0 ? 1f : -1f;
            Vector3 deslocamento = new Vector3(-fileira * 0.45f * (paraDireita ? 1f : -1f), fileira * 0.3f * lado, 0f);

            GameObject objeto = new GameObject("Quero-quero");
            objeto.transform.SetParent(transform, false);
            objeto.transform.position = new Vector3(inicioX, inicioY, 0f) + deslocamento;
            SpriteRenderer renderizador = objeto.AddComponent<SpriteRenderer>();
            renderizador.sprite = asaCima;
            renderizador.sortingOrder = OrdemPassaros;
            renderizador.flipX = !paraDireita;
            bando.Add(new Passaro { transform = objeto.transform, renderizador = renderizador, fase = Random.Range(0f, 6f) });
        }

        bandoVoando = true;
        if (piado != null)
            fonteDoPiado.PlayOneShot(piado, 0.25f);
    }

    Sprite SpriteDePassaro(bool asasParaCima)
    {
        // Quero-quero visto de cima em pixel art 9x7: corpo cinza, peito preto e asas com ponta escura.
        string[] desenho = asasParaCima
            ? new[]
            {
                "d.......d",
                "gd.....dg",
                ".gg...gg.",
                "..gpwpg..",
                "...gbr...",
                ".........",
                "........."
            }
            : new[]
            {
                ".........",
                ".........",
                "...gpwr..",
                ".ggggbgg.",
                "gd.....dg",
                "d.......d",
                "........."
            };
        int altura = desenho.Length;
        int largura = desenho[0].Length;
        Texture2D textura = new Texture2D(largura, altura, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Point;
        textura.wrapMode = TextureWrapMode.Clamp;
        criados.Add(textura);
        for (int y = 0; y < altura; y++)
        {
            string linha = desenho[altura - 1 - y];
            for (int x = 0; x < largura; x++)
                textura.SetPixel(x, y, CorDoPixel(linha[x]));
        }
        textura.Apply();
        Sprite sprite = Sprite.Create(textura, new Rect(0f, 0f, largura, altura), new Vector2(0.5f, 0.5f), 16f);
        criados.Add(sprite);
        return sprite;
    }

    static Color CorDoPixel(char c)
    {
        switch (c)
        {
            case 'g': return new Color(0.62f, 0.64f, 0.66f);
            case 'd': return new Color(0.2f, 0.22f, 0.24f);
            case 'p': return new Color(0.08f, 0.08f, 0.1f);
            case 'w': return new Color(0.95f, 0.95f, 0.95f);
            case 'b': return new Color(0.12f, 0.12f, 0.14f);
            case 'r': return new Color(0.85f, 0.25f, 0.2f);
            default: return new Color(0f, 0f, 0f, 0f);
        }
    }

    /// <summary>"Quero-quero": dois gritos curtos e agudos, o segundo mais grave.</summary>
    static AudioClip CriarPiado()
    {
        const int taxa = 22050;
        float[] notas = { 2300f, 1800f, 2300f, 1800f };
        float[] inicios = { 0f, 0.09f, 0.3f, 0.39f };
        const float duracaoNota = 0.08f;
        int amostras = Mathf.CeilToInt(taxa * 0.5f);
        float[] dados = new float[amostras];
        for (int n = 0; n < notas.Length; n++)
        {
            int inicio = Mathf.FloorToInt(inicios[n] * taxa);
            int duracao = Mathf.FloorToInt(duracaoNota * taxa);
            for (int i = 0; i < duracao && inicio + i < amostras; i++)
            {
                float t = (float)i / taxa;
                float progresso = (float)i / duracao;
                float frequencia = notas[n] * (1f - 0.15f * progresso);
                float envelope = Mathf.Sin(Mathf.PI * progresso);
                dados[inicio + i] += Mathf.Sin(2f * Mathf.PI * frequencia * t) * envelope * 0.35f;
            }
        }
        AudioClip clip = AudioClip.Create("Quero-quero", amostras, 1, taxa, false);
        clip.SetData(dados, 0);
        return clip;
    }

    // ---------- Ovelhas pastando ----------

    void PrepararOvelhas()
    {
        foreach (SpriteRenderer sr in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (sr.gameObject.name != "Ovelha")
                continue;
            ovelhas.Add(new Ovelha
            {
                transform = sr.transform,
                renderizador = sr,
                origem = sr.transform.position,
                destino = sr.transform.position,
                esperaAte = Time.time + Random.Range(1f, 4f)
            });
        }
    }

    void AtualizarOvelhas()
    {
        foreach (Ovelha ovelha in ovelhas)
        {
            if (ovelha.transform == null)
                continue;

            Vector3 atual = ovelha.transform.position;
            if ((ovelha.destino - atual).sqrMagnitude < 0.0004f)
            {
                // Chegou: pasta um pouco e escolhe outro ponto perto de onde comecou.
                if (Time.time < ovelha.esperaAte)
                    continue;
                ovelha.destino = ovelha.origem + (Vector3)(Random.insideUnitCircle * 0.45f);
                ovelha.esperaAte = Time.time + Random.Range(2.5f, 6f);
                ovelha.renderizador.flipX = ovelha.destino.x < atual.x;
                continue;
            }

            Vector3 novo = Vector3.MoveTowards(atual, ovelha.destino, 0.35f * Time.deltaTime);
            // Leve saltitar enquanto anda.
            novo.y += Mathf.Sin(Time.time * 14f) * 0.004f;
            ovelha.transform.position = novo;
        }
    }

    float LarguraVisivel()
    {
        if (cameraPrincipal == null || !cameraPrincipal.orthographic)
            return 20f;
        return cameraPrincipal.orthographicSize * 2f * cameraPrincipal.aspect;
    }

    void OnDestroy()
    {
        foreach (Object objeto in criados)
        {
            if (objeto != null)
                Destroy(objeto);
        }
    }
}
