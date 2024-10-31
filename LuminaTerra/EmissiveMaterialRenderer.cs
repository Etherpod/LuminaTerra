using UnityEngine;
using static LuminaTerra.Util.Extensions;

namespace LuminaTerra;

public class EmissiveMaterialRenderer : MonoBehaviour
{
    [SerializeField] private int materialIndex = 0;
    [SerializeField] private Material unlitMaterial = null;
    [SerializeField] private Material litMaterial = null;

    private Renderer _renderer = null;

    private float _emissionScale = 0f;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public float GetEmissionScale() => _emissionScale;

    public void SetEmissionScale(float newEmissionScale)
    {
        // LTPrint($"emission scale [{name}]: {newEmissionScale}");
        _emissionScale = Mathf.Clamp01(newEmissionScale);
        _renderer.materials[materialIndex].Lerp(unlitMaterial, litMaterial, _emissionScale);
    }
}