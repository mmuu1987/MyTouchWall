using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ComputeShaderTestFluid : MonoBehaviour
{

    public ComputeShader ScaleImageComputeShader;

    public RawImage DstRawImage;

    public Vector4 Vector4;

    private RenderTexture rtDes;
    // Start is called before the first frame update
    void Start()
    {

         rtDes = new RenderTexture((int)1024, (int)1024, 24);
         rtDes.enableRandomWrite = true;
         rtDes.Create();
         int k = ScaleImageComputeShader.FindKernel("CSMain");
        ScaleImageComputeShader.SetTexture(k, "Dst", rtDes);
        Vector4 = new Vector4(512, 512, 0f, 0f);
       
    }

    // Update is called once per frame
    void Update()
    {
        ScaleImageUserRt();
    }

    public void ScaleImageUserRt()
    {
       
       

        //    Compute Shader

        //1 找到compute shader中所要使用的KernelID
        int k = ScaleImageComputeShader.FindKernel("CSMain");

      
      
        ScaleImageComputeShader.SetVector("pos", Vector4);

        //3 运行shader  参数1=kid  参数2=线程组在x维度的数量 参数3=线程组在y维度的数量 参数4=线程组在z维度的数量
        ScaleImageComputeShader.Dispatch(k, (int)1024, (int)1024, 1);

        //cumputeShader gpu那边已经计算完毕。rtDes是gpu计算后的结果
       
        DstRawImage.texture = rtDes;
       

        //后续操作，把reDes转为Texture2D  
        //删掉rtDes,SourceTexture2D，我们就得到了所要的目标，并且不产生内存垃圾
    }
}
