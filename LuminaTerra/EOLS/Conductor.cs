using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LuminaTerra;

public class Conductor : MonoBehaviour
{
    [SerializeField] private EndOfLoopController endOfLoopController = null;
    [SerializeField] private OWAudioSource musicSource = null;
    [SerializeField] private OWTriggerVolume trigger = null;

    private bool startedSequence = false;

    private void Awake()
    {
        trigger.OnEntry += TriggerEntered;
    }

    private void OnDestroy()
    {
        trigger.OnEntry -= TriggerEntered;
    }

    private void Update()
    {
        if (Keyboard.current.slashKey.wasPressedThisFrame && !startedSequence)
        {
            StartCoroutine(StartEOLS());
            startedSequence = true;
        }
        else if (!startedSequence && Time.timeSinceLevelLoad > 1000000f - 32f)
        {
            StartCoroutine(StartEOLS());
            startedSequence = true;
        }
    }

    private IEnumerator StartEOLS()
    {
        Locator.GetAudioMixer().MixEndTimes(5f);
        musicSource.Play();
        
        yield return new WaitUntil(() => musicSource.time >= 29f);

        FindObjectOfType<PlayerCameraEffectController>().CloseEyes(3f);

        yield return new WaitUntil(() => musicSource.time >= 32f);

        endOfLoopController.StartEOLS();
    }

    private void TriggerEntered(GameObject hitObj)
    {
        if (!hitObj.CompareTag("PlayerDetector")) return;

        endOfLoopController.StartEOLS();
    }
}