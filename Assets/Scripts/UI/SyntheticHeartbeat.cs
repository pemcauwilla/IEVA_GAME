using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SyntheticHeartbeat : MonoBehaviour
{
    public LowCoherenceFeedback Feedback;

    private AudioSource source;
    private AudioClip generatedClip;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.Stop();

        const int sampleRate = 22050;
        float[] samples = new float[sampleRate];

        // Une seconde : deux battements, puis du silence.
        for (int i = 0; i < samples.Length; i++)
        {
            float time = (float)i / sampleRate;

            samples[i] =
                Beat(time, 0.05f, 0.16f, 150f, 0.8f) +
                Beat(time, 0.28f, 0.13f, 180f, 0.55f);
        }

        generatedClip = AudioClip.Create(
            "SyntheticHeartbeat", samples.Length, 1, sampleRate, false
        );

        generatedClip.SetData(samples, 0);
        source.clip = generatedClip;
    }

    private static float Beat(
        float time, float start, float duration,
        float frequency, float amplitude)
    {
        float elapsed = time - start;

        if (elapsed < 0f || elapsed >= duration)
            return 0f;

        float progress = elapsed / duration;

        // Attaque et décroissance douces pour éviter les clics.
        float envelope =
            Mathf.Sin(Mathf.PI * progress) *
            Mathf.Exp(-4f * progress);

        return amplitude * envelope *
            Mathf.Sin(2f * Mathf.PI * frequency * elapsed);
    }

    private void Update()
    {
        bool shouldPlay =
            Feedback != null &&
            Feedback.isActiveAndEnabled &&
            Feedback.IsAlertActive;

        if (shouldPlay && !source.isPlaying)
            source.Play();
        else if (!shouldPlay && source.isPlaying)
            source.Stop();
    }

    private void OnDisable()
    {
        if (source != null)
            source.Stop();
    }

    private void OnDestroy()
    {
        if (generatedClip != null)
            Destroy(generatedClip);
    }

    [ContextMenu("Afficher le diagnostic audio")]
    private void DiagnoseAudio()
    {
        AudioSource audioSource = GetComponent<AudioSource>();

        Debug.Log(
            $"Lecture={audioSource.isPlaying}, " +
            $"Clip={audioSource.clip?.name}, " +
            $"Volume={audioSource.volume}, " +
            $"Mute={audioSource.mute}, " +
            $"ListenerPause={AudioListener.pause}, " +
            $"ListenerVolume={AudioListener.volume}",
            this
        );
    }
}