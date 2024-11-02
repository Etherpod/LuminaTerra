using UnityEngine;

namespace LuminaTerra;

public class EOLSTransferable : MonoBehaviour
{
    [SerializeField] private bool StartActivated = false;
    
    private bool _isActivated = false;
    private bool _onRitual = false;
    private int _currentRitualSlot = -1;

    public bool IsActivated => _isActivated;
    public bool OnRitual => _onRitual;
    public int CurrentRitualSlot => _currentRitualSlot;

    private void Awake()
    {
        _isActivated = StartActivated;
    }

    public void SetEOLSActivation(bool isActivated)
    {
        _isActivated = isActivated;
    }

    public void SetRitualActivation(bool onRitual, int slot = -1)
    {
        _onRitual = onRitual;
        _currentRitualSlot = slot;
    }
}