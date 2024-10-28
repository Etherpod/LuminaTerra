using UnityEngine;
using System.Collections;

namespace LuminaTerra;

public class HeartSlidesInterface : MonoBehaviour
{
    [SerializeField]
    private OWTriggerVolume _heartRoomTrigger = null;
    [SerializeField]
    private OWAudioSource _musicAudio = null;
    [SerializeField]
    private OWAudioSource _oneShotAudio = null;
    [SerializeField]
    private OWAudioSource _loopingAudio = null;

    private MindProjectorTrigger _projector = null;
    private Animator _animator = null;
    private bool _hasActivated = false;

    private void Awake()
    {
        _projector = GetComponentInChildren<MindProjectorTrigger>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _heartRoomTrigger.OnEntry += OnEntry;
        _projector._mindProjector.OnProjectionStart += PlayMusic;
        _projector._mindProjector.OnProjectionStop += StopMusic;
    }

    private void OnEntry(GameObject hitObj)
    {
        if (!_hasActivated && hitObj.CompareTag("PlayerDetector"))
        {
            StartCoroutine(EmergenceSequence());
            _hasActivated = true;
        }
    }

    private IEnumerator EmergenceSequence()
    {
        yield return new WaitForSeconds(6f);
        _animator.SetTrigger("Emerge");
        _oneShotAudio.PlayOneShot(AudioType.NomaiDoorStart);
        _loopingAudio.AssignAudioLibraryClip(AudioType.NomaiDoorSlide_LP);
        _loopingAudio.FadeIn(0.5f, true);
        yield return new WaitForSeconds(8f);
        _projector.SetProjectorActive(true);
    }

    public void CompleteEmerge()
    {
        _oneShotAudio.PlayOneShot(AudioType.NomaiDoorStop);
        _loopingAudio.Stop();
    }

    private void PlayMusic()
    {
        _musicAudio.time = 0f;
        _musicAudio.FadeIn(3f);
    }

    private void StopMusic()
    {
        _musicAudio.FadeOut(10f);
    }

    private void OnDestroy()
    {
        _heartRoomTrigger.OnEntry -= OnEntry;
        _projector._mindProjector.OnProjectionStart -= PlayMusic;
        _projector._mindProjector.OnProjectionStop -= StopMusic;
    }
}
