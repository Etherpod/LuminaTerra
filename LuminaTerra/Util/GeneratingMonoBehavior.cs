using System;
using NewHorizons.Utility;
using UnityEngine;

namespace LuminaTerra.Util;

public class GeneratingMonoBehavior : MonoBehaviour
{
    protected GameObject GenerativeParent = null;
    
    protected bool IsInEditor => Application.isEditor;
    protected string GenerativeParentName => $"{name}_GenerativeParent";

    protected virtual void OnEnable()
    {
        GenerativeParent = transform.Find(GenerativeParentName)?.gameObject;
        if (GenerativeParent)
        {
            DestroyImmediate(GenerativeParent);
        }
        GenerativeParent = new GameObject(GenerativeParentName);
        GenerativeParent.transform.SetParent(transform, false);
    }
    
    protected virtual void OnDisable()
    {
        if (IsInEditor)
        {
            DestroyImmediate(GenerativeParent);
        }
        else
        {
            Destroy(GenerativeParent);
        }
    }

    protected GameObject GenerateObject()
    {
        var go = new GameObject($"{name}_Generated_{GenerativeParent.GetAllChildren().Count}");
        go.transform.SetParent(GenerativeParent.transform, false);
        return go;
    }
}