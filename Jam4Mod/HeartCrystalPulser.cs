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
    [SerializeField]
    private OWLightController _lightController = null;
    [SerializeField]
    private float _minLight = 0f;
    [SerializeField]
    private float _maxLight = 0f;

    OWEmissiveRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<OWEmissiveRenderer>();
    }

    private void Update()
    {
        float sin = (Mathf.Sin(Time.timeSinceLevelLoad * _pulseSpeed) + 1) / 2;
        float nextEmissive = Mathf.Lerp(_minEmission, _maxEmission, sin);
        float nextLight = Mathf.Lerp(_minLight, _maxLight, sin);
        _renderer.SetEmissiveScale(nextEmissive);
        _lightController.SetIntensity(nextLight);
    }
}
