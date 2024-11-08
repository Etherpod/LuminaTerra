﻿using NewHorizons.External;
using System.Collections.Generic;
using UnityEngine;

namespace LuminaTerra;

public class CrystalItem : OWItem
{
    [SerializeField]
    private string _namePrefix = null;
    [SerializeField]
    private GameObject _signalParent = null;
    [SerializeField]
    private SphereShape _lightSourceShape = null;
    [SerializeField]
    private OWEmissiveRenderer[] _emissiveRenderers = null;
    [SerializeField]
    private Light _light = null;
    [SerializeField]
    private bool _startCharged = false;

    public static ItemType ItemType;

    private bool _charged;
    private float _fadeLength = 1f;
    private float _fadeT;
    private bool _fading = false;
    private float _baseLightIntensity;
    private List<CrystalDetector> _currentDetectors = [];
    private EOLSTransferable _eolsTransferable = null;
    private float _originalLightSourceShapeRadius;
    private bool _knowsHeartSignal = false;
    private EndOfLoopController _loopController;

    public override void Awake()
    {
        ItemType = LuminaTerra.Instance.CrystalItemType;
        _baseLightIntensity = _light.intensity;
        _eolsTransferable = gameObject.GetComponent<EOLSTransferable>();
        _originalLightSourceShapeRadius = _lightSourceShape.radius;
        _knowsHeartSignal = NewHorizonsData.KnowsSignal("Heart of the Planet");
        LuminaTerra.Instance.OnLearnHeartSignal += OnLearnHeartSignal;
        base.Awake();
    }

    private void Start()
    {
        if (!_startCharged)
        {
            _signalParent.SetActive(false);
            SetEmissiveScale(0f);
            _light.intensity = 0f;
        }
        else
        {
            _charged = true;
            _fadeT = 1f;
            _signalParent.SetActive(_knowsHeartSignal);
            SetEmissiveScale(1f);
            _light.intensity = _baseLightIntensity;
            _eolsTransferable?.SetEOLSActivation(true);
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
                _signalParent.SetActive(_knowsHeartSignal && _charged);
                _eolsTransferable?.SetEOLSActivation(_charged);
                Locator.GetShipLogManager().RevealFact("LT_CRYSTAL_ENERGIZER_USED");
                _fading = false;
            }
        }
        else if (IsLit() && EndOfLoopController.EnteredSequence)
        {
            if (_loopController == null)
            {
                _loopController = FindObjectOfType<Conductor>().GetEOLController();
            }

            float heartDistSqr = _loopController.GetDistSqrFromHeart(transform.position);
            float ratio = Mathf.InverseLerp(30f * 30f, 5f * 5f, heartDistSqr);
            SetEmissiveScale(ratio);
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

        if (_charged)
        {
            _signalParent.SetActive(_knowsHeartSignal);
        }

        _lightSourceShape.radius = _originalLightSourceShapeRadius;
    }

    public override void PickUpItem(Transform holdTranform)
    {
        base.PickUpItem(holdTranform);
        foreach (var detector in _currentDetectors)
        {
            detector.OnExit(this);
        }
        _currentDetectors.Clear();
        if (_charged)
        {
            _signalParent.SetActive(false);
        }

        _lightSourceShape.radius = _originalLightSourceShapeRadius / holdTranform.localScale.x;
    }

    public void SetCharged(bool newState, float fadeLength = 1f)
    {
        if (_charged && !newState) _eolsTransferable.SetEOLSActivation(false);
        _charged = newState;
        _fadeLength = fadeLength;
        _fading = true;
    }

    public bool IsLit()
    {
        return _charged && _fadeT >= 1f;
    }

    public AudioClip GetSignal()
    {
        return _signalParent.GetComponentInChildren<OWAudioSource>().clip;
    }

    public string GetPrefix()
    {
        return _namePrefix;
    }

    private void OnLearnHeartSignal()
    {
        if (!_knowsHeartSignal)
        {
            _knowsHeartSignal = true;
            _signalParent.SetActive(_charged);
        }
    }

    private void SetEmissiveScale(float scale)
    {
        foreach (var rend in _emissiveRenderers)
        {
            rend.SetEmissiveScale(scale);
        }
        _light.intensity = Mathf.InverseLerp(0f, _baseLightIntensity, scale);
    }

    public override string GetDisplayName()
    {
        return _namePrefix + " Crystal";
    }

    public bool IsCharged() => _charged;

    public override void OnDestroy()
    {
        base.OnDestroy();
        LuminaTerra.Instance.OnLearnHeartSignal -= OnLearnHeartSignal;
    }
}
