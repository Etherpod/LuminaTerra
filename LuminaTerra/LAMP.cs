using System.Collections.Generic;
using System.Linq;
using DitzyExtensions.Collection;
using UnityEngine;
using static LuminaTerra.Util.Extensions;

namespace LuminaTerra;

public class LAMP : OWItem
{
    private static readonly int AnimStateOpened = Animator.StringToHash("opened");
    private static readonly int AnimStateClosed = Animator.StringToHash("closed");
    private static readonly int AnimBoolOpen = Animator.StringToHash("open");
    private static readonly float LightsFadeDurationSeconds = 1;
    private static readonly float LAMPFadeDurationSeconds = 2;

    public static ItemType LAMPType = (ItemType)(10 << 1);

    [SerializeField] private OWAudioSource audioSource = null;
    [SerializeField] private AudioClip openClip = null;
    [SerializeField] private AudioClip closeClip = null;

    private Animator _animator;
    private OWTriggerVolume _triggerVolume;
    private CapturableLight _lightController;
    private List<ItemDetector> _currentDetectors = [];

    private IList<CapturableLight> _capturedLights = new List<CapturableLight>(8);

    public override string GetDisplayName() => "Lantern";

    // public bool IsOpen => _animator.GetCurrentAnimatorStateInfo(0).shortNameHash == AnimStateOpened;

    public override void Awake()
    {
        _animator = GetComponent<Animator>();
        _triggerVolume = GetComponentInChildren<OWTriggerVolume>();
        _lightController = GetComponent<CapturableLight>();
        _type = LAMPType;

        _lightController.OnScaleChangeComplete += LAMPScaleChangeComplete;

        base.Awake();
    }

    public override void OnDestroy()
    {
        _lightController.OnScaleChangeComplete -= LAMPScaleChangeComplete;
        
        base.OnDestroy();
    }

    private void LAMPScaleChangeComplete()
    {
        if (_animator.GetBool(AnimBoolOpen)) CloseLamp();
    }

    private void FixedUpdate()
    {
        if (!Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item)) return;
        if (!OWInput.IsNewlyPressed(InputLibrary.toolActionSecondary, InputMode.Character)) return;

        OpenLamp();
    }

    private void CloseLamp()
    {
        // LTPrint("close");
        _animator.SetBool(AnimBoolOpen, false);
        audioSource.PlayOneShot(closeClip);
    }

    private void OpenLamp()
    {
        _animator.SetBool(AnimBoolOpen, true);
        audioSource.PlayOneShot(openClip);

        if (_capturedLights.IsEmpty())
        {
            CaptureLights();
        }
        else
        {
            ReleaseLights();
        }
    }

    private void CaptureLights()
    {
        // LTPrint("capture");
        var detectedLights = _triggerVolume
            .getTrackedObjects()
            .Select(obj => obj.GetComponent<CapturableLight>())
            .Where(light => light)
            .AsList();

        if (detectedLights.IsNotEmpty())
        {
            detectedLights
                .ForEach(light => light.SetScale(0, LightsFadeDurationSeconds))
                .ForEach(light => _capturedLights.Add(light));
            _lightController.SetScale(1, LAMPFadeDurationSeconds);
        }
    }

    private void ReleaseLights()
    {
        // LTPrint("release");
        _capturedLights.ForEach(light => light.SetScale(1, LightsFadeDurationSeconds));
        _capturedLights.Clear();
        _lightController.SetScale(0, LAMPFadeDurationSeconds);
    }

    public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
    {
        base.DropItem(position, normal, parent, sector, customDropTarget);
        Collider[] results = Physics.OverlapSphere(transform.position + transform.up * 0.15f, 0.2f);
        if (results.Length > 0)
        {
            foreach (var col in results)
            {
                LTPrint(col.gameObject.name);
                if (col.gameObject.TryGetComponent(out ItemDetector detector))
                {
                    _currentDetectors.Add(detector);
                    detector.OnEntry(this);
                }
            }
        }
    }

    public override void PickUpItem(Transform holdTranform)
    {
        base.PickUpItem(holdTranform);
        
        foreach (ItemDetector detector in _currentDetectors)
        {
            detector.OnExit(this);
        }
        _currentDetectors.Clear();
    }
}