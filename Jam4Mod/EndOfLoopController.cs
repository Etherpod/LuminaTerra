using System;
using UnityEngine;

namespace Jam4Mod;

public class EndOfLoopController : MonoBehaviour
{
  [SerializeField] private Transform spawnPoint;

  public void StartEOLS()
  {
    enabled = true;
    
    var player = Locator.GetPlayerBody();
    player.SetPosition(spawnPoint.position);
    player.SetRotation(spawnPoint.rotation);
  }
}