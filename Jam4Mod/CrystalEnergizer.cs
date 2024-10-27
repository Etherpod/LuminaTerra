using UnityEngine;

namespace Jam4Mod;

public class CrystalEnergizer : MonoBehaviour
{
    [SerializeField]
    private CrystalDetector _detector = null;

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
    }

    private void OnCrystalExit(CrystalItem crystal)
    {
        if (!crystal.IsLit())
        {
            crystal.SetCharged(false, 1f);
        }
    }

    private void OnDestroy()
    {
        _detector.OnCrystalEnter -= OnCrystalEnter;
        _detector.OnCrystalExit -= OnCrystalExit;
    }
}
