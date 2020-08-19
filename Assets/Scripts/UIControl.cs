using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;


public enum UIState
{
    
    None,
    /// <summary>
    /// 关闭界面状态
    /// </summary>
    Close,
    /// <summary>
    /// 公司介绍
    /// </summary>
    CompanyIntroduction,
    /// <summary>
    /// 私享传家
    /// </summary>
    PrivateHeirs,
    /// <summary>
    /// 卓越风采
    /// </summary>
    OutstandingStyle
}
/// <summary>
/// 大屏互动UI控制器
/// </summary>
public class UIControl : MonoBehaviour
{
    /// <summary>
    /// 荣誉墙按钮
    /// </summary>
    public Button HonorWallBtn;

    /// <summary>
    /// 公司介绍按钮
    /// </summary>
    public Button CompanyIntroductionBtn;

    /// <summary>
    /// 关闭公司介绍，私享传家  所共用的界面
    /// </summary>
    public Button CloseButton;
    /// <summary>
    /// 私享传家按钮
    /// </summary>
    public Button PrivateHeirsBtn;

    public Button OutstandingStyleBtn;

    public Button Btn2000_2009;

    public Button Btn2010_2019;

    public Button Btn2020;


    public MultiDepthMotion MultiDepthMotion;

    /// <summary>
    /// 荣誉墙
    /// </summary>
    public RectTransform HonorWall;

    public UIStateMachine _Machine;

    public Dictionary<UIState, UIStateFSM> DicUI;

    public Sprite HonorWallBtnLeft;
    public Sprite HonorWallBtnRight;

    private void Awake()
    {



        Screen.SetResolution(7680, 3240, true, 60);

       
    }
    
	// Use this for initialization
	void Start () 
    {
       
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
