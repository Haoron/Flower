using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CreateDefaultLut : MonoBehaviour {

	// Use this for initialization
	void Start () {
        CreateIdentityLut(32);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void CreateIdentityLut(int dim)
    {
        Color[] newC = new Color[dim * dim * dim];
        float oneOverDim = 1.0f / (1.0f * dim - 1.0f);
        for (int i=0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                for (int k = 0; k < dim; k++)
                {
                    newC[i + (j * dim) + (k * dim * dim)] = new Color((i * 1.0f) * oneOverDim, Mathf.Abs(((k * 1.0f) * oneOverDim) - 1.0f), (j * 1.0f) * oneOverDim, 1.0f);
                }
            }
        }

        Texture2D tex2D = new Texture2D(dim * dim, dim, TextureFormat.RGB24, false);
        tex2D.SetPixels(newC);
        tex2D.Apply();
        var bytes = tex2D.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../3DLUT" + (dim * dim).ToString() + "ll" + ".png", bytes);
        print(Application.dataPath);
    }
}
