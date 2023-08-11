using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTeleportingTrick : MonoBehaviour
{

    private FollowThePlayer ftp;
    private Vector2 oldPos;

    [Header("Teleports when arriving at these screens...")]
    public List<Vector2> teleCurr;
    [Header("But only when the last screen was this.")]
    public List<Vector2> teleLast;
    [Header("The act of the above teleportation goes here.")]
    public List<Vector2> teleTo;
    [Header("(10000,10000) is special universal vector")]
    public bool nothing;

    void Start()
    {
        ftp = GetComponent<FollowThePlayer>();
        oldPos = ftp.perScreenPosition;
    }

    void Update()
    {
        if (Time.timeScale > 0 && ftp.perScreenPosition != oldPos)
        {
            int i = teleCurr.FindIndex((a) => (a == ftp.perScreenPosition));
            bool lastRight = (i == -1)?(false):(teleLast[i] == oldPos || teleLast[i] == new Vector2(10000,10000));
            if (i != -1 && lastRight)
            {
                Vector2 dif = teleTo[i] - teleCurr[i];
                Vector2 scrollDist = new Vector2(320f, 216f);
                if (ftp.customScrollDistance != Vector2.zero) { scrollDist = ftp.customScrollDistance; }
                Vector3 totalmvt = new Vector3(dif.x * scrollDist.x, dif.y * scrollDist.y);
                if (ftp.rotateScene)
                {
                    float c = Mathf.Cos(ftp.rotateScene.goalRotation * Mathf.Deg2Rad);
                    float s = Mathf.Sin(ftp.rotateScene.goalRotation * Mathf.Deg2Rad);
                    totalmvt = new Vector3(c * totalmvt.x - s * totalmvt.y, s * totalmvt.x + c * totalmvt.y);
                }

                ftp.SetTransformPosition(transform.position + totalmvt);
                
                if (ftp.target) { ftp.target.position += totalmvt; }
                foreach (SuperRay sr in FindObjectsOfType<SuperRay>())
                {
                    sr.transform.position += totalmvt;
                }
                ftp.perScreenPosition += dif;
            }

            oldPos = ftp.perScreenPosition;
        }
    }
}
