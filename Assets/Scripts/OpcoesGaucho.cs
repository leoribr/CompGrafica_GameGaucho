using UnityEngine;

public static class OpcoesGaucho
{
    const string ChaveVolume = "volumeGeral";
    const string ChaveMusica = "volumeMusica";

    static bool carregado;

    public static float VolumeGeral { get; private set; } = 1f;
    public static float VolumeMusica { get; private set; } = 1f;

    public static void Carregar()
    {
        if (carregado)
            return;
        carregado = true;
        VolumeGeral = PlayerPrefs.GetFloat(ChaveVolume, 1f);
        VolumeMusica = PlayerPrefs.GetFloat(ChaveMusica, 1f);
        Aplicar();
    }

    static void Aplicar()
    {
        AudioListener.volume = VolumeGeral;
        MusicaDeFundo.AjustarVolume(VolumeMusica);
    }

    public static float Desenhar(Rect area, GUIStyle texto, GUIStyle botao, float e)
    {
        Carregar();
        float y = area.y;
        float rotulo = area.width * 0.38f;
        GUI.Label(new Rect(area.x, y, rotulo, 34f * e), "Volume geral", texto);
        float geral = GUI.HorizontalSlider(new Rect(area.x + rotulo, y + 12f * e, area.width - rotulo, 20f * e), VolumeGeral, 0f, 1f);
        y += 46f * e;
        GUI.Label(new Rect(area.x, y, rotulo, 34f * e), "Música", texto);
        float musica = GUI.HorizontalSlider(new Rect(area.x + rotulo, y + 12f * e, area.width - rotulo, 20f * e), VolumeMusica, 0f, 1f);
        y += 52f * e;

        if (!Mathf.Approximately(geral, VolumeGeral) || !Mathf.Approximately(musica, VolumeMusica))
        {
            VolumeGeral = geral;
            VolumeMusica = musica;
            PlayerPrefs.SetFloat(ChaveVolume, geral);
            PlayerPrefs.SetFloat(ChaveMusica, musica);
            Aplicar();
        }

        if (GUI.Button(new Rect(area.x, y, area.width, 54f * e), "TELA CHEIA: " + (Screen.fullScreen ? "SIM" : "NÃO"), botao))
            Screen.fullScreen = !Screen.fullScreen;
        y += 64f * e;
        return y - area.y;
    }
}
