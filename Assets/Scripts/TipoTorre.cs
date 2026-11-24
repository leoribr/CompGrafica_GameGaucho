using System.Collections.Generic;
using UnityEngine;

public enum TipoDano
{
    Corte,
    Bala,
    Impacto,
    Explosao,
    Fogo,
    Frio,
    Magica
}

public enum ModoAtaque
{
    Projetil,
    Radial,
    Pulso,
    Instantaneo,
    CorpoACorpo,
    Bumerangue,
    PontoFixo,
    Nenhum
}

public enum ModoMovimento
{
    Parado,
    Seguir,
    Patrulhar,
    Circular
}

public enum Prioridade
{
    Primeiro,
    Ultimo,
    MaisPerto,
    MaisForte,
    Mouse
}

public enum ClasseTorre
{
    Peoes,
    Farrapos,
    Lendas,
    Estancia
}

public enum TipoEspecial
{
    Disparo,
    Pulso,
    Rede,
    Zona,
    PuxarGrande,
    Carregar,
    Armadilha,
    Espantalho,
    BuffTorre,
    Almas
}

public sealed class Especial
{
    public TipoEspecial tipo;
    public string chave = "";
    public float intervalo = 5f;
    public float raio = 1f;
    public float duracao = 2f;
    public int quantidade = 1;
    public int maximo = 10;
    public float distancia = 1f;
    public bool preferirGrandes;
    public bool esperto;
    public bool mata;
    public bool captura;
    public int bonusPilas;
    public bool porOnda;
    public bool permanente;
    public float buffDano;
    public float buffAlcance;
    public float buffCadencia;
    public string desenho = "";
    public Color cor = Color.white;
    public AtributosTorre ataque = new AtributosTorre();
}

public sealed class AtributosTorre
{
    public ModoAtaque modo = ModoAtaque.Projetil;
    public TipoDano tipoDano = TipoDano.Corte;
    public int dano = 1;
    public float alcance = 2f;
    public float intervalo = 1f;
    public int perfuracao = 1;
    public int quantidade = 1;
    public float abertura;
    public int alvos = 1;
    public int rajada = 1;
    public int armasNaMao = 1;
    public bool teleguiado;
    public bool tiroTraseiro;
    public bool lateraisMetade;
    public bool rolaPeloCaminho;
    public bool comecaNoFimDoCaminho;
    public float velocidadeProjetil = 10f;
    public float escalaProjetil = 1f;
    public string desenhoProjetil = "";
    public Color corProjetil = Color.white;
    public bool giraProjetil;
    public float raioExplosao;
    public float imprecisao;
    public int ricochetes;
    public int estilhacos;
    public bool estilhacosExplodem;
    public int zonaDano;
    public float zonaDuracao;
    public float zonaRaio = 0.6f;
    public bool veFurtivo;
    public bool furaBlindagem;
    public bool ignoraImunidades;
    public int danoExtraGrandes;
    public int danoExtraFurtivos;
    public int criticoACada;
    public int multiplicadorCritico = 1;
    public int forca = 1;
    public float lentidao;
    public float duracaoLentidao = 1f;
    public float congelar;
    public float atordoar;
    public float chanceAtordoar = 1f;
    public float empurrar;
    public float chanceEmpurrar = 1f;
    public int queimaDano;
    public float queimaDuracao = 2f;
    public TipoDano tipoQueima = TipoDano.Fogo;
    public float marca;
    public float duracaoMarca = 3f;
    public bool revelaAoAcertar;
    public int bonusPilasAoCair;
    public bool viraPilas;
    public int explodeAoCair;
    public bool contagiaFilhotes;
    public float lentidaoNoAlcance;
    public float marcaNoAlcance;
    public float revelaRaio;
    public bool vigiaGalpao;
    public float multiplicadorVigia = 2f;
    public ModoMovimento movimento = ModoMovimento.Parado;
    public float velocidadeMovimento = 2.5f;
    public float raioCirculo = 2.2f;
    public int ajudantes;
    public int pilasPorOnda;
    public int vidasPorOnda;
    public int producao;
    public float multiplicadorProducao = 1f;
    public bool coletaAutomatica;
    public bool guaiaca;
    public float rendimento;
    public int limiteGuardado;
    public float bonusPilasGlobal;
    public float bonusPilasNoAlcance;
    public float auraAlcance;
    public float auraCadencia;
    public int auraDano;
    public int auraPerfuracao;
    public bool auraVeFurtivo;
    public bool auraFuraBlindagem;
    public bool auraIgnoraImunidades;
    public float auraDanoGrandes;
    public float auraDesconto;
    public float auraDescontoMelhoria;
    public float lentidaoGlobal;
    public float reducaoVidaGrandes;
    public bool miraNoMouse;
    public readonly List<Especial> especiais = new List<Especial>();

