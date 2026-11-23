using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Uma partida registrada no ranking.</summary>
[Serializable]
public sealed class RegistroRanking
{
    public int pontos;
    public string nivel;
    public bool vitoria;
    public int ondas;
    public string data;
}

/// <summary>
/// Ranking simples das melhores partidas (top 10), salvo em JSON no PlayerPrefs.
/// O JogoGaucho registra cada partida encerrada e o MenuGaucho mostra a lista.
/// </summary>
public static class Ranking
{
    const string ChavePrefs = "ranking";
    public const int Maximo = 10;

    [Serializable]
    sealed class Lista
    {
        public List<RegistroRanking> itens = new List<RegistroRanking>();
    }

    public static List<RegistroRanking> Carregar()
    {
        string json = PlayerPrefs.GetString(ChavePrefs, "");
        if (string.IsNullOrEmpty(json))
            return new List<RegistroRanking>();
        try
        {
            Lista lista = JsonUtility.FromJson<Lista>(json);
            return lista != null && lista.itens != null ? lista.itens : new List<RegistroRanking>();
        }
        catch (ArgumentException)
        {
            return new List<RegistroRanking>();
        }
    }

    /// <summary>Guarda a partida e devolve a posicao dela (1 = primeiro), ou 0 se nao entrou no top.</summary>
    public static int Registrar(RegistroRanking registro)
    {
        List<RegistroRanking> itens = Carregar();
        itens.Add(registro);
        // Ordena do maior para o menor; em empate, fica na frente quem ja estava.
        List<RegistroRanking> ordenados = new List<RegistroRanking>(itens);
        ordenados.Sort((a, b) =>
        {
            int comparacao = b.pontos.CompareTo(a.pontos);
            return comparacao != 0 ? comparacao : itens.IndexOf(a).CompareTo(itens.IndexOf(b));
        });
        if (ordenados.Count > Maximo)
            ordenados.RemoveRange(Maximo, ordenados.Count - Maximo);

        PlayerPrefs.SetString(ChavePrefs, JsonUtility.ToJson(new Lista { itens = ordenados }));
        PlayerPrefs.Save();
        return ordenados.IndexOf(registro) + 1;
    }

    public static void Limpar()
    {
        PlayerPrefs.DeleteKey(ChavePrefs);
        PlayerPrefs.Save();
    }
}
