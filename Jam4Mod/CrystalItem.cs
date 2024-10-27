using System.Collections.Generic;
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
    private List<CrystalDetector> _currentDetectors = [];

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
            SetEmissiveScale(Mathf.InverseLerp(0f, 1f, _fadeT));

            if (_charged && _fadeT < 1f)
            {
                _fadeT += Time.deltaTime / _fadeLength;
            }
            else if (!_charged && _fadeT > 0f)
            {
                _fadeT -= Time.deltaTime / _fadeLength;
            }
            else
            {
                _signalParent.SetActive(_charged);
                _fading = false;
            }
        }
    }

    public override void DropItem(Vector3 position, Vector3 normal, Transform parent, Sector sector, IItemDropTarget customDropTarget)
    {
        base.DropItem(position, normal, parent, sector, customDropTarget);
        Collider[] results = Physics.OverlapCapsule(transform.position, transform.position + transform.up, 0.25f);
        if (results.Length > 0)
        {
            foreach (var col in results)
            {
                if (col.gameObject.TryGetComponent(out CrystalDetector detector))
                {
                    _currentDetectors.Add(detector);
                    detector.OnEntry(this);
                }
            }
        }
    }

    public override void PickUpItem(Transform holdTranform)
    {
        base.PickUpItem(holdTranform);
        foreach (var detector in _currentDetectors)
        {
            detector.OnExit(this);
        }
    }

    public void SetCharged(bool newState, float fadeLength = 1f)
    {
        _charged = newState;
        _fadeLength = fadeLength;
        _fading = true;
    }

    public bool IsLit()
    {
        return _charged && _fadeT >= 1f;
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
