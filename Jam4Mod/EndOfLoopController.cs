using System;
using UnityEngine;

namespace Jam4Mod;

public class EndOfLoopController : MonoBehaviour
{
  [SerializeField] private Transform spawnPoint;
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
    Debug.LogError("triggered");
    if (!hitObj.CompareTag("PlayerDetector")) return;

    var player = Locator.GetPlayerBody();
    player.SetPosition(spawnPoint.position);
    player.SetRotation(spawnPoint.rotation);
  }
}