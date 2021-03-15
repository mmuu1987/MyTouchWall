using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public struct FluidStruct
{
    public Vector4 position;

    public Vector3 velocity;
    /// <summary>
    /// 物体初始速度
    /// </summary>
    public Vector3 initialVelocity;

    public Vector4 oldPos;
    /// <summary>
    /// 插值所需要到的容器圆上
    /// </summary>
    public Vector3 addvalUp;
    /// <summary>
    /// 插值所需要到的容器圆下
    /// </summary>
    public Vector3 addvalDown;
    /// <summary>
    /// 水平移动的向量圆的上半部分
    /// </summary>
    public Vector3 fluidUp;
    /// <summary>
    /// 圆的下班部分的运动
    /// </summary>
    public Vector3 fluidDown;
    /// <summary>
    /// 头索引，如果是就大于0，如果不是则为-1
    /// </summary>
    public int heardIndex;
    /// <summary>
    /// 初始状态的位置
    /// </summary>
    public Vector4 originalPos;
    /// <summary>
    /// 水平随机自由移动需要的参数,x,为在Y轴的最上触发距离，y为Y轴最下触发距离，为移动强度，w为触发的方向
    /// </summary>
    public Vector4 freeMoveArg;
    public int delayFrame;
}

public class FluidMotion : MotionInputMoveBase
{

    private FluidStruct[] _posDirs;

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

    protected override void Init()
    {
        base.Init();
        MotionType = MotionType.Fluid;
       

        int stride = Marshal.SizeOf(typeof(FluidStruct));
        Debug.Log("stride byte size is " + stride);
        ComputeBuffer = new ComputeBuffer(ComputeBuffer.count, stride);

        ComputeBuffer colorBuffer = new ComputeBuffer(ComputeBuffer.count, 16);

        Vector4[] colors = new Vector4[ComputeBuffer.count];
        _posDirs = new FluidStruct[ComputeBuffer.count];

        TextureInstanced.Instance.ChangeInstanceMat(CurMaterial);
        CurMaterial.enableInstancing = true;

        int count = ComputeBuffer.count / Column;//得到列数
        Vector3 velocity = new Vector3(0f, -2f, 0f);

        //使物体组成线段
        for (int i = 0; i < count; i++)
        {
            float distance = Random.Range(-18f, 18f);//随机x轴位置
            Color col = Random.ColorHSV();// new Color(92f/255,178f/255,255f,0.3f);//随机颜色
            float h = Random.Range(20, 40);//随机高度，在屏幕外
            float cameraHeight = Camera.main.orthographicSize;
            for (int j = 0; j < Column; j++)
            {
                int index = Column * i + j;
                //y轴个数，即求一列的个数的算法。z为缩放，跟y有关系，因必须等比例缩放。算法要求的是一条线组成的quad必须看起来是连贯的甚至重叠的，不允许有缝隙，因为这条线是很多quad组成
                //高度是从下往上算的，一个一个堆积起来,底部是线头，顶部是线尾
                Vector4 pos = new Vector4(distance, 2 * (j * 10f / Column - 5f) + h, 0, 60f / Column);
                _posDirs[index].position = pos;
                _posDirs[index].originalPos = pos;
                _posDirs[index].delayFrame = 0;
                _posDirs[index].initialVelocity = InitialVelocity;
                _posDirs[index].velocity = InitialVelocity;

                if (j == 0)
                {
                    _posDirs[index].heardIndex = index; //一条线中的头部物体索引
                    _posDirs[index].freeMoveArg = GetRangeValue(cameraHeight);
                    //Debug.Log("heardIndex index is " + index);
                }
                else if (j == Column - 1)
                {
                }
                else // 否则将视为中间段
                {
                    // Debug.Log("tailIndex index is " + -1);
                    _posDirs[index].heardIndex = -1;
                }
                //posDirs[index].velocity = Vector3.down;
                colors[index] = col;
            }
        }



        colorBuffer.SetData(colors);
        ComputeBuffer.SetData(_posDirs);

        CurMaterial.SetBuffer("positionBuffer", ComputeBuffer);
        CurMaterial.SetBuffer("colorBuffer", colorBuffer);

        ComputeShader.SetBuffer(dispatchID, "positionBuffer", ComputeBuffer);
        ComputeShader.SetFloat("_Dim", Mathf.Sqrt(ComputeBuffer.count));
        ComputeShader.SetFloat("_DeltaTime", Time.deltaTime);
        ComputeShader.SetVector("_Gravity", Gravity);//_Threshold
        ComputeShader.SetFloat("_Threshold", Threshold);
        ComputeShader.SetFloat("_Radius", Radius);
        ComputeShader.SetInt("_Column", Column);

    }

    protected override void Dispatch(ComputeBuffer system)
    {
        ComputeShader.SetVector("_Pos", Collision.position);
        ComputeShader.SetFloat("_Radius", Radius);
        ComputeShader.SetFloat("Seed", Random.Range(0f, 1f));
        base.Dispatch(dispatchID, system);
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
}
