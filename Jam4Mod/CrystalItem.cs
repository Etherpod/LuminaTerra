using UnityEngine;

namespace Jam4Mod;

public class CrystalItem : OWItem
{
    [SerializeField]
    private string _namePrefix;
    [SerializeField]
    private GameObject _signalParent;
    [SerializeField]
    private OWEmissiveRenderer[] _emissiveRenderers;
    [SerializeField]
    private bool _startCharged;

    public static ItemType ItemType;

    private bool _charged;
    private float _fadeLength = 1f;
    private float _fadeT;
    private bool _fading = false;

    public override void Awake()
    {
        ItemType = Jam4Mod.Instance.CrystalItemType;
        base.Awake();
    }

    private void Start()
    {
        if (!_startCharged)
        {
            _signalParent.SetActive(false);
            SetEmissiveScale(0f);
        }
        else
        {
            _signalParent.SetActive(true);
            SetEmissiveScale(1f);
        }
    }

    private void Update()
    {
        if (_fading)
        {
            SetEmissiveScale(Mathf.InverseLerp(0, _fadeLength, _fadeT));

            if (_charged && _fadeT < _fadeLength)
            {
                _fadeT += Time.deltaTime;
            }
            else if (!_charged && _fadeT > 0)
            {
                _fadeT -= Time.deltaTime;
            }
            else
            {
                _fading = false;
            }
        }
    }

    public void SetCharged(bool newState)
    {
        _charged = newState;

        _signalParent.SetActive(_charged);
        _fading = true;
    }

    private void SetEmissiveScale(float scale)
    {
        foreach (var rend in _emissiveRenderers)
        {
            rend.SetEmissiveScale(scale);
        }
    }

    public override string GetDisplayName()
    {
        return _namePrefix + " Crystal";
    }

    public bool IsCharged() => _charged;
}
