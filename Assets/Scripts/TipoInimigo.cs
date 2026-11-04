using UnityEngine;

/// <summary>Como um tipo de javali difere do padrao da onda.</summary>
public sealed class DefinicaoInimigo
{
    public readonly string nome;
    public readonly bool origemCinza;
    public readonly Color cor;
    public readonly float escala;
    public readonly float multiplicadorVida;
    public readonly float multiplicadorVelocidade;
    public readonly int bonusRecompensa;
    public readonly int danoNaBase;
    public readonly int peso;
    public readonly int ondaMinima;

    public DefinicaoInimigo(
        string nome,
        bool origemCinza,
        Color cor,
        float escala,
        float multiplicadorVida,
        float multiplicadorVelocidade,
        int bonusRecompensa,
        int danoNaBase,
        int peso,
        int ondaMinima)
    {
        this.nome = nome;
        this.origemCinza = origemCinza;
        this.cor = cor;
        this.escala = escala;
        this.multiplicadorVida = multiplicadorVida;
        this.multiplicadorVelocidade = multiplicadorVelocidade;
        this.bonusRecompensa = bonusRecompensa;
        this.danoNaBase = danoNaBase;
        this.peso = peso;
        this.ondaMinima = ondaMinima;
    }
}

/// <summary>Tabela dos tipos de javali e sorteio ponderado por onda.</summary>
public static class TiposInimigo
{
    public static readonly DefinicaoInimigo[] Todos =
    {
        new DefinicaoInimigo("Javali", false, Color.white, 1f, 1f, 1f, 0, 1, 10, 1),
        new DefinicaoInimigo("Javali Cinza", true, Color.white, 1f, 1.9f, 0.82f, 8, 2, 3, 2),
        new DefinicaoInimigo("Javali Veloz", false, new Color(1f, 0.9f, 0.45f), 0.85f, 0.55f, 1.55f, 3, 1, 4, 2),
        new DefinicaoInimigo("Javali Pesado", false, new Color(0.55f, 0.45f, 0.45f), 1.3f, 3.2f, 0.65f, 16, 3, 2, 4)
    };

    public static DefinicaoInimigo Sortear(int onda)
    {
        int total = 0;
        for (int i = 0; i < Todos.Length; i++)
        {
            if (Todos[i].ondaMinima <= onda)
                total += Todos[i].peso;
        }

        if (total <= 0)
            return Todos[0];

        int sorteio = Random.Range(0, total);
        for (int i = 0; i < Todos.Length; i++)
        {
            if (Todos[i].ondaMinima > onda)
                continue;
            sorteio -= Todos[i].peso;
            if (sorteio < 0)
                return Todos[i];
        }

        return Todos[0];
    }
}
