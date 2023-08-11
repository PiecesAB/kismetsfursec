using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;




public class ModifyOtherText : ModifyMeshes {

    public bool rainbow;
    public float wave;
    public float shake;
    private float floatI;
	// Use this for initialization
	void Start () {
	
	}


    public override void ModifyMesh(VertexHelper vh)
    {
        List<UIVertex> vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);

        if (rainbow)
        {
            for (int ii = 0; ii < vertexList.Count; ii = ii + 6)
            {




                for (int j = 0; j < 6; j++)
                {
                    UIVertex hi = vertexList[ii + j];
                    floatI = ii;
                    hi.color = Color.HSVToRGB(Mathf.PingPong((float)((floatI / 160) + DoubleTime.ScaledTimeSinceLoad), 1), 0.65f, 1);
                    vertexList[ii + j] = hi;
                }


            }
        }

        if (wave != 0f)
        {
            for (int ii = 0; ii < vertexList.Count; ii = ii + 6)
            {




                for (int j = 0; j < 6; j++)
                {
                    UIVertex hi = vertexList[ii + j];
                    floatI = ii;
                    hi.position += new Vector3(wave * Mathf.Cos(Time.realtimeSinceStartup * 6.2f + (ii / 25f)), wave * Mathf.Sin(Time.realtimeSinceStartup * 6.2f + (ii / 25f)), 0);
                    vertexList[ii + j] = hi;
                }


            }
        }

        if (shake != 0f)
        {
            for (int ii = 0; ii < vertexList.Count; ii = ii + 6)
            {


                Vector3 af = new Vector3((Fakerand.Single() - 0.5f) * shake, (Fakerand.Single() - 0.5f) * shake, 0);

                for (int j = 0; j < 6; j++)
                {
                    UIVertex hi = vertexList[ii + j];
                    floatI = ii;
                    hi.position += af;
                    vertexList[ii + j] = hi;
                }


            }
        }


        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);
    }


        // Update is called once per frame
        void Update () {
        GetComponent<Text>().text = GetComponent<Text>().text + "﻿";
        GetComponent<Text>().text = GetComponent<Text>().text.TrimEnd('﻿');
    }


    public override void ModifyMesh(Mesh meshText)
    {

    }
}
