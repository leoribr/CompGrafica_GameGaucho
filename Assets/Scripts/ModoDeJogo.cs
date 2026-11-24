using UnityEngine;

public sealed class ModoDeJogo
{
    const string ChavePrefs = "modo";

    public readonly string nome;
    public readonly string descricao;
    public readonly bool soPeoes;
    public readonly bool metadeDasPilas;
    public readonly bool semVender;
    public readonly bool umaVida;
    public readonly float multiplicadorPontuacao;

    ModoDeJogo(string nome, string descricao, bool soPeoes, bool metadeDasPilas, bool semVender, bool umaVida, float multiplicadorPontuacao)
    {
        this.nome = nome;
        this.descricao = descricao;
        this.soPeoes = soPeoes;
        this.metadeDasPilas = metadeDasPilas;
        this.semVender = semVender;
        this.umaVida = umaVida;
        this.multiplicadorPontuacao = multiplicadorPontuacao;
    }

    public static readonly ModoDeJogo Padrao = new ModoDeJogo("Padrão", "Regras normais", false, false, false, false, 1f);
    public static readonly ModoDeJogo SoPeoes = new ModoDeJogo("Só Peões", "A loja só tem a classe Peões", true, false, false, false, 1.2f);
    public static readonly ModoDeJogo MetadeDasPilas = new ModoDeJogo("Metade das Pilas", "Bichos, bônus e rendas pagam a metade", false, true, false, false, 1.3f);
    public static readonly ModoDeJogo Desafio = new ModoDeJogo("Desafio", "Sem vender e com uma vida só", false, false, true, true, 1.5f);

    public static readonly ModoDeJogo[] Todos = { Padrao, SoPeoes, MetadeDasPilas, Desafio };

    static int indiceAtual = -1;

    public static ModoDeJogo Atual
    {
        get
        {
            if (indiceAtual < 0)
                indiceAtual = Mathf.Clamp(PlayerPrefs.GetInt(ChavePrefs, 0), 0, Todos.Length - 1);
            return Todos[indiceAtual];
        }
    }

    public static void Alternar()
    {
        indiceAtual = (System.Array.IndexOf(Todos, Atual) + 1) % Todos.Length;
        PlayerPrefs.SetInt(ChavePrefs, indiceAtual);
        PlayerPrefs.Save();
    }
}
