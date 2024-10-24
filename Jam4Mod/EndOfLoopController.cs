using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jam4Mod;

public class EndOfLoopController : MonoBehaviour
{
  [FormerlySerializedAs("spawnPoint")] [SerializeField] private Transform playerSpawn = null;
  [SerializeField] private Transform sunSpawn = null;
  [SerializeField] private MeshRenderer bounds = null;
  [SerializeField] private ParticleSystem stars = null;

  public void Awake()
  {
    bounds.enabled = false;
  }

  public void StartEOLS()
  {
    gameObject.SetActive(true);
    
    stars.Play();
    
    var sun = Instantiate(GameObject.Find("Jam4Sun_Body/Sector/Star/Surface"));
    sun.transform.position = sunSpawn.position;
    sun.transform.rotation = sunSpawn.rotation;
    
    var player = Locator.GetPlayerBody();
    player.SetPosition(playerSpawn.position);
    player.SetRotation(playerSpawn.rotation);
  }
}