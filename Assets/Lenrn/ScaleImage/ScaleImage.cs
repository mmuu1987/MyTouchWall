using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScaleImage : MonoBehaviour
{


    public RawImage SrcRawImage;

    public RawImage DstRawImage;

    public Texture2D TargeTexture2D;

    public Texture2D TiTexture2D;

    public ComputeShader ScaleImageComputeShader;

    public int width = 10;//边框像素单位宽度

    public int LableHeight = 128;//文字占有高度像素单位

    /// <summary>
    /// 缩放的宽度倍数
    /// </summary>
    public float WidthScale = 0.5f;
    /// <summary>
    /// 缩放的高度倍数
    /// </summary>
    public float HeightScale = 0.5f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Texture2D ScaleImageUserRt(Texture targeTexture2D, int dstWidth, int dstHeight)
    {


        float widthScale = dstWidth *1f / targeTexture2D.width;
        float heightScale = dstHeight*1f / targeTexture2D.height;

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
        ScaleImageComputeShader.SetFloat( "widthScale", widthScale);
        ScaleImageComputeShader.SetFloat( "heightScale", heightScale);
       



        //Debug.Log("tex info width is " + texWidth + "  Height is " + texHeight);
        //3 运行shader  参数1=kid  参数2=线程组在x维度的数量 参数3=线程组在y维度的数量 参数4=线程组在z维度的数量
        ScaleImageComputeShader.Dispatch(k, dstWidth, dstHeight, 1);

       
        Texture2D jpg = new Texture2D(rtDes.width, rtDes.height, TextureFormat.RGBA32, false);
        //RenderTexture.active = rtDes;
        RenderTexture.active = rtDes;
      
        jpg.ReadPixels(new Rect(0, 0, rtDes.width, rtDes.height), 0, 0);
        jpg.Apply();
        
        RenderTexture.active = null;



        //SrcRawImage.texture = targeTexture2D;
        //DstRawImage.texture = jpg;

        Destroy(targeTexture2D);
        Destroy(rtDes);
        return jpg;


    }


    /// <summary>
    /// 给图片加边框和标题，标题写的是年份，并且缩放图片规格，返回字节数据
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


    private void OnGUI()
    {
        if (GUI.Button(new Rect(0f, 0f, 100f, 100f), "test"))
        {
            FileInfo fileInfo = new FileInfo(Application.streamingAssetsPath + "/广东博物馆/宝玉石标本/东北软玉雕“渔获”/东北软玉雕“渔获”.jpg");
           
            Vector2 size;
            Texture tex = HandlePicture(TiTexture2D,fileInfo.DirectoryName, fileInfo.Name, out size);

            ScaleImageUserRt(tex, 512, 512);
           // Resources.UnloadUnusedAssets();
        }
    }
}
