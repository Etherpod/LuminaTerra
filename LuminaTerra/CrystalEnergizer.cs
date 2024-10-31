using System.Collections.Generic;
using UnityEngine;

namespace LuminaTerra;

public class CrystalEnergizer : MonoBehaviour
{
    [SerializeField]
    private CrystalDetector _detector = null;
    [SerializeField]
    private OWAudioSource _energizerAudio = null;
    [SerializeField]
    private OWAudioSource[] _crystalAudioSources = null;

    private Dictionary<CrystalItem, OWAudioSource> _connectedCrystalAudioSources = [];

    private void Start()
    {
        _detector.OnCrystalEnter += OnCrystalEnter;
        _detector.OnCrystalExit += OnCrystalExit;
    }

    private void OnCrystalEnter(CrystalItem crystal)
    {
        if (!crystal.IsCharged())
        {
            crystal.SetCharged(true, 3f);
        }

        if (_detector.GetNumCrystals() == 1)
        {
            _energizerAudio.FadeIn(1f, false, true);
        }

        foreach (OWAudioSource source in _crystalAudioSources)
        {
            if (!_connectedCrystalAudioSources.ContainsValue(source))
            {
                _connectedCrystalAudioSources.Add(crystal, source);
                source.clip = crystal.GetSignal();
                source.FadeIn(2f);
                break;
            }
        }
    }

    private void OnCrystalExit(CrystalItem crystal)
    {
        if (!crystal.IsLit())
        {
            crystal.SetCharged(false, 1f);
        }

        if (_detector.GetNumCrystals() == 0)
        {
            _energizerAudio.FadeOut(1f);
        }

        if (_connectedCrystalAudioSources.ContainsKey(crystal))
        {
            _connectedCrystalAudioSources[crystal].FadeOut(2f);
            _connectedCrystalAudioSources.Remove(crystal);
        }
    }

    private void OnDestroy()
    {
        _detector.OnCrystalEnter -= OnCrystalEnter;
        _detector.OnCrystalExit -= OnCrystalExit;
    }
}
