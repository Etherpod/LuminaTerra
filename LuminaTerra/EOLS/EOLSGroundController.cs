using System;
using UnityEngine;

namespace LuminaTerra.EOLS;

public class EOLSGroundController : MonoBehaviour
{
    private static readonly int ShaderPropEolsMInverse = Shader.PropertyToID("_EOLS_M_Inverse");
    private static readonly int ShaderPropLampPos = Shader.PropertyToID("_LampPos");
    
    [SerializeField] private bool isBridge = false;
    [SerializeField] private Transform eolsCenter = null;
    [SerializeField] private bool lampRequired = false;
    [SerializeField] private Transform lamp = null;

    private Renderer _renderer = null;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        
        _renderer.material.SetInt("_LampRequired", lampRequired ? 1 : 0);
        _renderer.material.SetInt("_Shape", isBridge ? 1 : 0);
    }

    public void SetLamp(Transform lamp)
    {
        this.lamp = lamp;
    }

    private void Update()
    {
        var centerWTLMatrix = eolsCenter.worldToLocalMatrix;
        _renderer.material.SetMatrix(ShaderPropEolsMInverse, centerWTLMatrix);
        if (lampRequired && lamp)
        {
            _renderer.material.SetVector(ShaderPropLampPos, eolsCenter.InverseTransformVector(lamp.position));
        }
    }
}