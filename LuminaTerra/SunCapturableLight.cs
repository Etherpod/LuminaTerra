using UnityEngine;

namespace LuminaTerra;

public class SunCapturableLight : CapturableLight
{
    [SerializeField] private TessellatedSphereRenderer _sunRenderer = null;

    private readonly int SunColorPropID = Shader.PropertyToID("_ColorTime");
    private float _lastProgression = 0f;

    protected override float FadeDurationMultiplier => 3f;
    public override bool IsBeingCaptured => base.IsBeingCaptured && _fader.StartTime != 0f;

    public void SetLastSunState(float progression)
    {
        _lastProgression = progression;
    }

    protected override void UpdateScale()
    {
        base.UpdateScale();
        _sunRenderer._materials[0].SetFloat(SunColorPropID, Mathf.Lerp(1f, _lastProgression, _scale));
    }
}
