using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuddleAnim : MonoBehaviour
{
    public float damageMult = 15f;
    public bool rainbowColor;

    private SkinnedMeshRenderer smr;

    private bool absorbing = false;

    private const double _pi = 3.14159265358979;

    private float crand1;
    private float crand2;

    void Start()
    {
        smr = GetComponent<SkinnedMeshRenderer>();
        absorbing = false;
        crand1 = Fakerand.Single();
        crand2 = Fakerand.Single();
    }

    Mesh MakeMeshClone(Mesh orig)
    {
        Mesh newmesh = new Mesh();
        newmesh.vertices = orig.vertices;
        newmesh.triangles = orig.triangles;
        newmesh.uv = orig.uv;
        newmesh.normals = orig.normals;
        newmesh.colors = orig.colors;
        newmesh.tangents = orig.tangents;
        return newmesh;
    }

    public IEnumerator Absorb(GameObject plr)
    {
        if (smr && !absorbing)
        {
            absorbing = true;
            Transform plrt = plr.transform;
            Mesh mt = new Mesh();
            smr.BakeMesh(mt);
            smr.sharedMesh = MakeMeshClone(mt);

            List<float> displacements = new List<float>(smr.sharedMesh.vertices.Length);
            for (int i = 0; i < smr.sharedMesh.vertices.Length; i++)
            {
                displacements.Add(Fakerand.Single(-4f, 4f));
            }

            GetComponent<AudioSource>().Play();

            for (int i = 30; i > 0; i--)
            {
                if (plr.activeInHierarchy)
                {
                    List<Vector3> newverts = new List<Vector3>(smr.sharedMesh.vertices.Length);
                    for (int j = 0; j < smr.sharedMesh.vertices.Length; j++)
                    {
                        newverts.Add(Vector3.Lerp(smr.sharedMesh.vertices[j], transform.InverseTransformPoint(plrt.position), 1f / i) + new Vector3(0f, displacements[j], 0f));
                    }

                    smr.sharedMesh.SetVertices(newverts);
                }
                else
                {
                    break;
                }
                yield return new WaitForEndOfFrame();
            }

            if (!plr.activeInHierarchy)
            {
                GetComponent<AudioSource>().Stop();
            }
            else if (plr.GetComponent<KHealth>() && plr.GetComponent<BasicMove>())
            {
                BasicMove bm = plr.GetComponent<BasicMove>();
                plr.GetComponent<KHealth>().ChangeHealth(-damageMult * bm.Damage, "acid puddle");
                bm.Damage += 3;
            }

            Destroy(smr);
            Destroy(gameObject, 1f);
        }
        
        yield return new WaitForEndOfFrame();
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.gameObject && c.gameObject.CompareTag("Player"))
        {
            GetComponent<Collider2D>().enabled = false;
            StartCoroutine(Absorb(c.gameObject));
        }
    }

    void Update()
    {
        double t = DoubleTime.ScaledTimeSinceLoad + crand1;

        if (smr)
        {
            if (!absorbing)
            {
                smr.SetBlendShapeWeight(0, (float)DoubleTime.DoublePong(t * 200.0, 100.0));
                smr.SetBlendShapeWeight(1, (float)DoubleTime.DoublePong(t * 200.0 + 100.0, 100.0));
            }

            if (rainbowColor)
            {
                smr.material.color = Color.HSVToRGB((float)((t * 0.2 + crand2) % 1.0), 1f, 1f);
            }
        }

        /*
        Bezier bezTest = new Bezier();
        bezTest.offset = transform.position;
        bezTest.loop = true;

        bezTest.points.Add(new Bezier.Point(new Vector3(0, 8), new Vector3(0,0), new Vector3(0, 8)));
        bezTest.points.Add(new Bezier.Point(new Vector3(8, 8)));
        bezTest.points.Add(new Bezier.Point(new Vector3(8, -8)));
        bezTest.points.Add(new Bezier.Point(new Vector3(-24, -8), new Vector3(-24, -8), new Vector3(-24, 0)));

        bezTest.DrawInSceneView();
        */

    }
}
