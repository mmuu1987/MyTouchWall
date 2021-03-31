using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using DG.Tweening;
using Microsoft.Win32.SafeHandles;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;
using Graphics = UnityEngine.Graphics;

/// <summary>
/// 处理图片，整理，分类
/// </summary>
public class PictureHandle : MonoBehaviour
{

    public static PictureHandle Instance;

    public MotionType MotionType;

    public RawImage TestImage;

    public Canvas Canvas;

    public Texture2DArray TexArr { get; set; }

    public List<Texture2D> YearTexs = new List<Texture2D>();
    /// <summary>
    /// 所有的大事件集合
    /// </summary>
    private List<ClassInfo> _classInfos = new List<ClassInfo>();

    private List<ClassInfo> _firstYearsInfos = new List<ClassInfo>();

    private List<ClassInfo> _secondYearInfos = new List<ClassInfo>();

    private List<ClassInfo> _thirdYearInfos = new List<ClassInfo>();

    public List<Texture2D> Texs = new List<Texture2D>();


   /// <summary>
   /// 每个类别存放的图片索引集合，第一个int存储的是类别编号
   /// </summary>
    private Dictionary<int, List<int>> _dicIndex = new Dictionary<int, List<int>>(); 

    /// <summary>
    /// 卓越风采
    /// </summary>
    public List<List<PersonInfo>> PersonInfos = new List<List<PersonInfo>>();

    /// <summary>
    /// 荣誉墙
    /// </summary>
    public List<PersonInfo> HonorWall = new List<PersonInfo>();

    private GameObject _info;
    private int _pictureCount = 0;
    public int PictureCount
    {
        get { return _pictureCount; }
    }

    public ComputeShader ScaleImageComputeShader;

    public int width = 10;//边框像素单位宽度

    public int LableHeight = 10;//文字占有高度像素单位

    /// <summary>
    /// 物品分类的种数
    /// </summary>
    public int ClassCount = 0;

    public bool IsInitEnd = false;

    public List<string> showList = new List<string>();
    /// <summary>
    /// 每个类别存放的图片索引集合，第一个int存储的是类别编号
    /// </summary>
    public Dictionary<int, List<int>> DicIndex
    {
        get { return _dicIndex; }
    }


    private void Awake()
    {
        if (Instance != null) throw new UnityException("单例错误");

        Instance = this;
        DontDestroyOnLoad(this.gameObject);



    }
    IEnumerator Start()
    {
        // LoadPicture();
        List<string> paths = GetAllPath("广东博物馆");


        foreach (string path in paths)
        {
            FileInfo info = new FileInfo(path);

            if (showList.Contains(info.Name))
            {
                List<ClassInfo> infos = LoadPicture(path);
                _classInfos.AddRange(infos);
               
            }
        }

        _pictureCount = 0;
        yield return  StartCoroutine(LoadTextureAssets());

         for (int i = 0; i < showList.Count; i++)
         {
             List<int> indexs = GetClassInfoIndexs(showList[i]);
             _dicIndex.Add(i,indexs);
         }

        
         HandleTextureArry(Texs);

         DestroyTexture();//贴图加载到GPU那边后这边内存就清理掉

         _info = Resources.Load<GameObject>("Prefabs/Info");
         //预制体缩放，后面用来做缩放动画
         _info.transform.localScale = Vector3.one * 0.1f;

        IsInitEnd = true;

    }


    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// 获取根目录下的所有子目录路径
    /// </summary>
    /// <returns></returns>
    List<string> GetAllPath(string root)
    {
        List<string> allPath = new List<string>();

        string path = Application.streamingAssetsPath + "/" + root;

        DirectoryInfo rootDir = new DirectoryInfo(path);

        foreach (DirectoryInfo directory in rootDir.GetDirectories())
        {
            if (directory.Parent.Name == rootDir.Name)
            {

                if (directory.GetDirectories().Length != 0)
                {
                    allPath.Add(directory.FullName);
                    //Debug.Log("path is  " + directory);
                    ClassCount++;
                }

            }
        }

        return allPath;
    }

