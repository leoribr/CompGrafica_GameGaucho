using UnityEngine;

public sealed class MusicaDeFundo : MonoBehaviour
{
    const string CaminhoDaMusica = "Musica/las_dos_fronteras";
    const float Volume = 0.35f;

    static MusicaDeFundo instancia;

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

        AudioSource fonte = objeto.AddComponent<AudioSource>();
        fonte.clip = musica;
        fonte.loop = true;
        fonte.volume = Volume;
        fonte.spatialBlend = 0f;
        fonte.Play();
    }
}
