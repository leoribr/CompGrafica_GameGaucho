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
        lista.Add(Cavalo());
        lista.Add(Espingarda());
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
