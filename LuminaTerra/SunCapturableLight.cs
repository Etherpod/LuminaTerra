using UnityEngine;

namespace LuminaTerra;

public class SunCapturableLight : CapturableLight
{
    [SerializeField] private TessellatedSphereRenderer _sunRenderer;

    private readonly int SunColorPropID = Shader.PropertyToID("_ColorTime");

    protected override float FadeDurationMultiplier => 3f;

    protected override void UpdateScale()
    {
        base.UpdateScale();
        _sunRenderer._materials[0].SetFloat(SunColorPropID, 1 - _scale);
    }
}