    /// <summary>
    /// 获取该层次的图片索引
    /// </summary>
    /// <returns></returns>
    public int GetLevelIndex(int count, int level)
    {

        List<int> indexs;

        if (_dicIndex.ContainsKey(level))
        {
            indexs = _dicIndex[level];
            //根据个数分配索引
            int temp = count % indexs.Count;
            return indexs[temp];
        }

        return -1;
    }

    /// <summary>
    /// 获取该层次图片索引的宽高,并返回该索引
    /// </summary>
    /// <param name="count">图片所在的个数</param>
    /// <param name="level">层级</param>
    /// <param name="index">返回图片的索引</param>
    /// <returns></returns>
    public Vector2 GetLevelIndexSize(int count, int level, out int index)
    {

        index = GetLevelIndex(count, level);

        foreach (ClassInfo yesrsInfo in _classInfos)
        {
            foreach (ObjectInfo @event in yesrsInfo.ObjectInfos)
            {
                if (@event.PictureSizeInfo.ContainsKey(index))
                {
                    return @event.PictureSizeInfo[index];
                }
            }
        }

        return Vector2.zero;

    }

    /// <summary>
    /// 根据数量获取该数量下的图片索引和尺寸,是否是重复获取，索引所在的类别位置
    /// </summary>
    /// <param name="number"></param>
    /// <param name="level"></param>
    /// <param name="isRest"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public Vector2 GetIndexSizeOfNumber(int number,out int level,out int isRest, out int index)
    {
        index = number % _pictureCount;

        if (number > _pictureCount) isRest = 0;
        else isRest = 1;

        int temp = -1;
        foreach (KeyValuePair<int, List<int>> pair in _dicIndex)
        {
            foreach (int i in pair.Value)
            {
                if (index == i)
                {
                    temp = pair.Key;
                    break;
                }
            }
            if (temp != -1) break;
        }

        level = temp;



        foreach (ClassInfo classInfo in _classInfos)
        {
            foreach (ObjectInfo objectInfo in classInfo.ObjectInfos)
            {
                if (objectInfo.PictureSizeInfo.ContainsKey(index))
                {
                    if (objectInfo.BelongsClass == "东北软玉雕“渔获”")
                    {
                       // Debug.Log(objectInfo.BelongsClass);
                        //level = 2;
                    }
                    
                    return objectInfo.PictureSizeInfo[index];
                }
            }
        }
        return Vector2.zero;
    }

    /// <summary>
    /// 根据图片索引拿到年代事件信息
    /// </summary>
    public void GetYearInfo(PosAndDir pad, Transform canvas)
    {
        if (pad.picIndex < 0) return;

        ObjectInfo ye = null;
        foreach (ClassInfo yearsInfo in _classInfos)
        {
            foreach (var yearsEvent in yearsInfo.ObjectInfos)
            {
                foreach (int inde in yearsEvent.PictureIndes)
                {
                    if (pad.picIndex == inde)
                    {
                        ye = yearsEvent;
                        break;
                    }
                }
            }

        }

        if (ye == null) throw new UnityException("没有找到相应的年代事件");




        GameObject temp = Instantiate(_info, canvas.transform);

        Item item = temp.GetComponent<Item>();

        item.LoadData(ye, TexArr);

        Vector3 screenPos = Camera.main.WorldToScreenPoint(pad.position);

        RectTransform rectTransform = item.GetComponent<RectTransform>();
        // rectTransform.SetSiblingIndex(2);
        rectTransform.DOScale(0.35f, 0.75f);
        //rectTransform.DOLocalRotate(new Vector3(0f, 360, 0f), 1f, RotateMode.LocalAxisAdd).OnComplete((() =>
        //{
        //    item.RotEnd();
        //}));
        rectTransform.anchoredPosition = screenPos;

        if (screenPos.y >= 800) screenPos.y = 800;
        if (screenPos.y <= 250) screenPos.y = 250;
        if (screenPos.x >= 1700f) screenPos.x = 1700f;
        if (screenPos.x <= 200f) screenPos.x = 200f;


        rectTransform.DOAnchorPos(screenPos, 0.35f);


        //Debug.Log(ye.ToString());
    }

