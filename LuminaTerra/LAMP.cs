using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using DitzyExtensions.Collection;
using LuminaTerra.Util;
using OWML.Utils;
using UnityEngine;
using static LuminaTerra.Util.Extensions;

namespace LuminaTerra;

public class LAMP : OWItem
{
    private static readonly int AnimStateOpened = Animator.StringToHash("opened");
    private static readonly int AnimStateClosed = Animator.StringToHash("closed");
    private static readonly int AnimBoolOpen = Animator.StringToHash("open");

    public static ItemType LampType = EnumUtils.Create<ItemType>("LTLamp");

    [SerializeField] private SphereShape _lightVolumeShape = null;
    [SerializeField] private OWAudioSource doorAudioSource = null;
    [SerializeField] private OWAudioSource siphonAudioSource = null;
    [SerializeField] private AudioClip doorOpenClip = null;
    [SerializeField] private AudioClip doorCloseClip = null;
    [SerializeField] private AudioClip siphonCaptureClip = null;
    [SerializeField] private AudioClip siphonReleaseClip = null;
    [SerializeField] private GameObject signalParent = null;
    [SerializeField] private float lightsFadeDurationSeconds = 1;
    [SerializeField] private float lampFadeDurationSeconds = 2;
    [SerializeField] private float lampCaptureTimeLimitSeconds = 8;
    [SerializeField] private ParticleSystem captureParticleSystem = null;
    [SerializeField] private ParticleSystem releaseParticleSystem = null;

    private Animator _animator;
    private OWTriggerVolume _triggerVolume;
    private CapturableLight _lightController;

    private readonly IDictionary<int, CapturableLight> _capturedLights = new Dictionary<int, CapturableLight>(8);
    private readonly List<ItemDetector> _currentDetectors = [];
    private readonly Fader _succTimer = new Fader();
    
    private float _originalLightVolumeShapeScale;
    private bool _isOpen = false;
    private bool _isReleasing = false;
    private bool _firstPlayerInput = true;

    public override string GetDisplayName() => "Lantern";

    public bool IsDoorClosed => _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == AnimStateClosed;
    public bool IsDoorOpened => _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == AnimStateOpened;
    public bool IsDoorInMotion => IsDoorClosed || IsDoorOpened;

    public override void Awake()
    {
        _animator = GetComponent<Animator>();
        _triggerVolume = GetComponentInChildren<OWTriggerVolume>();
        _lightController = GetComponent<CapturableLight>();
        _type = LampType;
        _originalLightVolumeShapeScale = _lightVolumeShape.radius;

        _lightController.OnScaleChangeComplete += LAMPScaleChangeComplete;
        _triggerVolume.OnExit += ObjectExitedLampRange;

        base.Awake();
    }

    private void Start()
    {
        signalParent.SetActive(false);
        enabled = false;
        LuminaTerra.Instance.ModHelper.Events.Unity.RunWhen(() => captureParticleSystem.isEmitting, captureParticleSystem.Stop);
    }

    public override void OnDestroy()
    {
        _lightController.OnScaleChangeComplete -= LAMPScaleChangeComplete;
        _triggerVolume.OnExit -= ObjectExitedLampRange;

        base.OnDestroy();
    }

    private void LAMPScaleChangeComplete(CapturableLight affectedLight, float finalScale)
    {
        if (finalScale == 0 && IsDoorOpened) CloseLamp();
    }

    private void ObjectExitedLampRange(GameObject trackedObj)
    {
        if (!_isOpen) return;
        if (!_capturedLights.TryGetValue(trackedObj.GetInstanceID(), out var capturedLight)) return;
        if (capturedLight.GetScale() == 0) return;
        
        LTPrint($"[{trackedObj.name}] out of range");
        capturedLight.SetScale(1, lightsFadeDurationSeconds);
        _capturedLights.Remove(trackedObj.GetInstanceID());

        UpdateLampLight();
    }

    public void PlayDoorOpenSFX() => doorAudioSource.PlayOneShot(doorOpenClip);

    public void PlayDoorCloseSFX() => doorAudioSource.PlayOneShot(doorCloseClip);

    public void PlaySiphonCaptureSFX()
    {
        siphonAudioSource.Stop();
        siphonAudioSource.clip = siphonCaptureClip;
        siphonAudioSource.FadeIn(0.5f);
    }

    public void PlaySiphonReleaseSFX()
    {
        siphonAudioSource.Stop();
        siphonAudioSource.clip = siphonReleaseClip;
        siphonAudioSource.FadeIn(0.5f);
    }

    public void OnDoorClosed()
    {
        _isOpen = false;
        _isReleasing = false;
        PlayDoorCloseSFX();
    }

    public void OnDoorOpened()
    {
        _isOpen = true;
        PlayDoorOpenSFX();
    }

    private void FixedUpdate()
    {
        if (!Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item)) return;
        if (!OWInput.IsPressed(InputLibrary.toolActionSecondary, InputMode.Character))
        {
            _firstPlayerInput = true;
            if (!_isReleasing && _isOpen && IsDoorOpened) CloseLamp();
            return;
        }

