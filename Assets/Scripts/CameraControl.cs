using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraControl : MonoBehaviour
{

    public float DistanceMultiple = 1;

    private Vector3 originalPos;

    private Vector3 m_lastMousePosition;

    private Camera MainCamera;

    public float PanSensitivity = 5f;
    // Start is called before the first frame update
    void Start()
    {
        originalPos = this.transform.position;
        MainCamera = this.GetComponent<Camera>();
        TextureInstanced.Instance.DragAction += Pan;
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    /// <summary>
    /// 移动相机
    /// </summary>
    public void MoveForward(float multiple)
    {
        
        float value = DistanceMultiple * multiple;

        Vector3 pos = originalPos;

        pos.z += value;
        if (pos.z < -10) pos.z = -10;
        if (pos.z > 100000) pos.z = 100000;
        this.transform.position = pos;

       
    }
    public void MoveLeftRight(float multiple)
    {

        float value = DistanceMultiple * multiple;

        Vector3 pos = originalPos;

        pos.z += value;
        if (pos.z < -10) pos.z = -10;
        if (pos.z > 100000) pos.z = 100000;
        this.transform.position = pos;


    }

    private void OnDestroy()
    {
        TextureInstanced.Instance.DragAction -= Pan;
    }
    private void Pan(PointerEventData data)
    {
        Vector3 delta = data.delta;

        delta = delta / Mathf.Sqrt(MainCamera.pixelHeight * MainCamera.pixelHeight + MainCamera.pixelWidth * MainCamera.pixelWidth);

        delta *= PanSensitivity;

        delta = MainCamera.cameraToWorldMatrix.MultiplyVector(delta);
        MainCamera.transform.position += delta;
        originalPos.x += delta.x;
        originalPos.y += delta.y;

        m_lastMousePosition = Input.mousePosition;
    }

   
}
