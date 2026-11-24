using System.Collections.Generic;
using UnityEngine;

public static class TorresEstancia
{
    static Melhoria M(string nome, string descricao, System.Action<AtributosTorre> aplicar)
    {
        return new Melhoria(nome, descricao, aplicar);
    }

    public static void Adicionar(List<DefinicaoTorre> lista)
    {
        lista.Add(Charqueada());
        lista.Add(Alambrador());
        lista.Add(RodaDeChimarrao());
        lista.Add(Ferreiro());
        lista.Add(Domador());
    }

    static DefinicaoTorre Ferreiro()
    {
        return new DefinicaoTorre
        {
            nome = "Ferreiro da Estância",
            descricao = "Arremessa cravos de ferradura (corte) e, com melhorias, monta espantalhos, afia os gaúchos e arma arapucas.",
            classe = ClasseTorre.Estancia,
            preco = 160,
            corPoncho = new Color(0.35f, 0.3f, 0.28f),
            arma = "martelo",
            comprimentoArma = 0.5f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Corte;
                a.dano = 2;
                a.alcance = 1.8f;
                a.intervalo = 0.8f;
                a.perfuracao = 2;
                a.velocidadeProjetil = 10f;
                a.desenhoProjetil = "faca";
                a.corProjetil = new Color(0.75f, 0.75f, 0.8f);
            },
            nomesCaminhos = new[] { "Espantalho", "Bigorna", "Arapuca" },
            caminhos = new[]
            {
                new[]
                {
                    M("Espantalho", "A cada 10 s monta um espantalho que atira cravos por 20 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Espantalho);
                        e.intervalo = 10f;
                        e.duracao = 20f;
                        e.maximo = 3;
                        e.ataque = new AtributosTorre
                        {
                            modo = ModoAtaque.Projetil,
                            tipoDano = TipoDano.Corte,
                            dano = 1,
                            alcance = 1.8f,
                            intervalo = 0.9f,
                            perfuracao = 2,
                            velocidadeProjetil = 10f,
                            desenhoProjetil = "faca",
                            corProjetil = new Color(0.75f, 0.75f, 0.8f)
                        };
                    }),
                    M("Espantalho Ligeiro", "Monta um espantalho a cada 7 s.", a => a.Pegar(TipoEspecial.Espantalho).intervalo = 7f),
                    M("Espantalho com Rojão", "Os espantalhos atiram rojões que explodem.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Espantalho);
                        e.ataque.tipoDano = TipoDano.Explosao;
                        e.ataque.raioExplosao = 0.5f;
                        e.ataque.desenhoProjetil = "foguete";
                        e.ataque.dano = 2;
                    }),
                    M("Espantalho de Ferro", "Os espantalhos duram 35 s e furam blindagem.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Espantalho);
                        e.duracao = 35f;
                        e.ataque.furaBlindagem = true;
                    }),
                    M("Exército de Espantalhos", "Até 6 espantalhos, um a cada 3 s, com dano +3.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Espantalho);
                        e.intervalo = 3f;
                        e.maximo = 6;
                        e.ataque.dano += 3;
                    })
                },
                new[]
                {
                    M("Cravo Grande", "Dano +1.", a => a.dano += 1),
                    M("Ferradura Quente", "Cravos em brasa (fogo) que queimam 1 por segundo.", a =>
                    {
                        a.tipoDano = TipoDano.Fogo;
                        a.queimaDano = 1;
                        a.queimaDuracao = 2f;
                        a.corProjetil = new Color(1f, 0.6f, 0.3f);
                    }),
                    M("Afiar a Peixeira", "No começo de cada onda afia o gaúcho mais caro perto: +40% de cadência na onda toda.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.BuffTorre);
                        e.porOnda = true;
                        e.quantidade = 1;
                        e.buffCadencia = 0.4f;
                    }),
                    M("Bigorna Boa", "Afia 2 gaúchos por onda.", a => a.Pegar(TipoEspecial.BuffTorre).quantidade = 2),
                    M("Mestre Ferreiro", "Afia 3 gaúchos, e cada gaúcho afiado ganha +1 de dano para sempre.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.BuffTorre);
                        e.quantidade = 3;
                        e.permanente = true;
                    })
                },
                new[]
                {
                    M("Mira de Ferreiro", "Alcance +20%.", a => a.alcance *= 1.2f),
                    M("Cravo que Quica", "Os cravos ricocheteiam em mais 2 bichos.", a => a.ricochetes = 2),
                    M("Arapuca", "A cada 8 s arma uma arapuca no caminho que prende 3 bichos pequenos (+2 pilas cada).", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Armadilha);
                        e.intervalo = 8f;
                        e.maximo = 4;
                        e.captura = true;
                        e.bonusPilas = 2;
                        e.desenho = "arapuca";
                        e.duracao = 0f;
                        e.ataque = new AtributosTorre { forca = 1, perfuracao = 3 };
                    }),
                    M("Arapuca Grande", "Prende 5 bichos, inclusive especiais e capivaras.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Armadilha);
                        e.ataque.forca = 2;
                        e.ataque.perfuracao = 5;
                    }),
                    M("Armadilha de Javali", "Prende até um Javali, e cada bicho preso rende +20 pilas.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Armadilha);
                        e.ataque.forca = 3;
                        e.bonusPilas = 20;
                    })
                }
            }
        };
    }

    static DefinicaoTorre Domador()
    {
        return new DefinicaoTorre
        {
            nome = "Peão Domador",
            descricao = "Anda com um cusco que morde os bichos perto (corte); com melhorias, cachorrada, gado e aves de rapina.",
            classe = ClasseTorre.Estancia,
            preco = 110,
            corpo = "domador",
            corPoncho = new Color(0.8f, 0.65f, 0.45f),
            basico = a =>
            {
                a.modo = ModoAtaque.CorpoACorpo;
                a.tipoDano = TipoDano.Corte;
                a.dano = 2;
                a.alcance = 1.5f;
                a.intervalo = 0.8f;
                a.quantidade = 1;
            },
            nomesCaminhos = new[] { "Cachorrada", "Gado", "Aves de Rapina" },
            caminhos = new[]
            {
                new[]
                {
                    M("Cusco Brabo", "Dano +1.", a => a.dano += 1),
                    M("Ovelheiro", "Morde 30% mais rápido e às vezes pastoreia os pequenos para trás.", a =>
                    {
                        a.intervalo /= 1.3f;
                        a.empurrar = 0.4f;
                        a.chanceEmpurrar = 0.3f;
                        a.forca = 1;
                    }),
                    M("Cão de Faro", "Dano +2 e vê furtivo.", a => { a.dano += 2; a.veFurtivo = true; }),
                    M("Matilha", "3 cães: morde 3 bichos por vez.", a => { a.quantidade = 3; a.ajudantes = 2; }),
                    M("Rei dos Cuscos", "6 cães fortes: dano +3 e morde 6 bichos.", a => { a.quantidade = 6; a.dano += 3; a.ajudantes = 5; })
                },
                new[]
                {
                    M("Terneiro", "A cada 3 s uma cabeçada empurra os pequenos.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Rede, "cabecada");
                        e.intervalo = 3f;
                        e.raio = 0.6f;
                        e.desenho = "redemoinho";
                        e.cor = new Color(0.8f, 0.65f, 0.45f, 0.9f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Impacto, dano = 1, empurrar = 0.6f, forca = 1 };
                    }),
                    M("Novilho", "Cabeçadas mais fortes (dano 3).", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Rede, "cabecada");
                        e.ataque.empurrar = 1f;
                        e.ataque.dano = 3;
                    }),
                    M("Touro Brabo", "A cada 6 s um touro arranca pelo caminho atropelando tudo.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "touro");
                        e.intervalo = 6f;
                        e.ataque = new AtributosTorre
                        {
                            tipoDano = TipoDano.Impacto,
                            dano = 3,
                            perfuracao = 30,
                            rolaPeloCaminho = true,
                            velocidadeProjetil = 5f,
                            desenhoProjetil = "touro",
                            empurrar = 0.3f,
                            forca = 2
                        };
                    }),
                    M("Boi Barroso", "O touro empurra até os grandes e o dano sobe +3.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "touro");
                        e.ataque.forca = 4;
                        e.ataque.dano += 3;
                        e.ataque.empurrar = 0.5f;
                    }),
                    M("Estouro da Boiada", "Uma boiada inteira a cada 4 s: dano +10 e perfuração 200.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "touro");
                        e.ataque.dano += 10;
                        e.ataque.perfuracao = 200;
                        e.ataque.escalaProjetil = 2f;
                        e.intervalo = 4f;
                    })
                },
                new[]
                {
                    M("Carancho", "Um carancho voa pelo mapa e bica os bichos (dano 2 a cada 1,5 s).", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "ave");
                        e.intervalo = 1.5f;
                        e.ataque = new AtributosTorre
                        {
                            tipoDano = TipoDano.Corte,
                            dano = 2,
                            teleguiado = true,
                            velocidadeProjetil = 8f,
                            desenhoProjetil = "ave",
                            alcance = 99f
                        };
                    }),
                    M("Gavião-carijó", "Mais rápido e vê furtivo.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "ave");
                        e.intervalo = 1f;
                        e.ataque.veFurtivo = true;
                        e.ataque.velocidadeProjetil = 11f;
                    }),
                    M("Gavião-asa-de-telha", "A cada 6 s leva um bicho pequeno de volta para o começo do caminho.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Carregar);
                        e.intervalo = 6f;
                        e.ataque = new AtributosTorre { forca = 1, veFurtivo = true };
                    }),
                    M("Águia-cinzenta", "Leva até especiais e capivaras.", a => a.Pegar(TipoEspecial.Carregar).ataque.forca = 2),
                    M("Rainha do Céu", "Leva até um Javali, a cada 4 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Carregar);
                        e.ataque.forca = 3;
                        e.intervalo = 4f;
                    })
                }
            }
        };
    }

    static Especial Rosetas(AtributosTorre a)
    {
        return a.Pegar(TipoEspecial.Armadilha);
    }

    static DefinicaoTorre Alambrador()
    {
        return new DefinicaoTorre
        {
            nome = "Alambrador das Rosetas",
            descricao = "Espalha rosetas (o carrapicho espinhudo) no caminho; cada monte fura 5 bichos e pega até furtivos.",
            classe = ClasseTorre.Estancia,
            preco = 400,
            corPoncho = new Color(0.55f, 0.55f, 0.6f),
            arma = "arame",
            comprimentoArma = 0.3f,
            basico = a =>
            {
                a.modo = ModoAtaque.Nenhum;
                a.alcance = 2f;
                Especial e = a.Pegar(TipoEspecial.Armadilha);
                e.intervalo = 1.6f;
                e.maximo = 25;
                e.desenho = "rosetas";
                e.duracao = 0f;
                e.ataque = new AtributosTorre { tipoDano = TipoDano.Corte, dano = 1, perfuracao = 5, veFurtivo = true };
            },
            nomesCaminhos = new[] { "Arame", "Produção", "Espinho Duradouro" },
            caminhos = new[]
            {
                new[]
                {
                    M("Roseta Grande", "Cada monte fura 8 bichos.", a => Rosetas(a).ataque.perfuracao = 8),
                    M("Roseta de Ferro", "As rosetas furam blindagem.", a => Rosetas(a).ataque.furaBlindagem = true),
                    M("Arame Farpado", "Rolos de arame que furam 30 e dão +3 em grandes.", a =>
                    {
                        Especial e = Rosetas(a);
                        e.ataque.perfuracao = 30;
                        e.ataque.danoExtraGrandes += 3;
                        e.desenho = "aramechao";
                    }),
                    M("Alambrado Reforçado", "Dano +2 e +5 em grandes.", a => { Rosetas(a).ataque.dano += 2; Rosetas(a).ataque.danoExtraGrandes += 5; }),
                    M("Muralha de Arame", "Furam 100 e +15 em grandes.", a => { Rosetas(a).ataque.perfuracao = 100; Rosetas(a).ataque.danoExtraGrandes += 15; })
                },
                new[]
                {
                    M("Mais Rosetas", "Joga rosetas 20% mais rápido.", a => Rosetas(a).intervalo /= 1.2f),
                    M("Saco de Rosetas", "Joga rosetas 40% mais rápido.", a => Rosetas(a).intervalo /= 1.4f),
                    M("Campo de Rosetas", "Joga 3 montes de uma vez.", a => Rosetas(a).quantidade = 3),
                    M("Roseta Esperta", "Joga só onde tem mais bicho no caminho.", a => Rosetas(a).esperto = true),
                    M("Fábrica de Arame", "Produção dobrada e 5 montes de uma vez.", a => { Rosetas(a).intervalo /= 2f; Rosetas(a).quantidade = 5; Rosetas(a).maximo = 40; })
                },
                new[]
                {
                    M("Roseta Seca", "As rosetas duram até o fim da próxima onda.", a => Rosetas(a).duracao = 1f),
                    M("Maricá", "Duram 2 ondas e furam +5.", a => { Rosetas(a).duracao = 2f; Rosetas(a).ataque.perfuracao += 5; }),
                    M("Roseta que Estoura", "Quando acaba, o monte solta mais espinhos.", a => Rosetas(a).ataque.estilhacos = 6),
                    M("Roseta Venenosa", "Os espinhos deixam veneno: 1 de dano por segundo.", a =>
                    {
                        Especial e = Rosetas(a);
                        e.ataque.queimaDano = 1;
                        e.ataque.queimaDuracao = 3f;
                        e.ataque.tipoQueima = TipoDano.Impacto;
                    }),
                    M("Caminho de Espinhos", "As rosetas só somem quando gastam os furos e furam blindados (+5 em grandes).", a =>
                    {
                        Especial e = Rosetas(a);
                        e.duracao = 99f;
                        e.maximo = 60;
                        e.ataque.furaBlindagem = true;
                        e.ataque.danoExtraGrandes += 5;
                    })
                }
            }
        };
    }

    static DefinicaoTorre RodaDeChimarrao()
    {
        return new DefinicaoTorre
        {
            nome = "Roda de Chimarrão",
            descricao = "Não ataca: os gaúchos em volta tomam mate e ganham +15% de alcance.",
            classe = ClasseTorre.Estancia,
            preco = 450,
            corpo = "roda",
            basico = a =>
            {
                a.modo = ModoAtaque.Nenhum;
                a.alcance = 2.2f;
                a.auraAlcance = 0.15f;
            },
            nomesCaminhos = new[] { "Chimarrão", "Vigia", "Bolicho" },
            caminhos = new[]
            {
                new[]
                {
                    M("Cuia Cheia", "Gaúchos perto atacam 10% mais rápido.", a => a.auraCadencia = 0.1f),
                    M("Erva Forte", "Gaúchos perto atacam 15% mais rápido.", a => a.auraCadencia = 0.15f),
                    M("Mate Amargo", "20% mais rápido e perfuração +1.", a => { a.auraCadencia = 0.2f; a.auraPerfuracao = 1; }),
                    M("Água no Ponto", "25% mais rápido e dano +1 (sem ferver a água!).", a => { a.auraCadencia = 0.25f; a.auraDano = 1; }),
                    M("Roda Lendária", "35% mais rápido e dano +2.", a => { a.auraCadencia = 0.35f; a.auraDano = 2; })
                },
                new[]
                {
                    M("Quero-quero de Guarda", "Gaúchos perto veem furtivo: o quero-quero grita quando alguém chega.", a => a.auraVeFurtivo = true),
                    M("Vaqueano", "Alcance da roda +20% e tira a camuflagem de quem passa perto.", a => { a.alcance *= 1.2f; a.revelaRaio = a.alcance; }),
                    M("Ferreiro de Plantão", "Gaúchos perto furam blindagem.", a => a.auraFuraBlindagem = true),
                    M("Capataz", "Gaúchos perto dão +25% de dano em grandes.", a => a.auraDanoGrandes = 0.25f),
                    M("Estância Fortificada", "+50% em grandes e acertam qualquer tipo de bicho.", a => { a.auraDanoGrandes = 0.5f; a.auraIgnoraImunidades = true; })
                },
                new[]
                {
                    M("Conta no Bolicho", "Gaúchos contratados perto custam 10% menos.", a => a.auraDesconto = 0.1f),
                    M("Bolicho Grande", "Melhorias dos gaúchos perto custam 10% menos.", a => a.auraDescontoMelhoria = 0.1f),
                    M("Armazém de Campanha", "+70 pilas por onda.", a => a.pilasPorOnda += 70),
                    M("Patrão do CTG", "+150 pilas por onda e 15% de desconto.", a => { a.pilasPorOnda += 80; a.auraDesconto = 0.15f; a.auraDescontoMelhoria = 0.15f; }),
                    M("Grande Patrão", "+350 pilas por onda.", a => a.pilasPorOnda += 200)
                }
            }
        };
    }

    static DefinicaoTorre Charqueada()
    {
        return new DefinicaoTorre
        {
            nome = "Charqueada",
            descricao = "Não ataca: no fim de cada onda solta pacotes de charque. Clique neles para pegar as pilas.",
            classe = ClasseTorre.Estancia,
            preco = 350,
            corpo = "charqueada",
            basico = a =>
            {
                a.modo = ModoAtaque.Nenhum;
                a.alcance = 1.2f;
                a.producao = 45;
            },
            nomesCaminhos = new[] { "Produção", "Guaiaca", "Feira" },
            caminhos = new[]
            {
                new[]
                {
                    M("Mais Sal", "+25 pilas por onda.", a => a.producao += 25),
                    M("Varal de Charque", "+40 pilas por onda.", a => a.producao += 40),
                    M("Charqueada Grande", "+100 pilas por onda.", a => a.producao += 100),
                    M("Charqueadas de Pelotas", "+250 pilas por onda.", a => a.producao += 250),
                    M("Império do Charque", "+600 pilas por onda.", a => a.producao += 600)
                },
                new[]
                {
                    M("Guaiaca", "A produção vai para a guaiaca e rende 10% por onda (botão SACAR).", a => { a.guaiaca = true; a.rendimento = 0.1f; a.limiteGuardado = 600; }),
                    M("Guaiaca Grande", "Rende 15% e guarda até $1500.", a => { a.rendimento = 0.15f; a.limiteGuardado = 1500; }),
                    M("Cofre da Estância", "Rende 20% e guarda até $3000.", a => { a.rendimento = 0.2f; a.limiteGuardado = 3000; }),
                    M("Banco do Interior", "Rende 30% e guarda até $6000.", a => { a.rendimento = 0.3f; a.limiteGuardado = 6000; }),
                    M("Leilão de Gado", "Rende 40%, guarda até $12000 e todo bicho derrubado rende +10%.", a =>
                    {
                        a.rendimento = 0.4f;
                        a.limiteGuardado = 12000;
                        a.bonusPilasGlobal = 0.1f;
                    })
                },
                new[]
                {
                    M("Carreta de Boi", "Coleta sozinha, sem precisar clicar.", a => a.coletaAutomatica = true),
                    M("Feira do Produtor", "Produção x1,2.", a => a.multiplicadorProducao = 1.2f),
                    M("Mercado Público", "Produção x1,4.", a => a.multiplicadorProducao = 1.4f),
                    M("Exportação pelo Porto", "Produção x1,8.", a => a.multiplicadorProducao = 1.8f),
                    M("Grande Exposição Rural", "Produção x2 e +2 vidas por onda.", a => { a.multiplicadorProducao = 2f; a.vidasPorOnda += 2; })
                }
            }
        };
    }
}
