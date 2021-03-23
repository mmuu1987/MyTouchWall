using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class VortexStreetManager : MonoBehaviour
{
  public Material DivergenceMat;
  public Material PressureMat;
  public Material SubtractMat;
  public Material AdvectionDyeMat;
  public Material AdvectionVelocityMat;
  public Material InitDyeMat;
  public Material BlockMat;
  public Material DisplayMat;
  public Material DisplayRainbowMat;
  public Material ViscosityMat;
  private int TexWidth = Screen.width;
  private int TexHeight = Screen.height;

  public RenderTexture DivergenceRT;
  public RenderTexture DyeRT;
  public RenderTexture DyeRT2;
  public RenderTexture VelocityRT;
  public RenderTexture VelocityRT2;
  public RenderTexture PressureRT;
  public RenderTexture PressureRT2;
  public RenderTexture InitDyeRT;
  public RenderTexture BlockRT;


  public float dt = 0.1f;


  public ComputeShader ScaleImageComputeShader;



  public Vector4 Vector4;

  private RenderTexture rtDes;

  void Start()
  {



    DivergenceRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RHalf); DivergenceRT.Create();
    DyeRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGBHalf); DyeRT.Create();
    DyeRT2 = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGBHalf); DyeRT2.Create();
    InitDyeRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGBHalf); InitDyeRT.Create();
    VelocityRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RGHalf); VelocityRT.Create();
    VelocityRT2 = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RGHalf);
    VelocityRT2.enableRandomWrite = true;
    VelocityRT2.Create(); 

    PressureRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RHalf); PressureRT.Create();
    PressureRT2 = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RHalf); PressureRT2.Create();
    BlockRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGB32);
    BlockRT.enableRandomWrite = true;
    BlockRT.Create();
    int k = ScaleImageComputeShader.FindKernel("CSMain");
    ScaleImageComputeShader.SetTexture(k, "Dst", BlockRT);
    Vector4 = new Vector4(TexWidth / 2, TexHeight / 2, 50f, 0f);


    Graphics.Blit(null, InitDyeRT, InitDyeMat);
    // Graphics.Blit(null, BlockRT, BlockMat);
    ComputeBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(Data)));
    Data[] data = new Data[1];
    ComputeBuffer.SetData(data);
  }

  void OnRenderImage(RenderTexture source, RenderTexture destination)
  {
    ScaleImageUserRt();
        //第一步：平流速度
        AdvectionVelocityMat.SetTexture("VelocityTex", VelocityRT2);
        AdvectionVelocityMat.SetTexture("BlockTex", BlockRT);
        AdvectionVelocityMat.SetFloat("dt", dt);
        Graphics.Blit(VelocityRT2, VelocityRT, AdvectionVelocityMat);
        Graphics.Blit(VelocityRT, VelocityRT2);

        //第二步，加大流体粘性可抑制边界层分离现象
        for (int i = 0; i < 0; i++)
        {
            ViscosityMat.SetTexture("_VelocityTex", VelocityRT2);
            Graphics.Blit(VelocityRT2, VelocityRT, ViscosityMat);
            Graphics.Blit(VelocityRT, VelocityRT2);
        }

        //第三步：计算散度
        DivergenceMat.SetTexture("VelocityTex", VelocityRT2);
        Graphics.Blit(VelocityRT2, DivergenceRT, DivergenceMat);

        //第四步：计算压力
        PressureMat.SetTexture("DivergenceTex", DivergenceRT);
        for (int i = 0; i < 100; i++)
        {
            PressureMat.SetTexture("PressureTex", PressureRT2);
            Graphics.Blit(PressureRT2, PressureRT, PressureMat);
            Graphics.Blit(PressureRT, PressureRT2);
        }
        //第五步：速度场减去压力梯度，得到无散度的速度场
        SubtractMat.SetTexture("PressureTex", PressureRT2);
        SubtractMat.SetTexture("VelocityTex", VelocityRT2);
        Graphics.Blit(VelocityRT2, VelocityRT, SubtractMat);
        Graphics.Blit(VelocityRT, VelocityRT2);

        //第六步：用最终速度去平流密度  
        Graphics.Blit(DyeRT, DyeRT2);
        AdvectionDyeMat.SetTexture("VelocityTex", VelocityRT2);
        AdvectionDyeMat.SetTexture("DensityTex", DyeRT2);
        AdvectionDyeMat.SetTexture("BlockTex", BlockRT);
        AdvectionDyeMat.SetTexture("InitDyeTex", InitDyeRT);
        AdvectionDyeMat.SetFloat("dt", dt);
        Graphics.Blit(null, DyeRT, AdvectionDyeMat);

       // MoveObject(VelocityRT2);
        //第七步：显示  
        DisplayMat.SetTexture("BlockTex", BlockRT);
        //DisplayRainbowMat.SetTexture("BlockTex", BlockRT); 
        Graphics.Blit(DyeRT, destination, DisplayMat);
        //Graphics.Blit(VelocityRT2, destination, DisplayRainbowMat);
        //Graphics.Blit(PressureRT2, destination, DisplayRainbowMat);
        //Graphics.Blit(source, destination);
    }

  private void OnPostRender()
  {
      MoveObject(null);
  }
    public void ScaleImageUserRt()
  {
    //    Compute Shader

    //1 找到compute shader中所要使用的KernelID
    int k = ScaleImageComputeShader.FindKernel("CSMain");



    ScaleImageComputeShader.SetVector("pos", Vector4);

    //3 运行shader  参数1=kid  参数2=线程组在x维度的数量 参数3=线程组在y维度的数量 参数4=线程组在z维度的数量
    ScaleImageComputeShader.Dispatch(k, (int)TexWidth, (int)TexHeight, 1);

    //cumputeShader gpu那边已经计算完毕。rtDes是gpu计算后的结果




    //后续操作，把reDes转为Texture2D  
    //删掉rtDes,SourceTexture2D，我们就得到了所要的目标，并且不产生内存垃圾
  }

  public Transform _moveTransform;
  public Camera MainCamera;
  public ComputeBuffer ComputeBuffer;

  private void MoveObject(RenderTexture rt)
  {
    Vector3 screenPos = MainCamera.WorldToScreenPoint(_moveTransform.position);

    Vector2 pos = new Vector2((int)(screenPos.x), (int)(screenPos.y));

    int k = ScaleImageComputeShader.FindKernel("GetPos");

    ScaleImageComputeShader.SetBuffer(k, "PosBuff", ComputeBuffer);
    //ScaleImageComputeShader.SetTexture(k, "Velocity", rt);
    ScaleImageComputeShader.SetVector("screenPos", pos);

    ScaleImageComputeShader.Dispatch(k, (int)TexWidth, (int)TexHeight, 1);



    Data[] data = new Data[1];
    ComputeBuffer.GetData(data);

    Debug.Log(data[0].pos);

  }

}

public struct Data
{
  public Vector2 pos;
}
