using System.Collections.Generic;
using UnityEngine;

public static class TorresLendas
{
    static Melhoria M(string nome, string descricao, System.Action<AtributosTorre> aplicar)
    {
        return new Melhoria(nome, descricao, aplicar);
    }

    public static void Adicionar(List<DefinicaoTorre> lista)
    {
        lista.Add(Dancador());
    }

    static DefinicaoTorre Dancador()
    {
        return new DefinicaoTorre
        {
            nome = "Dançador da Chula",
            descricao = "Atira rosetas de espora bem rápido e vê furtivo: sente o bicho escondido pelo sapateado.",
            classe = ClasseTorre.Lendas,
            preco = 150,
            corPoncho = new Color(0.85f, 0.2f, 0.2f),
            corLenco = new Color(0.95f, 0.95f, 0.95f),
            arma = "roseta",
            comprimentoArma = 0.3f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Corte;
                a.dano = 1;
                a.alcance = 2.2f;
                a.intervalo = 0.38f;
                a.perfuracao = 2;
                a.veFurtivo = true;
                a.velocidadeProjetil = 12f;
                a.desenhoProjetil = "roseta";
                a.giraProjetil = true;
            },
            nomesCaminhos = new[] { "Sapateado", "Distração", "Fandango" },
            caminhos = new[]
            {
                new[]
                {
                    M("Pé Ligeiro", "Cadência +40%.", a => a.intervalo /= 1.4f),
                    M("Rosetas Afiadas", "Perfuração +2 e dano +1.", a => { a.perfuracao += 2; a.dano += 1; }),
                    M("Sapateado Duplo", "2 rosetas por vez.", a => { a.quantidade = 2; a.abertura = 10f; }),
                    M("Mestre da Chula", "4 rosetas por vez e cadência +50%.", a => { a.quantidade = 4; a.abertura = 9f; a.intervalo /= 1.5f; }),
                    M("Lenda do Sapateado", "Rosetas gigantes que vão e voltam: dano +5 e perfuração +10.", a =>
                    {
                        a.modo = ModoAtaque.Bumerangue;
                        a.escalaProjetil = 2.2f;
                        a.dano += 5;
                        a.perfuracao += 10;
                        a.quantidade = 2;
                        a.abertura = 40f;
                        a.velocidadeProjetil = 8f;
                    })
                },
                new[]
                {
                    M("Dança que Distrai", "15% de chance do bicho atingido voltar um pouco no caminho.", a => { a.chanceEmpurrar = 0.15f; a.empurrar = 0.8f; a.forca = 1; }),
                    M("Batida do Tacão", "A cada 5 s atordoa os bichos pequenos em volta por 1 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso);
                        e.intervalo = 5f;
                        e.raio = 1.4f;
                        e.cor = new Color(1f, 0.9f, 0.5f, 0.9f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Impacto, dano = 0, atordoar = 1f, forca = 1 };
                    }),
                    M("Dança da Lança", "O atordoamento pega bichos especiais e dura 1,5 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso);
                        e.ataque.forca = 2;
                        e.ataque.atordoar = 1.5f;
                    }),
                    M("Pisada Forte", "O atordoamento pega até bichos grandes.", a => a.Pegar(TipoEspecial.Pulso).ataque.forca = 3),
                    M("Terremoto de Sapateado", "A cada 3 s, onda de choque com dano 10 que atordoa tudo em volta.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso);
                        e.intervalo = 3f;
                        e.raio *= 1.5f;
                        e.ataque.dano = 10;
                        e.ataque.forca = 5;
                    })
                },
                new[]
                {
                    M("Olho Vivo", "Alcance +20%.", a => a.alcance *= 1.2f),
                    M("Gaita ao Fundo", "Todos os bichos do mapa ficam 10% mais lentos.", a => a.lentidaoGlobal = 0.1f),
                    M("Baile no Galpão", "Todos os bichos do mapa ficam 25% mais lentos.", a => a.lentidaoGlobal = 0.25f),
                    M("Fandango até de Madrugada", "Bichos grandes entram no mapa com 25% menos vida.", a => a.reducaoVidaGrandes = 0.25f),
                    M("Grande Fandango", "Bichos 40% mais lentos e grandes com 40% menos vida.", a => { a.lentidaoGlobal = 0.4f; a.reducaoVidaGrandes = 0.4f; })
                }
            }
        };
    }
}
