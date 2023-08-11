using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mesh4DData : MonoBehaviour
{
    public Vector4[] points;

    [System.Serializable]
    public struct BoxedArray
    {
        public int[] main;
        public int this[int i]
        {
            get { return main[i]; }
            set { main[i] = value; }
        }
        public BoxedArray(int[] newMain)
        {
            main = newMain;
        }
    }

    public BoxedArray[] tetras;
    public int[] tetraMaterials;
    public Material[] materialList;

    public enum PremadeShape
    {
        None, Tesseract, Duocylinder
    }

    public PremadeShape premadeShape = PremadeShape.None;

    private void Start()
    {
        switch (premadeShape)
        {
            case PremadeShape.Tesseract:
                points = new Vector4[16];
                Func<int, int, float> parity = (val, part) =>
                {
                    return ((val & part) == 0)?-0.5f:0.5f;
                };
                for (int i = 0; i < 16; ++i)
                {
                    points[i] = new Vector4(parity(i, 8), parity(i, 4), parity(i, 2), parity(i, 1));
                }
                tetras = new BoxedArray[48];
                tetraMaterials = new int[48];
                int currTet = 0;
                Func<int[], bool> cube = (a) =>
                {
                    tetras[currTet] =   new BoxedArray(new int[4] { a[2], a[5], a[4], a[6] });
                    tetras[currTet+1] = new BoxedArray(new int[4] { a[2], a[5], a[6], a[7] });
                    tetras[currTet+2] = new BoxedArray(new int[4] { a[2], a[5], a[0], a[1] });
                    tetras[currTet+3] = new BoxedArray(new int[4] { a[2], a[5], a[3], a[7] });
                    tetras[currTet+4] = new BoxedArray(new int[4] { a[2], a[5], a[1], a[3] });
                    tetras[currTet+5] = new BoxedArray(new int[4] { a[2], a[5], a[0], a[4] });
                    for (int i = 0; i < 6; ++i)
                    {
                        tetraMaterials[currTet + i] = currTet / 6;
                    }
                    currTet += 6;
                    return true;
                };
                cube(new int[8] { 0, 2, 4, 6, 8, 10, 12, 14 }); // kata
                cube(new int[8] { 1, 3, 5, 7, 9, 11, 13, 15 }); // ana
                cube(new int[8] { 0, 1, 4, 5, 8, 9, 12, 13 }); //back
                cube(new int[8] { 2, 3, 6, 7, 10, 11, 14, 15 }); //front
                cube(new int[8] { 0, 1, 2, 3, 8, 9, 10, 11 }); //down
                cube(new int[8] { 4, 5, 6, 7, 12, 13, 14, 15 }); //up
                cube(new int[8] { 0, 1, 2, 3, 4, 5, 6, 7 }); //left
                cube(new int[8] { 8, 9, 10, 11, 12, 13, 14, 15 }); //right
                break;
            case PremadeShape.Duocylinder:
                int resolution = 12;
                int matLen = resolution * (resolution - 2) * 6;
                int matLenHalf = resolution * (resolution - 2) * 3;

                points = new Vector4[resolution * resolution];
                tetras = new BoxedArray[matLen];
                tetraMaterials = new int[matLen];

                //make the points
                for (int i = 0; i < resolution; ++i)
                {
                    float a = 2 * i * Mathf.PI / resolution;
                    Vector4 center = new Vector4(0, 0.5f * Mathf.Sin(a), 0, 0.5f * Mathf.Cos(a));
                    for (int j = 0; j < resolution; ++j)
                    {
                        float b = 2 * j * Mathf.PI / resolution;
                        points[resolution * i + j] = center + new Vector4(0.5f * Mathf.Cos(b), 0, 0.5f * Mathf.Sin(b), 0);
                    }
                }

                //cylinder 1
                for (int i = 0; i < resolution; ++i)
                {
                    int o = (i + 1) % resolution;
                    for (int j = 2; j < resolution; ++j)
                    {
                        int[] top = new int[3] { resolution * i, resolution * i + j - 1, resolution * i + j};
                        int[] bot = new int[3] { resolution * o, resolution * o + j - 1, resolution * o + j};
                        int tind = (((j - 2) * resolution) + i) * 3;
                        tetras[tind] = new BoxedArray(new int[4] { top[1], bot[0], bot[1], bot[2] });
                        tetras[tind + 1] = new BoxedArray(new int[4] { top[1], top[2], bot[0], bot[2] });
                        tetras[tind + 2] = new BoxedArray(new int[4] { top[0], top[1], top[2], bot[0] });
                        tetraMaterials[tind] = tetraMaterials[tind + 1] = tetraMaterials[tind + 2] = 0;
                    }
                }

                //cylinder 2
                for (int i = 1; i < resolution + 1; ++i)
                {
                    int o = (i == resolution)?1:(i + 1);
                    for (int j = 2; j < resolution; ++j)
                    {
                        int[] top = new int[3] { i - 1, i + resolution*(j-1) - 1, i + resolution*j - 1 };
                        int[] bot = new int[3] { o - 1, o + resolution * (j - 1) - 1, o + resolution*j - 1 };
                        int tind = matLenHalf + (((j - 2) * resolution) + (i-1)) * 3;
                        tetras[tind] = new BoxedArray(new int[4] { top[1], bot[0], bot[1], bot[2] });
                        tetras[tind + 1] = new BoxedArray(new int[4] { top[1], top[2], bot[0], bot[2] });
                        tetras[tind + 2] = new BoxedArray(new int[4] { top[0], top[1], top[2], bot[0] });
                        tetraMaterials[tind] = tetraMaterials[tind + 1] = tetraMaterials[tind + 2] = 1;
                    }
                }

                break;
            case PremadeShape.None:
            default:
                break;
        }
    }
}