    public ObjectInfo GetYearInfo(PosAndDir pad)
    {
        if (pad.picIndex < 0) return null;

        ObjectInfo ye = null;
        foreach (ClassInfo yearsInfo in _classInfos)
        {
            foreach (var yearsEvent in yearsInfo.ObjectInfos)
            {
                foreach (int inde in yearsEvent.PictureIndes)
                {
                    if (pad.picIndex == inde)
                    {
                        ye = yearsEvent;
                        break;
                    }
                }
            }

        }

        if (ye == null) throw new UnityException("没有找到相应的年代事件");




        return ye;


        //Debug.Log(ye.ToString());
    }
    /// <summary>
    /// 根据类的名字获取该类下所有的图片索引
    /// </summary>
    /// <returns></returns>
    private List<int> GetClassInfoIndexs(string className)
    {
        List<int> indes = new List<int>();
        foreach (ClassInfo yearsInfo in _classInfos)
        {
           
            if (yearsInfo.ClassName == className)
            {
                foreach (ObjectInfo objectInfo in yearsInfo.ObjectInfos)
                {
                    indes.AddRange(objectInfo.PictureIndes);
                }
              
            }
            
        }


        return indes;
    }
   

    public List<ClassInfo> LoadPicture(string path)
    {
        List<ClassInfo> classInfos = new List<ClassInfo>();

        DirectoryInfo directoryInfo = new DirectoryInfo(path);

        FileInfo cfileInfo = new FileInfo(path);

        ClassInfo cinfo = new ClassInfo();
        cinfo.ClassName = cfileInfo.Name;
        classInfos.Add(cinfo);

        DirectoryInfo[] infos = directoryInfo.GetDirectories();//获取年份目录
        cinfo.ClassCount = infos.Length;
        foreach (DirectoryInfo info in infos)
        {
            ObjectInfo objectInfo = new ObjectInfo();
            objectInfo.BelongsClass = info.Name;
            objectInfo.IndexPos = 1;

            FileInfo[] fileInfos = info.GetFiles();

            foreach (FileInfo fileInfo in fileInfos)
            {
                if (fileInfo.Extension == ".txt")
                {

                    objectInfo.DescribePath = fileInfo.FullName;

                    byte[] bytes = File.ReadAllBytes(fileInfo.FullName);

                    string str = Encoding.UTF8.GetString(bytes);

                    objectInfo.Describe = str;
                }
                else if (fileInfo.Extension == ".jpg" || fileInfo.Extension == ".JPG" || fileInfo.Extension == ".jpeg")
                {

                    objectInfo.PicturesPath.Add(fileInfo.FullName);
                }
                else if (fileInfo.Extension == ".mp4")
                {
                    objectInfo.ObjectVideo = fileInfo.FullName;
                }
                else if (fileInfo.Extension == ".png" || fileInfo.Extension == ".PNG")
                {
                    objectInfo.PicturesPath.Add(fileInfo.FullName);
                    // Debug.Log(fileInfo.FullName);
                }
            }

            cinfo.ObjectInfos.Add(objectInfo);


        }
        return classInfos;

    }

    public List<CompanyInfo> CompanyAllTexList { get; set; }

   
    public List<CompanyInfo> PrivateHeirsAllTexList;

