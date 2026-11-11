using UnityEngine;

/// <summary>
/// Nivel de dificuldade da partida: ajusta os animais (vida e velocidade) e o que o jogador
/// recebe no comeco (pilas, vidas e tempo da missao). A escolha fica salva no PlayerPrefs.
/// </summary>
public sealed class Dificuldade
{
    const string ChavePrefs = "dificuldade";

    public readonly string nome;
    public readonly float multiplicadorVida;
    public readonly float multiplicadorVelocidade;
    public readonly int pilasIniciais;
    public readonly int vidas;
    public readonly float tempoSegundos;
    public readonly float multiplicadorPontuacao;

    Dificuldade(string nome, float multiplicadorVida, float multiplicadorVelocidade,
        int pilasIniciais, int vidas, float tempoSegundos, float multiplicadorPontuacao)
    {
        this.nome = nome;
        this.multiplicadorVida = multiplicadorVida;
        this.multiplicadorVelocidade = multiplicadorVelocidade;
        this.pilasIniciais = pilasIniciais;
        this.vidas = vidas;
        this.tempoSegundos = tempoSegundos;
        this.multiplicadorPontuacao = multiplicadorPontuacao;
    }

    public static readonly Dificuldade Facil = new Dificuldade("Facil", 0.75f, 0.9f, 420, 30, 420f, 1f);
    public static readonly Dificuldade Normal = new Dificuldade("Normal", 1f, 1f, 360, 20, 300f, 1.5f);
    public static readonly Dificuldade Dificil = new Dificuldade("Dificil", 1.35f, 1.15f, 300, 12, 240f, 2f);

    public static readonly Dificuldade[] Todas = { Facil, Normal, Dificil };

    static int indiceAtual = -1;

    /// <summary>Dificuldade escolhida; sobrevive ao recarregar a cena e a fechar o jogo.</summary>
    public static Dificuldade Atual
    {
        get
        {
            if (indiceAtual < 0)
                indiceAtual = Mathf.Clamp(PlayerPrefs.GetInt(ChavePrefs, 1), 0, Todas.Length - 1);
            return Todas[indiceAtual];
        }
        set
        {
            int indice = System.Array.IndexOf(Todas, value);
            if (indice < 0)
                return;
            indiceAtual = indice;
            PlayerPrefs.SetInt(ChavePrefs, indice);
            PlayerPrefs.Save();
        }
    }

    /// <summary>Passa para a proxima dificuldade (Facil -> Normal -> Dificil -> Facil).</summary>
    public static void Alternar()
    {
        int indice = System.Array.IndexOf(Todas, Atual);
        Atual = Todas[(indice + 1) % Todas.Length];
    }
}
