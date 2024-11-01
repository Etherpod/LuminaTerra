using UnityEngine;

namespace LuminaTerra;

public class ItemDetector : MonoBehaviour
{
    public delegate void ItemDetectionEvent(OWItem item);
    public ItemDetectionEvent OnItemEnter;
    public ItemDetectionEvent OnItemExit;

    private int _numItems = 0;

    public void OnEntry(OWItem item)
    {
        LuminaTerra.Instance.ModHelper.Console.WriteLine("enter " + item);
        _numItems++;
        OnItemEnter?.Invoke(item);
    }

    public void OnExit(OWItem item)
    {
        LuminaTerra.Instance.ModHelper.Console.WriteLine("exit " + item);
        _numItems = Mathf.Max(_numItems - 1, 0);
        OnItemExit?.Invoke(item);
    }

    public int GetNumItems() => _numItems;
}
