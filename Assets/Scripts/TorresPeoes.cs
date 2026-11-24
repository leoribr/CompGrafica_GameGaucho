using System.Collections.Generic;
using UnityEngine;

public static class TorresPeoes
{
    static Melhoria M(string nome, string descricao, System.Action<AtributosTorre> aplicar)
    {
        return new Melhoria(nome, descricao, aplicar);
    }

    public static void Adicionar(List<DefinicaoTorre> lista)
    {
        lista.Add(Peixeira());
        lista.Add(Assador());
        lista.Add(Revolver());
    }

    static DefinicaoTorre Peixeira()
    {
        return new DefinicaoTorre
        {
            nome = "Peão da Peixeira",
            descricao = "Barato e ligeiro: arremessa peixeiras que atravessam 2 bichos (corte).",
            classe = ClasseTorre.Peoes,
            preco = 70,
            corPoncho = new Color(0.85f, 0.75f, 0.55f),
            arma = "peixeira",
            comprimentoArma = 0.45f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Corte;
                a.dano = 1;
                a.alcance = 2.2f;
                a.intervalo = 0.95f;
                a.perfuracao = 2;
                a.velocidadeProjetil = 11f;
                a.desenhoProjetil = "faca";
            },
            nomesCaminhos = new[] { "Fio da Navalha", "Mão Ligeira", "Olho de Gavião" },
            caminhos = new[]
            {
                new[]
                {
                    M("Peixeira Afiada", "Perfuração +1.", a => a.perfuracao += 1),
                    M("Pedra de Afiar", "Perfuração +2 e dano +1.", a => { a.perfuracao += 2; a.dano += 1; }),
                    M("Facão Rolante", "Lança um facão que rola pelo caminho e atravessa 20 bichos.", a =>
                    {
                        a.rolaPeloCaminho = true;
                        a.perfuracao += 15;
                        a.desenhoProjetil = "facao";
                        a.giraProjetil = true;
                        a.velocidadeProjetil = 4.5f;
                        a.intervalo *= 1.6f;
                    }),
                    M("Facão de Prata", "Facão maior: perfuração +20, dano +1 e fura blindagem.", a =>
                    {
                        a.perfuracao += 20;
                        a.dano += 1;
                        a.furaBlindagem = true;
                        a.escalaProjetil = 1.3f;
                        a.corProjetil = new Color(0.85f, 0.9f, 1f);
                    }),
                    M("Facão Gigante", "Facão enorme (dano +3, perfuração +60) que se parte em 8 peixeiras no fim.", a =>
                    {
                        a.dano += 3;
                        a.perfuracao += 60;
                        a.escalaProjetil = 1.8f;
                        a.estilhacos = 8;
                    })
                },
                new[]
                {
                    M("Pulso Firme", "Cadência +15%.", a => a.intervalo /= 1.15f),
                    M("Mão Rápida", "Cadência +33%.", a => a.intervalo /= 1.33f),
                    M("Três Peixeiras", "Arremessa 3 peixeiras de uma vez.", a => { a.quantidade = 3; a.abertura = 12f; }),
                    M("Mestre da Peixeira", "Cadência dobrada e peixeiras que furam blindagem.", a => { a.intervalo /= 2f; a.furaBlindagem = true; }),
                    M("Lenda da Faca", "6 peixeiras por vez, cadência dobrada e dano +1.", a => { a.quantidade = 6; a.abertura = 9f; a.intervalo /= 2f; a.dano += 1; })
                },
                new[]
                {
                    M("Mira Longa", "Alcance +25%.", a => a.alcance *= 1.25f),
                    M("Olho de Vaqueano", "Vê furtivo.", a => a.veFurtivo = true),
                    M("Arremesso Certeiro", "Dano +2, perfuração +2 e alcance +30%.", a => { a.dano += 2; a.perfuracao += 2; a.alcance *= 1.3f; a.velocidadeProjetil *= 1.4f; }),
                    M("Golpe Certeiro", "1 a cada 8 arremessos dá dano 10x.", a => { a.criticoACada = 8; a.multiplicadorCritico = 10; }),
                    M("Mestre Cuteleiro", "Dano +6, alcance +50%, fura blindagem e crítico a cada 4.", a =>
                    {
                        a.dano += 6;
                        a.alcance *= 1.5f;
                        a.criticoACada = 4;
                        a.furaBlindagem = true;
                    })
                }
            }
        };
    }

    static DefinicaoTorre Assador()
    {
        return new DefinicaoTorre
        {
            nome = "Assador do Churrasco",
            descricao = "Atira 8 espetos para todos os lados, de perto (corte).",
            classe = ClasseTorre.Peoes,
            preco = 100,
            corPoncho = new Color(0.95f, 0.95f, 0.9f),
            arma = "espeto",
            comprimentoArma = 0.6f,
            basico = a =>
            {
                a.modo = ModoAtaque.Radial;
                a.tipoDano = TipoDano.Corte;
                a.dano = 1;
                a.quantidade = 8;
                a.alcance = 1.7f;
                a.intervalo = 1f;
                a.perfuracao = 1;
                a.velocidadeProjetil = 8f;
                a.desenhoProjetil = "espetinho";
            },
            nomesCaminhos = new[] { "Espeto Corrido", "Fogo de Chão", "Espetão" },
            caminhos = new[]
            {
                new[]
                {
                    M("Espeto Ligeiro", "Cadência +30%.", a => a.intervalo /= 1.3f),
                    M("Mão de Assador", "Cadência +50%.", a => a.intervalo /= 1.5f),
                    M("Rodízio", "Cadência +30% e cada espeto atravessa 3 bichos.", a => { a.intervalo /= 1.3f; a.perfuracao += 2; }),
                    M("Rodízio Completo", "Cadência +50% e dano +1.", a => { a.intervalo /= 1.5f; a.dano += 1; }),
                    M("Churrascaria Gaúcha", "Dispara espetos sem parar (cadência x3).", a => a.intervalo /= 3f)
                },
                new[]
                {
                    M("Brasa Viva", "Espetos em brasa: queimam 1 por segundo.", a =>
                    {
                        a.queimaDano = 1;
                        a.queimaDuracao = 2f;
                        a.tipoQueima = TipoDano.Fogo;
                        a.corProjetil = new Color(1f, 0.7f, 0.5f);
                    }),
                    M("Carvão Bom", "Os espetos viram fogo: a brasa passa o casco do Tatu.", a => { a.tipoDano = TipoDano.Fogo; a.queimaDuracao = 3f; }),
                    M("Anel de Brasa", "Solta um anel de fogo que acerta todos no alcance.", a =>
                    {
                        a.modo = ModoAtaque.Pulso;
                        a.tipoDano = TipoDano.Fogo;
                        a.dano += 1;
                        a.intervalo *= 1.2f;
                    }),
                    M("Braseiro", "Anel maior, dano +2 e +3 em grandes.", a => { a.alcance *= 1.2f; a.dano += 2; a.danoExtraGrandes += 3; }),
                    M("Costelão de 12 Horas", "Anel enorme (dano +4) e labaredas que vão atrás dos grandes.", a =>
                    {
                        a.alcance *= 1.3f;
                        a.dano += 4;
                        Especial e = a.Pegar(TipoEspecial.Disparo);
                        e.intervalo = 1f;
                        e.preferirGrandes = true;
                        e.quantidade = 2;
                        e.ataque = new AtributosTorre
                        {
                            tipoDano = TipoDano.Fogo,
                            dano = 15,
                            teleguiado = true,
                            alcance = 4f,
                            velocidadeProjetil = 7f,
                            desenhoProjetil = "fogo",
                            escalaProjetil = 1.4f,
                            queimaDano = 3,
                            queimaDuracao = 2f
                        };
                    })
                },
                new[]
                {
                    M("Espeto Comprido", "Alcance +20%.", a => a.alcance *= 1.2f),
                    M("Mais Espetos", "10 espetos por vez.", a => a.quantidade = 10),
                    M("Espeto de Ferro", "Os espetos furam blindagem.", a => a.furaBlindagem = true),
                    M("Espetada Dupla", "16 espetos e perfuração +2.", a => { a.quantidade = 16; a.perfuracao += 2; }),
                    M("Mestre Assador", "32 espetos por vez e dano +2.", a => { a.quantidade = 32; a.dano += 2; })
                }
            }
        };
    }

    static DefinicaoTorre Revolver()
    {
        return new DefinicaoTorre
        {
            nome = "Gaúcho do Revólver",
            descricao = "Tiro rápido que persegue o bicho (bala).",
            classe = ClasseTorre.Peoes,
            preco = 90,
            usaModeloDaCena = true,
            arma = "revolver",
            comprimentoArma = 0.6f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Bala;
                a.dano = 2;
                a.alcance = 2.125f;
                a.intervalo = 0.48f;
                a.velocidadeProjetil = 12f;
            },
            nomesCaminhos = new[] { "Guaiaca Cheia", "Olho de Carancho", "Bala Ricocheteira" },
            caminhos = new[]
            {
                new[]
                {
                    M("Cartuchos na Guaiaca", "Cadência +15%.", a => a.intervalo /= 1.15f),
                    M("Dedo Nervoso", "Cadência +30%.", a => a.intervalo /= 1.3f),
                    M("Par de Garruchas", "Atira com duas armas ao mesmo tempo.", a => a.quantidade = 2),
                    M("Tambor Cheio", "Rajadas de 6 tiros antes de recarregar.", a => { a.rajada = 6; a.intervalo *= 2.2f; }),
                    M("Atirador da Fronteira", "Rajadas mais rápidas, dano +2 e fura blindagem.", a => { a.intervalo /= 1.5f; a.dano += 2; a.furaBlindagem = true; })
                },
                new[]
                {
                    M("Mira Firme", "Alcance +20%.", a => a.alcance *= 1.2f),
                    M("Olho de Carancho", "Vê furtivo.", a => a.veFurtivo = true),
                    M("Ponta de Aço", "Fura blindagem.", a => a.furaBlindagem = true),
                    M("Tiro Certeiro", "1 a cada 6 tiros dá dano 8x.", a => { a.criticoACada = 6; a.multiplicadorCritico = 8; }),
                    M("Bala de Prata", "+20 de dano em grandes e marca o alvo: todos dão +50% de dano nele.", a =>
                    {
                        a.danoExtraGrandes += 20;
                        a.marca = 0.5f;
                        a.duracaoMarca = 3f;
                    })
                },
                new[]
                {
                    M("Ricochete", "A bala quica para um segundo bicho.", a => a.ricochetes = 1),
                    M("Ricochete Duplo", "A bala quica para 3 bichos.", a => a.ricochetes = 3),
                    M("Bala que Atravessa", "Perfuração 5.", a => a.perfuracao += 4),
                    M("Bala de Rifle Velho", "Perfuração 30, bala mais rápida e alcance +20%.", a => { a.perfuracao += 25; a.velocidadeProjetil *= 1.6f; a.alcance *= 1.2f; }),
                    M("Duelo ao Pôr do Sol", "A bala quica em até 10 bichos e o dano sobe +2.", a => { a.ricochetes = 10; a.dano += 2; })
                }
            }
        };
    }
}
