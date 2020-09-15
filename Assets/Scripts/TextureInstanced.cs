using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

/// <summary>
/// 传递给GPU的结构体，在不同的运动类型，变量的意义有些不一样
/// </summary>
public struct PosAndDir
{
    public Vector4 position;
    /// <summary>
    /// 一般指速度，在不同的运动类有不同的意义
    /// </summary>
    public Vector4 velocity;
    /// <summary>
    /// 物体初始速度
    /// </summary>
    public Vector3 initialVelocity;
    /// <summary>
    /// 初始状态的位置
    /// </summary>
    public Vector4 originalPos;

    /// <summary>
    /// 移动到的目标点
    /// </summary>
    public Vector3 moveTarget;

    /// <summary>
    /// 粒子靠这个向量来自动移动
    /// </summary>
    public Vector3 moveDir;

    /// <summary>
    /// 所在的行和列的位置
    /// </summary>
    public Vector2 indexRC;

    /// <summary>
    /// 索要表现的贴图
    /// </summary>
    public int picIndex;

    /// <summary>
    /// 显示图片局部的index
    /// </summary>
    public int bigIndex;
    /// <summary>
    /// 第一套 UV加UV偏移
    /// </summary>
    public Vector4 uvOffset;
    /// <summary>
    /// 第二套UV加UV偏移 
    /// </summary>
    public Vector4 uv2Offset;

    /// <summary>
    /// 状态码
    /// </summary>
    public int stateCode;

    

    public PosAndDir(int id)
    {
        position = new Vector4();


        velocity = new Vector3();
        initialVelocity = new Vector3();
        originalPos = new Vector4(); 
        moveTarget = new Vector3();
        moveDir = new Vector3();
        indexRC = new Vector2();

        picIndex = id;
        bigIndex = 1;
        uvOffset = new Vector4();

        uv2Offset = new Vector4();
        stateCode = -1;
    }
}



/// <summary>
/// 运动类型 
/// </summary>
public enum MotionType
{
    None,
    Wall,
    /// <summary>
    /// 立方体
    /// </summary>
    Cube,
    /// <summary>
    /// 水平循环左或右运动
    /// </summary>
    Loop,
    /// <summary>
    /// 分类运动
    /// </summary>
    ClassiFicationMotion,
    /// <summary>
    /// z轴不同的运动
    /// </summary>
    MultiDepth,
    /// <summary>
    /// 第一排显示整齐规律的图片，后面的做无序运动
    /// </summary>
    ShowFirstMotion,
    /// <summary>
    /// 银河系运动
    /// </summary>
    Galaxy
}

