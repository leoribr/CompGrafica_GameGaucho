using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed partial class JogoGaucho
{
    sealed class Zona
    {
        public SpriteRenderer visual;
        public Vector3 centro;
        public float raio;
        public float fim;
        public AtributosTorre ataque;
        public TorreGaucha origem;
        public float resto;
        public float proximoTique;
        public Color cor;
    }

    sealed class Armadilha
    {
        public SpriteRenderer visual;
        public Vector3 centro;
        public int cargas;
        public AtributosTorre ataque;
        public TorreGaucha origem;
        public bool captura;
        public int bonusPilas;
        public int ondasRestantes;
        public readonly HashSet<InimigoGaucho> atingidos = new HashSet<InimigoGaucho>();
    }

    sealed class Pacote
    {
        public SpriteRenderer visual;
        public Vector3 base_;
        public int valor;
        public float expira;
    }

    sealed class Anel
    {
        public LineRenderer linha;
        public Vector3 centro;
        public float raio;
        public float inicio;
        public float duracao;
        public Color cor;
    }

    sealed class Rastro_
    {
        public LineRenderer linha;
        public float inicio;
        public float duracao;
        public Color cor;
    }

    sealed class Fumaca
    {
        public SpriteRenderer sprite;
        public float inicio;
        public float duracao;
        public float escala;
        public Color cor;
    }

    const int PontosDoAnelDeEfeito = 36;

    readonly List<Zona> zonas = new List<Zona>();
    readonly List<Armadilha> armadilhas = new List<Armadilha>();
    readonly List<Pacote> pacotes = new List<Pacote>();
    readonly List<Anel> aneis = new List<Anel>();
    readonly List<Rastro_> rastros = new List<Rastro_>();
    readonly List<Fumaca> fumacas = new List<Fumaca>();
    Material materialParticula;
    Projetil moldeBala;
    Projetil moldeChumbo;
    AtributosTorre ataqueAoCair;
    AtributosTorre verTudo;
    DefinicaoTorre definicaoEspantalho;
    bool bichoPertoDoGalpao;
    float bonusPilasGlobal;
    float reducaoVidaGrandes;

    public float LentidaoGlobal { get; private set; }
    public bool TemBichoPertoDoGalpao => bichoPertoDoGalpao;
    public int QuantidadeDePontos => caminho.Length;

    public Vector3 PontoDoCaminho(int indice)
    {
        return caminho[Mathf.Clamp(indice, 0, caminho.Length - 1)].position;
    }

    bool NaAgua(Vector3 posicao, float margem)
    {
        return lagoa != null && lagoa.Contem(posicao, margem);
    }

    Material MaterialDeEfeito()
    {
        if (materialParticula == null)
            materialParticula = new Material(Shader.Find("Sprites/Default"));
        return materialParticula;
    }

    AtributosTorre VerTudo()
    {
        if (verTudo == null)
            verTudo = new AtributosTorre { veFurtivo = true, ignoraImunidades = true };
        return verTudo;
    }

    public Vector3 PosicaoDoMouse()
    {
        if (Mouse.current == null)
            return Vector3.zero;
        Vector2 tela = Mouse.current.position.ReadValue();
        Vector3 mundo = cameraPrincipal.ScreenToWorldPoint(new Vector3(tela.x, tela.y, -cameraPrincipal.transform.position.z));
        mundo.z = 0f;
        return mundo;
    }

    public bool Enxerga(InimigoGaucho inimigo, AtributosTorre a)
    {
        return !inimigo.Furtivo || a.veFurtivo;
    }

    public bool PodeFerir(InimigoGaucho inimigo, AtributosTorre a)
    {
        if (a.ignoraImunidades)
            return true;
        DefinicaoInimigo d = inimigo.Definicao;
        if (d.blindado && (a.tipoDano == TipoDano.Corte || a.tipoDano == TipoDano.Bala) && !a.furaBlindagem)
            return false;
        for (int i = 0; i < d.imunidades.Length; i++)
        {
            if (d.imunidades[i] == a.tipoDano)
                return false;
        }
        return true;
    }

    public void AvisarImune(InimigoGaucho inimigo)
    {
        if (inimigo != null && inimigo.PodeAvisarImune())
            AdicionarTexto(inimigo.transform.position + Vector3.up * 0.4f, "IMUNE", new Color(0.8f, 0.85f, 1f));
    }

    float Nota(InimigoGaucho inimigo, Vector3 origem, Prioridade prioridade)
    {
        switch (prioridade)
        {
            case Prioridade.Ultimo:
                return -inimigo.ProgressoCaminho;
            case Prioridade.MaisPerto:
                return -((Vector2)(inimigo.transform.position - origem)).sqrMagnitude;
            case Prioridade.MaisForte:
                return inimigo.Peso * 100000f + inimigo.Vida + inimigo.ProgressoCaminho * 0.01f;
            default:
                return inimigo.ProgressoCaminho;
        }
    }

    public InimigoGaucho EncontrarAlvo(Vector3 origem, float alcance, Prioridade prioridade, AtributosTorre a)
    {
        InimigoGaucho melhor = null;
        float melhorNota = float.MinValue;
        float alcanceQuadrado = alcance * alcance;
        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo == null || !inimigo.EstaVivo)
                continue;
            if (((Vector2)(inimigo.transform.position - origem)).sqrMagnitude > alcanceQuadrado)
                continue;
            if (!Enxerga(inimigo, a) || !PodeFerir(inimigo, a))
                continue;
            float nota = Nota(inimigo, origem, prioridade);
            if (nota > melhorNota)
            {
                melhorNota = nota;
                melhor = inimigo;
            }
        }
        return melhor;
    }

    public int EncontrarAlvos(List<InimigoGaucho> saida, Vector3 origem, float alcance, Prioridade prioridade, AtributosTorre a, int maximo)
    {
        saida.Clear();
        if (maximo <= 1)
        {
            InimigoGaucho unico = EncontrarAlvo(origem, alcance, prioridade, a);
            if (unico != null)
                saida.Add(unico);
            return saida.Count;
        }

        List<float> notas = new List<float>();
        float alcanceQuadrado = alcance * alcance;
        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo == null || !inimigo.EstaVivo)
                continue;
            if (((Vector2)(inimigo.transform.position - origem)).sqrMagnitude > alcanceQuadrado)
                continue;
            if (!Enxerga(inimigo, a) || !PodeFerir(inimigo, a))
                continue;
            float nota = Nota(inimigo, origem, prioridade);
            int posicao = saida.Count;
            while (posicao > 0 && notas[posicao - 1] < nota)
                posicao--;
            if (posicao >= maximo)
                continue;
            saida.Insert(posicao, inimigo);
            notas.Insert(posicao, nota);
            if (saida.Count > maximo)
            {
                saida.RemoveAt(saida.Count - 1);
                notas.RemoveAt(notas.Count - 1);
            }
        }
        return saida.Count;
    }

    public InimigoGaucho ProximoParaRicochete(Vector3 ponto, float raio, HashSet<InimigoGaucho> ignorar, AtributosTorre a)
    {
        InimigoGaucho melhor = null;
        float melhorDistancia = raio * raio;
        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo == null || !inimigo.EstaVivo || ignorar.Contains(inimigo))
                continue;
            if (!Enxerga(inimigo, a) || !PodeFerir(inimigo, a))
                continue;
            float distancia = ((Vector2)(inimigo.transform.position - ponto)).sqrMagnitude;
            if (distancia < melhorDistancia)
            {
                melhorDistancia = distancia;
                melhor = inimigo;
            }
        }
        return melhor;
    }

    public void AplicarAcerto(InimigoGaucho alvo, AtributosTorre a, int dano, TorreGaucha origem)
    {
        if (alvo == null || !alvo.EstaVivo)
            return;
        if (!PodeFerir(alvo, a))
        {
            AvisarImune(alvo);
            return;
        }

        if (a.revelaAoAcertar)
            alvo.Revelar();
        if (a.bonusPilasAoCair > 0)
            alvo.BonusPilas = Mathf.Max(alvo.BonusPilas, a.bonusPilasAoCair);
        if (a.explodeAoCair > 0 && alvo.Grande)
            alvo.ExplodeAoCair = Mathf.Max(alvo.ExplodeAoCair, a.explodeAoCair);
        if (a.contagiaFilhotes)
            alvo.Contagia = true;
        if (a.marca > 0f)
            alvo.Marcar(a.marca, a.duracaoMarca);

        bool controla = alvo.Peso <= a.forca;
        if (a.lentidao > 0f)
            alvo.AplicarLentidao(a.lentidao, a.duracaoLentidao);
        if (a.congelar > 0f && controla && !alvo.Definicao.ImuneA(TipoDano.Frio))
            alvo.Parar(a.congelar, true);
        if (a.atordoar > 0f && controla && Random.value <= a.chanceAtordoar)
            alvo.Parar(a.atordoar, false);
        if (a.empurrar > 0f && controla && Random.value <= a.chanceEmpurrar)
            alvo.Recuar(a.empurrar);
        if (a.queimaDano > 0)
        {
            bool podeQueimar = a.ignoraImunidades || a.tipoQueima == a.tipoDano || !alvo.Definicao.ImuneA(a.tipoQueima);
            if (podeQueimar)
                alvo.Queimar(a.queimaDano, a.queimaDuracao, a.tipoQueima, origem);
        }

        if (a.viraPilas && alvo.Peso <= 1 && !alvo.Grande)
        {
            alvo.BonusPilas += 2;
            alvo.Matar(origem);
            return;
        }

        int total = dano;
        if (alvo.Grande)
            total += a.danoExtraGrandes;
        if (alvo.Definicao.furtivo)
            total += a.danoExtraFurtivos;
        float marca = alvo.MarcaAtual;
        if (marca > 0f && total > 0)
            total = Mathf.RoundToInt(total * (1f + marca));
        if (total > 0)
            alvo.ReceberDano(total, origem);
    }

    public void Explodir(Vector3 centro, float raio, AtributosTorre a, int dano, TorreGaucha origem)
    {
        Color cor = CorDoTipo(a.tipoDano);
        AfetarArea(centro, raio, a, dano, origem, cor, false);
        CriarEstouro(centro, cor, 14, raio * 0.5f);
        CriarAnel(centro, raio, cor, 0.22f);
        if (a.revelaAoAcertar)
            RevelarArea(centro, raio);
    }

    void RevelarArea(Vector3 centro, float raio)
    {
        float raioQuadrado = raio * raio;
        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo != null && ((Vector2)(inimigo.transform.position - centro)).sqrMagnitude <= raioQuadrado)
                inimigo.Revelar();
        }
    }

    public int AfetarArea(Vector3 centro, float raio, AtributosTorre a, int dano, TorreGaucha origem, Color cor, bool anel)
    {
        List<InimigoGaucho> afetados = new List<InimigoGaucho>();
        float raioQuadrado = raio * raio;
        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo != null && inimigo.EstaVivo && ((Vector2)(inimigo.transform.position - centro)).sqrMagnitude <= raioQuadrado)
                afetados.Add(inimigo);
        }
        foreach (InimigoGaucho inimigo in afetados)
            AplicarAcerto(inimigo, a, dano, origem);
        if (anel)
            CriarAnel(centro, raio, cor, 0.3f);
        return afetados.Count;
    }

    public void SoltarEstilhacos(Vector3 ponto, AtributosTorre a, int dano, TorreGaucha origem, HashSet<InimigoGaucho> ignorar)
    {
        AtributosTorre e = new AtributosTorre
        {
            tipoDano = a.estilhacosExplodem ? TipoDano.Explosao : a.tipoDano == TipoDano.Explosao ? TipoDano.Corte : a.tipoDano,
            dano = Mathf.Max(1, dano / 3),
            perfuracao = 1,
            velocidadeProjetil = 9f,
            escalaProjetil = 0.8f,
            desenhoProjetil = a.estilhacosExplodem ? "foguete" : "faca",
            raioExplosao = a.estilhacosExplodem ? 0.4f : 0f,
            furaBlindagem = a.furaBlindagem,
            ignoraImunidades = a.ignoraImunidades,
            veFurtivo = true
        };
        int quantidade = a.estilhacos;
        for (int i = 0; i < quantidade; i++)
        {
            float angulo = i * Mathf.PI * 2f / quantidade;
            Vector2 direcao = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo));
            Projetil p = CriarProjetil(origem, e, e.dano, ponto, direcao, null, 1.6f);
            if (p != null && ignorar != null)
            {
                foreach (InimigoGaucho inimigo in ignorar)
                    p.IgnorarInimigo(inimigo);
            }
        }
    }

    AtributosTorre AtaqueDeExplosaoAoCair()
    {
        if (ataqueAoCair == null)
            ataqueAoCair = new AtributosTorre { tipoDano = TipoDano.Magica, veFurtivo = true };
        return ataqueAoCair;
    }

    Projetil MoldeDoProjetil(AtributosTorre a)
    {
        if (a.desenhoProjetil == "chumbo")
        {
            if (moldeChumbo == null)
                moldeChumbo = Resources.Load<Projetil>("Projetil_Espingarda");
            return moldeChumbo;
        }
        if (moldeBala == null)
            moldeBala = Resources.Load<Projetil>("Projetil_Revolver");
        return moldeBala;
    }

    public Projetil CriarProjetil(TorreGaucha origem, AtributosTorre a, int dano, Vector3 posicao, Vector2 direcao, InimigoGaucho alvo, float alcanceMaximo)
    {
        Projetil molde = MoldeDoProjetil(a);
        if (molde == null)
            return null;
        posicao.z = 0f;
        Projetil projetil = Instantiate(molde, posicao, Quaternion.identity);
        projetil.Configurar(this, origem, a, dano, alvo, direcao, alcanceMaximo);
        return projetil;
    }

    public void CriarBumerangue(TorreGaucha origem, AtributosTorre a, int dano, Vector3 posicao, Vector3 destino)
    {
        Projetil projetil = CriarProjetil(origem, a, dano, posicao, destino - posicao, null, 30f);
        if (projetil != null)
            projetil.ConfigurarBumerangue(destino);
    }

    public void CriarProjetilAoPonto(TorreGaucha origem, AtributosTorre a, int dano, Vector3 posicao, Vector3 destino)
    {
        Projetil projetil = CriarProjetil(origem, a, dano, posicao, destino - posicao, null, 40f);
        if (projetil != null)
            projetil.ConfigurarDestino(destino);
    }

    public void CriarProjetilNoCaminho(TorreGaucha origem, AtributosTorre a, int dano, Vector3 perto)
    {
        int segmento;
        Vector3 inicio;
        if (a.comecaNoFimDoCaminho)
        {
            inicio = caminho[caminho.Length - 1].position;
            segmento = caminho.Length - 2;
        }
        else
            inicio = ProjetarNoCaminho(perto, out segmento);
        Projetil projetil = CriarProjetil(origem, a, dano, inicio, caminho[segmento].position - inicio, null, 60f);
        if (projetil != null)
            projetil.ConfigurarCaminho(caminho, segmento);
    }

    void LiberarAlmas(Vector3 posicao, int pontoSeguinte)
    {
        foreach (TorreGaucha torre in torres)
        {
            if (torre == null || torre.Ajudante || !torre.Atributos.Tem(TipoEspecial.Almas))
                continue;
            if (Vector2.Distance(torre.transform.position, posicao) > torre.Alcance)
                continue;
            Especial almas = torre.Atributos.Pegar(TipoEspecial.Almas);
            if (torre.AlmasAtivas >= almas.maximo)
                continue;
            int segmento = Mathf.Clamp(pontoSeguinte - 1, 0, caminho.Length - 2);
            Projetil alma = CriarProjetil(torre, almas.ataque, almas.ataque.dano, posicao, caminho[segmento].position - posicao, null, 60f);
            if (alma == null)
                continue;
            alma.ConfigurarCaminho(caminho, segmento);
            alma.EhAlma = true;
            torre.AlmasAtivas++;
        }
    }

    public bool UsarEspecial(TorreGaucha torre, Especial especial)
    {
        Vector3 origem = torre.transform.position;
        float alcance = torre.Alcance;
        AtributosTorre a = especial.ataque;
        bool achou;

        switch (especial.tipo)
        {
            case TipoEspecial.Disparo:
            {
                if (a.rolaPeloCaminho)
                {
                    if (inimigosAtivos.Count == 0)
                        return false;
                    for (int i = 0; i < especial.quantidade; i++)
                        CriarProjetilNoCaminho(torre, a, a.dano, origem);
                    return true;
                }
                float alcanceDoDisparo = a.alcance > 0f ? a.alcance : alcance;
                Prioridade prioridade = especial.preferirGrandes ? Prioridade.MaisForte : torre.Prioridade == Prioridade.Mouse ? Prioridade.Primeiro : torre.Prioridade;
                InimigoGaucho alvo = EncontrarAlvo(origem, alcanceDoDisparo, prioridade, a);
                if (alvo == null)
                    return false;
                Vector3 boca = torre.BocaDaArma();
                Vector2 direcao = (alvo.transform.position - boca).normalized;
                for (int i = 0; i < especial.quantidade; i++)
                {
                    Vector2 dir = Quaternion.Euler(0f, 0f, (i - (especial.quantidade - 1) * 0.5f) * 14f) * direcao;
                    if (a.raioExplosao > 0f && !a.teleguiado && a.velocidadeProjetil < 7f)
                        CriarProjetilAoPonto(torre, a, a.dano, boca, alvo.transform.position);
                    else
                        CriarProjetil(torre, a, a.dano, boca, dir, alvo, alcanceDoDisparo * 1.6f + 1f);
                }
                return true;
            }
            case TipoEspecial.Pulso:
            {
                if (EncontrarAlvo(origem, especial.raio, Prioridade.MaisPerto, VerTudo()) == null)
                    return false;
                AfetarArea(origem, especial.raio, a, a.dano, torre, especial.cor, true);
                return true;
            }
            case TipoEspecial.Rede:
            {
                Vector3 ponto = MelhorPontoNoCaminho(origem, alcance, out achou);
                if (!achou)
                    return false;
                AfetarArea(ponto, especial.raio, a, a.dano, torre, especial.cor, false);
                CriarFumaca(ponto, especial.desenho, especial.raio, especial.cor, 0.6f);
                return true;
            }
            case TipoEspecial.Zona:
            {
                Vector3 ponto;
                if (torre.Movimento == ModoMovimento.Circular)
                {
                    ponto = PontoMaisProximoDoCaminho(origem);
                    achou = Vector2.Distance(ponto, origem) < 1.3f;
                }
                else
                    ponto = MelhorPontoNoCaminho(origem, alcance, out achou);
                if (!achou)
                    return false;
                CriarZona(ponto, especial.raio, especial.duracao, a, torre, especial.cor);
                return true;
            }
            case TipoEspecial.PuxarGrande:
                return PuxarGrande(torre, especial);
            case TipoEspecial.Carregar:
                return Carregar(torre, especial);
            case TipoEspecial.Armadilha:
                return ColocarArmadilha(torre, especial);
            case TipoEspecial.Espantalho:
                return CriarEspantalho(torre, especial);
            case TipoEspecial.BuffTorre:
                return Fortalecer(torre, especial);
        }
        return false;
    }

    bool PuxarGrande(TorreGaucha torre, Especial especial)
    {
        InimigoGaucho melhor = null;
        float alcance = Mathf.Max(torre.Alcance, especial.raio);
        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo == null || !inimigo.EstaVivo || !inimigo.Grande || inimigo.Peso > especial.ataque.forca)
                continue;
            if (Vector2.Distance(inimigo.transform.position, torre.transform.position) > alcance)
                continue;
            if (melhor == null || inimigo.ProgressoCaminho > melhor.ProgressoCaminho)
                melhor = inimigo;
        }
        if (melhor == null)
            return false;

        Vector3 ponto = melhor.transform.position;
        Rastro(torre.BocaDaArma(), ponto, new Color(0.85f, 0.65f, 0.4f, 0.95f));
        if (especial.mata && melhor.Peso <= 3)
        {
            AdicionarTexto(ponto + Vector3.up * 0.6f, "ABORDADO!", new Color(1f, 0.9f, 0.5f));
            melhor.Matar(torre);
            return true;
        }
        melhor.Recuar(especial.distancia);
        AdicionarTexto(ponto + Vector3.up * 0.6f, "laçado!", new Color(1f, 0.9f, 0.5f));
        return true;
    }

    bool Carregar(TorreGaucha torre, Especial especial)
    {
        InimigoGaucho melhor = null;
        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo == null || !inimigo.EstaVivo || inimigo.Peso > especial.ataque.forca || inimigo.Definicao.imuneLentidao)
                continue;
            if (!Enxerga(inimigo, especial.ataque))
                continue;
            if (melhor == null || inimigo.ProgressoCaminho > melhor.ProgressoCaminho)
                melhor = inimigo;
        }
        if (melhor == null || melhor.ProgressoCaminho < 1.5f)
            return false;
        Vector3 ponto = melhor.transform.position;
        CriarEstouro(ponto, new Color(0.95f, 0.9f, 0.8f), 10, 0.2f);
        AdicionarTexto(ponto + Vector3.up * 0.5f, "levado!", new Color(0.9f, 0.95f, 1f));
        melhor.VoltarAoComeco();
        return true;
    }

    bool ColocarArmadilha(TorreGaucha torre, Especial especial)
    {
        if (torre.ArmadilhasAtivas >= especial.maximo)
            return false;
        bool achou;
        Vector3 ponto = especial.esperto
            ? MelhorPontoNoCaminho(torre.transform.position, torre.Alcance, out achou)
            : PontoAleatorioNoCaminho(torre.transform.position, torre.Alcance, out achou);
        if (!achou)
            return false;

        for (int i = 0; i < especial.quantidade && torre.ArmadilhasAtivas < especial.maximo; i++)
        {
            Vector3 p = ponto + (Vector3)(Random.insideUnitCircle * 0.3f * i);
            SpriteRenderer visual = CriarSpriteSolto(especial.desenho, p, 3, especial.cor);
            armadilhas.Add(new Armadilha
            {
                visual = visual,
                centro = p,
                cargas = Mathf.Max(1, especial.ataque.perfuracao),
                ataque = especial.ataque,
                origem = torre,
                captura = especial.captura,
                bonusPilas = especial.bonusPilas,
                ondasRestantes = Mathf.RoundToInt(especial.duracao)
            });
            torre.ArmadilhasAtivas++;
        }
        return true;
    }

    void AtualizarArmadilhas()
    {
        for (int k = armadilhas.Count - 1; k >= 0; k--)
        {
            Armadilha t = armadilhas[k];
            if (t.cargas <= 0)
            {
                RemoverArmadilha(k);
                continue;
            }

            List<InimigoGaucho> tocando = null;
            foreach (InimigoGaucho inimigo in inimigosAtivos)
            {
                if (inimigo == null || !inimigo.EstaVivo || t.atingidos.Contains(inimigo))
                    continue;
                if (((Vector2)(inimigo.transform.position - t.centro)).sqrMagnitude > 0.16f)
                    continue;
                if (tocando == null)
                    tocando = new List<InimigoGaucho>();
                tocando.Add(inimigo);
            }
            if (tocando == null)
                continue;

            foreach (InimigoGaucho inimigo in tocando)
            {
                if (t.cargas <= 0)
                    break;
                t.atingidos.Add(inimigo);
                if (t.captura)
                {
                    if (inimigo.Peso > t.ataque.forca)
                        continue;
                    inimigo.BonusPilas += t.bonusPilas;
                    AdicionarTexto(inimigo.transform.position + Vector3.up * 0.4f, "preso!", new Color(1f, 0.9f, 0.5f));
                    inimigo.Matar(t.origem);
                    t.cargas--;
                }
                else
                {
                    if (!PodeFerir(inimigo, t.ataque))
                    {
                        AvisarImune(inimigo);
                        continue;
                    }
                    AplicarAcerto(inimigo, t.ataque, t.ataque.dano, t.origem);
                    t.cargas--;
                }
            }

            if (t.cargas <= 0)
            {
                if (t.ataque.estilhacos > 0)
                    SoltarEstilhacos(t.centro, t.ataque, t.ataque.dano * 3, t.origem, null);
                RemoverArmadilha(k);
            }
        }
    }

    void RemoverArmadilha(int indice)
    {
        Armadilha t = armadilhas[indice];
        if (t.visual != null)
            Destroy(t.visual.gameObject);
        if (t.origem != null)
            t.origem.ArmadilhasAtivas--;
        armadilhas.RemoveAt(indice);
    }

    void ExpirarArmadilhas()
    {
        for (int k = armadilhas.Count - 1; k >= 0; k--)
        {
            if (armadilhas[k].ondasRestantes <= 0)
                RemoverArmadilha(k);
            else
                armadilhas[k].ondasRestantes--;
        }
    }

    public void CriarZonaDoAtaque(Vector3 ponto, AtributosTorre a, TorreGaucha origem)
    {
        AtributosTorre z = new AtributosTorre
        {
            tipoDano = a.tipoDano == TipoDano.Explosao ? TipoDano.Fogo : a.tipoDano,
            dano = a.zonaDano,
            ignoraImunidades = a.ignoraImunidades,
            furaBlindagem = true,
            veFurtivo = true
        };
        CriarZona(ponto, a.zonaRaio, a.zonaDuracao, z, origem, CorDoTipo(z.tipoDano));
    }

    public void CriarZona(Vector3 ponto, float raio, float duracao, AtributosTorre a, TorreGaucha origem, Color cor)
    {
        SpriteRenderer visual = CriarSpriteSolto("", ponto, 3, new Color(cor.r, cor.g, cor.b, 0.35f));
        visual.sprite = SpritesGaucho.Circulo();
        visual.transform.localScale = Vector3.one * raio * 2f;
        zonas.Add(new Zona { visual = visual, centro = ponto, raio = raio, fim = Time.time + duracao, ataque = a, origem = origem, cor = cor });
    }

    void AtualizarZonas()
    {
        for (int k = zonas.Count - 1; k >= 0; k--)
        {
            Zona z = zonas[k];
            if (Time.time >= z.fim)
            {
                if (z.visual != null)
                    Destroy(z.visual.gameObject);
                zonas.RemoveAt(k);
                continue;
            }

            float restante = z.fim - Time.time;
            if (z.visual != null)
                z.visual.color = new Color(z.cor.r, z.cor.g, z.cor.b, 0.35f * Mathf.Clamp01(restante * 2f));
            if (Time.time < z.proximoTique)
                continue;

            z.proximoTique = Time.time + 0.25f;
            z.resto += z.ataque.dano * 0.25f;
            int dano = Mathf.FloorToInt(z.resto);
            z.resto -= dano;

            List<InimigoGaucho> dentro = new List<InimigoGaucho>();
            float raioQuadrado = z.raio * z.raio;
            foreach (InimigoGaucho inimigo in inimigosAtivos)
            {
                if (inimigo != null && inimigo.EstaVivo && ((Vector2)(inimigo.transform.position - z.centro)).sqrMagnitude <= raioQuadrado)
                    dentro.Add(inimigo);
            }
            foreach (InimigoGaucho inimigo in dentro)
                AplicarAcerto(inimigo, z.ataque, dano, z.origem);
        }
    }

    void CriarPacotes(TorreGaucha torre, int valor)
    {
        const int quantidade = 3;
        int parte = Mathf.Max(1, valor / quantidade);
        int sobra = Mathf.Max(0, valor - parte * quantidade);
        for (int i = 0; i < quantidade; i++)
        {
            Vector3 p = torre.transform.position + (Vector3)(Random.insideUnitCircle.normalized * Random.Range(0.55f, 0.95f));
            SpriteRenderer visual = CriarSpriteSolto("charque", p, 29, Color.white);
            visual.transform.localScale = Vector3.one * 1.4f;
            pacotes.Add(new Pacote { visual = visual, base_ = p, valor = parte + (i == 0 ? sobra : 0), expira = Time.time + 25f });
        }
    }

    void AtualizarPacotes()
    {
        for (int k = pacotes.Count - 1; k >= 0; k--)
        {
            Pacote p = pacotes[k];
            if (Time.time >= p.expira || p.visual == null)
            {
                if (p.visual != null)
                    Destroy(p.visual.gameObject);
                pacotes.RemoveAt(k);
                continue;
            }
            p.visual.transform.position = p.base_ + Vector3.up * (Mathf.Sin(Time.time * 4f + k) * 0.05f);
            float restante = p.expira - Time.time;
            p.visual.enabled = restante > 5f || Mathf.Repeat(Time.time * 4f, 1f) > 0.35f;
        }
    }

    bool ColetarPacoteEm(Vector3 mundo)
    {
        for (int k = pacotes.Count - 1; k >= 0; k--)
        {
            Pacote p = pacotes[k];
            if (p.visual == null || Vector2.Distance(p.visual.transform.position, mundo) > 0.45f)
                continue;
            pilas += p.valor;
            AdicionarTexto(p.visual.transform.position + Vector3.up * 0.3f, "+" + p.valor + " pilas", new Color(1f, 0.86f, 0.25f));
            Destroy(p.visual.gameObject);
            pacotes.RemoveAt(k);
            return true;
        }
        return false;
    }

    public void ColetarTodosOsPacotes()
    {
        while (pacotes.Count > 0)
        {
            Pacote p = pacotes[0];
            pilas += p.valor;
            if (p.visual != null)
                Destroy(p.visual.gameObject);
            pacotes.RemoveAt(0);
        }
    }

    bool Fortalecer(TorreGaucha torre, Especial especial)
    {
        List<TorreGaucha> candidatos = new List<TorreGaucha>();
        foreach (TorreGaucha t in torres)
        {
            if (t == null || t == torre || t.Ajudante || t.Atributos.modo == ModoAtaque.Nenhum)
                continue;
            if (Vector2.Distance(t.Casa, torre.Casa) > torre.Alcance)
                continue;
            candidatos.Add(t);
        }
        if (candidatos.Count == 0)
            return false;

        candidatos.Sort((a, b) => b.Gasto.CompareTo(a.Gasto));
        int quantidade = Mathf.Min(especial.quantidade, candidatos.Count);
        for (int i = 0; i < quantidade; i++)
        {
            TorreGaucha alvo = candidatos[i];
            alvo.AplicarBuff(especial.buffDano, especial.buffAlcance, especial.buffCadencia, especial.duracao, especial.porOnda);
            if (especial.permanente)
                alvo.AfiarParaSempre();
            CriarEstouro(alvo.transform.position + Vector3.up * 0.3f, new Color(1f, 0.95f, 0.4f), 8, 0.25f);
        }
        return true;
    }

    DefinicaoTorre DefinicaoEspantalho()
    {
        if (definicaoEspantalho == null)
        {
            definicaoEspantalho = new DefinicaoTorre
            {
                nome = "Espantalho",
                corpo = "espantalho",
                ajudante = true,
                classe = ClasseTorre.Estancia
            };
            for (int c = 0; c < 3; c++)
                definicaoEspantalho.caminhos[c] = new Melhoria[0];
        }
        return definicaoEspantalho;
    }

    bool CriarEspantalho(TorreGaucha torre, Especial especial)
    {
        if (torre.AjudantesAtivos >= especial.maximo)
            return false;
        DefinicaoTorre definicao = DefinicaoEspantalho();
        for (int tentativa = 0; tentativa < 14; tentativa++)
        {
            Vector2 sorteio = Random.insideUnitCircle.normalized * Random.Range(0.95f, Mathf.Max(1.05f, torre.Alcance * 0.9f));
            Vector3 p = torre.Casa + (Vector3)sorteio;
            p.x = Mathf.Round(p.x * 2f) / 2f;
            p.y = Mathf.Round(p.y * 2f) / 2f;
            if (!PosicaoDeTorreValida(p, definicao))
                continue;
            TorreGaucha ajudante = CriarTorre(definicao, p, false);
            ajudante.ConfigurarAjudante(this, definicao, torre, especial.ataque, especial.duracao);
            torres.Add(ajudante);
            torre.AjudantesAtivos++;
            CriarEstouro(p, new Color(0.9f, 0.8f, 0.4f), 8, 0.2f);
            return true;
        }
        return false;
    }

    public void AplicarAuras(TorreGaucha alvo, AtributosTorre e)
    {
        float alcance = 0f;
        float cadencia = 0f;
        float danoGrandes = 0f;
        int dano = 0;
        int perfuracao = 0;
        bool furtivo = false;
        bool blindagem = false;
        bool imunidades = false;

        foreach (TorreGaucha apoio in torres)
        {
            if (apoio == null || apoio == alvo || apoio.EhPrevia || apoio.Atributos == null)
                continue;
            AtributosTorre a = apoio.Atributos;
            if (!a.EhApoio())
                continue;
            if (Vector2.Distance(apoio.transform.position, alvo.transform.position) > a.alcance)
                continue;
            alcance = Mathf.Max(alcance, a.auraAlcance);
            cadencia = Mathf.Max(cadencia, a.auraCadencia);
            danoGrandes = Mathf.Max(danoGrandes, a.auraDanoGrandes);
            dano = Mathf.Max(dano, a.auraDano);
            perfuracao = Mathf.Max(perfuracao, a.auraPerfuracao);
            furtivo |= a.auraVeFurtivo;
            blindagem |= a.auraFuraBlindagem;
            imunidades |= a.auraIgnoraImunidades;
        }

        e.alcance *= 1f + alcance;
        e.intervalo /= 1f + cadencia;
        e.dano += dano;
        e.perfuracao += perfuracao;
        e.veFurtivo |= furtivo;
        e.furaBlindagem |= blindagem;
        e.ignoraImunidades |= imunidades;
        if (danoGrandes > 0f)
            e.danoExtraGrandes += Mathf.Max(1, Mathf.RoundToInt(e.dano * danoGrandes));
    }

    float DescontoNoPonto(Vector3 posicao)
    {
        float melhor = 0f;
        foreach (TorreGaucha t in torres)
        {
            if (t == null || t.Atributos == null || t.Atributos.auraDesconto <= 0f)
                continue;
            if (Vector2.Distance(t.transform.position, posicao) <= t.Atributos.alcance)
                melhor = Mathf.Max(melhor, t.Atributos.auraDesconto);
        }
        return melhor;
    }

    float DescontoDeMelhoria(TorreGaucha torre)
    {
        float melhor = 0f;
        foreach (TorreGaucha t in torres)
        {
            if (t == null || t == torre || t.Atributos == null || t.Atributos.auraDescontoMelhoria <= 0f)
                continue;
            if (Vector2.Distance(t.transform.position, torre.transform.position) <= t.Atributos.alcance)
                melhor = Mathf.Max(melhor, t.Atributos.auraDescontoMelhoria);
        }
        return melhor;
    }

    float BonusDePilasNoPonto(Vector3 posicao)
    {
        float melhor = 0f;
        foreach (TorreGaucha t in torres)
        {
            if (t == null || t.Atributos == null || t.Atributos.bonusPilasNoAlcance <= 0f)
                continue;
            if (Vector2.Distance(t.transform.position, posicao) <= t.Alcance)
                melhor = Mathf.Max(melhor, t.Atributos.bonusPilasNoAlcance);
        }
        return melhor;
    }

    public void AplicarEfeitosDeAlcance(TorreGaucha torre, AtributosTorre e)
    {
        if (e.lentidaoNoAlcance <= 0f && e.marcaNoAlcance <= 0f && e.revelaRaio <= 0f)
            return;
        Vector3 centro = torre.transform.position;
        float alcanceQuadrado = e.alcance * e.alcance;
        float revelaQuadrado = e.revelaRaio * e.revelaRaio;
        foreach (InimigoGaucho inimigo in inimigosAtivos)
        {
            if (inimigo == null || !inimigo.EstaVivo)
                continue;
            float distancia = ((Vector2)(inimigo.transform.position - centro)).sqrMagnitude;
            if (distancia <= alcanceQuadrado && PodeFerir(inimigo, e))
            {
                if (e.lentidaoNoAlcance > 0f)
                    inimigo.AplicarLentidao(e.lentidaoNoAlcance, 0.35f);
                if (e.marcaNoAlcance > 0f)
                    inimigo.Marcar(e.marcaNoAlcance, 0.35f);
            }
            if (distancia <= revelaQuadrado)
                inimigo.Revelar();
        }
    }

    void RecalcularGlobais()
    {
        float lentidao = 0f;
        float reducao = 0f;
        float bonus = 0f;
        foreach (TorreGaucha t in torres)
        {
            if (t == null || t.Atributos == null || t.EhPrevia)
                continue;
            lentidao = Mathf.Max(lentidao, t.Atributos.lentidaoGlobal);
            reducao = Mathf.Max(reducao, t.Atributos.reducaoVidaGrandes);
            bonus = Mathf.Max(bonus, t.Atributos.bonusPilasGlobal);
        }
        LentidaoGlobal = lentidao;
        reducaoVidaGrandes = reducao;
        bonusPilasGlobal = bonus;

        bichoPertoDoGalpao = false;
        if (galpao != null)
        {
            foreach (InimigoGaucho inimigo in inimigosAtivos)
            {
                if (inimigo != null && Vector2.Distance(inimigo.transform.position, galpao.position) < 3.2f)
                {
                    bichoPertoDoGalpao = true;
                    break;
                }
            }
        }
    }

    public Vector3 PontoMaisProximoDoCaminho(Vector3 posicao)
    {
        int segmento;
        return ProjetarNoCaminho(posicao, out segmento);
    }

    Vector3 ProjetarNoCaminho(Vector3 posicao, out int segmento)
    {
        segmento = 0;
        Vector3 melhor = caminho[0].position;
        float melhorDistancia = float.MaxValue;
        for (int i = 0; i < caminho.Length - 1; i++)
        {
            Vector2 a = caminho[i].position;
            Vector2 b = caminho[i + 1].position;
            Vector2 ab = b - a;
            float t = ab.sqrMagnitude > 0.0001f ? Mathf.Clamp01(Vector2.Dot((Vector2)posicao - a, ab) / ab.sqrMagnitude) : 0f;
            Vector2 ponto = a + ab * t;
            float distancia = ((Vector2)posicao - ponto).sqrMagnitude;
            if (distancia < melhorDistancia)
            {
                melhorDistancia = distancia;
                melhor = ponto;
                segmento = i;
            }
        }
        return melhor;
    }

    public int IndiceMaisProximoDoCaminho(Vector3 posicao)
    {
        int melhor = 1;
        float melhorDistancia = float.MaxValue;
        for (int i = 1; i < caminho.Length; i++)
        {
            float distancia = Vector2.Distance(posicao, caminho[i].position);
            if (distancia < melhorDistancia)
            {
                melhorDistancia = distancia;
                melhor = i;
            }
        }
        return melhor;
    }

    List<Vector3> AmostrasDoCaminho(Vector3 origem, float alcance, List<float> progresso)
    {
        List<Vector3> amostras = new List<Vector3>();
        float alcanceQuadrado = alcance * alcance;
        for (int i = 0; i < caminho.Length - 1; i++)
        {
            Vector3 a = caminho[i].position;
            Vector3 b = caminho[i + 1].position;
            float comprimento = Vector2.Distance(a, b);
            int passos = Mathf.Max(1, Mathf.CeilToInt(comprimento / 0.4f));
            for (int s = 0; s <= passos; s++)
            {
                float t = (float)s / passos;
                Vector3 p = Vector3.Lerp(a, b, t);
                if (((Vector2)(p - origem)).sqrMagnitude > alcanceQuadrado)
                    continue;
                Vector3 viewport = cameraPrincipal.WorldToViewportPoint(p);
                if (viewport.x < 0f || viewport.x > 1f)
                    continue;
                amostras.Add(p);
                progresso.Add(i + t);
            }
        }
        return amostras;
    }

    Vector3 MelhorPontoNoCaminho(Vector3 origem, float alcance, out bool achou)
    {
        List<float> progresso = new List<float>();
        List<Vector3> amostras = AmostrasDoCaminho(origem, alcance, progresso);
        achou = false;
        if (amostras.Count == 0)
            return origem;

        int melhor = -1;
        int melhorContagem = 0;
        for (int i = 0; i < amostras.Count; i++)
        {
            int contagem = 0;
            foreach (InimigoGaucho inimigo in inimigosAtivos)
            {
                if (inimigo != null && inimigo.EstaVivo && ((Vector2)(inimigo.transform.position - amostras[i])).sqrMagnitude < 1f)
                    contagem++;
            }
            if (contagem > melhorContagem || (contagem == melhorContagem && contagem > 0 && progresso[i] > progresso[melhor]))
            {
                melhorContagem = contagem;
                melhor = i;
            }
        }
        if (melhor < 0)
            return origem;
        achou = true;
        return amostras[melhor];
    }

    Vector3 PontoAleatorioNoCaminho(Vector3 origem, float alcance, out bool achou)
    {
        List<float> progresso = new List<float>();
        List<Vector3> amostras = AmostrasDoCaminho(origem, alcance, progresso);
        achou = amostras.Count > 0;
        if (!achou)
            return origem;
        return amostras[Random.Range(0, amostras.Count)];
    }

    public static Color CorDoTipo(TipoDano tipo)
    {
        switch (tipo)
        {
            case TipoDano.Fogo: return new Color(1f, 0.5f, 0.15f, 0.9f);
            case TipoDano.Frio: return new Color(0.65f, 0.9f, 1f, 0.9f);
            case TipoDano.Magica: return new Color(0.75f, 0.5f, 1f, 0.9f);
            case TipoDano.Explosao: return new Color(1f, 0.7f, 0.3f, 0.9f);
            case TipoDano.Bala: return new Color(1f, 0.95f, 0.6f, 0.9f);
            case TipoDano.Corte: return new Color(0.9f, 0.9f, 0.95f, 0.9f);
            default: return new Color(0.95f, 0.85f, 0.6f, 0.9f);
        }
    }

    SpriteRenderer CriarSpriteSolto(string desenho, Vector3 posicao, int ordem, Color cor)
    {
        GameObject objeto = new GameObject(string.IsNullOrEmpty(desenho) ? "Efeito" : desenho);
        objeto.transform.SetParent(transform, false);
        posicao.z = 0f;
        objeto.transform.position = posicao;
        SpriteRenderer sr = objeto.AddComponent<SpriteRenderer>();
        if (!string.IsNullOrEmpty(desenho))
            sr.sprite = SpritesGaucho.Pegar(desenho);
        sr.sortingOrder = ordem;
        sr.color = cor;
        return sr;
    }

    void CriarFumaca(Vector3 posicao, string desenho, float raio, Color cor, float duracao)
    {
        SpriteRenderer sr = CriarSpriteSolto(desenho, posicao, 23, cor);
        if (sr.sprite == null)
            sr.sprite = SpritesGaucho.Circulo();
        float largura = Mathf.Max(0.1f, sr.sprite.bounds.size.x);
        float escala = raio * 2f / largura;
        sr.transform.localScale = Vector3.one * escala * 0.6f;
        fumacas.Add(new Fumaca { sprite = sr, inicio = Time.time, duracao = duracao, escala = escala, cor = cor });
    }

    public void CriarAnel(Vector3 centro, float raio, Color cor, float duracao)
    {
        GameObject objeto = new GameObject("Anel");
        objeto.transform.SetParent(transform, false);
        LineRenderer linha = objeto.AddComponent<LineRenderer>();
        linha.material = MaterialDeEfeito();
        linha.useWorldSpace = true;
        linha.loop = true;
        linha.positionCount = PontosDoAnelDeEfeito;
        linha.startWidth = 0.06f;
        linha.endWidth = 0.06f;
        linha.sortingOrder = 22;
        aneis.Add(new Anel { linha = linha, centro = centro, raio = raio, inicio = Time.time, duracao = duracao, cor = cor });
    }

    public void Rastro(Vector3 de, Vector3 ate, Color cor)
    {
        GameObject objeto = new GameObject("Rastro");
        objeto.transform.SetParent(transform, false);
        LineRenderer linha = objeto.AddComponent<LineRenderer>();
        linha.material = MaterialDeEfeito();
        linha.useWorldSpace = true;
        linha.positionCount = 2;
        linha.SetPosition(0, new Vector3(de.x, de.y, -0.1f));
        linha.SetPosition(1, new Vector3(ate.x, ate.y, -0.1f));
        linha.startWidth = 0.05f;
        linha.endWidth = 0.03f;
        linha.sortingOrder = 22;
        linha.startColor = cor;
        linha.endColor = cor;
        rastros.Add(new Rastro_ { linha = linha, inicio = Time.time, duracao = 0.1f, cor = cor });
    }

    public void Golpe(Vector3 de, Vector3 ate)
    {
        Rastro(de, ate, new Color(1f, 1f, 0.85f, 0.9f));
        CriarEstouro(ate, new Color(1f, 0.95f, 0.8f), 4, 0.08f);
    }

    void AtualizarEfeitos()
    {
        for (int i = aneis.Count - 1; i >= 0; i--)
        {
            Anel a = aneis[i];
            float t = (Time.time - a.inicio) / a.duracao;
            if (t >= 1f || a.linha == null)
            {
                if (a.linha != null)
                    Destroy(a.linha.gameObject);
                aneis.RemoveAt(i);
                continue;
            }
            float raio = Mathf.Lerp(0.2f, a.raio, Mathf.Sqrt(t));
            for (int p = 0; p < PontosDoAnelDeEfeito; p++)
            {
                float angulo = p * Mathf.PI * 2f / PontosDoAnelDeEfeito;
                a.linha.SetPosition(p, new Vector3(a.centro.x + Mathf.Cos(angulo) * raio, a.centro.y + Mathf.Sin(angulo) * raio, -0.1f));
            }
            Color cor = a.cor;
            cor.a *= 1f - t;
            a.linha.startColor = cor;
            a.linha.endColor = cor;
        }

        for (int i = rastros.Count - 1; i >= 0; i--)
        {
            Rastro_ r = rastros[i];
            float t = (Time.time - r.inicio) / r.duracao;
            if (t >= 1f || r.linha == null)
            {
                if (r.linha != null)
                    Destroy(r.linha.gameObject);
                rastros.RemoveAt(i);
                continue;
            }
            Color cor = r.cor;
            cor.a *= 1f - t;
            r.linha.startColor = cor;
            r.linha.endColor = cor;
        }

        for (int i = fumacas.Count - 1; i >= 0; i--)
        {
            Fumaca f = fumacas[i];
            float t = (Time.time - f.inicio) / f.duracao;
            if (t >= 1f || f.sprite == null)
            {
                if (f.sprite != null)
                    Destroy(f.sprite.gameObject);
                fumacas.RemoveAt(i);
                continue;
            }
            f.sprite.transform.localScale = Vector3.one * f.escala * Mathf.Lerp(0.6f, 1.1f, t);
            f.sprite.transform.Rotate(0f, 0f, 200f * Time.deltaTime);
            Color cor = f.cor;
            cor.a *= 1f - t;
            f.sprite.color = cor;
        }
    }

    public void CriarEstouro(Vector3 posicao, Color cor, int quantidade, float raio)
    {
        GameObject efeito = new GameObject("Estouro");
        efeito.transform.position = posicao;
        ParticleSystem particulas = efeito.AddComponent<ParticleSystem>();
        particulas.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule principal = particulas.main;
        principal.duration = 0.12f;
        principal.loop = false;
        principal.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.42f);
        principal.startSpeed = new ParticleSystem.MinMaxCurve(0.7f, 1.8f);
        principal.startSize = new ParticleSystem.MinMaxCurve(0.055f, 0.12f);
        principal.startColor = new ParticleSystem.MinMaxGradient(cor, Color.white);
        principal.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emissao = particulas.emission;
        emissao.rateOverTime = 0f;
        emissao.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)Mathf.Clamp(quantidade, 1, 40)) });

        ParticleSystem.ShapeModule forma = particulas.shape;
        forma.shapeType = ParticleSystemShapeType.Circle;
        forma.radius = Mathf.Max(0.05f, raio);

        ParticleSystem.ColorOverLifetimeModule corAoLongoDaVida = particulas.colorOverLifetime;
        corAoLongoDaVida.enabled = true;
        Gradient gradiente = new Gradient();
        gradiente.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(cor, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        corAoLongoDaVida.color = gradiente;

        ParticleSystemRenderer renderizador = efeito.GetComponent<ParticleSystemRenderer>();
        renderizador.material = MaterialDeEfeito();
        renderizador.sortingOrder = 30;
        particulas.Play();
        Destroy(efeito, 0.8f);
    }

    void LimparEfeitos()
    {
        if (materialParticula != null)
            Destroy(materialParticula);
    }
}
