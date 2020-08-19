using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class ShowFirstMotion : MotionInputMoveBase
{
    private DepthInfo[] _depths;

    private ComputeBuffer _depthBuffer;

    public Material CurMaterial;


    public Canvas Canvas;

    public Vector3 SrcPos;

    private int _selectClass = -1;

    private int _hideClass = -1;


    public int Depth = 2;
    private int _zeroIndexCount;


    /// <summary>
    /// 点击点 
    /// </summary>
    private Vector3 _clickPoint;

    /// <summary>
    /// 触摸互动吸引的点击buff
    /// </summary>
    private ComputeBuffer _clickBuff;

    /// <summary>
    /// 获取点击到图片的信息
    /// </summary>
    private ComputeBuffer _clickPointBuff;

    /// <summary>
    /// 触摸互动吸引的点击buff
    /// </summary>
    private ComputeBuffer _selectClassBuffer;


    /// <summary>
    /// 触摸数据int为id,vector,4为屏幕位置，加 点击的 时间点
    /// </summary>
    private Dictionary<int, ClickData> _touchIds;

    private MultiDepthPictureMove _depthPictureMove;

    private int _widthScale=1;
    protected override void Init()
    {
        base.Init();
          
        MotionType = MotionType.ShowFirstMotion;
        // Camera.main.fieldOfView = 30f;
        _touchIds = new Dictionary<int, ClickData>();
        PosAndDir[] datas = new PosAndDir[ComputeBuffer.count];

        ComputeBuffer.GetData(datas);

        SetValue(0f);


        for (int i = 0; i < datas.Length; i++)
        {
            float r = Random.Range(25f, 100f);
            if (r <= 60f) r = Random.Range(35f, 100f);//让半径长度偏向球体外侧
            Vector3 dir = Random.onUnitSphere * r;


            Vector3 dis = SrcPos + dir;
            datas[i].position = new Vector4(dis.x, dis.y, dis.z, 1);
            datas[i].moveTarget = dis;

            datas[i].moveDir = dir;//存储方向

            Vector4 otherData = new Vector4();
            otherData.w = r;//存储半径
            int randomxyz = Random.Range(1, 8);
            float speed = Random.Range(0.015f, 0.1f);

            //speed = 0.05f;
            randomxyz = 5;
            switch (randomxyz)
            {
                case 1:
                    otherData = new Vector4(1, 0, 0, speed);
                    break;
                case 2:
                    otherData = new Vector4(1, 1, 0, speed);
                    break;
                case 3:
                    otherData = new Vector4(1, 1, 1, speed);
                    break;
                case 4:
                    otherData = new Vector4(0, 1, 1, speed);
                    break;
                case 5:
                    otherData = new Vector4(0, 1, 0, speed);
                    break;
                case 6:
                    otherData = new Vector4(0, 0, 1, speed);
                    break;
                case 7:
                    otherData = new Vector4(1, 0, 1, speed);
                    break;
                case 8:
                    otherData = new Vector4(1, 0, 0, speed);
                    break;
                default:
                    break;
            }


            datas[i].originalPos = otherData;


            
            //UV偏移另做他用
            datas[i].uvOffset = new Vector4(0f, 0f, 0f, 0f);

            datas[i].uv2Offset = new Vector4(0f, 0f, 0f, 0f);

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


            datas[i].initialVelocity = new Vector3(xScale, yScale, 0f);//填充真实宽高
            datas[i].picIndex = picIndex;

            datas[i].bigIndex = picIndex;
            //x存储的是类别的索引,y存储透明度,   z存储，x轴右边的边界值，为正数   ,最后一个为是否是重复的index，表示的是数据里已经有图片index跟他是一样的了
            int tempVal = level;
            datas[i].velocity = new Vector4(tempVal, 1f, _screenPosRightDown.x, isRest);

            datas[i].stateCode = -1;
        }



        TextureInstanced.Instance.ChangeInstanceMat(CurMaterial);
        CurMaterial.enableInstancing = true;

        TextureInstanced.Instance.CurMaterial.SetVector("_WHScale", new Vector4(1f, 1f, 1f, 1f));


        ComputeBuffer.SetData(datas);
        ComputeShader.SetBuffer(dispatchID, "positionBuffer", ComputeBuffer);

        ComputeShader.SetFloat("Width", Screen.width);
        ComputeShader.SetFloat("Height", Screen.height);

        Matrix4x4 camMatri = Camera.main.projectionMatrix;
        ComputeShader.SetFloat("m32", camMatri.m32);
        ComputeShader.SetFloat("m00", camMatri.m00);
        ComputeShader.SetFloat("m11", camMatri.m11);
        ComputeShader.SetVector("camPos", Camera.main.transform.position);

        TextureInstanced.Instance.CurMaterial.SetBuffer("positionBuffer", ComputeBuffer);
        TextureInstanced.Instance.CurMaterial.SetTexture("_TexArr", TextureInstanced.Instance.TexArr);


        MoveSpeed = 50f;//更改更快的插值速度
        ComputeShader.SetFloat("MoveSpeed", MoveSpeed);
        ComputeShader.SetFloat("dis", 800);
        ComputeShader.SetVector("srcPos", SrcPos);
        ComputeShader.SetInt("selectClass", _selectClass);
        ComputeShader.SetInt("hideClass", _hideClass);

        if (_selectClassBuffer != null)
            _selectClassBuffer.Release();
        temps.Add(Vector4.one);
        _selectClassBuffer = new ComputeBuffer(temps.Count, 16);
        _selectClassBuffer.SetData(temps.ToArray());
        ComputeShader.SetBuffer(dispatchID, "randomPosData", _selectClassBuffer);

        InitDisPatch(InitID);



    }

    private List<Vector2> _randomPos;
    /// <summary>
    /// 获取一定区域内可以填充面片的个数
    /// </summary>
    /// <param name="tempZ"></param>
    /// <param name="widthScale"></param>
    private void SetValue(float tempZ)
    {
        _screenPosLeftDown = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, tempZ - Camera.main.transform.position.z));

        _screenPosLeftUp = Camera.main.ScreenToWorldPoint(new Vector3(0, Height, tempZ - Camera.main.transform.position.z));
        _screenPosRightDown = Camera.main.ScreenToWorldPoint(new Vector3(Width, 0, tempZ - Camera.main.transform.position.z));
        //Debug.Log(_screenPosRightDown.x +"    "+ _widthScale);
        _screenPosRightUp = Camera.main.ScreenToWorldPoint(new Vector3(Width, Height, tempZ - Camera.main.transform.position.z));
        _randomPos = Common.Sample2D((_screenPosRightDown.x - _screenPosLeftDown.x) * _widthScale, (_screenPosLeftUp.y - _screenPosLeftDown.y) * 0.55f, 1 + 0.75f);

    }
    protected override void Dispatch(ComputeBuffer system)
    {
        ComputeShader.SetFloat("deltaTime", Time.deltaTime);
        ComputeShader.SetVector("srcPos", SrcPos);

       base. Dispatch(dispatchID, system);
    }
    public override void ExitMotion()
    {
        base.ExitMotion();
        if (_depthBuffer != null)
            _depthBuffer.Release();
        _depthBuffer = null;

        if (_clickBuff != null)
            _clickBuff.Release();
        _clickBuff = null;

        if (_clickPointBuff != null)
            _clickPointBuff.Release();
        _clickPointBuff = null;
    }
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        Vector3 clickPos = Vector3.one * 100000;
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
        {
            clickPos = hitInfo.transform.position;

        }

        //  _depthPictureMove.SetClickPoint(clickPos);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (MotionType != TextureInstanced.Instance.Type) return;
        base.OnPointerUp(eventData);

        if (_touchIds.ContainsKey(eventData.pointerId))
        {
            float temp = _touchIds[eventData.pointerId].ClickTime;

            // Debug.Log(temp);
            if (temp <= 0.5f)
            {
                // Debug.Log("产生点击事件");
                _clickPoint = new Vector3(eventData.position.x, eventData.position.y, -1);//-1表示有点击事件产生
            }
            _touchIds.Remove(eventData.pointerId);
        }


    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (MotionType != TextureInstanced.Instance.Type) return;
        base.OnPointerDown(eventData);
        //  Debug.Log("this is OnPointerDown  eventData.clickTime " + eventData.clickTime);

        if (!_touchIds.ContainsKey(eventData.pointerId))
        {
            Vector3 pos = eventData.position;
            ClickData data = new ClickData();
            data.Position = pos;
            data.ClickTime = 0;
            _touchIds.Add(eventData.pointerId, data);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (MotionType != TextureInstanced.Instance.Type) return;
        base.OnDrag(eventData);
        if (_touchIds.ContainsKey(eventData.pointerId))
        {
            Vector3 pos = eventData.position;
            _touchIds[eventData.pointerId].Position = pos;//保留初始点击时间
        }
    }

    protected override void Update()
    {

    }
    private void LateUpdate()
    {
        if (_touchIds != null)
            foreach (KeyValuePair<int, ClickData> data in _touchIds)
            {
                data.Value.ClickTime += Time.deltaTime;
            }
    }
    List<Vector4> temps = new List<Vector4>();
    private void ShowClass(int classIndex)
    {
        _selectClass = classIndex;
        _widthScale = 1;
        SetValue(0f);

        //获取显示的个数
        List<int> indexs = PictureHandle.Instance.DicIndex[classIndex];
        int count = indexs.Count;

        int tempCount = _randomPos.Count;//可以容纳的个数

        _widthScale = Mathf.CeilToInt((count / (tempCount * 1f)));

        if (_widthScale > 1)
        {
            SetValue(0f);
        }
        //计算Y轴的位置(1 - scaleY)为空余的位置(1 - scaleY)/2f上下空余的位置，(1 - scaleY)/2f*(_screenPosLeftUp.y - _screenPosLeftDown.y)空余位置的距离
        float heightTmep = (1 - 0.55f) / 2f * (_screenPosLeftUp.y - _screenPosLeftDown.y);

        temps = new List<Vector4>();

       // Debug.Log(_widthScale + "    " + indexs.Count + "   " + _randomPos.Count);
        for (int i = 0; i < _randomPos.Count; i++)
        {
            int index = -1;//图片索引

            if (i >= count)
            {
                int temp = i % count;
                // Debug.Log(" 重复的 indexs is " + temp);
                index = indexs[temp];
            }
            else
            {
                index = indexs[i];
            }


            temps.Add(new Vector4(index, _randomPos[i].x + _screenPosLeftDown.x, _randomPos[i].y + _screenPosLeftDown.y + heightTmep,1f));
        }
        //打散按顺序排列的数据
        for (int i = 0; i < temps.Count; i++)
        {
            int rang = Random.Range(0, temps.Count);

            var val = temps[rang];
            temps[rang] = temps[temps.Count - 1 - rang];
            temps[temps.Count - 1 - rang] = val;
        }

        //Debug.Log(temps.Count);
        _hideClass = -10;
        ComputeShader.SetInt("selectClass", _selectClass);
        ComputeShader.SetInt("hideClass", _hideClass);

        if (_selectClassBuffer!=null)
            _selectClassBuffer.Release();
        _selectClassBuffer = new ComputeBuffer(temps.Count, 16);
        _selectClassBuffer.SetData(temps.ToArray());
        ComputeShader.SetBuffer(dispatchID, "randomPosData", _selectClassBuffer);
        ComputeShader.SetInt("widthScale", _widthScale);
        
        Dispatch(ComputeBuffer);
    }

    private void HideClass(int classIndex)
    {
        _hideClass = classIndex;
        _selectClass = -1;
     
        ComputeShader.SetInt("selectClass", _selectClass);
        ComputeShader.SetInt("hideClass", _hideClass);

      
        Dispatch(ComputeBuffer);
    }
    private void OnGUI()
    {
        if (GUI.Button(new Rect(0f, 0f, 500f, 500f), "test"))
        {
            ShowClass(1);
        }

        if (GUI.Button(new Rect(500f, 0f, 500f, 500f), "test1"))
        {
            HideClass(1);
        }
    }

}
