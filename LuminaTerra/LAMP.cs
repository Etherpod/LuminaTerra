using System;
using System.Linq;
using DitzyExtensions.Collection;
using UnityEngine;

namespace LuminaTerra;

public class LAMP : OWItem
{
    private static readonly int AnimStateClosed = Animator.StringToHash("closed");
    private static readonly int AnimTriggerOpen = Animator.StringToHash("open");
    private static readonly float FadeDuration = 2;

    public static ItemType LAMPType = (ItemType)(10 << 1);

    private OWTriggerVolume _triggerVolume;
    private Animator _animator;
    private CapturableLight _lightController;

    public override string GetDisplayName() => "Lantern";

    public override void Awake()
    {
        _triggerVolume = GetComponent<OWTriggerVolume>();
        _animator = GetComponent<Animator>();
        _lightController = gameObject.GetAddComponent<CapturableLight>();
        _type = LAMPType;
        
        base.Awake();
    }

    private void Update()
    {
        if (!Locator.GetToolModeSwapper().IsInToolMode(ToolMode.Item)) return;
        if (!OWInput.IsPressed(InputLibrary.toolActionPrimary, InputMode.Character)) return;

        print("LAMP");
        var animState = _animator.GetCurrentAnimatorStateInfo(0);
        if (animState.shortNameHash != AnimStateClosed) return;
        
        _animator.SetTrigger(AnimTriggerOpen);

        var consumedALight = _triggerVolume
            .getTrackedObjects()
            .Select(obj =>
            {
                var capturableLight = obj.GetComponent<CapturableLight>();
                if (!capturableLight) return false;

                capturableLight.SetScale(0, FadeDuration);
                return true;
            })
            .AsList()
            .Any(foundLight => foundLight);
        if (consumedALight)
        {
            _lightController.SetScale(1, FadeDuration);
        }
    }
}