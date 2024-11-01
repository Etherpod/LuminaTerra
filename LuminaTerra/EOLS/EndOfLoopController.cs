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
    [SerializeField] private OWAudioSource windAudio = null;
    [SerializeField] private CylinderShape transferArea = null;
    [SerializeField] private Material sunMaterial = null;
    [SerializeField] private float sunDeathProgress = 0f;
    [SerializeField] private GameObject[] refs = null;
    
    private PlayerCameraEffectController _playerCameraEffectController;
    private GameObject _sun;
    private Animator _sunAnimator;
    private Conductor _conductor;
    private OWAudioSource[] _planetAmbienceVolumes;

    private void Awake()
    {
        _playerCameraEffectController = FindObjectOfType<PlayerCameraEffectController>();
        _sunAnimator = gameObject.GetComponent<Animator>();
        LuminaTerra.Instance.NewHorizons.GetBodyLoadedEvent().AddListener(OnPlanetLoad);

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
        SunOverrideVolume vol = GetComponentInChildren<SunOverrideVolume>();
        vol._sector = GetComponentInParent<Sector>();
        vol._sector.OnSectorOccupantsUpdated += vol.OnSectorOccupantsUpdated;
    }

    private void OnPlanetLoad(string name)
    {
        if (name == "Living Planet")
        {
            _conductor = LuminaTerra.Instance.NewHorizons.GetPlanet(name).GetComponentInChildren<Conductor>();
            _planetAmbienceVolumes = _conductor.GetPlanetAmbience();
            _conductor.AssignEOLController(this);
        }
    }

    private void Update()
    {
        sunMaterial.SetFloat(PropColorTime, sunDeathProgress);

        float num = Mathf.InverseLerp(ambientSound.clip.length - 20f, ambientSound.clip.length, ambientSound.time);
        _playerCameraEffectController._owCamera.postProcessingSettings.colorGrading.saturation = Mathf.Lerp(1f, 0f, num);
        _playerCameraEffectController._owCamera.postProcessingSettings.colorGrading.postExposure = Mathf.Lerp(1f, 0.2f, num);
        _playerCameraEffectController._owCamera.postProcessingSettings.vignette.intensity = Mathf.Lerp(0f, 0.6f, num);

        if (num >= 0.99)
        {
            EndEOLS();
        }
        else if (!windAudio.isPlaying && num > 0)
        {
            windAudio.FadeIn(15f, true, true);
        }
    }

    public void StartEOLS()
    {
        gameObject.SetActive(true);

        foreach (var transferable in FindObjectsOfType<EOLSTransferable>())
        {
             if (!transferable.IsActivated) continue;
            var objTransform = transferable.transform;
            var objRelativePos = _conductor.GetMainRitualTable().InverseTransformVector(objTransform.position - _conductor.GetMainRitualTable().position);
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

        foreach (OWAudioSource source in _planetAmbienceVolumes)
        {
            source.Stop();
        }

        ambientSound.FadeIn(3, true);
        _sunAnimator.SetTrigger(TriggerDie);

        _playerCameraEffectController._owCamera.postProcessingSettings.vignetteEnabled = true;
        _playerCameraEffectController._owCamera.postProcessingSettings.vignette.color = Color.black;
        _playerCameraEffectController._owCamera.postProcessingSettings.vignette.intensity = 0f;
        _playerCameraEffectController._owCamera.postProcessingSettings.vignette.opacity = 1f;
        _playerCameraEffectController._owCamera.postProcessingSettings.vignette.smoothness = 1f;
        _playerCameraEffectController.OpenEyes(0.3f);
    }

    private void EndEOLS()
    {
        Locator.GetDeathManager().KillPlayer(DeathType.TimeLoop);
        enabled = false;
    }

    private void OnDestroy()
    {
        LuminaTerra.Instance.NewHorizons.GetBodyLoadedEvent().RemoveListener(OnPlanetLoad);
    }
}