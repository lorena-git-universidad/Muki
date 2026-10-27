
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MukiScreamerVideos : MonoBehaviour
{
    [Header("Referencias")]
    public VideoPlayer videoPlayer;
    public RawImage videoImage;
    public RenderTexture videoTexture;

    [Header("Videos")]
    public VideoClip attackScreamer;
    public VideoClip finalDeathScreamer;

    private bool isPlayingVideo;
    private bool videoFinished;

    public bool HasAttackScreamer => attackScreamer != null;
    public bool HasFinalDeathScreamer => finalDeathScreamer != null;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoImage != null)
        {
            // Ocultar el objeto completo al comenzar.
            videoImage.gameObject.SetActive(false);
            videoImage.enabled = true;
        }

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.waitForFirstFrame = true;

            if (videoTexture != null)
            {
                videoPlayer.targetTexture = videoTexture;

                // Asegurar que la textura esté disponible.
                if (!videoTexture.IsCreated())
                    videoTexture.Create();
            }

            videoPlayer.Stop();

            // Detectar cuándo termina realmente el video.
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        videoFinished = true;
    }

    public IEnumerator PlayAttackScreamer()
    {
        if (attackScreamer == null)
        {
            Debug.LogWarning(
                "MukiScreamerVideos: no se asignó Attack Screamer."
            );
            yield break;
        }

        yield return StartCoroutine(
            PlayVideo(attackScreamer, true)
        );
    }

    public IEnumerator PlayFinalDeathScreamer()
    {
        if (finalDeathScreamer == null)
        {
            Debug.LogWarning(
                "MukiScreamerVideos: no se asignó Final Death Screamer."
            );
            yield break;
        }

        yield return StartCoroutine(
            PlayVideo(finalDeathScreamer, false)
        );
    }

    private IEnumerator PlayVideo(VideoClip clip, bool hideAfter)
    {
        if (isPlayingVideo)
        {
            Debug.LogWarning(
                "MukiScreamerVideos: ya hay un video reproduciéndose."
            );
            yield break;
        }

        if (videoPlayer == null ||
            videoImage == null ||
            videoTexture == null)
        {
            Debug.LogError(
                "MukiScreamerVideos: asigna Video Player, " +
                "Video Image y Video Texture en el Inspector."
            );
            yield break;
        }

        isPlayingVideo = true;
        videoFinished = false;

        // Mantener oculto el screamer mientras se prepara.
        videoImage.gameObject.SetActive(false);

        videoPlayer.Stop();
        videoPlayer.clip = clip;
        videoPlayer.isLooping = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoTexture;
        videoImage.texture = videoTexture;

        if (!videoTexture.IsCreated())
            videoTexture.Create();

        Debug.Log("SCREAMER: preparando video " + clip.name);

        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        // Mostrar el screamer y reproducirlo.
        videoImage.gameObject.SetActive(true);
        videoImage.enabled = true;

        Debug.Log("SCREAMER: reproduciendo " + clip.name);

        videoPlayer.Play();

        // Esperar hasta que VideoPlayer indique que terminó.
        while (!videoFinished)
            yield return null;

        videoPlayer.Stop();

        if (hideAfter)
        {
            // El screamer normal desaparece al terminar.
            videoImage.gameObject.SetActive(false);
        }
        else
        {
            // El screamer final queda visible hasta que
            // se cargue la escena de derrota.
            videoImage.gameObject.SetActive(true);
        }

        isPlayingVideo = false;

        Debug.Log("SCREAMER: terminó " + clip.name);
    }
}
