using UnityEngine;

public sealed class MusicaDeFundo : MonoBehaviour
{
    const string CaminhoDaMusica = "Musica/las_dos_fronteras";
    const float Volume = 0.35f;

    static MusicaDeFundo instancia;
    static AudioSource fonte;

    public static void AjustarVolume(float fator)
    {
        if (fonte != null)
            fonte.volume = Volume * fator;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Iniciar()
    {
        if (instancia != null)
            return;

        AudioClip musica = Resources.Load<AudioClip>(CaminhoDaMusica);
        if (musica == null)
        {
            Debug.LogWarning("Game Gaucho: nao encontrei a musica Resources/" + CaminhoDaMusica + ".");
            return;
        }

        GameObject objeto = new GameObject("Musica de fundo");
        DontDestroyOnLoad(objeto);
        instancia = objeto.AddComponent<MusicaDeFundo>();

        fonte = objeto.AddComponent<AudioSource>();
        fonte.clip = musica;
        fonte.loop = true;
        fonte.spatialBlend = 0f;
        OpcoesGaucho.Carregar();
        AjustarVolume(OpcoesGaucho.VolumeMusica);
        fonte.Play();
    }
}
