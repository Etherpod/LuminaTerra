using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Jam4Mod;

public class EndOfLoopController : MonoBehaviour
{
    private static readonly float SunScale = 0.3f;

    [SerializeField] private Transform playerSpawn = null;
    [SerializeField] private Transform sunSpawn = null;
    [FormerlySerializedAs("bounds")] [SerializeField] private MeshRenderer boundsRenderer = null;
    [SerializeField] private ParticleSystem stars = null;
    [SerializeField] private GameObject[] refs = null;

    public void Awake()
    {
        boundsRenderer.enabled = false;
        foreach (var refObj in refs)
        {
            refObj.SetActive(false);
        }
        stars.Simulate(22f);
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
        sun.transform.localScale = Vector3.one * SunScale;

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