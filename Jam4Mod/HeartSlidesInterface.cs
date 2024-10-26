using UnityEngine;
using System.Collections;

namespace Jam4Mod;

public class HeartSlidesInterface : MonoBehaviour
{
    [SerializeField]
    private OWTriggerVolume _heartRoomTrigger = null;
    [SerializeField]
    private OWAudioSource _oneShotAudio;
    [SerializeField]
    private OWAudioSource _loopingAudio;

    private MindProjectorTrigger _projector;
    private Animator _animator;
    private bool _hasActivated = false;

    private void Awake()
    {
        _projector = GetComponentInChildren<MindProjectorTrigger>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _heartRoomTrigger.OnEntry += OnEntry;
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

    private void OnDestroy()
    {
        _heartRoomTrigger.OnEntry -= OnEntry;
    }
}
