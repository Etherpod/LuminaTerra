using UnityEngine;

namespace LuminaTerra.Util;

public class V3Fader
{
    private float _fadeStartTime = 0f;
    private float _fadeEndTime = 0f;
    private Vector3 _fadeStartValue = Vector3.zero;
    private Vector3 _fadeTargetValue = Vector3.zero;

    public void StartFade(Vector3 currentValue, Vector3 targetValue, float fadeDurationSeconds)
    {
        _fadeStartTime = Time.time;
        _fadeEndTime = _fadeStartTime + fadeDurationSeconds;
        _fadeStartValue = currentValue;
        _fadeTargetValue = targetValue;
    }

    public bool IsFading => Time.time < _fadeEndTime;

    public Vector3 Value
    {
        get
        {
            var t = Mathf.InverseLerp(_fadeStartTime, _fadeEndTime, Time.time);
            return Vector3.Lerp(_fadeStartValue, _fadeTargetValue, t);
        }
    }

    public Vector3 ValueS
    {
        get
        {
            var t = Mathf.InverseLerp(_fadeStartTime, _fadeEndTime, Time.time);
            return Vector3.Slerp(_fadeStartValue, _fadeTargetValue, t);
        }
    }
}