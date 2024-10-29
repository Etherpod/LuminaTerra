using UnityEngine;

namespace LuminaTerra;

public class LightDarkNomaiDoor : MonoBehaviour
{
    [SerializeField] SingleLightSensor _lightSensor = null;
    [SerializeField] NomaiMultiPartDoor _door = null;
    [SerializeField] NomaiInterfaceSlot _slot = null;

    private void Start()
    {
        _lightSensor.OnDetectDarkness += OnDetectDarkness;
    }

    private void OnDetectDarkness()
    {
        if (!_door.IsOpen())
        {
            _door.Open(_slot);
        }
    }

    private void OnDestroy()
    {
        _lightSensor.OnDetectDarkness -= OnDetectDarkness;
    }
}
