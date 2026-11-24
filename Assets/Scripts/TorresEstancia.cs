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
