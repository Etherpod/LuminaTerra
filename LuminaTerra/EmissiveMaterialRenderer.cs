using UnityEngine;

namespace LuminaTerra;

public class EmissiveMaterialRenderer : MonoBehaviour
{
    [SerializeField] private int materialIndex = 0;
    [SerializeField] private Material unlitMaterial = null;
    [SerializeField] private Material litMaterial = null;
    [SerializeField, Range(0, 1)] private float startingEmissionScale = 1f;

    private Renderer _renderer = null;

    private float _emissionScale = 0f;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        SetEmissionScale(startingEmissionScale);
    }

    public float GetEmissionScale() => _emissionScale;

    public void SetEmissionScale(float newEmissionScale)
    {
        _emissionScale = Mathf.Clamp01(newEmissionScale);
        _renderer.materials[materialIndex].Lerp(unlitMaterial, litMaterial, _emissionScale);
    }
}