    /// <summary>
    /// 加载图片资源
    /// </summary>
    public IEnumerator LoadTextureAssets()
    {
        //先默认为512*512的图片,原始图片的长宽我们在用另外的vector2保存
        //生成需要表现的图片

        int count = 0;

      
        foreach (ClassInfo classInfo in _classInfos)
        {
            foreach (ObjectInfo objectInfo in classInfo.ObjectInfos)
            {
                if (objectInfo.PicturesPath.Count <= 0)//如果没有图片，我们生成一个logo的先填充
                {
                    string s = Application.streamingAssetsPath + "/logo.png";

                    Vector2 vector2;

                    FileInfo fileInfo = new FileInfo(s);


                    Texture newTex = HandlePicture(YearTexs[0], fileInfo.DirectoryName, fileInfo.Name, out vector2);

                    Texture2D tex2D = ScaleImageUserRt(newTex, Common.PictureWidth, Common.PictureHeight);



                    Texs.Add(tex2D);

                    //yearsEvent.PictureIndes.Add(pictureIndex);

                    objectInfo.AddPictureInfo(_pictureCount, vector2);

                    _pictureCount++;
                }
                else
                    foreach (string s in objectInfo.PicturesPath)
                    {
                       

                        if (File.Exists(s))
                        {

                            yield return null;

                            //count++;
                            //if (count >= 100) yield break;

                            Vector2 vector2;

                            FileInfo fileInfo = new FileInfo(s);

                            

                            Texture newTex = HandlePicture(YearTexs[0], fileInfo.DirectoryName, fileInfo.Name, out vector2);

                            Texture2D tex2D = ScaleImageUserRt(newTex, Common.PictureWidth, Common.PictureHeight);

                            

                           // GC.Collect();

                            Texs.Add(tex2D);

                            objectInfo.AddPictureInfo(_pictureCount, vector2);

                            _pictureCount++;
                        }

                    }
            }
        }


    }
    /// <summary>
    /// 缩放图片
    /// </summary>
    /// <param name="targeTexture2D"></param>
    /// <param name="dstWidth">目标宽</param>
    /// <param name="dstHeight">目标高</param>
    /// <returns></returns>
    public Texture2D ScaleImageUserRt(Texture targeTexture2D, int dstWidth, int dstHeight)
    {


        float widthScale = dstWidth * 1f / targeTexture2D.width;
        float heightScale = dstHeight * 1f / targeTexture2D.height;



        RenderTexture rtDes = new RenderTexture(dstWidth, dstHeight, 24);
        rtDes.enableRandomWrite = true;
        rtDes.Create();


        ////////////////////////////////////////
        //    Compute Shader
        ////////////////////////////////////////
        //1 找到compute shader中所要使用的KernelID
        int k = ScaleImageComputeShader.FindKernel("CSMain");
        //2 设置贴图    参数1=kid  参数2=shader中对应的buffer名 参数3=对应的texture, 如果要写入贴图，贴图必须是RenderTexture并enableRandomWrite
        ScaleImageComputeShader.SetTexture(k, "Source", targeTexture2D);

        ScaleImageComputeShader.SetTexture(k, "Dst", rtDes);
        ScaleImageComputeShader.SetFloat("widthScale", widthScale);
        ScaleImageComputeShader.SetFloat("heightScale", heightScale);




        //Debug.Log("tex info width is " + texWidth + "  Height is " + texHeight);
        //3 运行shader  参数1=kid  参数2=线程组在x维度的数量 参数3=线程组在y维度的数量 参数4=线程组在z维度的数量
        ScaleImageComputeShader.Dispatch(k, dstWidth, dstHeight, 1);


        Texture2D jpg = new Texture2D(rtDes.width, rtDes.height, TextureFormat.ARGB32, false);
        //RenderTexture.active = rtDes;
        RenderTexture.active = rtDes;

        jpg.ReadPixels(new Rect(0, 0, rtDes.width, rtDes.height), 0, 0);
        jpg.Apply();
        RenderTexture.active = null;



        // SrcRawImage.texture = targeTexture2D;
        // DstRawImage.texture = jpg;

        Destroy(targeTexture2D);
        Destroy(rtDes);
        return jpg;


    }


