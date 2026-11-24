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
        lista.Add(Bruxo());
        lista.Add(Monarca());
        lista.Add(Dancador());
        lista.Add(Benzedeira());
        lista.Add(Guardiao());
        lista.Add(Iara());
        lista.Add(Pia());
    }

    static readonly Color CorDoFogo = new Color(1f, 0.5f, 0.15f, 0.9f);

    static AtributosTorre Raio(int dano, int ricochetes, float alcance)
    {
        return new AtributosTorre
        {
            tipoDano = TipoDano.Magica,
            dano = dano,
            ricochetes = ricochetes,
            velocidadeProjetil = 16f,
            desenhoProjetil = "raio",
            alcance = alcance
        };
    }

    static DefinicaoTorre Bruxo()
    {
        return new DefinicaoTorre
        {
            nome = "Bruxo da Salamanca",
            descricao = "Faíscas mágicas que atravessam 2 bichos (mágica). Inspirado na lenda da Salamanca do Jarau.",
            classe = ClasseTorre.Lendas,
            preco = 140,
            corPoncho = new Color(0.55f, 0.35f, 0.75f),
            corLenco = new Color(0.3f, 0.2f, 0.45f),
            arma = "cajado",
            comprimentoArma = 0.75f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Magica;
                a.dano = 2;
                a.alcance = 2.3f;
                a.intervalo = 0.9f;
                a.perfuracao = 2;
                a.velocidadeProjetil = 9f;
                a.desenhoProjetil = "faisca";
            },
            nomesCaminhos = new[] { "Feitiço", "Fogo do Boitatá", "Assombração" },
            caminhos = new[]
            {
                new[]
                {
                    M("Faísca Forte", "Dano +1 e perfuração +2.", a => { a.dano += 1; a.perfuracao += 2; }),
                    M("Clarão", "Faíscas maiores e mais rápidas, cadência +20%.", a => { a.escalaProjetil = 1.4f; a.velocidadeProjetil *= 1.5f; a.intervalo /= 1.2f; }),
                    M("Raio da Salamanca", "A cada 2 s solta um raio que pula de bicho em bicho, até 6.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "raio");
                        e.intervalo = 2f;
                        e.ataque = Raio(2, 6, 3f);
                    }),
                    M("Tempestade Encantada", "O raio pula até 15 bichos e o dano sobe +2.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "raio");
                        e.ataque.ricochetes = 15;
                        e.ataque.dano += 2;
                    }),
                    M("Mestre da Salamanca", "Raios a cada 0,8 s com dano +6.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "raio");
                        e.intervalo = 0.8f;
                        e.ataque.dano += 6;
                    })
                },
                new[]
                {
                    M("Bola de Fogo", "A cada 2 s lança uma bola de fogo que explode (fogo).", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "bola");
                        e.intervalo = 2f;
                        e.ataque = new AtributosTorre
                        {
                            tipoDano = TipoDano.Fogo,
                            dano = 3,
                            raioExplosao = 0.6f,
                            teleguiado = true,
                            velocidadeProjetil = 8f,
                            desenhoProjetil = "fogo",
                            alcance = 3f
                        };
                    }),
                    M("Muro de Fogo", "A cada 6 s acende um muro de fogo no caminho (2 por segundo).", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Zona, "muro");
                        e.intervalo = 6f;
                        e.raio = 0.8f;
                        e.duracao = 4f;
                        e.cor = CorDoFogo;
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Fogo, dano = 2, veFurtivo = true };
                    }),
                    M("Bafo do Boitatá", "Passa a soprar chamas sem parar, que queimam.", a =>
                    {
                        a.tipoDano = TipoDano.Fogo;
                        a.intervalo = 0.15f;
                        a.perfuracao = 4;
                        a.alcance *= 0.85f;
                        a.queimaDano = 1;
                        a.queimaDuracao = 1.5f;
                        a.desenhoProjetil = "fogo";
                        a.velocidadeProjetil = 8f;
                    }),
                    M("Boitatá Pequeno", "Um boitatá voa em volta do bruxo queimando tudo (2 a cada 0,5 s).", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso, "boitata");
                        e.intervalo = 0.5f;
                        e.raio = 1.4f;
                        e.cor = CorDoFogo;
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Fogo, dano = 2 };
                    }),
                    M("Boitatá Ancião", "Um boitatá gigante percorre o caminho inteiro a cada 4 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "anciao");
                        e.intervalo = 4f;
                        e.ataque = new AtributosTorre
                        {
                            tipoDano = TipoDano.Fogo,
                            dano = 8,
                            perfuracao = 200,
                            rolaPeloCaminho = true,
                            comecaNoFimDoCaminho = true,
                            velocidadeProjetil = 6f,
                            desenhoProjetil = "boitata",
                            escalaProjetil = 2.5f,
                            danoExtraGrandes = 20,
                            queimaDano = 3,
                            queimaDuracao = 2f
                        };
                    })
                },
                new[]
                {
                    M("Sexto Sentido", "Vê furtivo.", a => a.veFurtivo = true),
                    M("Alma Penada", "Bicho derrubado por perto vira fantasma que anda ao contrário no caminho batendo nos outros (até 10).", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Almas);
                        e.maximo = 10;
                        e.ataque = new AtributosTorre
                        {
                            tipoDano = TipoDano.Magica,
                            dano = 1,
                            perfuracao = 3,
                            velocidadeProjetil = 1.8f,
                            desenhoProjetil = "fantasma",
                            corProjetil = new Color(1f, 1f, 1f, 0.8f),
                            veFurtivo = true
                        };
                    }),
                    M("Procissão das Almas", "Até 30 fantasmas, com dano 2 e perfuração 6.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Almas);
                        e.maximo = 30;
                        e.ataque.dano = 2;
                        e.ataque.perfuracao = 6;
                    }),
                    M("Cavalgada Fantasma", "Fantasmas 2x mais rápidos e dano +2.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Almas);
                        e.ataque.velocidadeProjetil *= 2f;
                        e.ataque.dano += 2;
                    }),
                    M("Senhor das Almas", "Dano +5, perfuração 20 e +10 em grandes.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Almas);
                        e.ataque.dano += 5;
                        e.ataque.perfuracao = 20;
                        e.ataque.danoExtraGrandes += 10;
                    })
                }
            }
        };
    }

    static DefinicaoTorre Monarca()
    {
        return new DefinicaoTorre
        {
            nome = "Monarca das Coxilhas",
            descricao = "O maior gaúcho do pampa: estala o relho sem parar e de longe (impacto).",
            classe = ClasseTorre.Lendas,
            preco = 1000,
            corPoncho = new Color(0.95f, 0.85f, 0.4f),
            corLenco = new Color(0.95f, 0.95f, 0.95f),
            arma = "relho",
            comprimentoArma = 0.8f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Impacto;
                a.dano = 1;
                a.alcance = 3.2f;
                a.intervalo = 0.09f;
                a.velocidadeProjetil = 16f;
                a.desenhoProjetil = "estalo";
            },
            nomesCaminhos = new[] { "Sol do Pampa", "Gaúcho de Ferro", "Lua Cheia" },
            caminhos = new[]
            {
                new[]
                {
                    M("Relho em Brasa", "Os estalos viram rajadas de calor (fogo), perfuração +2.", a =>
                    {
                        a.tipoDano = TipoDano.Fogo;
                        a.perfuracao += 2;
                        a.desenhoProjetil = "fogo";
                        a.escalaProjetil = 0.8f;
                    }),
                    M("Calor da Fronteira", "Dano +2 e fura blindagem.", a => { a.dano += 2; a.furaBlindagem = true; }),
                    M("Sol de Rachar", "Raios de sol que atravessam tudo (perfuração 20).", a => { a.perfuracao = 20; a.escalaProjetil = 1.5f; }),
                    M("Sol do Meio-dia", "Dano +5.", a => a.dano += 5),
                    M("Pôr do Sol do Guaíba", "Dano +15, +30 em grandes e perfuração +30.", a =>
                    {
                        a.dano += 15;
                        a.danoExtraGrandes += 30;
                        a.perfuracao += 30;
                        a.escalaProjetil = 2.2f;
                    })
                },
                new[]
                {
                    M("Bota de Ferro", "Alcance +20%.", a => a.alcance *= 1.2f),
                    M("Dois Braços", "Ataca 2 bichos ao mesmo tempo.", a => { a.alvos = 2; a.armasNaMao = 2; }),
                    M("Gaúcho de Lata", "Um dos braços solta um raio a cada 1 s (pula 4 bichos).", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "raio");
                        e.intervalo = 1f;
                        e.ataque = Raio(4, 4, 4f);
                    }),
                    M("Gaúcho Mecânico", "+8 de dano em grandes.", a => a.danoExtraGrandes += 8),
                    M("Gaúcho Robô", "Dano +6 e ataca 3 bichos ao mesmo tempo.", a => { a.dano += 6; a.alvos = 3; })
                },
                new[]
                {
                    M("Sombra da Noite", "Perfuração +3.", a => a.perfuracao += 3),
                    M("Uivo", "Vê furtivo e dano +1.", a => { a.veFurtivo = true; a.dano += 1; }),
                    M("Lobisomem dos Pampas", "Ignora qualquer imunidade e +5 em grandes.", a => { a.ignoraImunidades = true; a.danoExtraGrandes += 5; }),
                    M("Fera da Lua", "Cadência +40% e às vezes empurra os bichos.", a =>
                    {
                        a.intervalo /= 1.4f;
                        a.empurrar = 0.3f;
                        a.chanceEmpurrar = 0.2f;
                        a.forca = 2;
                    }),
                    M("Lenda da Noite Gaúcha", "Dano +10 e +25 em grandes.", a => { a.dano += 10; a.danoExtraGrandes += 25; })
                }
            }
        };
    }

    static DefinicaoTorre Benzedeira()
    {
        return new DefinicaoTorre
        {
            nome = "Benzedeira",
            descricao = "Joga garrafadas que espirram e corroem (mágica) e, com melhorias, fortalece os gaúchos com chá de macela.",
            classe = ClasseTorre.Lendas,
            preco = 200,
            corpo = "benzedeira",
            arma = "garrafa",
            comprimentoArma = 0.3f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Magica;
                a.dano = 2;
                a.alcance = 2.2f;
                a.intervalo = 1.2f;
                a.raioExplosao = 0.5f;
                a.queimaDano = 1;
                a.queimaDuracao = 3f;
                a.tipoQueima = TipoDano.Magica;
                a.velocidadeProjetil = 6f;
                a.desenhoProjetil = "garrafa";
                a.giraProjetil = true;
            },
            nomesCaminhos = new[] { "Chá de Macela", "Garrafada Braba", "Simpatia" },
            caminhos = new[]
            {
                new[]
                {
                    M("Garrafada Forte", "O melado corrói mais rápido (2 por segundo).", a => a.queimaDano = 2),
                    M("Chá de Macela", "A cada 6 s dá chá ao gaúcho mais forte perto: +25% de dano e alcance por 5 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.BuffTorre);
                        e.intervalo = 6f;
                        e.duracao = 5f;
                        e.quantidade = 1;
                        e.buffDano = 0.25f;
                        e.buffAlcance = 0.25f;
                    }),
                    M("Mate Forte", "O chá também dá +25% de cadência.", a => a.Pegar(TipoEspecial.BuffTorre).buffCadencia = 0.25f),
                    M("Benzedura de Proteção", "O chá dura 8 s e pega 2 gaúchos.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.BuffTorre);
                        e.duracao = 8f;
                        e.quantidade = 2;
                    }),
                    M("Benzedeira Mestra", "Gaúchos perto ficam sempre fortalecidos: +20% de cadência e alcance e dano +1.", a =>
                    {
                        a.auraCadencia = 0.2f;
                        a.auraAlcance = 0.2f;
                        a.auraDano = 1;
                    })
                },
                new[]
                {
                    M("Garrafada Amarga", "+3 de dano em grandes.", a => a.danoExtraGrandes += 3),
                    M("Espirro Largo", "Área +50%.", a => a.raioExplosao *= 1.5f),
                    M("Pega-javali", "Bichos atingidos tomam +30% de dano de todos por 3 s.", a => { a.marca = 0.3f; a.duracaoMarca = 3f; }),
                    M("Garrafada Explosiva", "Quando um grande atingido cai, explode nos vizinhos (dano 30).", a => a.explodeAoCair = 30),
                    M("Elixir da Salamanca", "Corrosão de 10 por segundo por 4 s.", a => { a.queimaDano = 10; a.queimaDuracao = 4f; })
                },
                new[]
                {
                    M("Arremesso Ligeiro", "Cadência +25%.", a => a.intervalo /= 1.25f),
                    M("Mistura Forte", "Dano +1 e área +20%.", a => { a.dano += 1; a.raioExplosao *= 1.2f; }),
                    M("Couro e Charque", "Bichos atingidos rendem +1 pila ao cair.", a => a.bonusPilasAoCair = 1),
                    M("Simpatia da Fartura", "Bichos atingidos rendem +3 pilas ao cair.", a => a.bonusPilasAoCair = 3),
                    M("Mão Santa", "Bichos do bando do campo atingidos viram pilas na hora.", a => a.viraPilas = true)
                }
            }
        };
    }

    static DefinicaoTorre Guardiao()
    {
        return new DefinicaoTorre
        {
            nome = "Guardião da Araucária",
            descricao = "Atira pinhões em leque (impacto) e, com melhorias, chama o temporal, o mato e a fartura da Serra.",
            classe = ClasseTorre.Lendas,
            preco = 160,
            corPoncho = new Color(0.25f, 0.5f, 0.25f),
            arma = "pinha",
            comprimentoArma = 0.35f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Impacto;
                a.dano = 1;
                a.quantidade = 3;
                a.abertura = 14f;
                a.alcance = 2.3f;
                a.intervalo = 0.95f;
                a.velocidadeProjetil = 9f;
                a.desenhoProjetil = "pinhao";
            },
            nomesCaminhos = new[] { "Temporal", "Mato Fechado", "Colheita do Pinhão" },
            caminhos = new[]
            {
                new[]
                {
                    M("Pinhão Duro", "Dano +1.", a => a.dano += 1),
                    M("Ventania da Serra", "A cada 4 s um redemoinho joga os bichos pequenos para trás.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Rede, "vento");
                        e.intervalo = 4f;
                        e.raio = 1f;
                        e.desenho = "redemoinho";
                        e.cor = new Color(0.85f, 0.9f, 1f, 0.9f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Impacto, dano = 0, empurrar = 1f, forca = 1 };
                    }),
                    M("Raio de Temporal", "A cada 1,5 s cai um raio que pula entre 4 bichos.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "raio");
                        e.intervalo = 1.5f;
                        e.ataque = Raio(2, 4, 3f);
                    }),
                    M("Temporal Forte", "Redemoinho maior que empurra especiais e raio que pula 8 bichos.", a =>
                    {
                        Especial vento = a.Pegar(TipoEspecial.Rede, "vento");
                        vento.raio = 1.4f;
                        vento.ataque.forca = 2;
                        a.Pegar(TipoEspecial.Disparo, "raio").ataque.ricochetes = 8;
                    }),
                    M("Tempestade da Serra", "Redemoinho a cada 2 s que empurra até grandes e raio com dano +4.", a =>
                    {
                        Especial vento = a.Pegar(TipoEspecial.Rede, "vento");
                        vento.intervalo = 2f;
                        vento.ataque.forca = 3;
                        a.Pegar(TipoEspecial.Disparo, "raio").ataque.dano += 4;
                    })
                },
                new[]
                {
                    M("Unha-de-gato", "Os pinhões viram espinhos que atravessam 3 bichos.", a => a.perfuracao += 2),
                    M("Cipó", "A cada 4 s cresce cipó no caminho: bichos ficam 30% mais lentos.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Zona, "cipo");
                        e.intervalo = 4f;
                        e.raio = 0.8f;
                        e.duracao = 3f;
                        e.cor = new Color(0.35f, 0.7f, 0.3f, 0.9f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Impacto, dano = 0, lentidao = 0.3f, duracaoLentidao = 0.4f, veFurtivo = true };
                    }),
                    M("Capão de Mato", "Os cipós dão 1 de dano por segundo e crescem mais.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Zona, "cipo");
                        e.ataque.dano = 1;
                        e.duracao = 5f;
                        e.raio *= 1.3f;
                    }),
                    M("Mata de Araucária", "Cipós com 3 de dano por segundo.", a => a.Pegar(TipoEspecial.Zona, "cipo").ataque.dano = 3),
                    M("Espírito do Pinheiral", "Cipós enormes com 7 de dano por segundo e 50% de lentidão.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Zona, "cipo");
                        e.raio *= 1.6f;
                        e.ataque.dano = 7;
                        e.ataque.lentidao = 0.5f;
                    })
                },
                new[]
                {
                    M("Pinha Cheia", "+10 pilas por onda.", a => a.pilasPorOnda += 10),
                    M("Safra de Pinhão", "+25 pilas por onda.", a => a.pilasPorOnda += 15),
                    M("Festa do Pinhão", "Gaúchos perto atacam 10% mais rápido.", a => a.auraCadencia = 0.1f),
                    M("Fartura da Serra", "+80 pilas por onda e gaúchos perto 15% mais rápidos.", a => { a.pilasPorOnda += 55; a.auraCadencia = 0.15f; }),
                    M("Coração do Pinheiral", "+200 pilas por onda e gaúchos perto com dano +1.", a => { a.pilasPorOnda += 120; a.auraDano = 1; })
                }
            }
        };
    }

    static DefinicaoTorre Iara()
    {
        return new DefinicaoTorre
        {
            nome = "Iara da Lagoa",
            descricao = "Fica na água ou na beira: jatos d'água que atravessam 3 bichos (mágica) e um canto que encanta.",
            classe = ClasseTorre.Lendas,
            preco = 180,
            corpo = "iara",
            aguaOuMargem = true,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Magica;
                a.dano = 2;
                a.alcance = 2.4f;
                a.intervalo = 0.8f;
                a.perfuracao = 3;
                a.velocidadeProjetil = 10f;
                a.desenhoProjetil = "gota";
            },
            nomesCaminhos = new[] { "Canto da Iara", "Correnteza", "Fisga" },
            caminhos = new[]
            {
                new[]
                {
                    M("Voz Doce", "Bichos atingidos ficam 20% mais lentos por 1 s.", a => { a.lentidao = 0.2f; a.duracaoLentidao = 1f; }),
                    M("Canto que Encanta", "10% de chance de o bicho parar encantado por 1,5 s.", a => { a.atordoar = 1.5f; a.chanceAtordoar = 0.1f; a.forca = 2; }),
                    M("Serenata", "A cada 6 s encanta todos os pequenos em volta por 1 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso, "canto");
                        e.intervalo = 6f;
                        e.raio = a.alcance;
                        e.cor = new Color(0.6f, 0.9f, 1f, 0.8f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Magica, dano = 0, atordoar = 1f, forca = 1 };
                    }),
                    M("Coral das Águas", "O canto encanta especiais e os encantados tomam +20% de dano.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso, "canto");
                        e.ataque.forca = 2;
                        e.ataque.marca = 0.2f;
                        e.ataque.duracaoMarca = 2f;
                    }),
                    M("Canto do Entardecer", "Encanta até bichos grandes, a cada 4 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso, "canto");
                        e.ataque.forca = 4;
                        e.intervalo = 4f;
                    })
                },
                new[]
                {
                    M("Jato Forte", "Dano +1.", a => a.dano += 1),
                    M("Onda", "A cada 5 s uma onda empurra os pequenos para trás.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso, "onda");
                        e.intervalo = 5f;
                        e.raio = a.alcance;
                        e.cor = new Color(0.3f, 0.6f, 0.95f, 0.8f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Impacto, dano = 0, empurrar = 1f, forca = 1 };
                    }),
                    M("Correnteza Forte", "Ondas maiores que empurram os especiais.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso, "onda");
                        e.ataque.empurrar = 1.5f;
                        e.ataque.forca = 2;
                    }),
                    M("Maré Alta", "Ondas a cada 3 s que empurram até os grandes.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso, "onda");
                        e.intervalo = 3f;
                        e.ataque.forca = 4;
                    }),
                    M("Rainha das Águas", "Ondas com dano 5 que empurram tudo.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso, "onda");
                        e.ataque.dano = 5;
                        e.ataque.forca = 5;
                    })
                },
                new[]
                {
                    M("Fisga", "Dano +1 e perfuração +2.", a => { a.dano += 1; a.perfuracao += 2; }),
                    M("Fisga Tripla", "3 jatos por vez.", a => { a.quantidade = 3; a.abertura = 12f; }),
                    M("Fisga de Prata", "Os jatos ignoram imunidades (acertam até o Sapo-cururu).", a => a.ignoraImunidades = true),
                    M("Tridente da Lagoa", "+5 de dano em grandes.", a => a.danoExtraGrandes += 5),
                    M("Senhora da Lagoa", "Dano +4 e cadência +50%.", a => { a.dano += 4; a.intervalo /= 1.5f; })
                }
            }
        };
    }

    static DefinicaoTorre Pia()
    {
        return new DefinicaoTorre
        {
            nome = "Piá da Pandorga",
            descricao = "O piá empina a pandorga e manda rajadas de vento que empurram os bichos pequenos (impacto).",
            classe = ClasseTorre.Lendas,
            preco = 90,
            corpo = "pia",
            corPoncho = new Color(0.95f, 0.6f, 0.3f),
            escalaCorpo = 0.8f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Impacto;
                a.dano = 1;
                a.alcance = 2.2f;
                a.intervalo = 0.9f;
                a.perfuracao = 2;
                a.empurrar = 0.3f;
                a.forca = 1;
                a.velocidadeProjetil = 9f;
                a.desenhoProjetil = "vento";
            },
            nomesCaminhos = new[] { "Vento Norte", "Trovoada", "Céu de Agosto" },
            caminhos = new[]
            {
                new[]
                {
                    M("Rabiola Comprida", "Alcance +20%.", a => a.alcance *= 1.2f),
                    M("Vento Norte", "Empurrão maior.", a => a.empurrar += 0.3f),
                    M("Redemoinho", "A cada 5 s um redemoinho joga vários bichos para trás.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Rede, "redemoinho");
                        e.intervalo = 5f;
                        e.raio = 1.1f;
                        e.desenho = "redemoinho";
                        e.cor = new Color(0.9f, 0.95f, 1f, 0.9f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Impacto, dano = 1, empurrar = 1.2f, forca = 1 };
                    }),
                    M("Vento Norte Brabo", "Empurra os especiais também.", a =>
                    {
                        a.forca = 2;
                        a.Pegar(TipoEspecial.Rede, "redemoinho").ataque.forca = 2;
                    }),
                    M("Dono do Vento", "Empurra até os grandes e solta redemoinhos a cada 2 s.", a =>
                    {
                        a.forca = 3;
                        Especial e = a.Pegar(TipoEspecial.Rede, "redemoinho");
                        e.ataque.forca = 4;
                        e.intervalo = 2f;
                    })
                },
                new[]
                {
                    M("Linha Forte", "Dano +1.", a => a.dano += 1),
                    M("Faísca na Linha", "As rajadas viram choques (mágica) que pulam para mais 2 bichos.", a => { a.tipoDano = TipoDano.Magica; a.ricochetes = 2; }),
                    M("Raio na Pandorga", "A cada 2 s um raio pula em até 8 bichos.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "raio");
                        e.intervalo = 2f;
                        e.ataque = Raio(2, 8, 3.5f);
                    }),
                    M("Trovão Forte", "Raio a cada 1,2 s com dano +3.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "raio");
                        e.intervalo = 1.2f;
                        e.ataque.dano += 3;
                    }),
                    M("Tempestade de Verão", "3 raios a cada 1 s com dano +4.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "raio");
                        e.quantidade = 3;
                        e.intervalo = 1f;
                        e.ataque.dano += 4;
                    })
                },
                new[]
                {
                    M("Duas Pandorgas", "2 rajadas por vez.", a => { a.quantidade = 2; a.abertura = 12f; a.armasNaMao = 2; }),
                    M("Pandorga Grande", "Perfuração +3.", a => a.perfuracao += 3),
                    M("Festival de Pandorgas", "4 rajadas por vez.", a => { a.quantidade = 4; a.abertura = 12f; }),
                    M("Pandorga Rasante", "Vê furtivo e +2 de dano neles.", a => { a.veFurtivo = true; a.danoExtraFurtivos += 2; }),
                    M("Céu de Agosto", "8 rajadas por vez e alcance dobrado.", a => { a.quantidade = 8; a.abertura = 9f; a.alcance *= 2f; })
                }
            }
        };
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
                    M("Sapateado Duplo", "2 rosetas por vez.", a => { a.quantidade = 2; a.abertura = 10f; a.armasNaMao = 2; }),
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
