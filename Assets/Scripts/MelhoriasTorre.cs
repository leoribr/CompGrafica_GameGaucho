using UnityEngine;

public static class MelhoriasTorre
{
    public const int NivelMaximo = 5;
    public const int LimiteDoSegundoCaminho = 2;
    public const float FracaoDaVenda = 0.7f;

    static readonly float[] FracaoDoCusto = { 0.5f, 0.8f, 1.5f, 4f, 10f };

    public static int Custo(int precoDaTorre, int nivelAtual)
    {
        if (nivelAtual < 0 || nivelAtual >= NivelMaximo)
            return 0;
        int custo = Mathf.RoundToInt(precoDaTorre * FracaoDoCusto[nivelAtual] / 5f) * 5;
        return Mathf.Max(5, custo);
    }

    public static bool PodeMelhorar(int[] niveis, int caminho)
    {
        if (niveis[caminho] >= NivelMaximo)
            return false;

        int caminhosUsados = 0;
        int acimaDoLimite = 0;
        for (int i = 0; i < niveis.Length; i++)
        {
            int nivel = i == caminho ? niveis[i] + 1 : niveis[i];
            if (nivel > 0)
                caminhosUsados++;
            if (nivel > LimiteDoSegundoCaminho)
                acimaDoLimite++;
        }
        return caminhosUsados <= 2 && acimaDoLimite <= 1;
    }

    public static string Bloqueio(int[] niveis, int caminho)
    {
        if (niveis[caminho] >= NivelMaximo)
            return "MÁXIMO";
        return "BLOQUEADO";
    }

    public static int ValorDeVenda(int gasto)
    {
        return Mathf.RoundToInt(gasto * FracaoDaVenda);
    }
}