    /// <summary>
    /// 给图片加边框和标题
    /// </summary>
    /// <param name="yearTex">附着在左上角的提示贴图</param>
    /// <param name="contents"></param>
    /// <param name="fileName"></param>
    /// <param name="size">返回图片原始尺寸</param>
    /// <returns></returns>
    Texture HandlePicture(Texture2D yearTex, string contents, string fileName, out Vector2 size)
    {
        byte[] bytes;

        bytes = File.ReadAllBytes(contents + "/" + fileName);

        //512,512参数只是临时，下面的apply应用后，会自动把图片变为原图尺寸
        Texture2D sourceTex = new Texture2D(512, 512);

        sourceTex.LoadImage(bytes);

        sourceTex.Apply();

        size.x = sourceTex.width;
        size.y = sourceTex.height;

        int texWidth = sourceTex.width + 2 * width;
        int texHeight = sourceTex.height + 2 * width + LableHeight;
        ////////////////////////////////////////
        //    RenderTexture
        ////////////////////////////////////////
        //1 新建RenderTexture
        RenderTexture rt = new RenderTexture(texWidth, texHeight, 24);
        //2 开启随机写入
        rt.enableRandomWrite = true;
        //3 创建RenderTexture
        rt.Create();

        ////////////////////////////////////////
        //    Compute Shader
        ////////////////////////////////////////
        //1 找到compute shader中所要使用的KernelID
        int k = ScaleImageComputeShader.FindKernel("CSMain1");
        //2 设置贴图    参数1=kid  参数2=shader中对应的buffer名 参数3=对应的texture, 如果要写入贴图，贴图必须是RenderTexture并enableRandomWrite
        ScaleImageComputeShader.SetTexture(k, "Result", rt);
        ScaleImageComputeShader.SetTexture(k, "Source", sourceTex);
        ScaleImageComputeShader.SetTexture(k, "YearTex", yearTex);
        ScaleImageComputeShader.SetInt("BorderWidth", width);
        ScaleImageComputeShader.SetInt("LableHeight", LableHeight);



        //Debug.Log("tex info width is " + texWidth + "  Height is " + texHeight);
        //3 运行shader  参数1=kid  参数2=线程组在x维度的数量 参数3=线程组在y维度的数量 参数4=线程组在z维度的数量
        ScaleImageComputeShader.Dispatch(k, texWidth, texHeight, 1);

        Destroy(sourceTex);

        return rt;


    }

   
    public void DestroyTexture()
    {
        foreach (Texture2D texture2D in Texs)
        {
            Destroy(texture2D);
        }
        Texs.Clear();
        Texs = null;
        Resources.UnloadUnusedAssets();
    }


    private void HandleTextureArry(List<Texture2D> texs)
    {

        if (texs == null || texs.Count == 0)
        {
            enabled = false;
            return;
        }

        if (SystemInfo.copyTextureSupport == CopyTextureSupport.None ||
            !SystemInfo.supports2DArrayTextures)
        {
            enabled = false;
            return;
        }
        TexArr = new Texture2DArray(texs[0].width, texs[0].width, texs.Count, TextureFormat.RGBA32, false, false);

        for (int i = 0; i < texs.Count; i++)
        {
             //Debug.Log(" index is" + i);
            try
            {
                Graphics.CopyTexture(texs[i], 0, 0, TexArr, i, 0);
            }
            catch (Exception e)
            {
                Debug.Log("index is" + i);
                throw e;
            }
            

        }

        TexArr.wrapMode = TextureWrapMode.Clamp;
        TexArr.filterMode = FilterMode.Bilinear;

        Debug.Log("HandleTextureArry End ===============>>>>>>>>>>>   TexArr Length is " + TexArr.depth);
    }

}



/// <summary>
/// 物品所属的大类
/// </summary>

