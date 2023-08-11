using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;




public class JustRainbowText : ModifyMeshes {
    private float floatI;
	// Use this for initialization
	void Start () {
	
	}


    public override void ModifyMesh(VertexHelper vh)
    {
        List<UIVertex> vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);



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


            vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);
    }


        // Update is called once per frame
        void FixedUpdate () {
        GetComponent<Text>().text = GetComponent<Text>().text + "﻿";
    }


    public override void ModifyMesh(Mesh meshText)
    {

    }
}