public enum TouchType
{
    None,
    /// <summary>
    /// 触摸离开
    /// </summary>
    TouchUp,
    /// <summary>
    /// 首次触摸按下
    /// </summary>
    TouchDown,
    /// <summary>
    /// 触摸中
    /// </summary>
    Touching
}
/// <summary>
/// This demo shows the use of Compute Shaders to update the object's
/// positions. The buffer is stored and updated directly in GPU.
/// </summary>
public class TextureInstanced : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public int InstanceCount = 100000;

    public Mesh InstanceMesh;
    public Material InstanceMaterial;
    public  MotionType Type;
    /// <summary>
    /// 每一列的元素个数
    /// </summary>
    private int _column;

    public ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    private ComputeBuffer colorBuffer;
    private ComputeBuffer boundaryBuffer;


    /// <summary>
    /// 图片范围的长，就是屏幕的宽
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// 图片范围的高，就是屏幕的高
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// 横列，一横有多少个数
    /// </summary>
    public int HorizontalColumn = 10;
    /// <summary>
    /// 竖列，一竖有多少个数
    /// </summary>
    public int VerticalColumn = 10;

    /// <summary>
    /// 面片的长
    /// </summary>
    public float SizeWidth = 1;
    /// <summary>
    /// 面片的高
    /// </summary>
    public float SizeHeight = 1f;


    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    /// <summary>
    /// 图片距离相机的距离
    /// </summary>
    public float Z = 10;

    public Transform tipLD;
    public Transform tipLU;
    public Transform tipRD;
    public Transform tipRU;


    public Texture2DArray TexArr;

   

    /// <summary>
    /// 贴图个数
    /// </summary>
    public int TextureCount = 0;
    public Texture2DArrayStyle.ECopyTexMethpd copyTexMethod;

    public CubeMotion CubeMotion;

    public LoopMotion LoopMotion;

    public MultiDepthMotion MultiDepthMotion;

    public ClassiFicationMotion ClassiFicationMotion;

    public ShowFirstMotion ShowFirstMotion;

    public GalaxyMotion GalaxyMotion;

    public static TextureInstanced Instance;

    /// <summary>
    /// 银河位置需要用到的图片
    /// </summary>
    public Texture2D PosTexture2D;

    public RawImage MoveTexture;

    /// <summary>
    /// 当前实例渲染的材质
    /// </summary>
    public Material CurMaterial { get; private set; }

    public List<Vector3> GalaxyPosList;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("已经有单例了，不能重复赋值");

        Instance = this;
        
    }

    void Start()
    {
        Width = Screen.width;
        Height = Screen.height;

        Type = PictureHandle.Instance.MotionType;

        InstanceCount = HorizontalColumn * VerticalColumn;
        CurMaterial = InstanceMaterial;
        HandleTextureArry(PictureHandle.Instance.TexArr);
        
       // PictureHandle.Instance.DestroyTexture();//贴图加载到GPU那边后这边内存就清理掉

        argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);

        CreateBuffers();
        // StartCoroutine(LoadVideo(path));
    }


    void Update()
    {
        InputManager.Instance.HandleInput();
        // UpdateBuffers();
        UpdateBuffers(Type);
        // Render
        Graphics.DrawMeshInstancedIndirect(InstanceMesh, 0, CurMaterial, InstanceMesh.bounds, argsBuffer, 0, null, ShadowCastingMode.Off, false);
    }

    /// <summary>
    /// 用不同的材质渲染实例对象
    /// </summary>
    public void ChangeInstanceMat(Material mat)
    {
        if (mat != null)
        {
            CurMaterial = mat;
        }
        else //如果为null，默认自带的材质
        {
            CurMaterial = InstanceMaterial;
        }
    }

    void UpdateCubeBuffers()
    {
        LoopMotion.ExitMotion();
        ClassiFicationMotion.ExitMotion();
        ShowFirstMotion.ExitMotion();
        MultiDepthMotion.ExitMotion();
        GalaxyMotion.ExitMotion();
        CubeMotion.StartMotion(this);

    }

    void UpdateLoop()
    {
        CubeMotion.ExitMotion();
        ClassiFicationMotion.ExitMotion();
        MultiDepthMotion.ExitMotion();
        GalaxyMotion.ExitMotion();
        ShowFirstMotion.ExitMotion();
        LoopMotion.StartMotion(this);
    }

    void UpdateClassiFicationMotion()
    {
        CubeMotion.ExitMotion();
        LoopMotion.ExitMotion();
        ShowFirstMotion.ExitMotion();
        MultiDepthMotion.ExitMotion();
        ClassiFicationMotion.StartMotion(this);
    }
    void UpdateMultiDepthMotion()
    {
        CubeMotion.ExitMotion();
        GalaxyMotion.ExitMotion();
        LoopMotion.ExitMotion();
        ShowFirstMotion.ExitMotion();
        ClassiFicationMotion.ExitMotion();
        MultiDepthMotion.StartMotion(this);
        
    }

    void UpdateShowFirstMotion()
    {
        CubeMotion.ExitMotion();
        GalaxyMotion.ExitMotion();
        LoopMotion.ExitMotion();
        ClassiFicationMotion.ExitMotion();
        MultiDepthMotion.ExitMotion();
        ShowFirstMotion.StartMotion(this);
    }
    void UpdateGalaxyMotion()
    {
        CubeMotion.ExitMotion();
        LoopMotion.ExitMotion();
        ClassiFicationMotion.ExitMotion();
        MultiDepthMotion.ExitMotion();
        ShowFirstMotion.ExitMotion();
        GalaxyMotion.StartMotion(this);
    }

    public void CubeType()
    {
        Type = MotionType.Cube;
    }

    public void LoopType()
    {
        Type = MotionType.Loop;
    }
    void UpdateBuffers(MotionType type)
    {

        switch (type)
        {
            case MotionType.None:
                break;
            case MotionType.Wall:
                //UpdateWallBuffers();
                break;
            case MotionType.Cube:
                UpdateCubeBuffers();
                break;
            case MotionType.Loop:
                UpdateLoop();
                break;
            case MotionType.ClassiFicationMotion:
                UpdateClassiFicationMotion();
                break;
            case MotionType.MultiDepth:
                UpdateMultiDepthMotion();
                break;
            case MotionType.ShowFirstMotion:
                UpdateShowFirstMotion();
                break;
            case MotionType.Galaxy:
                UpdateGalaxyMotion();
                break;
            default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }


    }

    void CreateBuffers()
    {
        if (InstanceCount < 1) InstanceCount = 1;

        if (_column < 100) _column = 100;

        InstanceCount = Mathf.ClosestPowerOfTwo(InstanceCount);

        GalaxyPosList = Common.GetPos(PosTexture2D, 1.5f, InstanceCount,10);

        InstanceMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);

        // Positions & Colors
        if (positionBuffer != null) positionBuffer.Release();
        if (colorBuffer != null) colorBuffer.Release();
        if (boundaryBuffer != null) boundaryBuffer.Release();


        int stride = Marshal.SizeOf(typeof(PosAndDir));
        //Debug.Log("stride byte size is " + stride);
        positionBuffer = new ComputeBuffer(InstanceCount, stride);//16

        colorBuffer = new ComputeBuffer(InstanceCount, 16);
        int boundbuff = Marshal.SizeOf(typeof(Vector4));
        boundaryBuffer = new ComputeBuffer(4, boundbuff);

        Vector4[] colors = new Vector4[InstanceCount];
        PosAndDir[] posDirs = new PosAndDir[InstanceCount];


        for (int i = 0; i < InstanceCount; i++)
        {
            posDirs[i].position = Vector4.one;
            posDirs[i].picIndex = i % TexArr.depth;
        }

        colorBuffer.SetData(colors);
        positionBuffer.SetData(posDirs);



        CurMaterial.SetBuffer("positionBuffer", positionBuffer);
        
      //  CurMaterial.SetBuffer("colorBuffer", colorBuffer);


        // indirect args
        uint numIndices = (InstanceMesh != null) ? InstanceMesh.GetIndexCount(0) : 0;
        args[0] = numIndices;
        args[1] = (uint)InstanceCount;
        argsBuffer.SetData(args);
    }



    private void HandleTextureArry(Texture2DArray texArr)
    {
        this.TexArr = texArr;
        CurMaterial.SetTexture("_TexArr", texArr);
    }

   
    void OnDisable()
    {
        if (positionBuffer != null) positionBuffer.Release();
        positionBuffer = null;

        if (colorBuffer != null) colorBuffer.Release();
        colorBuffer = null;

        if (argsBuffer != null) argsBuffer.Release();
        argsBuffer = null;

        if (boundaryBuffer != null) boundaryBuffer.Release();
        boundaryBuffer = null;


    }
    private Vector2 _delta;
    public void OnDrag(PointerEventData eventData)
    {
        _delta = eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _delta = Vector2.zero;
    }



    private int depth = 2;
    //private void OnGUI()
    //{
    //    if (GUI.Button(new Rect(0f, 0f, 300f, 300f), "test"))
    //    {
    //        MultiDepthMotion.ChangeState(depth);
    //        depth--;
    //        if (depth < 0) depth = 2;
    //        //ClassiFicationMotion.ChangeState(1);
    //    }
    //    if (GUI.Button(new Rect(300f, 0f, 300f, 300f), "test2"))
    //    {

           
    //    }
    //}

}
