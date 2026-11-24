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
        lista.Add(Boleadeira());
        lista.Add(Fogueteiro());
        lista.Add(Assador());
        lista.Add(Minuano());
        lista.Add(Lacador());
        lista.Add(Revolver());
    }

    static DefinicaoTorre Boleadeira()
    {
        return new DefinicaoTorre
        {
            nome = "Gaúcho da Boleadeira",
            descricao = "A boleadeira gira em arco e volta, acertando vários no caminho (impacto: pega até blindado).",
            classe = ClasseTorre.Peoes,
            preco = 110,
            corPoncho = new Color(0.45f, 0.7f, 0.35f),
            arma = "boleadeira",
            comprimentoArma = 0.4f,
            basico = a =>
            {
                a.modo = ModoAtaque.Bumerangue;
                a.tipoDano = TipoDano.Impacto;
                a.dano = 1;
                a.alcance = 2.4f;
                a.intervalo = 1.2f;
                a.perfuracao = 4;
                a.velocidadeProjetil = 7f;
                a.desenhoProjetil = "bolas";
                a.giraProjetil = true;
            },
            nomesCaminhos = new[] { "Três-marias", "Enrosca-perna", "Braço Forte" },
            caminhos = new[]
            {
                new[]
                {
                    M("Boleadeira Pesada", "Perfuração +4.", a => a.perfuracao += 4),
                    M("Três-marias", "Dano +1 e perfuração +4.", a => { a.dano += 1; a.perfuracao += 4; }),
                    M("Ricocheteira", "A boleadeira quica de bicho em bicho, até 10.", a => { a.modo = ModoAtaque.Projetil; a.ricochetes = 10; a.velocidadeProjetil = 9f; }),
                    M("Bolas de Ferro", "Cadência +50%, ricochete mais rápido e +3 em grandes.", a => { a.intervalo /= 1.5f; a.velocidadeProjetil *= 1.3f; a.danoExtraGrandes += 3; }),
                    M("Tempestade de Bolas", "Quica em até 50 bichos e dano +3.", a => { a.ricochetes = 50; a.dano += 3; })
                },
                new[]
                {
                    M("Cordas de Couro", "Bichos atingidos ficam 20% mais lentos por 1 s.", a => { a.lentidao = 0.2f; a.duracaoLentidao = 1f; }),
                    M("Enrosca-perna", "15% de chance de derrubar o bicho pequeno por 1 s.", a => { a.atordoar = 1f; a.chanceAtordoar = 0.15f; a.forca = 1; }),
                    M("Boleadeira Dupla", "Joga 2 por vez e derruba também os especiais.", a => { a.quantidade = 2; a.abertura = 30f; a.forca = 2; }),
                    M("Tombo Garantido", "Todo acerto derruba por 0,5 s e deixa 30% mais lento.", a => { a.chanceAtordoar = 1f; a.atordoar = 0.5f; a.lentidao = 0.3f; }),
                    M("Rei da Boleadeira", "Derruba até bichos grandes por 1 s.", a => { a.forca = 4; a.atordoar = 1f; })
                },
                new[]
                {
                    M("Braço Longo", "Alcance +20%.", a => a.alcance *= 1.2f),
                    M("Giro Rápido", "Cadência +30%.", a => a.intervalo /= 1.3f),
                    M("Arremesso de Longe", "Alcance +30% e a boleadeira voa mais rápido.", a => { a.alcance *= 1.3f; a.velocidadeProjetil *= 1.3f; }),
                    M("Ventania de Bolas", "4 boleadeiras por vez, em direções diferentes.", a => { a.quantidade = 4; a.abertura = 90f; }),
                    M("Boleador do Pampa", "Boleadeiras girando em volta dele sem parar (dano 2 a cada 0,3 s).", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso);
                        e.intervalo = 0.3f;
                        e.raio = 1.3f;
                        e.cor = new Color(0.9f, 0.8f, 0.6f, 0.45f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Impacto, dano = 2 };
                    })
                }
            }
        };
    }

    static DefinicaoTorre Fogueteiro()
    {
        return new DefinicaoTorre
        {
            nome = "Fogueteiro",
            descricao = "Rojão que explode numa área (explosão). Não pega Tuco-tuco, Zorrilho nem Jacaré.",
            classe = ClasseTorre.Peoes,
            preco = 140,
            corPoncho = new Color(0.95f, 0.8f, 0.25f),
            arma = "rojao",
            comprimentoArma = 0.55f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Explosao;
                a.dano = 1;
                a.alcance = 2.6f;
                a.intervalo = 1.4f;
                a.raioExplosao = 0.7f;
                a.velocidadeProjetil = 6f;
                a.desenhoProjetil = "foguete";
            },
            nomesCaminhos = new[] { "Rojão Maior", "Pavio Curto", "Cacho de Bombas" },
            caminhos = new[]
            {
                new[]
                {
                    M("Pólvora Extra", "Área da explosão +30%.", a => a.raioExplosao *= 1.3f),
                    M("Rojão de 12 Tiros", "Dano +1 e área +20%.", a => { a.dano += 1; a.raioExplosao *= 1.2f; }),
                    M("Bombão de São João", "Dano +2, área +30% e atordoa os pequenos por 0,3 s.", a => { a.dano += 2; a.raioExplosao *= 1.3f; a.atordoar = 0.3f; a.forca = 1; }),
                    M("Morteiro de Festa", "Dano +3 e empurra os bichos pequenos.", a => { a.dano += 3; a.empurrar = 0.5f; }),
                    M("Fogos de Ano-Novo", "Explosão gigante (área x1,8, dano +6) que atordoa até os grandes.", a => { a.raioExplosao *= 1.8f; a.dano += 6; a.atordoar = 1f; a.forca = 4; })
                },
                new[]
                {
                    M("Pavio Curto", "Cadência +25%.", a => a.intervalo /= 1.25f),
                    M("Acendedor Rápido", "Cadência +25% e alcance +10%.", a => { a.intervalo /= 1.25f; a.alcance *= 1.1f; }),
                    M("Busca-pé", "O rojão fica mais rápido e nunca erra o alvo.", a => { a.teleguiado = true; a.velocidadeProjetil *= 1.5f; }),
                    M("Bateria de Busca-pés", "3 busca-pés por disparo.", a => { a.quantidade = 3; a.abertura = 18f; }),
                    M("Festa do Interior", "6 busca-pés e +10 de dano em grandes.", a => { a.quantidade = 6; a.abertura = 14f; a.danoExtraGrandes += 10; })
                },
                new[]
                {
                    M("Estilhaços", "A explosão solta 8 estilhaços de corte.", a => a.estilhacos = 8),
                    M("Cacho de Bombinhas", "Os estilhaços viram bombinhas que explodem.", a => a.estilhacosExplodem = true),
                    M("Bombinha em Cadeia", "12 bombinhas e dano +1.", a => { a.estilhacos = 12; a.dano += 1; }),
                    M("Pólvora Especial", "Passa a acertar quem é imune a explosão.", a => a.ignoraImunidades = true),
                    M("Chuva de Rojões", "20 bombinhas, dano +3 e área +30%.", a => { a.estilhacos = 20; a.dano += 3; a.raioExplosao *= 1.3f; })
                }
            }
        };
    }

    static DefinicaoTorre Minuano()
    {
        return new DefinicaoTorre
        {
            nome = "Gaúcho do Minuano",
            descricao = "Sopra o vento frio e congela os bichos em volta (frio). Não congela Pinguim, Zorrilho nem bichos grandes.",
            classe = ClasseTorre.Peoes,
            preco = 150,
            corPoncho = new Color(0.6f, 0.8f, 0.95f),
            corLenco = new Color(0.9f, 0.95f, 1f),
            basico = a =>
            {
                a.modo = ModoAtaque.Pulso;
                a.tipoDano = TipoDano.Frio;
                a.dano = 1;
                a.alcance = 1.6f;
                a.intervalo = 2f;
                a.congelar = 1f;
                a.forca = 2;
            },
            nomesCaminhos = new[] { "Geada", "Vento Encanado", "Pingente" },
            caminhos = new[]
            {
                new[]
                {
                    M("Geada Forte", "Congela 0,5 s a mais.", a => a.congelar += 0.5f),
                    M("Geada Negra", "Depois de descongelar, o bicho fica 50% mais lento por 2 s.", a => { a.lentidao = 0.5f; a.duracaoLentidao = a.congelar + 2f; }),
                    M("Granizo", "Dano +2.", a => a.dano += 2),
                    M("Frio de São José dos Ausentes", "Congela até bichos grandes e dano +2.", a => { a.forca = 4; a.dano += 2; }),
                    M("Neve na Serra", "Congelados tomam o dobro de dano de qualquer gaúcho.", a => { a.marca = 1f; a.duracaoMarca = a.congelar + 0.5f; a.forca = 5; })
                },
                new[]
                {
                    M("Ventania", "Alcance +20%.", a => a.alcance *= 1.2f),
                    M("Vento Gelado", "Todo bicho no alcance fica 30% mais lento.", a => a.lentidaoNoAlcance = 0.3f),
                    M("Geada Branca", "Revela os furtivos no alcance.", a => a.revelaRaio = a.alcance),
                    M("Frente Fria", "Lentidão de 45% e bichos no alcance tomam +25% de dano.", a => { a.lentidaoNoAlcance = 0.45f; a.marcaNoAlcance = 0.25f; }),
                    M("Massa de Ar Polar", "Alcance +50% e 50% de lentidão no alcance.", a => { a.alcance *= 1.5f; a.lentidaoNoAlcance = 0.5f; a.revelaRaio = a.alcance; })
                },
                new[]
                {
                    M("Gelo Afiado", "Dano +1.", a => a.dano += 1),
                    M("Lança de Gelo", "Passa a atirar pingentes de longe, que congelam por um instante.", a =>
                    {
                        a.modo = ModoAtaque.Projetil;
                        a.alcance *= 1.8f;
                        a.intervalo = 0.9f;
                        a.perfuracao = 3;
                        a.congelar = Mathf.Max(0.5f, a.congelar * 0.5f);
                        a.velocidadeProjetil = 10f;
                        a.desenhoProjetil = "gelo";
                    }),
                    M("Estilhaço de Gelo", "Os pingentes se partem em 4 estilhaços ao acertar.", a => a.estilhacos = 4),
                    M("Gelo Eterno", "Congela até bichos grandes e +4 de dano neles.", a => { a.forca = 4; a.danoExtraGrandes += 4; }),
                    M("Era do Gelo no Pampa", "Pingentes enormes: dano +6, perfuração +10 e congelam 1 s.", a => { a.dano += 6; a.perfuracao += 10; a.escalaProjetil = 2f; a.congelar = 1f; })
                }
            }
        };
    }

    static DefinicaoTorre Lacador()
    {
        return new DefinicaoTorre
        {
            nome = "Laçador",
            descricao = "Joga o laço no bicho da frente: fica 50% mais lento por 2 s (bichos grandes sentem metade).",
            classe = ClasseTorre.Peoes,
            preco = 90,
            corPoncho = new Color(0.65f, 0.45f, 0.3f),
            arma = "laco",
            comprimentoArma = 0.4f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Impacto;
                a.dano = 0;
                a.alcance = 2.3f;
                a.intervalo = 1f;
                a.lentidao = 0.5f;
                a.duracaoLentidao = 2f;
                a.velocidadeProjetil = 8f;
                a.desenhoProjetil = "lacada";
            },
            nomesCaminhos = new[] { "Laço Firme", "Laço com Espinho", "Armada Larga" },
            caminhos = new[]
            {
                new[]
                {
                    M("Couro Cru", "A lentidão dura 3 s.", a => a.duracaoLentidao = 3f),
                    M("Laço Trançado", "Pega 2 bichos por vez.", a => a.perfuracao = 2),
                    M("Pealo", "Cada laçada derruba o bicho por 1 s.", a => { a.atordoar = 1f; a.forca = 2; }),
                    M("Tiro de Laço", "Laça 5 bichos por vez e derruba.", a => a.perfuracao = 5),
                    M("Campeão do Rodeio", "Laça até bichos grandes e segura por 1,5 s.", a => { a.forca = 4; a.atordoar = 1.5f; })
                },
                new[]
                {
                    M("Laço de Arame", "Laçados tomam 1 de dano por segundo.", a => { a.queimaDano = 1; a.queimaDuracao = 3f; a.tipoQueima = TipoDano.Corte; }),
                    M("Laço com Roseta", "2 de dano por segundo, e acerta o Tatu.", a => { a.queimaDano = 2; a.tipoQueima = TipoDano.Impacto; }),
                    M("Laço que Aperta", "5 de dano por segundo.", a => a.queimaDano = 5),
                    M("Correntão", "8 de dano por segundo e laçados tomam +30% de dano de todos.", a => { a.queimaDano = 8; a.marca = 0.3f; a.duracaoMarca = 3f; }),
                    M("Nó Gaúcho", "12 de dano por segundo e os filhotes de quem cai já nascem laçados.", a => { a.queimaDano = 12; a.contagiaFilhotes = true; })
                },
                new[]
                {
                    M("Armada Grande", "Alcance +25%.", a => a.alcance *= 1.25f),
                    M("Laçada Rápida", "Cadência +40%.", a => a.intervalo /= 1.4f),
                    M("Laço em Rede", "Laça todos os bichos de uma área pequena.", a => a.raioExplosao = 0.6f),
                    M("Laçada de Tropa", "Área maior e lentidão de 70%.", a => { a.raioExplosao = 0.9f; a.lentidao = 0.7f; }),
                    M("Grande Laçada", "A cada 2,5 s laça todos os bichos do alcance.", a => { a.modo = ModoAtaque.Pulso; a.intervalo = 2.5f; a.raioExplosao = 0f; })
                }
            }
        };
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
