using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyDistanceVolume : MonoBehaviour
{
    [SerializeField] private Transform player;

    [SerializeField] private float maxHearingDistance = 20f;
    [SerializeField] private float fullVolumeDistance = 2f;
    [SerializeField, Range(0f, 1f)] private float maxVolume = 1f;

    private AudioSource src;
    private float originalVolume;

    private void Awake()
    {
        src = GetComponent<AudioSource>();
        originalVolume = src.volume;

        src.loop = true;
        src.spatialBlend = 1f;

        if (!src.isPlaying) src.Play();
    }

    private void Update()
    {
        if (player == null) return;

        float d = Vector3.Distance(player.position, transform.position);

        float t = Mathf.InverseLerp(maxHearingDistance, fullVolumeDistance, d);
        src.volume = Mathf.Clamp01(t) * maxVolume;
    }

    private void OnDisable()
    {
        // When you disable/remove the script during play, restore a reasonable volume
        if (src != null) src.volume = originalVolume;
    }

    private void OnDestroy()
    {
        if (src != null) src.volume = originalVolume;
    }
}
