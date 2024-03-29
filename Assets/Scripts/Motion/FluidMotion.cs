﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;




public class FluidMotion : InputMovementBase
{

    private PosAndDir[] _posDirs;

    public Material CurMaterial;


    /// <summary>
    /// 线条的初始速度
    /// </summary>
    public Vector3 InitialVelocity;

    /// <summary>
    /// 重力
    /// </summary>
    public Vector3 Gravity;



    /// <summary>
    /// 弹性系数
    /// </summary>
    public float Threshold;


    public float Radius = 10;

    public Transform Collision;

    /// <summary>
    /// 每一列的元素个数
    /// </summary>
    public int Column;

    public RawImage Tip;

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

    public RenderTexture VelocityRT2Temp;

    public float dt = 0.1f;


    public ComputeShader ScaleImageComputeShader;

    /// <summary>
    /// 与相机的距离
    /// </summary>
    private float Zdepth = 30;

    public Vector4 Vector4;

    private RenderTexture rtDes;
    protected override void Start()
    {
      
        this.Type = MotionType.Fluid;
        base.Start();
    }
    protected override void Init()
    {
        base.Init();
       

        Debug.Log("screen width is " + Screen.width + "  screen height is " + Screen.height);
        TexWidth = Screen.width;
        TexHeight = Screen.height;

        int stride = Marshal.SizeOf(typeof(PosAndDir));
        Debug.Log("stride byte size is " + stride);
        ComputeBuffer = new ComputeBuffer(ComputeBuffer.count, stride);

        ComputeBuffer colorBuffer = new ComputeBuffer(ComputeBuffer.count, 16);

        Vector4[] colors = new Vector4[ComputeBuffer.count];
        _posDirs = new PosAndDir[ComputeBuffer.count];

        TextureInstanced.Instance.ChangeInstanceMat(CurMaterial);
        CurMaterial.enableInstancing = true;

        int count = ComputeBuffer.count / Column;//得到列数
        Vector3 velocity = new Vector3(0f, -2f, 0f);


        _screenPosLeftDown = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Zdepth - Camera.main.transform.position.z));
        _screenPosLeftUp = Camera.main.ScreenToWorldPoint(new Vector3(0, Height, Zdepth - Camera.main.transform.position.z));
        _screenPosRightDown = Camera.main.ScreenToWorldPoint(new Vector3(Width, 0, Zdepth - Camera.main.transform.position.z));
        _screenPosRightUp = Camera.main.ScreenToWorldPoint(new Vector3(Width, Height, Zdepth - Camera.main.transform.position.z));

       // List<Vector2> randomPos = Common.Sample2D((_screenPosRightDown.x - _screenPosLeftDown.x)*3 , (_screenPosLeftUp.y - _screenPosLeftDown.y)*3 , 1 , 25);


        #region  备份的粒子排列数据

        ////使物体组成线段
        //for (int i = 0; i < count; i++)
        //{
        //    float distance = Random.Range(-18f, 18f);//随机x轴位置
        //    Color col = Random.ColorHSV();// new Color(92f/255,178f/255,255f,0.3f);//随机颜色
        //    float h = Random.Range(20, 40);//随机高度，在屏幕外
        //    float cameraHeight = Camera.main.orthographicSize;
        //    for (int j = 0; j < Column; j++)
        //    {
        //        int index = Column * i + j;
        //        //y轴个数，即求一列的个数的算法。z为缩放，跟y有关系，因必须等比例缩放。算法要求的是一条线组成的quad必须看起来是连贯的甚至重叠的，不允许有缝隙，因为这条线是很多quad组成
        //        //高度是从下往上算的，一个一个堆积起来,底部是线头，顶部是线尾
        //        Vector4 pos = new Vector4(distance, 2 * (j * 10f / Column - 5f) + h, 0, 60f / Column);
        //        _posDirs[index].position = pos;
        //        _posDirs[index].originalPos = pos;
        //        _posDirs[index].delayFrame = 0;
        //        _posDirs[index].initialVelocity = InitialVelocity;
        //        _posDirs[index].velocity = InitialVelocity;

        //        if (j == 0)
        //        {
        //            _posDirs[index].heardIndex = index; //一条线中的头部物体索引
        //            _posDirs[index].freeMoveArg = GetRangeValue(cameraHeight);
        //            //Debug.Log("heardIndex index is " + index);
        //        }
        //        else if (j == Column - 1)
        //        {
        //        }
        //        else // 否则将视为中间段  
        //        {
        //            // Debug.Log("tailIndex index is " + -1);
        //            _posDirs[index].heardIndex = -1;
        //        }
        //        //posDirs[index].velocity = Vector3.down;
        //        colors[index] = col;
        //    }
        //}

        #endregion


        float beginPosX = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Zdepth)).x;
       

        //for (int i = 0; i < _posDirs.Length; i++)
        //{
            
        //    int y = i/TexWidth;

        //    int x = i - y * TexWidth;
            
        //    Vector3 wPos = Camera.main.ScreenToWorldPoint(new Vector3(x, y, Zdepth));
        //    Vector4 wposVector4 = new Vector4(wPos.x,wPos.y,wPos.z,5f);
           
        //   // _posDirs[i].oldPos = new Vector4(beginPosX, wposVector4.y, wposVector4.z, wposVector4.w);
        //    _posDirs[i].position = wposVector4;
        //    _posDirs[i].originalPos = new Vector4(beginPosX, wposVector4.y, wposVector4.z, wposVector4.w);
        //    _posDirs[i].velocity = new Vector3(.1f,0f,0f);
        //    colors[i] = Random.ColorHSV();
           
        //}
        Vector3 ranomPos = new Vector3(100f,100f,500f);
        Vector3 camPos = Camera.main.transform.position;
        for (int i = 0; i < _posDirs.Length; i++)
        {
            int picIndex = 0;
            int isRest = 0;
            int level = -1;
            Vector2 size = PictureHandle.Instance.GetIndexSizeOfNumber(i, out level, out isRest, out picIndex);//得到缩放尺寸

            float xScale = size.x / 512f;
            float yScale = size.y / 512f;
            float proportion = size.x / size.y;
            if (xScale >= 2 || yScale >= 2)
            {
                //如果超过2倍大小，则强制缩放到一倍大小以内，并以宽度为准，等比例减少  
                int a = (int)xScale;
                xScale = xScale - (a) + 2f;

                yScale = xScale / proportion;
            }


            _posDirs[i].initialVelocity = new Vector3(xScale, yScale, 0f);//填充真实宽高
            _posDirs[i].picIndex = picIndex;

            _posDirs[i].bigIndex = picIndex;
            _posDirs[i].initialVelocity = new Vector3(xScale, yScale, 0f);//填充真实宽高
            //Vector2 rangeVector2 = randomPos[Random.Range(0, randomPos.Count)];
            //Vector4 value = new Vector4(rangeVector2.x+_screenPosLeftDown.x*3, rangeVector2.y+ _screenPosLeftDown.y, Zdepth+Random.Range(0,100),1f);
            Vector4 value = new Vector4(Random.Range(-ranomPos.x, ranomPos.x)+ camPos.x, Random.Range(-ranomPos.y, ranomPos.y)+ camPos.y, Random.Range(0, ranomPos.z)+ camPos.z, 1f);
            _posDirs[i].position = value;
            _posDirs[i].originalPos = new Vector4(beginPosX, value.y, value.z, value.w);
            _posDirs[i].velocity = new Vector3(0, 0f, 0f);
            colors[i] = Random.ColorHSV();

        }


        TextureInstanced.Instance.ChangeInstanceMat(CurMaterial);
        CurMaterial.enableInstancing = true;

       
        colorBuffer.SetData(colors);
        ComputeBuffer.SetData(_posDirs);

        CurMaterial.SetVector("_WHScale", new Vector4(1f, 1f, 1f, 1f));
        CurMaterial.SetBuffer("positionBuffer", ComputeBuffer);
        CurMaterial.SetBuffer("colorBuffer", colorBuffer);
        CurMaterial.SetTexture("_TexArr", TextureInstanced.Instance.TexArr);

        ComputeShader.SetBuffer(dispatchID, "positionBuffer", ComputeBuffer);
        ComputeShader.SetFloat("_Dim", Mathf.Sqrt(ComputeBuffer.count));
        ComputeShader.SetFloat("_DeltaTime", Time.deltaTime);
        ComputeShader.SetVector("_Gravity", Gravity);//_Threshold
        ComputeShader.SetFloat("_Threshold", Threshold);
        ComputeShader.SetFloat("_Radius", Radius);
        ComputeShader.SetInt("_Column", Column);
        //因不能从unityCG.cginc里拿到屏幕参数，所以从这里传入进去
        ComputeShader.SetInt("ScreenWidth", Screen.width);
        ComputeShader.SetInt("ScreenHeight", Screen.height);





        DivergenceRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RHalf); DivergenceRT.Create();
        DyeRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGBHalf); DyeRT.Create();
        DyeRT2 = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGBHalf); DyeRT2.Create();
        InitDyeRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGBHalf); InitDyeRT.Create();
        VelocityRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RGHalf); VelocityRT.Create();
        VelocityRT2 = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RGHalf);
        VelocityRT2.enableRandomWrite = true;
        VelocityRT2.Create();

        VelocityRT2Temp = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RGHalf);
        VelocityRT2Temp.enableRandomWrite = true;
        VelocityRT2Temp.Create();


        PressureRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RHalf); PressureRT.Create();
        PressureRT2 = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.RHalf); PressureRT2.Create();
        BlockRT = new RenderTexture(TexWidth, TexHeight, 0, RenderTextureFormat.ARGB32);
        BlockRT.enableRandomWrite = true;
        BlockRT.Create();
        int k = ScaleImageComputeShader.FindKernel("CSMain");
        ScaleImageComputeShader.SetTexture(k, "Dst", BlockRT);
       // Vector4 = new Vector4(TexWidth / 2, TexHeight / 2, 50f, 0f);
        Vector4 = new Vector4(640, 360, 50f, 0f);

        ComputeShader.SetTexture(dispatchID, "VelocityTemp", VelocityRT2Temp);

        Graphics.Blit(null, InitDyeRT, InitDyeMat);
         Graphics.Blit(null, BlockRT, BlockMat);


    }

    void UpdateRt()
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
        Graphics.Blit(DyeRT2, DyeRT, AdvectionDyeMat);

       // MoveObject(VelocityRT2);
        //第七步：显示
        DisplayMat.SetTexture("BlockTex", BlockRT);
        //DisplayRainbowMat.SetTexture("BlockTex", BlockRT);
        //Graphics.Blit(DyeRT, destination, DisplayMat);
        //Graphics.Blit(VelocityRT2, destination, DisplayRainbowMat);
        //Graphics.Blit(PressureRT2, destination, DisplayRainbowMat);
        // Graphics.Blit(source, destination);
        Tip.texture = VelocityRT2;
    }

    private Coroutine _coroutine;
    private bool _isCompleted = false;
    private IEnumerator WaitTime()
    {
        yield return new WaitForSeconds(4);
        ComputeShader.SetTexture(dispatchID, "Velocity", VelocityRT2);
        _isCompleted = true;

    }
    protected override void Dispatch(ComputeBuffer system)
    {
       // UpdateRt();


        //屏幕坐标转为投影坐标矩阵
        Matrix4x4 p = Camera.main.projectionMatrix;
        //世界坐标到相机坐标矩阵
        Matrix4x4 v = Camera.main.worldToCameraMatrix;
        Matrix4x4 ip = Matrix4x4.Inverse(p);
        Matrix4x4 iv = Matrix4x4.Inverse(v);

        ComputeShader.SetMatrix("p", p);
        ComputeShader.SetMatrix("v", v);
        ComputeShader.SetMatrix("ip", ip);
        ComputeShader.SetMatrix("iv", iv);

        
        ComputeShader.SetTexture(dispatchID, "Velocity", VelocityRT2);
        ComputeShader.SetVector("_Pos", Collision.position);
        ComputeShader.SetFloat("_Radius", Radius);
        ComputeShader.SetFloat("Seed", Random.Range(0f, 1f));
        ////因不能从unityCG.cginc里拿到屏幕参数，所以从这里传入进去
        //ComputeShader.SetInt("ScreenWidth", Screen.width);
        //ComputeShader.SetInt("ScreenHeight", Screen.height);
        base.Dispatch(dispatchID, system);


        //FluidStruct[] data = new FluidStruct[ComputeBuffer.count];
        //ComputeBuffer.GetData(data);
        //Debug.Log(data[0].position + "  " + data[1].position + "  " + data[2].position );
    }

    /// <summary>
    /// 获取随机值
    /// </summary>
    private Vector4 GetRangeValue(float cameraHeight)
    {
        Vector4 float4 = new Vector4();

        //float height = Camera.main.orthographicSize;//获取半屏幕高度

        float htightTemp = Random.Range(cameraHeight / 5, cameraHeight * 2);//随机移动范围,Y轴参考高度

        float upPos = Random.Range(0, cameraHeight);//顶部开始自由移动的位置

        float dowPos = upPos - htightTemp;//底部位置


        int value1;
        float strength;
        float temp1 = Random.Range(0f, 1f);
        if (temp1 >= 0.6f) value1 = -1;
        else value1 = 1;
        if (value1 == 1) strength = Random.Range(0f, 0.1f);
        else strength = 0;



        float dir;
        float temp = Random.Range(0f, 1f);
        if (temp >= 0.5f) dir = -1f;
        else dir = 1f;

        float4.x = upPos;
        float4.y = dowPos;
        float4.z = strength;
        float4.w = dir;

        return float4;
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

   


}