        if (_firstPlayerInput && !_isOpen && IsDoorClosed)
        {
            _firstPlayerInput = false;
            OpenLamp();
        }
        if (_isOpen && !_isReleasing)
        {
            if (!_succTimer.IsFading)
            {
                CloseLamp();
            }
            else
            {
                CaptureLights();
            }
        }
    }

    private void CloseDoor() => _animator.SetBool(AnimBoolOpen, false);
    
    private void OpenDoor() => _animator.SetBool(AnimBoolOpen, true);

    private void CloseLamp()
    {
        LTPrint("close lamp");
        //_isOpen = false;
        CloseDoor();
        
        if (siphonAudioSource.isPlaying && !siphonAudioSource.IsFadingOut())
        {
            siphonAudioSource.FadeOut(1);
        }

        captureParticleSystem.Stop();

        _capturedLights
            .Values
            .AsList()
            .Where(light => light.IsBeingCaptured && light.GetScale() != 0)
            .ForEach(light =>
            {
                light.SetScale(1, lightsFadeDurationSeconds);
                _capturedLights.Remove(light.gameObject.GetInstanceID());
            });

        UpdateLampLight();
    }

    private void OpenLamp()
    {
        LTPrint("open lamp");
        //_isOpen = true;
        OpenDoor();

        if (_capturedLights.IsEmpty())
        {
            if (GetDetectedLights().Length > 0)
            {
                PlaySiphonCaptureSFX();
                Locator.GetFlashlight().TurnOff();
                _succTimer.StartFade(0, 0, lampCaptureTimeLimitSeconds);

                //releaseParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                //captureParticleSystem.time = 0;
                captureParticleSystem.Play();
            }
        }
        else
        {
            ReleaseLights();
        }
    }

    private void CaptureLights()
    {
        // LTPrint("capture lights");
        /*var detectedLights = _triggerVolume
            .getTrackedObjects()
            .Where(obj => 
                !_capturedLights.ContainsKey(obj.GetInstanceID())
                || !_capturedLights[obj.GetInstanceID()].IsBeingCaptured
            )
            .Select(obj => obj.GetComponent<CapturableLight>())
            .Where(light => light)
            .AsList();*/

        var detectedLights = GetDetectedLights();

        detectedLights
            .ForEach(light => light.SetScale(0, lightsFadeDurationSeconds))
            .ForEach(light => _capturedLights[light.gameObject.GetInstanceID()] = light);
        
        if (_lightController.GetScale() == 0 && detectedLights.IsNotEmpty())
            _lightController.SetScale(1, lampFadeDurationSeconds);
    }

    private void ReleaseLights()
    {
        LTPrint("release lights");
        _isReleasing = true;

        captureParticleSystem.Stop();
        //releaseParticleSystem.time = 0;
        PlaySiphonReleaseSFX();
        releaseParticleSystem.Play();

        _capturedLights.ForEach(light => light.Value.SetScale(1, lightsFadeDurationSeconds));
        _capturedLights.Clear();
        _lightController.SetScale(0, lampFadeDurationSeconds);
    }

    public void ForceReleaseLights()
    {
        if (_capturedLights.IsEmpty())
        {
            return;
        }

        captureParticleSystem.Stop();
        _capturedLights.ForEach(light => light.Value.SetScale(1, lightsFadeDurationSeconds));
        _capturedLights.Clear();
        _lightController.SetScale(0, lampFadeDurationSeconds);
    }

    private CapturableLight[] GetDetectedLights()
    {
        return _triggerVolume
            .getTrackedObjects()
            .Where(obj =>
                !_capturedLights.ContainsKey(obj.GetInstanceID())
                || !_capturedLights[obj.GetInstanceID()].IsBeingCaptured
            )
            .Select(obj => obj.GetComponent<CapturableLight>())
            .Where(light => light)
            .AsList().ToArray();
    }

    public void UpdateSignalState(bool enabled)
    {
        signalParent.SetActive(enabled);
    }

    private void UpdateLampLight()
    {
        if (_capturedLights.IsEmpty())
        {
            _lightController.SetScale(0, lampFadeDurationSeconds);
        }
    }

    public override void DropItem(
        Vector3 position,
        Vector3 normal,
        Transform parent,
        Sector sector,
        IItemDropTarget customDropTarget)
    {
        enabled = false;
        
        base.DropItem(position, normal, parent, sector, customDropTarget);
        
        Collider[] results = Physics.OverlapSphere(transform.position + transform.up * 0.15f, 0.2f);
        if (results.Length > 0)
        {
            foreach (var col in results)
            {
                // LTPrint(col.gameObject.name);
                if (col.gameObject.TryGetComponent(out ItemDetector detector))
                {
                    _currentDetectors.Add(detector);
                    detector.OnEntry(this);
                }
            }
        }

        _lightVolumeShape.radius = _originalLightVolumeShapeScale;
    }

    public override void PickUpItem(Transform holdTranform)
    {
        _firstPlayerInput = true;
        enabled = true;
        
        base.PickUpItem(holdTranform);

        foreach (ItemDetector detector in _currentDetectors)
        {
            detector.OnExit(this);
        }

        _currentDetectors.Clear();

        _lightVolumeShape.radius = _originalLightVolumeShapeScale / holdTranform.localScale.x;
    }
}