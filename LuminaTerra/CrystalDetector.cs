using UnityEngine;

namespace LuminaTerra;

public class CrystalDetector : MonoBehaviour
{
    public delegate void CrystalDetectionEvent(CrystalItem crystal);
    public CrystalDetectionEvent OnCrystalEnter;
    public CrystalDetectionEvent OnCrystalExit;

    private int _numCrystals = 0;

    public void OnEntry(CrystalItem crystal)
    {
        _numCrystals++;
        OnCrystalEnter?.Invoke(crystal);
    }

    public void OnExit(CrystalItem crystal)
    {
        _numCrystals = Mathf.Min(_numCrystals - 1, 0);
        OnCrystalExit?.Invoke(crystal);
    }

    public int GetNumCrystals() => _numCrystals;
}