public class ClassInfo
{
    /// <summary>
    /// 该类物品的名字
    /// </summary>
    public string ClassName;
    /// <summary>
    /// 该类物体细分下去的个数
    /// </summary>
    public int ClassCount;
    /// <summary>
    /// 细分下去物体的详细信息
    /// </summary>
    public List<ObjectInfo> ObjectInfos;


    public ClassInfo()
    {
        ObjectInfos = new List<ObjectInfo>();
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append("\r\n");
        sb.Append("\r\n");
        sb.Append("Years is  " + ClassName + "  \r\n");
        sb.Append("EventCount is  " + ClassCount + " \r\n");
        foreach (ObjectInfo yearsEvent in ObjectInfos)
        {
            sb.Append(yearsEvent.ToString());
        }
        sb.Append("\r\n");
        sb.Append("\r\n");
        return sb.ToString();
    }
}

/// <summary>
/// 物品信息
/// </summary>
public class ObjectInfo
{
    /// <summary>
    /// 所属的类
    /// </summary>
    public string BelongsClass;

    public string ObjectName;

    /// <summary>
    /// 物品信息排列在集合的位置索引
    /// </summary>
    public int IndexPos;

    /// <summary>
    /// 物品索引集合，如果有多个表示一个物品有多张图片展示
    /// </summary>
    public List<int> PictureIndes;

    /// <summary>
    /// 物品的描述
    /// </summary>
    public string Describe;

    /// <summary>
    /// 物品的描述的文件路径
    /// </summary>
    public string DescribePath;


    /// <summary>
    /// 该物品下的图片描述集合，存的是路径
    /// </summary>
    public List<string> PicturesPath;

    /// <summary>
    /// 描述该物品的的视频
    /// </summary>
    public string ObjectVideo;
    /// <summary>
    /// 每个key对应着每个物品图片的源长和源宽
    /// </summary>
    public Dictionary<int, Vector2> PictureSizeInfo;

    public ObjectInfo()
    {
        PicturesPath = new List<string>();
        PictureIndes = new List<int>();
        PictureSizeInfo = new Dictionary<int, Vector2>();
    }

    public void AddPictureInfo(int index, Vector2 size)
    {
        if (!PictureIndes.Contains(index))
            PictureIndes.Add(index);
        PictureSizeInfo.Add(index, size);
    }
    public override string ToString()
    {


        StringBuilder sb = new StringBuilder();

        sb.Append("\r\n");
        sb.Append("\r\n");
        sb.Append("Years is  " + BelongsClass + " \r\n");
        sb.Append("IndexPos is  " + IndexPos + " \r\n");
        sb.Append("DescribePath is  " + DescribePath + " \r\n");
        foreach (string s in PicturesPath)
        {
            sb.Append("PicturesPath is " + s + "\r\n");
        }
        sb.Append("YearEventVideo is  " + ObjectVideo + "\r\n");
        sb.Append("\r\n");
        sb.Append("\r\n");

        return sb.ToString();
    }
}

/// <summary>
/// 人物信息
/// </summary>
public class PersonInfo
{
    /// <summary>
    /// 人物名字
    /// </summary>
    public string PersonName;
    /// <summary>
    /// 人物图片路径
    /// </summary>
    public string PicturePath;
    /// <summary>
    /// 人物图片所在数组中的索引
    /// </summary>
    public int PictureIndex;
    /// <summary>
    /// 人物描述文本的路径
    /// </summary>
    public string DescribeFilePath;
    /// <summary>
    /// 人物的描述
    /// </summary>
    public string Describe;

    /// <summary>
    /// 介绍角色的视频  
    /// </summary>
    public string YearEventVideo;

    /// <summary>
    /// 头像
    /// </summary>
    public Sprite headTex;


}

/// <summary>
/// 公司介绍信息
/// </summary>
public class CompanyInfo
{
    public List<Texture2D> TexInfo;

    public List<string> VideoInfo;
}
