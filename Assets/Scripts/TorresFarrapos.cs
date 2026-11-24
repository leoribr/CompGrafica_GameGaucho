using System.Collections.Generic;
using UnityEngine;

public static class TorresFarrapos
{
    static Melhoria M(string nome, string descricao, System.Action<AtributosTorre> aplicar)
    {
        return new Melhoria(nome, descricao, aplicar);
    }

    public static void Adicionar(List<DefinicaoTorre> lista)
    {
        lista.Add(Bombeador());
        lista.Add(Pescador());
        lista.Add(Lanchao());
        lista.Add(Aviador());
        lista.Add(Cavalo());
        lista.Add(Canhao());
        lista.Add(Espingarda());
    }

    static DefinicaoTorre Pescador()
    {
        return new DefinicaoTorre
        {
            nome = "Pescador da Lagoa",
            descricao = "Só na água: joga arpão (corte) e, com melhorias, tarrafa, mergulho e arpão com corda.",
            classe = ClasseTorre.Farrapos,
            preco = 120,
            corpo = "pescador",
            corPoncho = new Color(0.25f, 0.65f, 0.65f),
            arma = "arpao",
            comprimentoArma = 0.7f,
            soNaAgua = true,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Corte;
                a.dano = 2;
                a.alcance = 2.4f;
                a.intervalo = 0.9f;
                a.perfuracao = 2;
                a.velocidadeProjetil = 10f;
                a.desenhoProjetil = "arpao";
                a.escalaProjetil = 0.7f;
            },
            nomesCaminhos = new[] { "Tarrafa", "Mergulho", "Arpão" },
            caminhos = new[]
            {
                new[]
                {
                    M("Tarrafa", "A cada 3 s joga uma rede no caminho: bichos ficam 50% mais lentos.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Rede);
                        e.intervalo = 3f;
                        e.raio = 0.8f;
                        e.desenho = "rede";
                        e.cor = new Color(0.9f, 0.85f, 0.7f, 0.9f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Impacto, dano = 0, lentidao = 0.5f, duracaoLentidao = 2f };
                    }),
                    M("Tarrafa Grande", "Rede maior que segura os bichos por 0,5 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Rede);
                        e.raio = 1f;
                        e.ataque.atordoar = 0.5f;
                        e.ataque.forca = 2;
                    }),
                    M("Rede de Arrasto", "A rede puxa os bichos para trás.", a => a.Pegar(TipoEspecial.Rede).ataque.empurrar = 1f),
                    M("Rede da Lagoa dos Patos", "Segura até bichos grandes por 1 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Rede);
                        e.ataque.forca = 4;
                        e.ataque.atordoar = 1f;
                    }),
                    M("Safra da Tainha", "Redes gigantes a cada 2,5 s que seguram tudo por 2 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Rede);
                        e.raio = 1.5f;
                        e.intervalo = 2.5f;
                        e.ataque.atordoar = 2f;
                        e.ataque.forca = 5;
                    })
                },
                new[]
                {
                    M("Olho d'Água", "Vê furtivo.", a => a.veFurtivo = true),
                    M("Mergulho", "Revela os furtivos em volta para os outros gaúchos.", a => a.revelaRaio = 2f),
                    M("Sonar de Pescador", "Revela furtivos num raio enorme.", a => a.revelaRaio = 4.5f),
                    M("Pescador do Taim", "+3 de dano em bichos furtivos (como o Jacaré).", a => a.danoExtraFurtivos += 3),
                    M("Rei da Lagoa", "Gaúchos perto veem furtivo e dão +1 de dano.", a => { a.auraVeFurtivo = true; a.auraDano = 1; })
                },
                new[]
                {
                    M("Arpão Afiado", "Dano +1.", a => a.dano += 1),
                    M("Arpão Duplo", "2 arpões por vez.", a => { a.quantidade = 2; a.abertura = 10f; a.armasNaMao = 2; }),
                    M("Arpão de Baleeiro", "Dano +3 e fura blindagem.", a => { a.dano += 3; a.furaBlindagem = true; }),
                    M("Arpão com Corda", "A cada 6 s puxa um bicho grande para trás.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.PuxarGrande);
                        e.intervalo = 6f;
                        e.distancia = 1.2f;
                        e.raio = 2.5f;
                        e.ataque = new AtributosTorre { forca = 4 };
                    }),
                    M("Arpão Gigante", "3 arpões, dano +4 e +10 em grandes.", a => { a.quantidade = 3; a.dano += 4; a.danoExtraGrandes += 10; })
                }
            }
        };
    }

    static DefinicaoTorre Lanchao()
    {
        return new DefinicaoTorre
        {
            nome = "Lanchão Farroupilha",
            descricao = "Só na água: canhões para a frente e para trás (bala). Em 1839 os farrapos levaram lanchões por terra até o mar.",
            classe = ClasseTorre.Farrapos,
            preco = 150,
            corpo = "lanchao",
            arma = "cano",
            comprimentoArma = 0.55f,
            escalaCorpo = 0.9f,
            soNaAgua = true,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Bala;
                a.dano = 2;
                a.alcance = 2.6f;
                a.intervalo = 0.7f;
                a.perfuracao = 3;
                a.tiroTraseiro = true;
                a.velocidadeProjetil = 10f;
                a.desenhoProjetil = "balacanhao";
            },
            nomesCaminhos = new[] { "Canhões de Bordo", "Carga", "Arpéu" },
            caminhos = new[]
            {
                new[]
                {
                    M("Pólvora Seca", "Cadência +25%.", a => a.intervalo /= 1.25f),
                    M("Metralha", "Solta 5 balas em leque.", a => { a.quantidade = 5; a.abertura = 12f; }),
                    M("Canhão de Bombordo", "Balas explosivas e dano +1.", a => { a.raioExplosao = 0.55f; a.tipoDano = TipoDano.Explosao; a.dano += 1; }),
                    M("Lanchão Seival", "Cadência +50% e dano +2.", a => { a.intervalo /= 1.5f; a.dano += 2; }),
                    M("Esquadra Farroupilha", "Cadência dobrada e dano +4.", a => { a.intervalo /= 2f; a.dano += 4; })
                },
                new[]
                {
                    M("Erva-mate no Porão", "+20 pilas por onda.", a => a.pilasPorOnda += 20),
                    M("Carga de Charque", "+50 pilas por onda.", a => a.pilasPorOnda += 30),
                    M("Navio Mercante", "+120 pilas por onda.", a => a.pilasPorOnda += 70),
                    M("Companhia de Navegação", "+300 pilas por onda e bichos derrubados no alcance rendem +10%.", a => { a.pilasPorOnda += 180; a.bonusPilasNoAlcance = 0.1f; }),
                    M("Porto de Rio Grande", "+600 pilas por onda.", a => a.pilasPorOnda += 300)
                },
                new[]
                {
                    M("Olho do Vigia", "Alcance +20% e vê furtivo.", a => { a.alcance *= 1.2f; a.veFurtivo = true; }),
                    M("Arpéu", "A cada 6 s puxa um Javali (ou Jacaré) para trás.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.PuxarGrande);
                        e.intervalo = 6f;
                        e.distancia = 2f;
                        e.raio = 3f;
                        e.ataque = new AtributosTorre { forca = 3 };
                    }),
                    M("Abordagem", "O arpéu derruba o Javali de uma vez.", a => a.Pegar(TipoEspecial.PuxarGrande).mata = true),
                    M("Corsário", "Puxa também Javalis Cinza.", a => a.Pegar(TipoEspecial.PuxarGrande).ataque.forca = 4),
                    M("Almirante Farrapo", "Puxa até o Javaporco, a cada 4 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.PuxarGrande);
                        e.ataque.forca = 5;
                        e.intervalo = 4f;
                    })
                }
            }
        };
    }

    static DefinicaoTorre Aviador()
    {
        return new DefinicaoTorre
        {
            nome = "Aviador do Teco-teco",
            descricao = "Avião agrícola que voa em círculo e solta chumbinho para todos os lados (bala).",
            classe = ClasseTorre.Farrapos,
            preco = 300,
            corpo = "aviao",
            basico = a =>
            {
                a.modo = ModoAtaque.Radial;
                a.tipoDano = TipoDano.Bala;
                a.dano = 1;
                a.quantidade = 8;
                a.alcance = 2.5f;
                a.intervalo = 1.5f;
                a.velocidadeProjetil = 9f;
                a.desenhoProjetil = "chumbo";
                a.movimento = ModoMovimento.Circular;
                a.velocidadeMovimento = 3f;
                a.raioCirculo = 2.2f;
            },
            nomesCaminhos = new[] { "Rajada", "Pulverizador", "Piloto Fino" },
            caminhos = new[]
            {
                new[]
                {
                    M("Mais Chumbo", "12 chumbinhos por rajada.", a => a.quantidade = 12),
                    M("Rajada Rápida", "Cadência +40%.", a => a.intervalo /= 1.4f),
                    M("Chuva de Chumbo", "24 chumbinhos por rajada.", a => a.quantidade = 24),
                    M("Voo Rasante", "Chumbo que fura blindagem e dano +1.", a => { a.furaBlindagem = true; a.dano += 1; }),
                    M("Teco-teco de Guerra", "Cadência x3 e 32 chumbinhos.", a => { a.intervalo /= 3f; a.quantidade = 32; })
                },
                new[]
                {
                    M("Pulverizador", "Deixa um rastro no caminho que deixa os bichos 30% mais lentos.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Zona);
                        e.intervalo = 0.6f;
                        e.raio = 0.6f;
                        e.duracao = 2.5f;
                        e.cor = new Color(0.85f, 0.95f, 0.8f, 0.9f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Impacto, dano = 0, lentidao = 0.3f, duracaoLentidao = 0.4f, veFurtivo = true };
                    }),
                    M("Nuvem Pesada", "O rastro dá 1 de dano por segundo.", a => a.Pegar(TipoEspecial.Zona).ataque.dano = 1),
                    M("Fardo de Feno", "A cada 2 s joga um fardo explosivo no caminho.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "fardo");
                        e.intervalo = 2f;
                        e.ataque = new AtributosTorre
                        {
                            tipoDano = TipoDano.Explosao,
                            dano = 3,
                            raioExplosao = 0.8f,
                            velocidadeProjetil = 5f,
                            desenhoProjetil = "fardo",
                            alcance = 3f
                        };
                    }),
                    M("Bombardeio de Fardos", "Fardos a cada 1 s, com +5 em grandes.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "fardo");
                        e.intervalo = 1f;
                        e.ataque.danoExtraGrandes += 5;
                    }),
                    M("Safra Inteira", "Fardos a cada 0,4 s e dano +3.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "fardo");
                        e.intervalo = 0.4f;
                        e.ataque.dano += 3;
                    })
                },
                new[]
                {
                    M("Mira de Piloto", "Chumbinhos teleguiados.", a => a.teleguiado = true),
                    M("Óculos de Aviador", "Vê furtivo.", a => a.veFurtivo = true),
                    M("Foguete Teleguiado", "A cada 2 s dispara 2 foguetes nos bichos grandes.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "foguete");
                        e.intervalo = 2f;
                        e.quantidade = 2;
                        e.preferirGrandes = true;
                        e.ataque = new AtributosTorre
                        {
                            tipoDano = TipoDano.Explosao,
                            dano = 8,
                            raioExplosao = 0.5f,
                            teleguiado = true,
                            velocidadeProjetil = 9f,
                            desenhoProjetil = "foguete",
                            escalaProjetil = 1.3f,
                            alcance = 6f
                        };
                    }),
                    M("Caça-javali", "Foguetes com +20 de dano em grandes.", a => a.Pegar(TipoEspecial.Disparo, "foguete").ataque.danoExtraGrandes += 20),
                    M("Ás do Céu Gaúcho", "4 foguetes a cada 1 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Disparo, "foguete");
                        e.quantidade = 4;
                        e.intervalo = 1f;
                    })
                }
            }
        };
    }

    static DefinicaoTorre Canhao()
    {
        return new DefinicaoTorre
        {
            nome = "Canhão Farroupilha",
            descricao = "Bombardeia o ponto que você escolher no mapa (explosão): selecione e clique em MUDAR ALVO.",
            classe = ClasseTorre.Farrapos,
            preco = 300,
            corpo = "canhao",
            arma = "cano",
            comprimentoArma = 0.55f,
            basico = a =>
            {
                a.modo = ModoAtaque.PontoFixo;
                a.tipoDano = TipoDano.Explosao;
                a.dano = 2;
                a.alcance = 99f;
                a.raioExplosao = 0.8f;
                a.intervalo = 2f;
                a.imprecisao = 0.5f;
                a.velocidadeProjetil = 9f;
                a.desenhoProjetil = "balacanhao";
            },
            nomesCaminhos = new[] { "Pólvora", "Artilheiro", "Bala em Brasa" },
            caminhos = new[]
            {
                new[]
                {
                    M("Bala Maior", "Área +30%.", a => a.raioExplosao *= 1.3f),
                    M("Bala de Ferro", "Dano +1 e área +20%.", a => { a.dano += 1; a.raioExplosao *= 1.2f; }),
                    M("Bala de 12 Libras", "Atordoa os bichos pequenos por 0,5 s.", a => { a.atordoar = 0.5f; a.forca = 1; }),
                    M("Canhonaço", "Dano +4 e área +30%.", a => { a.dano += 4; a.raioExplosao *= 1.3f; }),
                    M("Batalha do Seival", "Área x1,6, dano +8 e atordoa até os grandes.", a => { a.raioExplosao *= 1.6f; a.dano += 8; a.atordoar = 1f; a.forca = 4; })
                },
                new[]
                {
                    M("Artilheiro Treinado", "Cadência +25%.", a => a.intervalo /= 1.25f),
                    M("Mira de Artilheiro", "Metade da imprecisão.", a => a.imprecisao *= 0.5f),
                    M("Fogo Contínuo", "Cadência +50%.", a => a.intervalo /= 1.5f),
                    M("Bateria de Canhões", "3 balas por tiro.", a => a.quantidade = 3),
                    M("Artilharia Farrapa", "Cadência dobrada e mira perfeita.", a => { a.intervalo /= 2f; a.imprecisao = 0f; })
                },
                new[]
                {
                    M("Bala em Brasa", "Deixa fogo no chão por 2,5 s (1 de dano por segundo).", a => { a.zonaDano = 1; a.zonaDuracao = 2.5f; a.zonaRaio = 0.6f; }),
                    M("Chama Forte", "Fogo de 3 por segundo por 4 s (pega até o Tuco-tuco).", a => { a.zonaDano = 3; a.zonaDuracao = 4f; }),
                    M("Bala de Sinal", "Revela os furtivos onde a bala cai.", a => a.revelaAoAcertar = true),
                    M("Braseiro de Guerra", "Fogo de 6 por segundo e +5 em grandes.", a => { a.zonaDano = 6; a.danoExtraGrandes += 5; }),
                    M("Bombardeio Farrapo", "3 balas por tiro e fogo de 10 por segundo.", a => { a.quantidade = 3; a.zonaDano = 10; })
                }
            }
        };
    }

    static DefinicaoTorre Bombeador()
    {
        return new DefinicaoTorre
        {
            nome = "Bombeador da Carabina",
            descricao = "O batedor das guerras do sul: acerta qualquer bicho do mapa, um por vez (bala).",
            classe = ClasseTorre.Farrapos,
            preco = 120,
            corPoncho = new Color(0.3f, 0.45f, 0.3f),
            arma = "carabina",
            comprimentoArma = 1f,
            basico = a =>
            {
                a.modo = ModoAtaque.Instantaneo;
                a.tipoDano = TipoDano.Bala;
                a.dano = 2;
                a.alcance = 99f;
                a.intervalo = 1.6f;
            },
            nomesCaminhos = new[] { "Calibre Grosso", "Luneta", "Cartucheira" },
            caminhos = new[]
            {
                new[]
                {
                    M("Bala Pesada", "Dano 4.", a => a.dano += 2),
                    M("Ponta de Aço", "Fura blindagem.", a => a.furaBlindagem = true),
                    M("Caça Grossa", "Dano 15.", a => a.dano += 11),
                    M("Rifle de Javali", "Dano 30 e +15 em grandes.", a => { a.dano += 15; a.danoExtraGrandes += 15; }),
                    M("Bombeador Lendário", "Dano 100 e o bicho grande atingido fica parado 1 s.", a => { a.dano += 70; a.atordoar = 1f; a.forca = 5; })
                },
                new[]
                {
                    M("Luneta", "Vê furtivo.", a => a.veFurtivo = true),
                    M("Estilhaço", "A bala se parte em 4 estilhaços ao acertar.", a => a.estilhacos = 4),
                    M("Bala que Desentoca", "Revela o bicho atingido e solta 6 estilhaços.", a => { a.revelaAoAcertar = true; a.estilhacos = 6; }),
                    M("Vigia da Estância", "Atira 2x mais rápido quando há bicho perto do galpão.", a => { a.vigiaGalpao = true; a.multiplicadorVigia = 2f; }),
                    M("Bombeador Chefe", "3x mais rápido perto do galpão, dano +2 e 10 estilhaços.", a => { a.multiplicadorVigia = 3f; a.estilhacos = 10; a.dano += 2; })
                },
                new[]
                {
                    M("Gatilho Leve", "Cadência +40%.", a => a.intervalo /= 1.4f),
                    M("Ferrolho Rápido", "Cadência +40%.", a => a.intervalo /= 1.4f),
                    M("Semiautomática", "Cadência x3.", a => a.intervalo /= 3f),
                    M("Remessa de Munição", "A cada onda ganha um caixote de 40 pilas.", a => a.pilasPorOnda += 40),
                    M("Bombeador da Fronteira", "Atira em 2 bichos por vez e o caixote vira 120 pilas.", a => { a.alvos = 2; a.pilasPorOnda += 80; })
                }
            }
        };
    }

    static DefinicaoTorre Cavalo()
    {
        return new DefinicaoTorre
        {
            nome = "Gaúcho a Cavalo",
            descricao = "Galopa atrás dos bichos e ataca com a lança (corte). Modo: seguir, patrulhar ou parado.",
            classe = ClasseTorre.Farrapos,
            preco = 450,
            corpo = "cavalo",
            corPoncho = new Color(0.35f, 0.55f, 0.85f),
            arma = "lanca",
            comprimentoArma = 0.9f,
            basico = a =>
            {
                a.modo = ModoAtaque.CorpoACorpo;
                a.tipoDano = TipoDano.Corte;
                a.dano = 2;
                a.alcance = 1.3f;
                a.intervalo = 0.45f;
                a.quantidade = 2;
                a.movimento = ModoMovimento.Seguir;
                a.velocidadeMovimento = 2.6f;
            },
            nomesCaminhos = new[] { "Lança", "Galope", "Laçador a Cavalo" },
            caminhos = new[]
            {
                new[]
                {
                    M("Lança Comprida", "Alcance +20%.", a => a.alcance *= 1.2f),
                    M("Lanceiro", "Acerta 3 bichos por golpe.", a => a.quantidade = 3),
                    M("Carga de Cavalaria", "Acerta até 10 bichos por golpe.", a => { a.quantidade = 10; a.alcance *= 1.1f; }),
                    M("Lanceiro Farrapo", "Dano +3 e fura blindagem.", a => { a.dano += 3; a.furaBlindagem = true; }),
                    M("Centauro dos Pampas", "Golpes 2x mais rápidos, dano +6 e +10 em grandes.", a => { a.intervalo /= 2f; a.dano += 6; a.danoExtraGrandes += 10; })
                },
                new[]
                {
                    M("Galope", "Anda 50% mais rápido.", a => a.velocidadeMovimento *= 1.5f),
                    M("Vaqueano", "Vê furtivo.", a => a.veFurtivo = true),
                    M("Poeirão", "O galope levanta poeira que empurra os bichos pequenos.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Pulso);
                        e.intervalo = 2f;
                        e.raio = 1.2f;
                        e.cor = new Color(0.85f, 0.75f, 0.55f, 0.9f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Impacto, dano = 0, empurrar = 0.6f, forca = 1 };
                    }),
                    M("Chasque", "A cada onda traz +1 vida e +60 pilas.", a => { a.vidasPorOnda += 1; a.pilasPorOnda += 60; }),
                    M("Tropeiro", "A cada onda +3 vidas e +200 pilas, e o poeirão fica maior.", a =>
                    {
                        a.vidasPorOnda += 2;
                        a.pilasPorOnda += 140;
                        a.Pegar(TipoEspecial.Pulso).raio *= 1.5f;
                    })
                },
                new[]
                {
                    M("Laço de Cavaleiro", "A cada 8 s laça um bicho grande e puxa para trás.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.PuxarGrande);
                        e.intervalo = 8f;
                        e.distancia = 1.5f;
                        e.raio = 2.5f;
                        e.ataque = new AtributosTorre { forca = 3 };
                    }),
                    M("Puxada Forte", "Puxa mais longe.", a => a.Pegar(TipoEspecial.PuxarGrande).distancia += 1.2f),
                    M("O Laçador", "Laça também Javalis Cinza, a cada 5 s.", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.PuxarGrande);
                        e.ataque.forca = 4;
                        e.intervalo = 5f;
                    }),
                    M("Tropilha", "2 cavaleiros acompanham: +2 bichos por golpe e dano +1.", a => { a.ajudantes = 2; a.quantidade += 2; a.dano += 1; }),
                    M("Cavalaria Gaúcha", "4 cavaleiros, dano +2 e o laço puxa até o Javaporco.", a =>
                    {
                        a.ajudantes = 4;
                        a.quantidade += 2;
                        a.dano += 2;
                        a.Pegar(TipoEspecial.PuxarGrande).ataque.forca = 5;
                    })
                }
            }
        };
    }

    static DefinicaoTorre Espingarda()
    {
        return new DefinicaoTorre
        {
            nome = "Gaúcho da Espingarda",
            descricao = "Chumbo em leque: forte de perto e acerta vários (bala).",
            classe = ClasseTorre.Farrapos,
            preco = 130,
            usaModeloDaCena = true,
            arma = "espingarda",
            comprimentoArma = 0.7f,
            basico = a =>
            {
                a.modo = ModoAtaque.Projetil;
                a.tipoDano = TipoDano.Bala;
                a.dano = 5;
                a.alcance = 2.45f;
                a.intervalo = 1.05f;
                a.quantidade = 3;
                a.abertura = 12f;
                a.lateraisMetade = true;
                a.velocidadeProjetil = 12f;
                a.desenhoProjetil = "chumbo";
            },
            nomesCaminhos = new[] { "Dois Canos", "Cartucheira", "Mira na Mão" },
            caminhos = new[]
            {
                new[]
                {
                    M("Mais Chumbo", "5 chumbos por tiro.", a => { a.quantidade = 5; a.abertura = 10f; }),
                    M("Dois Canos", "Dispara duas vezes seguidas.", a => a.rajada = 2),
                    M("Chumbo Grosso", "Dano dobrado e fura blindagem.", a => { a.dano *= 2; a.furaBlindagem = true; }),
                    M("Espingardão", "9 chumbos e empurra os bichos pequenos.", a =>
                    {
                        a.quantidade = 9;
                        a.abertura = 8f;
                        a.empurrar = 0.35f;
                        a.chanceEmpurrar = 0.5f;
                        a.forca = 1;
                    }),
                    M("Trovão do Pampa", "15 chumbos por tiro e dano +5.", a => { a.quantidade = 15; a.abertura = 6f; a.dano += 5; })
                },
                new[]
                {
                    M("Cartucheira no Peito", "Cadência +20%.", a => a.intervalo /= 1.2f),
                    M("Recarga Ligeira", "Cadência +30%.", a => a.intervalo /= 1.3f),
                    M("Espingarda de Repetição", "Cadência dobrada.", a => a.intervalo /= 2f),
                    M("Automática", "Cadência +50%.", a => a.intervalo /= 1.5f),
                    M("Saraivada", "Cadência dobrada de novo.", a => a.intervalo /= 2f)
                },
                new[]
                {
                    M("Mira Livre", "Libera o alvo MOUSE: atira onde o mouse aponta, no mapa todo.", a => a.miraNoMouse = true),
                    M("Mira Estável", "Leque mais fechado e alcance +20%.", a => { a.abertura *= 0.6f; a.alcance *= 1.2f; }),
                    M("Cartucho de Sal", "Bichos atingidos ficam 40% mais lentos.", a => { a.lentidao = 0.4f; a.duracaoLentidao = 2f; }),
                    M("Cartucho de Fogo", "Chumbo em brasa: queima 2 por segundo.", a => { a.queimaDano = 2; a.queimaDuracao = 2f; a.tipoQueima = TipoDano.Fogo; }),
                    M("Barreira de Chumbo", "A cada 3 s deixa uma zona de chumbo no caminho (3 de dano por segundo).", a =>
                    {
                        Especial e = a.Pegar(TipoEspecial.Zona);
                        e.intervalo = 3f;
                        e.raio = 0.9f;
                        e.duracao = 3f;
                        e.cor = new Color(1f, 0.9f, 0.5f, 0.9f);
                        e.ataque = new AtributosTorre { tipoDano = TipoDano.Bala, dano = 3, veFurtivo = true };
                    })
                }
            }
        };
    }
}
