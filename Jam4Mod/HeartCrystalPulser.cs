using UnityEngine;

namespace Jam4Mod;

public class HeartCrystalPulser : MonoBehaviour
{
    [SerializeField]
    private float _pulseSpeed = 0f;
    [SerializeField]
    private float _minEmission = 0f;
    [SerializeField]
    private float _maxEmission = 0f;

    OWEmissiveRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<OWEmissiveRenderer>();
    }

    private void Update()
    {
        float nextEmissive = Mathf.Lerp(_minEmission, _maxEmission, (Mathf.Sin(Time.time * _pulseSpeed) + 1) / 2);
        _renderer.SetEmissiveScale(nextEmissive);
    }
}
