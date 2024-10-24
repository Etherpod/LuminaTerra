using UnityEngine;

namespace Jam4Mod;

public class Conductor : MonoBehaviour
{
  [SerializeField] private EndOfLoopController endOfLoopController;
  [SerializeField] private OWTriggerVolume trigger;

  private void Awake()
  {
    trigger.OnEntry += TriggerEntered;
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