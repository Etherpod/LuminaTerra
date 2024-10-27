using System.Collections;
using UnityEngine;

namespace Jam4Mod;

public class EndOfLoopController : MonoBehaviour
{
    private static readonly float SunScale = 0.3f;

    [SerializeField] private GameObject sequenceParent = null;
    [SerializeField] private Transform playerSpawn = null;
    [SerializeField] private Transform sunSpawn = null;
    [SerializeField] private MeshRenderer bounds = null;
    [SerializeField] private ParticleSystem stars = null;
    [SerializeField] private GameObject[] refs = null;
    [SerializeField] private OWAudioSource musicAudio = null;

    public void Awake()
    {
        bounds.enabled = false;
        foreach (var refObj in refs)
        {
            refObj.SetActive(false);
        }
        sequenceParent.SetActive(false);
    }

    public void StartEOLS()
    {
        musicAudio.Play();
        StartCoroutine(TeleportDelay());
    }

    private IEnumerator TeleportDelay()
    {
        yield return new WaitUntil(() => musicAudio.time >= 29f);

        FindObjectOfType<PlayerCameraEffectController>().CloseEyes(3f);

        yield return new WaitUntil(() => musicAudio.time >= 32f);

        sequenceParent.SetActive(true);

        stars.Simulate(22f);
        stars.Play();

        var sun = Instantiate(
          GameObject.Find("Jam4Sun_Body/Sector/Star/Surface"),
          sunSpawn.position,
          sunSpawn.rotation,
          transform
        );
        sun.transform.localScale = Vector3.one * SunScale;

        var atmo = Instantiate(
            GameObject.Find("Jam4Sun_Body/Sector/Star/Atmosphere_Star/AtmoSphere/Atmosphere_LOD2"),
            sunSpawn.position,
            sunSpawn.rotation,
            transform);
        atmo.transform.localScale = Vector3.one * SunScale * 1.2f;

        var player = Locator.GetPlayerBody();
        player.SetPosition(playerSpawn.position);
        player.SetRotation(playerSpawn.rotation);
        player.SetVelocity(gameObject.GetAttachedOWRigidbody().GetVelocity());

        Locator.GetPlayerSuit().RemoveSuit(true);
        Locator.GetFlashlight().TurnOff();
        Locator.GetToolModeSwapper().UnequipTool();

        FindObjectOfType<PlayerCameraEffectController>().OpenEyes(0.3f);
    }
}