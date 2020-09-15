using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ScreenEffect : MonoBehaviour
{

    public Shader effectShader;
    private Material effectMaterial;
    public Material EffectMaterial
    {
        get
        {
            effectMaterial = CreateMaterial(effectShader, effectMaterial);
            return effectMaterial;
        }
    }

    [Range(0.0f, 3.0f)]
    public float brightness = 1.0f;  //亮度

    [Range(0.0f, 3.0f)]
    public float saturation = 1.0f;  //饱和度

    [Range(0.0f, 3.0f)]
    public float contrast = 1.0f;    //对比度


    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (EffectMaterial != null)
        {
            EffectMaterial.SetFloat("_Brightness", brightness);
            EffectMaterial.SetFloat("_Saturation", saturation);
            EffectMaterial.SetFloat("_Contrast", contrast);
            //对渲染纹理进行处理
            Graphics.Blit(src, dest, EffectMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    private void Start()
    {
        bool isSupported = CheckSupport();

        if (isSupported == false)
        {
            this.enabled = false;
        }
    }


    // 检测当前平台是否支持屏幕特效
    private bool CheckSupport()
    {
        if (SystemInfo.supportsImageEffects == false || SystemInfo.supportsRenderTextures == false)
        {
            Debug.LogWarning("当前平台不支持！");
            return false;
        }
        return true;
    }


    // 创建材质
    private Material CreateMaterial(Shader shader, Material material)
    {
        if (shader == null)
        {
            return null;
        }

        if (shader.isSupported && material && material.shader == shader)
            return material;

        if (!shader.isSupported)
        {
            return null;
        }
        else
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            if (material)
                return material;
            else
                return null;
        }
    }
}