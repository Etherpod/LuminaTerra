using UnityEngine;

namespace Jam4Mod;

public class HeartSlidesInterface : MonoBehaviour
{
    [SerializeField]
    private OWTriggerVolume _heartRoomTrigger;

    private MindProjectorTrigger _projector;
    private bool _hasActivated = false;

    private void Awake()
    {
        _projector = GetComponentInChildren<MindProjectorTrigger>();
    }

    private void Start()
    {
        _heartRoomTrigger.OnEntry += OnEntry;
    }

    private void OnEntry(GameObject hitObj)
    {
        if (!_hasActivated && hitObj.CompareTag("PlayerDetector"))
        {
            _projector.SetProjectorActive(true);
            _hasActivated = true;
        }
    }

    private void OnDestroy()
    {
        _heartRoomTrigger.OnEntry -= OnEntry;
    }
}
