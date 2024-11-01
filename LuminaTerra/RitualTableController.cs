using System.Collections.Generic;
using UnityEngine;

namespace LuminaTerra;

public class RitualTableController : MonoBehaviour
{
    [SerializeField] CrystalDetector _slot1Detector = null;
    [SerializeField] CrystalDetector _slot2Detector = null;
    [SerializeField] CrystalDetector _slot3Detector = null;
    [SerializeField] ItemDetector _itemDetector = null;
    [SerializeField] string _solution = "Red Blue Purple";

    private readonly CrystalItem[] _crystals = new CrystalItem[3];
    private List<OWItem> _activeItems = [];

    private void Awake()
    {
        _slot1Detector.OnCrystalEnter += (crystal) => OnCrystalEnter(crystal, 0);
        _slot1Detector.OnCrystalExit += (crystal) => OnCrystalExit(crystal, 0);
        _slot2Detector.OnCrystalEnter += (crystal) => OnCrystalEnter(crystal, 1);
        _slot2Detector.OnCrystalExit += (crystal) => OnCrystalExit(crystal, 1);
        _slot3Detector.OnCrystalEnter += (crystal) => OnCrystalEnter(crystal, 2);
        _slot3Detector.OnCrystalExit += (crystal) => OnCrystalExit(crystal, 2);
        _itemDetector.OnItemEnter += OnItemEnter;
        _itemDetector.OnItemExit += OnItemExit;
    }

    private void OnCrystalEnter(CrystalItem crystal, int slot)
    {
        if (_crystals[slot] == null)
        {
            _crystals[slot] = crystal;
        }
    }

    private void OnCrystalExit(CrystalItem item, int slot)
    {
        if (_crystals[slot] != null)
        {
            _crystals[slot] = null;
        }
    }

    private void OnItemEnter(OWItem item)
    {
        LuminaTerra.Instance.ModHelper.Console.WriteLine("correct: " + IsCorrectSolution());
        if (!_activeItems.Contains(item))
        {
            _activeItems.Add(item);
            if (item.TryGetComponent(out EOLSTransferable transfer))
            {
                transfer.SetEOLSActivation(true);
            }
        }
    }

    private void OnItemExit(OWItem item)
    {
        if (_activeItems.Contains(item))
        {
            _activeItems.Remove(item);
            if (item.TryGetComponent(out EOLSTransferable transfer))
            {
                transfer.SetEOLSActivation(true);
            }
        }
    }

    public bool IsCorrectSolution()
    {
        string[] splits = _solution.Split(' ');
        for (int i = 0; i < _crystals.Length; i++)
        {
            if (_crystals[i] == null || !_crystals[i].IsLit() || _crystals[i].GetPrefix() != splits[i])
            {
                return false;
            }
        }
        return true;
    }

    private void OnDestroy()
    {
        _slot1Detector.OnCrystalEnter -= (crystal) => OnCrystalEnter(crystal, 0);
        _slot1Detector.OnCrystalExit -= (crystal) => OnCrystalExit(crystal, 0);
        _slot2Detector.OnCrystalEnter -= (crystal) => OnCrystalEnter(crystal, 1);
        _slot2Detector.OnCrystalExit -= (crystal) => OnCrystalExit(crystal, 1);
        _slot3Detector.OnCrystalEnter -= (crystal) => OnCrystalEnter(crystal, 2);
        _slot3Detector.OnCrystalExit -= (crystal) => OnCrystalExit(crystal, 2);
        _itemDetector.OnItemEnter -= OnItemEnter;
        _itemDetector.OnItemExit -= OnItemExit;
    }
}
