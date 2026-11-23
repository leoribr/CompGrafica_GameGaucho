using UnityEngine;

/// <summary>
/// Efeitos sonoros do jogo (acerto, morte do animal, dano no galpao, vitoria e derrota).
/// Os sons sao sintetizados na hora, como os tiros das torres, entao nao dependem de arquivos.
/// </summary>
public static class EfeitosSonoros
{
    const int Taxa = 22050;

    static AudioSource fonte;
    static AudioClip somAcerto;
    static AudioClip somMorte;
    static AudioClip somDanoGalpao;
    static AudioClip somVitoria;
    static AudioClip somDerrota;
    static float ultimoAcerto;

    public static void Acerto()
    {
        // Muitos chumbos acertam no mesmo instante: limita para nao virar ruido.
        if (Time.unscaledTime - ultimoAcerto < 0.05f)
            return;
        ultimoAcerto = Time.unscaledTime;
        if (somAcerto == null)
            somAcerto = CriarTom("Acerto", 0.05f, 520f, 380f, 0.35f, 0.22f);
        Tocar(somAcerto, 0.5f, Random.Range(0.9f, 1.15f));
    }

    public static void Morte(bool chefe)
    {
        if (somMorte == null)
            somMorte = CriarTom("Morte do animal", 0.22f, 330f, 90f, 0.25f, 0.4f);
        Tocar(somMorte, chefe ? 1f : 0.7f, chefe ? 0.6f : Random.Range(0.92f, 1.1f));
    }

    public static void DanoNoGalpao()
    {
        if (somDanoGalpao == null)
            somDanoGalpao = CriarTom("Dano no galpao", 0.32f, 110f, 55f, 0.45f, 0.55f);
        Tocar(somDanoGalpao, 0.9f, 1f);
    }

    public static void Vitoria()
    {
        if (somVitoria == null)
            somVitoria = CriarArpejo("Vitoria", new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.14f);
        Tocar(somVitoria, 0.8f, 1f);
    }

    public static void Derrota()
    {
        if (somDerrota == null)
            somDerrota = CriarArpejo("Derrota", new[] { 392f, 349.23f, 311.13f, 261.63f }, 0.22f);
        Tocar(somDerrota, 0.8f, 1f);
    }

    static void Tocar(AudioClip clip, float volume, float pitch)
    {
        if (fonte == null)
        {
            GameObject objeto = new GameObject("Efeitos sonoros");
            Object.DontDestroyOnLoad(objeto);
            fonte = objeto.AddComponent<AudioSource>();
            fonte.playOnAwake = false;
            fonte.spatialBlend = 0f;
        }
        fonte.pitch = pitch;
        fonte.PlayOneShot(clip, volume);
    }

    /// <summary>Tom que desliza de uma frequencia para outra, com um pouco de ruido e decaimento rapido.</summary>
    static AudioClip CriarTom(string nome, float duracao, float freqInicial, float freqFinal, float ruido, float volume)
    {
        int amostras = Mathf.CeilToInt(Taxa * duracao);
        float[] dados = new float[amostras];
        float fase = 0f;
        for (int i = 0; i < amostras; i++)
        {
            float progresso = (float)i / amostras;
            float frequencia = Mathf.Lerp(freqInicial, freqFinal, progresso);
            fase += 2f * Mathf.PI * frequencia / Taxa;
            float envelope = Mathf.Pow(1f - progresso, 2f);
            float onda = Mathf.Sin(fase) * (1f - ruido) + Random.Range(-1f, 1f) * ruido;
            dados[i] = onda * envelope * volume;
        }
        AudioClip clip = AudioClip.Create(nome, amostras, 1, Taxa, false);
        clip.SetData(dados, 0);
        return clip;
    }

    /// <summary>Sequencia de notas curtas (onda quadrada suave), usada nos jingles de fim de jogo.</summary>
    static AudioClip CriarArpejo(string nome, float[] notas, float duracaoNota)
    {
        int porNota = Mathf.CeilToInt(Taxa * duracaoNota);
        int ultimaNotaExtra = porNota * 2;
        int amostras = porNota * notas.Length + ultimaNotaExtra;
        float[] dados = new float[amostras];
        for (int n = 0; n < notas.Length; n++)
        {
            int inicio = n * porNota;
            int duracao = n == notas.Length - 1 ? porNota + ultimaNotaExtra : porNota;
            for (int i = 0; i < duracao; i++)
            {
                float t = (float)i / Taxa;
                float envelope = Mathf.Min(1f, i / 200f) * Mathf.Pow(1f - (float)i / duracao, 1.5f);
                float onda = Mathf.Sin(2f * Mathf.PI * notas[n] * t);
                // Soma do 3o harmonico: lembra um chiptune sem ficar estridente.
                onda += Mathf.Sin(2f * Mathf.PI * notas[n] * 3f * t) * 0.25f;
                dados[inicio + i] += onda * envelope * 0.3f;
            }
        }
        AudioClip clip = AudioClip.Create(nome, amostras, 1, Taxa, false);
        clip.SetData(dados, 0);
        return clip;
    }
}
