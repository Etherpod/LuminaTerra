using System;
using DitzyExtensions.Collection;
using LuminaTerra.Util;
using UnityEngine;

namespace LuminaTerra;

public class CapturableLight : MonoBehaviour
{
    private readonly Fader _fader = new Fader();
    
    private OWLight2[] _lights = [];
    private EmissiveMaterialRenderer[] _emissives = [];

    private float _scale = 0f;

    private void Awake()
    {
        _lights = GetComponentsInChildren<OWLight2>();
        _emissives = GetComponentsInChildren<EmissiveMaterialRenderer>();

        enabled = false;
    }

    public void SetScale(float newScale, float fadeDurationSeconds = 0f)
    {
        _fader.StartFade(_scale, newScale, fadeDurationSeconds);
        enabled = true;
    }

    private void Update()
    {
        if (!_fader.IsFading())
        {
            enabled = false;
        }

        _scale = _fader.GetValue();
        _lights.ForEach(light => light.SetIntensityScale(_scale));
        _emissives.ForEach(emissive => emissive.SetEmissionScale(_scale));
    }
}