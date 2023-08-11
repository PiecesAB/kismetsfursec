using UnityEngine;
using System.Collections;
using Extension4DSpace;

public class SpecialAttackHopfFibration : MonoBehaviour {

    public Transform origSphere;
    public float angle1;
    public float angle2;
    private Transform[] sphereObjs = new Transform[16];

    private Vector4[] r;

    private void Start()
    {
        for (int i = 0; i < 16; ++i)
        {
            GameObject ns = Instantiate(origSphere.gameObject, transform.position, Quaternion.identity);
            ns.transform.SetParent(transform);
            sphereObjs[i] = ns.transform;
        }
        Destroy(origSphere.gameObject);
        r = Fakerand.Basis4D();
    }

    private void Update () {
        if (Time.timeScale == 0) { return; }
        //Vector4 s = Fakerand.UnitGlome(true);
        int i = 0;
        for (float t = 0; t < Mathf.PI*2 - 0.001f; t += Mathf.PI/8f)
        {
            Vector4 s0 = Mathf.Cos(t) * r[0] + Mathf.Sin(t) * r[1];
            Vector3 s = s0.StereographicTo3D();
            sphereObjs[i].position = Vector3.MoveTowards(sphereObjs[i].position, transform.position + 80 * s, 2f*Time.timeScale);
            ++i;
        }
        //print("(" + s.x.ToString() + ", " + s.y.ToString() + ", " + s.z.ToString() + ", " + s.w.ToString() + ")");
        Vector4[] oldr = (Vector4[])r.Clone();
        float cos1 = Mathf.Cos(angle1*Time.timeScale); float sin1 = Mathf.Sin(angle1 * Time.timeScale);
        float cos2 = Mathf.Cos(angle2 * Time.timeScale); float sin2 = Mathf.Sin(angle2 * Time.timeScale);
        r[0] = oldr[0] * cos1 + oldr[2] * sin1;
        r[2] = -oldr[0] * sin1 + oldr[2] * cos1;
        r[1] = oldr[1] * cos2 + oldr[3] * sin2;
        r[3] = -oldr[1] * sin2 + oldr[3] * cos2;
    }
}