    public Especial Pegar(TipoEspecial tipo, string chave = "")
    {
        for (int i = 0; i < especiais.Count; i++)
        {
            if (especiais[i].tipo == tipo && especiais[i].chave == chave)
                return especiais[i];
        }
        Especial novo = new Especial { tipo = tipo, chave = chave };
        especiais.Add(novo);
        return novo;
    }

    public bool Tem(TipoEspecial tipo)
    {
        for (int i = 0; i < especiais.Count; i++)
        {
            if (especiais[i].tipo == tipo)
                return true;
        }
        return false;
    }

    public bool EhApoio()
    {
        return auraAlcance > 0f || auraCadencia > 0f || auraDano > 0 || auraPerfuracao > 0 || auraVeFurtivo
            || auraFuraBlindagem || auraIgnoraImunidades || auraDanoGrandes > 0f || auraDesconto > 0f || auraDescontoMelhoria > 0f;
    }

    public AtributosTorre Copiar()
    {
        return (AtributosTorre)MemberwiseClone();
    }
}

public sealed class Melhoria
{
    public readonly string nome;
    public readonly string descricao;
    public readonly System.Action<AtributosTorre> aplicar;

    public Melhoria(string nome, string descricao, System.Action<AtributosTorre> aplicar)
    {
        this.nome = nome;
        this.descricao = descricao;
        this.aplicar = aplicar;
    }
}

public sealed class DefinicaoTorre
{
    public string nome;
    public string descricao;
    public ClasseTorre classe;
    public int preco;
    public string corpo = "gaucho";
    public Color corPoncho = new Color(0.95f, 0.52f, 0.38f);
    public Color corLenco = new Color(0.76f, 0.29f, 0.21f);
    public string arma = "";
    public float comprimentoArma = 0.5f;
    public float escalaCorpo = 1f;
    public bool usaModeloDaCena;
    public bool soNaAgua;
    public bool aguaOuMargem;
    public bool ajudante;
    public string[] nomesCaminhos = new string[3];
    public Melhoria[][] caminhos = new Melhoria[3][];
    public System.Action<AtributosTorre> basico;

    public AtributosTorre CriarBasico()
    {
        AtributosTorre a = new AtributosTorre();
        if (basico != null)
            basico(a);
        return a;
    }
}

public static class TiposTorre
{
    static List<DefinicaoTorre> todas;

    public static List<DefinicaoTorre> Todas
    {
        get
        {
            if (todas == null)
            {
                todas = new List<DefinicaoTorre>();
                TorresPeoes.Adicionar(todas);
                TorresFarrapos.Adicionar(todas);
                TorresLendas.Adicionar(todas);
                TorresEstancia.Adicionar(todas);
            }
            return todas;
        }
    }

    public static List<DefinicaoTorre> DaClasse(ClasseTorre classe)
    {
        List<DefinicaoTorre> lista = new List<DefinicaoTorre>();
        foreach (DefinicaoTorre d in Todas)
        {
            if (d.classe == classe && !d.ajudante)
                lista.Add(d);
        }
        return lista;
    }

    public static DefinicaoTorre PeloNome(string nome)
    {
        foreach (DefinicaoTorre d in Todas)
        {
            if (d.nome == nome)
                return d;
        }
        return null;
    }

    public static string NomeDaClasse(ClasseTorre classe)
    {
        switch (classe)
        {
            case ClasseTorre.Peoes: return "PEÕES";
            case ClasseTorre.Farrapos: return "FARRAPOS";
            case ClasseTorre.Lendas: return "LENDAS";
            default: return "ESTÂNCIA";
        }
    }

    public static Melhoria M(string nome, string descricao, System.Action<AtributosTorre> aplicar)
    {
        return new Melhoria(nome, descricao, aplicar);
    }
}
