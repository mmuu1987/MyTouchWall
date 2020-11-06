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

    public Dictionary<UIState, UIStateFSM> DicUI;

    public CameraControl CameraControl;

    public Slider Slider;

    private void Awake()
    {
        Screen.SetResolution(7680, 3240, true, 60);
    }
    
	// Use this for initialization
	void Start () 
    {
        Slider.onValueChanged.AddListener((arg0 =>
        {
            CameraControl.MoveForward(arg0);
        }));

       

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
