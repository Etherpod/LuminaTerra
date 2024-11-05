using System;
using UnityEngine;
using static LuminaTerra.Util.Extensions;

namespace LuminaTerra.EOLS;

[ExecuteInEditMode]
public class EOLSGroundControllerEditor : MonoBehaviour
{
    private static readonly int ShaderPropEolsMInverse = Shader.PropertyToID("_EOLS_M_Inverse");
    private static readonly int ShaderPropLampM = Shader.PropertyToID("_LAMP_M");
    private static readonly int ShaderPropLampPos = Shader.PropertyToID("_LampPos");
    private static readonly int ShaderPropEolsCenter = Shader.PropertyToID("_EOLSCenter");
    
    [SerializeField] private bool isBridge = false;
    [SerializeField] private Renderer eolsCenter = null;
    [SerializeField] private bool lampRequired = false;
    [SerializeField] private Transform lamp = null;

    private Renderer _renderer = null;
    private Renderer _lampRenderer = null;

    public bool IsLampRequired => lampRequired;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        if (lamp)
        {
            _lampRenderer = lamp.GetComponent<Renderer>();
        }
        
        _renderer.sharedMaterial.SetInt("_LampRequired", lampRequired ? 1 : 0);
        _renderer.sharedMaterial.SetInt("_Shape", isBridge ? 1 : 0);
    }

    public void SetLamp(Transform lamp)
    {
        this.lamp = lamp;
    }

    private void Update()
    {
        _renderer.sharedMaterial.SetMatrix(ShaderPropEolsMInverse, eolsCenter.worldToLocalMatrix);

        // var centerPos = eolsCenter.InverseTransformVector(eolsCenter.position);
        // _renderer.material.SetVector(ShaderPropEolsCenter, new Vector4(centerPos.x, centerPos.y, 0, 0));
        
        if (lampRequired && lamp)
        {
            // var lampPos = eolsCenter.transform.InverseTransformVector(lamp.position);
            // _renderer.material.SetVector(ShaderPropLampPos, new Vector4(lampPos.x, lampPos.z, 0, 0));
            _renderer.sharedMaterial.SetMatrix(ShaderPropLampM, _lampRenderer.localToWorldMatrix);
        }
    }
}