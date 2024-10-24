using UnityEngine;

namespace Jam4Mod;

public class EndOfLoopController : MonoBehaviour
{
  private static readonly float SunScale = 0.8f;

  [SerializeField] private Transform playerSpawn = null;
  [SerializeField] private Transform sunSpawn = null;
  [SerializeField] private MeshRenderer bounds = null;
  [SerializeField] private ParticleSystem stars = null;
  [SerializeField] private GameObject[] refs = null;

  public void Awake()
  {
    bounds.enabled = false;
    foreach (var refObj in refs)
    {
      refObj.SetActive(false);
    }
  }

  public void StartEOLS()
  {
    gameObject.SetActive(true);

    stars.Play();

    var sun = Instantiate(
      GameObject.Find("Jam4Sun_Body/Sector/Star/Surface"),
      sunSpawn.position,
      sunSpawn.rotation,
      transform
    );
    sun.transform.localScale = new Vector3(SunScale, SunScale, SunScale);

    var player = Locator.GetPlayerBody();
    player.SetPosition(playerSpawn.position);
    player.SetRotation(playerSpawn.rotation);

    Locator.GetPlayerSuit().RemoveSuit(true);
  }
}