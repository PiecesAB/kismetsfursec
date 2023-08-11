using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extension4DSpace;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshRend4DExtension : MonoBehaviour
{
    public Mesh4DData meshData;
    public float w;
    public Matrix4x4 extraRot = Matrix4x4.identity; // orthogonal

    [Header("XY XZ XW YZ YW ZW")]
    public float[] rotVels = new float[6];
    public bool useRotVel;

    private float[] prevRotVels = new float[6];
    private Matrix4x4 prevRotMatrix = Matrix4x4.identity;

    [HideInInspector]
    public List<int>[] tris;
    [HideInInspector]
    public List<Vector3> verts;
    public List<Vector3> normals;

    [HideInInspector]
    public MeshFilter mf;
    [HideInInspector]
    public MeshRenderer mr;

    private void Start()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();

        tris = new List<int>[meshData.materialList.Length];
        for (int i = 0; i < tris.Length; ++i) { tris[i] = new List<int>(); }
        mr.materials = meshData.materialList;
        verts = new List<Vector3>();
        normals = new List<Vector3>();
    }

    private Vector4 GetPoint(int t, int i)
    {
        return extraRot*(meshData.points[meshData.tetras[t][i]]) + new Vector4(0,0,0,w);
    }

    private void RotVelUpdate()
    {
        bool allSame = true;
        for (int i = 0; i < 6; ++i) { if (rotVels[i] != prevRotVels[i]) { allSame = false; break; } }
        if (!allSame)
        {
            float c0 = Mathf.Cos(rotVels[0]); float s0 = Mathf.Sin(rotVels[0]);
            float c1 = Mathf.Cos(rotVels[1]); float s1 = Mathf.Sin(rotVels[1]);
            float c2 = Mathf.Cos(rotVels[2]); float s2 = Mathf.Sin(rotVels[2]);
            float c3 = Mathf.Cos(rotVels[3]); float s3 = Mathf.Sin(rotVels[3]);
            float c4 = Mathf.Cos(rotVels[4]); float s4 = Mathf.Sin(rotVels[4]);
            float c5 = Mathf.Cos(rotVels[5]); float s5 = Mathf.Sin(rotVels[5]);
            prevRotMatrix = new Matrix4x4(
                new Vector4(c0, -s0, 0, 0),
                new Vector4(s0, c0, 0, 0),
                new Vector4(0, 0, c5, -s5),
                new Vector4(0, 0, s5, c5)
            );
            prevRotMatrix *= new Matrix4x4(
                new Vector4(c1, 0, -s1, 0),
                new Vector4(0, c4, 0, -s4),
                new Vector4(s1, 0, c1, 0),
                new Vector4(0, s4, 0, c4)
            );
            prevRotMatrix *= new Matrix4x4(
                new Vector4(c2, 0, 0, -s2),
                new Vector4(0, c3, -s3, 0),
                new Vector4(0, s3, c3, 0),
                new Vector4(s2, 0, 0, c2)
            );

        }

        extraRot *= prevRotMatrix;

        prevRotVels = (float[])rotVels.Clone();
    }

    private void Update()
    {
        if (useRotVel) { RotVelUpdate(); }

        mf.mesh.Clear();
        verts.Clear();
        normals.Clear();
        for (int i = 0; i < tris.Length; ++i) { tris[i].Clear(); }
        Tetra4D newTet = null;
        for (int t = 0; t < meshData.tetras.Length; ++t)
        {
            newTet = new Tetra4D(GetPoint(t, 0), GetPoint(t, 1), GetPoint(t, 2), GetPoint(t, 3));
            newTet.Draw(this, meshData.tetraMaterials[t]);
        }

        mf.mesh.SetVertices(verts);
        mf.mesh.subMeshCount = mr.materials.Length;
        for (int i = 0; i < mr.materials.Length; ++i)
        {
            mf.mesh.SetTriangles(tris[i], i, true, 0);
        }
        mf.mesh.SetNormals(normals);
        
    }
}
