using UnityEngine;

namespace Jam4Mod;

public class CrystalItem : OWItem
{
    [SerializeField]
    private string _namePrefix;
    [SerializeField]
    private GameObject _signalParent;
    [SerializeField]
    private bool _startCharged;

    public static ItemType ItemType;

    private bool _charged;

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
            // Set emission
        }
    }

    public void SetCharged(bool newState)
    {
        _charged = newState;

        _signalParent.SetActive(_charged);
        // Set emission
    }

    public override string GetDisplayName()
    {
        return _namePrefix + " Crystal";
    }

    public bool IsCharged() => _charged;
}
