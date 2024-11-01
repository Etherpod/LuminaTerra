using System;
using DitzyExtensions.Collection;
using LuminaTerra.Util;
using OWML.Utils;
using UnityEngine;

namespace LuminaTerra;

public class CapturableLight : MonoBehaviour
{
    public delegate void ScaleChangeComplete();

    public event ScaleChangeComplete OnScaleChangeComplete;
        
    [SerializeField, Range(0, 1)] private float initialLightScale = 1f;
        
    private readonly Fader _fader = new Fader();
    
    private OWLight2[] _lights = [];
    private EmissiveMaterialRenderer[] _emissives = [];

    private float _scale = 0f;

    private void Awake()
    {
        _lights = GetComponentsInChildren<OWLight2>();
        _emissives = GetComponentsInChildren<EmissiveMaterialRenderer>();
        _scale = initialLightScale;
    }

    private void Start()
    {
        UpdateScale();

        enabled = false;
    }

    public void SetScale(float newScale, float fadeDurationSeconds = 0f)
    {
        _fader.StartFade(_scale, newScale, fadeDurationSeconds);
        enabled = true;
    }

    private void Update()
    {
        _scale = _fader.Value;
        UpdateScale();
        
        if (!_fader.IsFading)
        {
            enabled = false;
            OnScaleChangeComplete?.Invoke();
        }
    }

    private void UpdateScale()
    {
        _lights.ForEach(light => light.SetIntensityScale(_scale));
        _emissives.ForEach(emissive => emissive.SetEmissionScale(_scale));
    }
}