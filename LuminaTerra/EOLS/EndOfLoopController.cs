using System;
using System.Collections;
using UnityEngine;

namespace LuminaTerra;

public class EndOfLoopController : MonoBehaviour
{
    private static readonly int PropColorTime = Shader.PropertyToID("_ColorTime");
    private static readonly int TriggerDie = Animator.StringToHash("die");
    
    [SerializeField] private Transform playerSpawn = null;
    [SerializeField] private MeshRenderer boundsRenderer = null;
    [SerializeField] private ParticleSystem stars = null;
    [SerializeField] private OWAudioSource ambientSound = null;
    [SerializeField] private CylinderShape transferArea = null;
    [SerializeField] private Transform mainRitualTable = null;
    [SerializeField] private Material sunMaterial = null;
    [SerializeField] private float sunDeathProgress = 0f;
    [SerializeField] private GameObject[] refs = null;
    
    private PlayerCameraEffectController _playerCameraEffectController;
    private GameObject _sun;
    private Animator _sunAnimator;

    private void Awake()
    {
        _playerCameraEffectController = FindObjectOfType<PlayerCameraEffectController>();
        _sunAnimator = gameObject.GetComponent<Animator>();
        
        boundsRenderer.enabled = false;
        var transferAreaRenderer = transferArea.GetComponent<MeshRenderer>();
        if (transferAreaRenderer) transferAreaRenderer.enabled = false;

        foreach (var refObj in refs)
        {
            refObj.SetActive(false);
        }
    }

    private void Start()
    {
        _sun = GameObject.Find("Jam4Sun_Body/Sector/Star");
    }

    private void Update()
    {
        sunMaterial.SetFloat(PropColorTime, sunDeathProgress);
    }

    public void StartEOLS()
    {
        gameObject.SetActive(true);

        foreach (var transferable in FindObjectsOfType<EOLSTransferable>())
        {
            print($"{transferable.name} - {transferable.IsActivated}");
            // if (!transferable.IsActivated) continue;
            var objTransform = transferable.transform;
            var objRelativePos = mainRitualTable.InverseTransformVector(objTransform.position - mainRitualTable.position);
            var objLocalPos = new Vector3(objRelativePos.x, 0, objRelativePos.z);
            var distance = (float)(2 / (1 + Math.Exp(-objLocalPos.magnitude)) - 1) * transferArea.radius;
            objTransform.SetParent(transferArea.transform);
            objTransform.localPosition = objLocalPos.normalized * distance;
            objTransform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        
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
        _sunAnimator.SetTrigger(TriggerDie);

        _playerCameraEffectController.OpenEyes(0.3f);

        StartCoroutine(EndEOLS());
    }

    private IEnumerator EndEOLS()
    {
        yield return new WaitUntil(() => ambientSound.isPlaying == false);

        Locator.GetDeathManager().KillPlayer(DeathType.Default);
    }
}