using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[ExecuteInEditMode]
public class VectorTest : MonoBehaviour
{

    public Transform Target;



    public Material material;

    public Texture2D Texture2D;

    private List<Vector3> positionList = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        //CreatPlane();

        positionList = Common.GetPos(Texture2D, 1.5f, 1000,10f);

        foreach (Vector2 vector2 in positionList)
        {
            float height = Random.Range(-1f, 1f);

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);

            go.GetComponent<MeshRenderer>().material = material;

            go.name = "quad";
            go.transform.position = new Vector3(vector2.x,height,vector2.y);
        }
    }

    

    public void SetMatArg()
    {
        material.SetFloat("rot_x", Target.eulerAngles.x);
        material.SetFloat("rot_y", Target.eulerAngles.y);
    }


    private void OnGUI()
    {
        if (GUI.Button(new Rect(0f, 0f, 100f, 100f), "test"))
        {
            Debug.Log(Camera.main.worldToCameraMatrix);
        }
    }
    // Update is called once per frame
    void Update()
    {
       // LookTarget();
        SetMatArg();
    }
}
