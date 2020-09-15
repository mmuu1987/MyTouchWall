using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPositionInTexture : MonoBehaviour
{
    public Texture2D Texture2D;


    private Color[,] colors;

    // Start is called before the first frame update
    void Start()
    {
      

        GetPos(Texture2D,1.5f,10000);


    }

    private List<Vector2> GetPos(Texture2D tex,float maxAlpha,int count)
    {

        var clos = tex.GetPixels();

        int texWidth = tex.width;

        int texHeight = tex.height;

        colors = new Color[tex.width, tex.height];

        int k = 0;
        for (int i = 0; i < clos.Length; i++)
        {

            if (i >= (k + 1) * tex.width)
            {
                k++;
            }

            colors[i - (k * tex.height), k] = clos[i];


        }

        List<Vector2> posList = new List<Vector2>();

       

        for (int i = 0; i < count; i++)
        {
            Vector2 pos;
            while (true)
            {
                int width = Random.Range(0, tex.width);
                int height = Random.Range(0, tex.height);

                Color col = colors[width, height];

                float val = col.r + col.g + col.b;

                pos = new Vector2(width, height) - new Vector2(texWidth/2, texHeight/2);

                    if (val >= maxAlpha)
                    {
                       
                        posList.Add(pos.normalized);
                        break;
                    }
                
            }

           
        }

        return posList;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
