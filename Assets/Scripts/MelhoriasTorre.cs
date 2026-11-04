using UnityEngine;

/// <summary>Ganhos e custos dos niveis de uma torre.</summary>
public static class MelhoriasTorre
{
    public const int NivelMaximo = 3;

    const float GanhoDano = 0.4f;
    const float GanhoAlcance = 0.08f;
    const float ReducaoIntervalo = 0.1f;

    // Fracao do preco da torre cobrada para passar do nivel 1 ao 2 e do 2 ao 3.
    static readonly float[] FracaoDoCusto = { 0.5f, 0.8f };

    public static int Dano(int danoBase, int nivel)
    {
        return Mathf.RoundToInt(danoBase * (1f + GanhoDano * (nivel - 1)));
    }

    public static float Alcance(float alcanceBase, int nivel)
    {
        return alcanceBase * (1f + GanhoAlcance * (nivel - 1));
    }

    public static float Intervalo(float intervaloBase, int nivel)
    {
        return intervaloBase * (1f - ReducaoIntervalo * (nivel - 1));
    }

    /// <summary>Custo de passar de nivelAtual para o proximo; 0 no nivel maximo.</summary>
    public static int Custo(int precoDaTorre, int nivelAtual)
    {
        if (nivelAtual < 1 || nivelAtual >= NivelMaximo)
            return 0;
        return Mathf.RoundToInt(precoDaTorre * FracaoDoCusto[nivelAtual - 1]);
    }
}
