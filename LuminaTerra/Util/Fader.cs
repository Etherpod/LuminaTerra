using UnityEngine;

namespace LuminaTerra.Util;

public class Fader
{
    private float _fadeStartTime = 0f;
    private float _fadeEndTime = 0f;
    private float _fadeStartValue = 0f;
    private float _fadeTargetValue = 0f;

    public void StartFade(float currentValue, float targetValue, float fadeDurationSeconds)
    {
        _fadeStartTime = Time.time;
        _fadeEndTime = _fadeStartTime + fadeDurationSeconds;
        _fadeStartValue = currentValue;
        _fadeTargetValue = targetValue;
    }

    public bool IsFading => Time.time < _fadeEndTime;
    public float StartTime => _fadeStartTime;
    public float EndTime => _fadeEndTime;
    public float StartValue => _fadeStartValue;
    public float TargetValue => _fadeTargetValue;

    public float Value
    {
        get
        {
            var t = Mathf.InverseLerp(_fadeStartTime, _fadeEndTime, Time.time);
            return Mathf.Lerp(_fadeStartValue, _fadeTargetValue, t);
        }
    }
}