using UnityEngine;

/// <summary>
/// Cronometro regressivo da missao. Comeca parado; ao esgotar, o jogo declara derrota.
/// E uma classe simples (sem MonoBehaviour): o JogoGaucho chama Atualizar a cada quadro.
/// </summary>
public sealed class TempoMissao
{
    public float Total { get; private set; }
    public float Restante { get; private set; }
    public bool Ativo { get; private set; }
    public bool Esgotou => Ativo && Restante <= 0f;

    public TempoMissao(float segundos)
    {
        Total = Mathf.Max(1f, segundos);
        Restante = Total;
    }

    public void Iniciar()
    {
        Ativo = true;
    }

    public void Parar()
    {
        Ativo = false;
    }

    public void Atualizar(float deltaTime)
    {
        if (!Ativo)
            return;
        Restante = Mathf.Max(0f, Restante - deltaTime);
    }

    /// <summary>Tempo restante no formato m:ss (ex.: 4:32).</summary>
    public string Formatado
    {
        get
        {
            int segundos = Mathf.CeilToInt(Restante);
            return (segundos / 60) + ":" + (segundos % 60).ToString("00");
        }
    }
}
