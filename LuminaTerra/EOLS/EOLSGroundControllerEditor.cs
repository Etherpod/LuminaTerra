using System;
using System.Collections.Generic;
using DitzyExtensions.Collection;
using NewHorizons.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using static LuminaTerra.Util.Extensions;

namespace LuminaTerra.EOLS;

[ExecuteInEditMode]
public class EOLSGroundControllerEditor : MonoBehaviour
{
    private static readonly int ShaderPropEolsMInverse = Shader.PropertyToID("_EOLS_M_Inverse");
    private static readonly int ShaderPropEolsM = Shader.PropertyToID("_EOLS_M");
    private static readonly int ShaderPropLampM = Shader.PropertyToID("_LAMP_M");
    private static readonly int ShaderPropLayerSeparation = Shader.PropertyToID("_LayerSeparation");
    private static readonly int ShaderPropLayerTimeOffset = Shader.PropertyToID("_LayerTimeOffset");
    
    [SerializeField] private bool isLeadController = false;
    [SerializeField] private bool isBridge = false;
    [SerializeField] private Renderer eolsCenter = null;
    [SerializeField] private bool lampRequired = false;
    [SerializeField] private LAMP lamp = null;
    [SerializeField] private int numLayers = 1;
    [Header("Shader Properties")]
    [SerializeField] private float layerSeparation = -1;
    [SerializeField] private float layerTimeOffset = 0.2f;

    private MeshRenderer _renderer = null;
    private Renderer _lampRenderer = null;
    private GameObject _layerParent = null;
    private IList<Material> _layers = [];

    private bool IsInEditor => Application.isEditor;
    public bool IsLampRequired => lampRequired;

    private void OnEnable()
    {
        _renderer = GetComponent<MeshRenderer>();
        _renderer.enabled = false;

        _layerParent = gameObject.FindChild($"{name}_layers");
        if (_layerParent)
        {
            DestroyImmediate(_layerParent);
        }
        _layerParent = new GameObject($"{name}_layers");
        _layerParent.transform.SetParent(transform, false);

        if (isLeadController)
        {
            Shader.SetGlobalFloat(ShaderPropLayerSeparation, layerSeparation);
            Shader.SetGlobalFloat(ShaderPropLayerTimeOffset, layerTimeOffset);
        }

        var meshFilter = GetComponent<MeshFilter>();
        for (var i = 0; i < numLayers; i++)
        {
            var layerObj = new GameObject($"{name}_layer_{i}");
            layerObj.transform.SetParent(_layerParent.transform, false);

            layerObj.GetAddComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;
            
            var layerRenderer = layerObj.GetAddComponent<MeshRenderer>();
            var layerMat = new Material(_renderer.sharedMaterial);
            layerRenderer.material = layerMat;
            
            if (!IsInEditor)
            {
                layerMat.SetInt("_LampRequired", lampRequired ? 1 : 0);
            }
            layerMat.SetInt("_Shape", isBridge ? 1 : 0);
            layerMat.SetInt("_Layer", i);

            _layers.Add(layerMat);
        }

        if (lamp)
        {
            _lampRenderer = lamp.GetComponent<Renderer>();
        }
    }

    private void OnDisable()
    {
        if (IsInEditor)
        {
            DestroyImmediate(_layerParent);
            _layers.ForEach(layerMat => DestroyImmediate(layerMat));
        }
        else
        {
            Destroy(_layerParent);
            _layers.ForEach(layerMat => Destroy(layerMat));
        }
    }

    public void SetLamp(LAMP lamp)
    {
        this.lamp = lamp;
        _lampRenderer = lamp.GetComponent<Renderer>();
    }

    private void Update()
    {
        if (IsInEditor && isLeadController)
        {
            Shader.SetGlobalFloat(ShaderPropLayerSeparation, layerSeparation);
            Shader.SetGlobalFloat(ShaderPropLayerTimeOffset, layerTimeOffset);
        }

        if (isLeadController)
        {
            Shader.SetGlobalFloat(ShaderPropLayerSeparation, layerSeparation);
            Shader.SetGlobalFloat(ShaderPropLayerTimeOffset, layerTimeOffset);
            Shader.SetGlobalMatrix(ShaderPropEolsMInverse, eolsCenter.worldToLocalMatrix);
            Shader.SetGlobalMatrix(ShaderPropEolsM, eolsCenter.localToWorldMatrix);
            if (lamp && (IsInEditor || lamp.IsLit))
            {
                Shader.SetGlobalMatrix(ShaderPropLampM, _lampRenderer.localToWorldMatrix);
            }
            else
            {
                Shader.SetGlobalMatrix(ShaderPropLampM, eolsCenter.localToWorldMatrix);
            }
        }
    }
}