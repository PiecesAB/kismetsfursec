using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public abstract class ModifyMeshes : MonoBehaviour, IMeshModifier
{
    public abstract void ModifyMesh(Mesh meshText);
    public abstract void ModifyMesh(VertexHelper vh);
}

public class MeshChangerLol : ModifyMeshes {

    /*[Serializable]
    public class FirstDimArray
    {
        public int[] switches;
    }

    [Serializable]
    public class SecondDimArray
    {
        public FirstDimArray[] places;
    }

    public SecondDimArray[] superLargeListOfDialogueEffectSpecifications;*/

    
    public bool trembleText;
    public bool waveText;
    public bool rainbowifyText;
    public bool flashText;
    public bool obfusText;
    public List<int> bitMasks;

    private float rand1;
    private float rand2;
    private float rand3;
    private float floatI;
    private int stage;

    private bool begintrembleText;
    private bool beginwaveText;
    private bool beginrainbowifyText;
    private bool beginflashText;
    private bool beginobfusText;

    public override void ModifyMesh(VertexHelper vh)
    {
        List<UIVertex> vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);

        //print(vertexList.Count);

        beginrainbowifyText = rainbowifyText;
        beginwaveText = waveText;
        begintrembleText = trembleText;
        beginflashText = flashText;
        beginobfusText = obfusText;
        int tempInt = 0;
        if (trembleText)
        {
            tempInt += 1;
        }
        if (waveText)
        {
            tempInt += 2;
        }
        if (rainbowifyText)
        {
            tempInt += 4;
        }
        if (flashText)
        {
            tempInt += 8;
        }
        if (obfusText)
        {
            tempInt += 16;
        }
        bitMasks.Add(tempInt);

        for (int ii = 0; ii < vertexList.Count; ii = ii + 6)
        {
            if (GetComponent<Text>().text.Substring(ii/6,1) == "\0")
            {
                ii = 60000000;
            }
            //This is where the huge array actually changes its stuff using the switches...

            stage = GetComponent<DefaultTalkingEngine>().i;
            /*foreach (FirstDimArray a in superLargeListOfDialogueEffectSpecifications[stage].places)
            {
                if (a.switches[0] == ii / 6)
                {
                    if (a.switches[1] == 1)
                    {
                        trembleText = !trembleText;
                    }
                    if (a.switches[1] == 2)
                    {
                        waveText = !waveText;
                    }
                    if (a.switches[1] == 3)
                    {
                        rainbowifyText = !rainbowifyText;
                    }
                    if (a.switches[1] == 4)
                    {
                        obfusText = !obfusText;
                    }
                }
            } */



            //Carry on then, next letter
           /* for (int j = 0; j < 6; j++)
            {
                UIVertex hi = vertexList[ii + j];
                hi.position = new Vector3(hi.position.x/2,hi.position.y/2,hi.position.z);
                vertexList[ii + j] = hi;
            }*/
            if (bitMasks.Count > ii/6)
            {
                float sizeRatio = GetComponent<Text>().fontSize / GetComponent<DefaultTalkingEngine>().originalFontSizeDoNotChange;


                if ( (bitMasks[ii/6] & 1) == 1 ) //shake
            {

                    
                rand1 = Fakerand.Single(-sizeRatio, sizeRatio);
                rand2 = Fakerand.Single(-sizeRatio, sizeRatio);

                for (int j = 0; j < 6; j++)
                {
                    UIVertex hi = vertexList[ii + j];
                    hi.position = hi.position + (new Vector3(rand1, rand2, 0));
                    vertexList[ii + j] = hi;
                }

            }

            if ((bitMasks[ii / 6] & 2) == 2) //wave
            {


                for (int j = 0; j < 6; j++)
                {
                    UIVertex hi = vertexList[ii + j];
                    hi.position = hi.position + (new Vector3(2.5f * (float)System.Math.Cos(1f * ((ii) + (10 * (DoubleTime.UnscaledTimeRunning))))*sizeRatio, 0f/*0f * (float)System.Math.Sin(1f * ((ii) + (10 * (DoubleTime.UnscaledTimeRunning))))*sizeRatio*/, 0));
                    vertexList[ii + j] = hi;
                }

            }

                if ((bitMasks[ii / 6] & 4) == 4) //rainbowify
                {
                    for (int j = 0; j < 6; j++)
                    {
                        UIVertex hi = vertexList[ii + j];
                        floatI = ii;
                        hi.color = Color.Lerp(hi.color, Color.HSVToRGB((float)((floatI / (60*sizeRatio) + DoubleTime.UnscaledTimeRunning) % 1), 1, 1), 0.5f);
                        vertexList[ii + j] = hi;
                    }
                }

                if ((bitMasks[ii / 6] & 8) == 8) //flash
                {


                    for (int j = 0; j < 6; j++)
                    {
                        UIVertex hi = vertexList[ii + j];
                        floatI = ii;
                        hi.color = Color.Lerp(Color.white,GetComponent<DefaultTalkingEngine>().flashTextColor,(float)System.Math.Sin(1.7f*Mathf.PI*DoubleTime.UnscaledTimeRunning)/2+0.5f);
                        vertexList[ii + j] = hi;
                    }
                }

                if ((bitMasks[ii / 6] & 16) == 16) //obfuscate
                {


                    for (int j = 0; j < 6; j++)
                    {
                        UIVertex hi = vertexList[ii + j];
                        floatI = ii;
                        hi.uv0 = hi.uv0 + Fakerand.UnitCircle() * 0.4f;
                        hi.uv1 = hi.uv1 + Fakerand.UnitCircle() * 0.4f;
                        vertexList[ii + j] = hi;
                    }
                }


            }

        }

        rainbowifyText = beginrainbowifyText;
        waveText = beginwaveText;
        trembleText = begintrembleText;
        flashText = beginflashText;
        obfusText = beginobfusText;

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);

    }

    public override void ModifyMesh(Mesh meshText)
    {
        
    }
}
