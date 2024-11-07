using System;
using UnityEngine;
using static LuminaTerra.Util.Extensions;

namespace LuminaTerra.EOLS;

public class EOLSGroundController : MonoBehaviour
{
    private static readonly int ShaderPropEolsMInverse = Shader.PropertyToID("_EOLS_M_Inverse");
    private static readonly int ShaderPropLampM = Shader.PropertyToID("_LAMP_M");
    private static readonly int ShaderPropLampPos = Shader.PropertyToID("_LampPos");
    private static readonly int ShaderPropEolsCenter = Shader.PropertyToID("_EOLSCenter");
    
    [SerializeField] private bool isBridge = false;
    [SerializeField] private Renderer eolsCenter = null;
    [SerializeField] private bool lampRequired = false;
    [SerializeField] private LAMP lamp = null;

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

        _renderer.material.SetInt("_LampRequired", lampRequired ? 1 : 0);
        _renderer.material.SetInt("_Shape", isBridge ? 1 : 0);
    }

    public void SetLamp(LAMP lamp)
    {
        this.lamp = lamp;
        _lampRenderer = lamp.GetComponent<Renderer>();
    }

    private void Update()
    {
        _renderer.material.SetMatrix(ShaderPropEolsMInverse, eolsCenter.worldToLocalMatrix);
        
        if (lampRequired && lamp && lamp.IsLit)
        {
            _renderer.material.SetMatrix(ShaderPropLampM, _lampRenderer.localToWorldMatrix);
        }
        else
        {
            _renderer.material.SetMatrix(ShaderPropLampM, eolsCenter.localToWorldMatrix);
        }
    }
}