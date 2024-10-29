using UnityEngine;

public class AmbientMusicTrigger : MonoBehaviour
{
    [SerializeField] private float _fadeTime = 5f;

    private OWTriggerVolume _trigger;
    private OWAudioSource _audio;

    private void Awake()
    {
        _trigger = GetComponent<OWTriggerVolume>();
        _audio = GetComponent<OWAudioSource>();
    }

    private void Start()
    {
        _trigger.OnEntry += OnEntry;
        _trigger.OnExit += OnExit;
    }

    private void OnEntry(GameObject hitObj)
    {
        if (hitObj.CompareTag("PlayerDetector"))
        {
            _audio.FadeIn(_fadeTime);
        }
    }

    private void OnExit(GameObject hitObj)
    {
        if (hitObj.CompareTag("PlayerDetector"))
        {
            _audio.FadeOut(_fadeTime);
        }
    }

    private void OnDestroy()
    {
        _trigger.OnEntry -= OnEntry;
        _trigger.OnExit -= OnExit;
    }
}
