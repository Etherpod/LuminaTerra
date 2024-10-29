using UnityEngine;

namespace LuminaTerra;

public class EOLSTransferable : MonoBehaviour
{
    [SerializeField] private bool StartActivated = false;
    
    private bool _isActivated = false;

    public bool IsActivated => _isActivated;

    private void Awake()
    {
        _isActivated = StartActivated;
    }

    public void SetEOLSActivation(bool isActivated)
    {
        _isActivated = isActivated;
    }
}