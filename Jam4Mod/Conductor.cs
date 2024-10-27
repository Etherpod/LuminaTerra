using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam4Mod;

public class Conductor : MonoBehaviour
{
    [SerializeField] private EndOfLoopController endOfLoopController = null;
    [SerializeField] private OWTriggerVolume trigger = null;

    private bool startedSequence = false;

    private void Awake()
    {
        trigger.OnEntry += TriggerEntered;
    }

    private void Update()
    {
        if (Keyboard.current.uKey.wasPressedThisFrame && !startedSequence)
        {
            endOfLoopController.StartEOLS();
            startedSequence = true;
        }
    }

    private void OnDestroy()
    {
        trigger.OnEntry -= TriggerEntered;
    }

    private void TriggerEntered(GameObject hitObj)
    {
        if (!hitObj.CompareTag("PlayerDetector")) return;

        endOfLoopController.StartEOLS();
    }
}