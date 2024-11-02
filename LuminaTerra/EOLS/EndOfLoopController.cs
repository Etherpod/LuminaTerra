using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Transform transferPointsParent = null;
    [SerializeField] private Transform[] ritualTransferPoints = null;
    [SerializeField] private Material sunMaterial = null;
    [SerializeField] private float sunDeathProgress = 0f;
    [SerializeField] private GameObject[] refs = null;
    [SerializeField] private Transform heartTransform = null;
    
    private PlayerCameraEffectController _playerCameraEffectController;
    private Animator _sunAnimator;
    private Conductor _conductor;
    private OWAudioSource[] _planetAmbienceVolumes;
    private float _lastMusicTime;

    public static bool EnteredSequence = false;

    private void Awake()
    {
        _playerCameraEffectController = FindObjectOfType<PlayerCameraEffectController>();
        _sunAnimator = gameObject.GetComponent<Animator>();
        LuminaTerra.Instance.NewHorizons.GetBodyLoadedEvent().AddListener(OnPlanetLoad);
        GlobalMessenger.AddListener("GamePaused", OnGamePaused);
        GlobalMessenger.AddListener("GameUnpaused", OnGameUnpaused);

        // boundsRenderer.enabled = false;

        foreach (var refObj in refs)
        {
            refObj.SetActive(false);
        }
    }

    private void Start()
    {
        SunOverrideVolume vol = GetComponentInChildren<SunOverrideVolume>();
        vol._sector = GetComponentInParent<Sector>();
        vol._sector.OnSectorOccupantsUpdated += vol.OnSectorOccupantsUpdated;
    }

    private void OnPlanetLoad(string name)
    {
        if (name == "Shimmering Heart")
        {
            _conductor = LuminaTerra.Instance.NewHorizons.GetPlanet(name).GetComponentInChildren<Conductor>();
            _planetAmbienceVolumes = _conductor.GetPlanetAmbience();
            _conductor.AssignEOLController(this);
            gameObject.SetActive(false);
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
            _lastMusicTime = -1;
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

        List<Transform> targetablePoints = [.. transferPointsParent.GetComponentsInChildren<Transform>()];
        foreach (var transferable in FindObjectsOfType<EOLSTransferable>())
        {
            if (!transferable.IsActivated) continue;

            var objTransform = transferable.transform;

            if (transferable.OnRitual && transferable.CurrentRitualSlot != -1)
            {
                Transform target = ritualTransferPoints[transferable.CurrentRitualSlot];
                objTransform.SetParent(target);
                objTransform.localPosition = Vector3.zero;
                objTransform.localRotation = Quaternion.Euler(0, 0, 0);
                continue;
            }

            int index = UnityEngine.Random.Range(0, targetablePoints.Count);
            if (targetablePoints[index] == transferPointsParent)
            {
                targetablePoints.RemoveAt(index);
                index = UnityEngine.Random.Range(0, targetablePoints.Count);
            }
            objTransform.SetParent(targetablePoints[index]);
            objTransform.localPosition = Vector3.zero;
            objTransform.localRotation = Quaternion.Euler(0, 0, 0);
            if (objTransform.TryGetComponent(out OWItem item))
            {
                item.SetSector(GetComponentInParent<Sector>());
            }
            targetablePoints.RemoveAt(index);
        }
        
        stars.Simulate(22f);
        stars.Play();
        
        var player = Locator.GetPlayerBody() as PlayerBody;

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

        EnteredSequence = true;
        Locator.GetShipLogManager().RevealFact("LT_VISION_REALM_ENTER");

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
        Locator.GetShipLogManager().RevealFact("LT_VISION_REALM_DEATH");
        enabled = false;
    }

    public float GetDistSqrFromHeart(Vector3 point)
    {
        return (heartTransform.position - point).sqrMagnitude;
    }

    private void OnGamePaused()
    {
        if (EnteredSequence)
        {
            if (ambientSound.isPlaying)
            {
                _lastMusicTime = ambientSound.time;
                ambientSound.FadeOut(2f);
            }
            if (windAudio.isPlaying)
            {
                windAudio.FadeOut(2f);
            }
        }
    }

    private void OnGameUnpaused()
    {
        if (_lastMusicTime >= 0f)
        {
            ambientSound.time = _lastMusicTime;
            ambientSound.FadeIn(2f);
        }
        float num = Mathf.InverseLerp(ambientSound.clip.length - 20f, ambientSound.clip.length, ambientSound.time);
        if (num > 0)
        {
            windAudio.FadeIn(4f);
        }
    }

    private void OnDestroy()
    {
        LuminaTerra.Instance.NewHorizons.GetBodyLoadedEvent().RemoveListener(OnPlanetLoad);
    }
}