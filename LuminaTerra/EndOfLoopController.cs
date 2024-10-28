using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace LuminaTerra;

public class EndOfLoopController : MonoBehaviour
{
    [SerializeField] private Transform playerSpawn = null;
    [SerializeField] private MeshRenderer boundsRenderer = null;
    [SerializeField] private ParticleSystem stars = null;
    [SerializeField] private OWAudioSource ambientSound = null;
    [SerializeField] private GameObject[] refs = null;
    
    private PlayerCameraEffectController _playerCameraEffectController;

    public void Awake()
    {
        _playerCameraEffectController = FindObjectOfType<PlayerCameraEffectController>();
        
        boundsRenderer.enabled = false;
        foreach (var refObj in refs)
        {
            refObj.SetActive(false);
        }
    }

    public void StartEOLS()
    {
        gameObject.SetActive(true);
        stars.Simulate(22f);
        stars.Play();

        var player = Locator.GetPlayerBody();
        player.SetPosition(playerSpawn.position);
        player.SetRotation(playerSpawn.rotation);
        player.SetVelocity(gameObject.GetAttachedOWRigidbody().GetVelocity());

        Locator.GetPlayerSuit().RemoveSuit(true);
        Locator.GetFlashlight().TurnOff();
        Locator.GetToolModeSwapper().UnequipTool();

        ambientSound.FadeIn(3, true);

        _playerCameraEffectController.OpenEyes(0.3f);

        StartCoroutine(EndEOLS());
    }

    public IEnumerator EndEOLS()
    {
        yield return new WaitUntil(() => ambientSound.isPlaying == false);
        
        _playerCameraEffectController.PlayRealityShatterEffect();
    }
}