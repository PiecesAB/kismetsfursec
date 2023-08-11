using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FollowThePlayer))]
public class SpecialWrapping : MonoBehaviour
{
    private FollowThePlayer ftp;
    public bool horizontal;
    public bool vertical;
    public bool horizontalTwist;
    public bool verticalTwist;
    public bool createCollisionCopies = false;

    public Camera sampleWrapCam;

    // left [0], right [1], up [2], down [3], UL [4], UR [5], DL [6], DR [7]
    private Camera[] cameras = new Camera[8];
    private bool[] camFlippedX = new bool[8] { false, false, false, false, false, false, false, false };
    private bool[] camFlippedY = new bool[8] { false, false, false, false, false, false, false, false };
    private List<GameObject> colliderCopies = new List<GameObject>();
    private int currColliderCopy = 0;

    private Camera mainCam;
    private Transform mainCamTr;

    private Camera NewCamera(Vector3 pos)
    {
        Camera newCam = Instantiate(sampleWrapCam.gameObject).GetComponent<Camera>();
        newCam.gameObject.SetActive(true);
        newCam.transform.SetParent(transform);
        newCam.transform.localPosition = pos;
        return newCam;
    }

    private void FlipY(int i)
    {
        Camera c = cameras[i];
        c.transform.localScale = new Vector3(c.transform.localScale.x, -c.transform.localScale.y, c.transform.localScale.z);
        Matrix4x4 m = c.projectionMatrix;
        m.m11 = -m.m11;
        c.projectionMatrix = m;
        camFlippedY[i] = !camFlippedY[i];
    }

    private void FlipX(int i)
    {
        Camera c = cameras[i];
        c.transform.localScale = new Vector3(-c.transform.localScale.x, c.transform.localScale.y, c.transform.localScale.z);
        Matrix4x4 m = c.projectionMatrix;
        m.m00 = -m.m00;
        c.projectionMatrix = m;
        camFlippedX[i] = !camFlippedX[i];
    }

    public void Initialize()
    {
        mainCam = GetComponent<Camera>();
        mainCamTr = mainCam.transform;
        ftp = GetComponent<FollowThePlayer>();

        for (int i = 0; i < cameras.Length; ++i)
        {
            if (cameras[i] == null || cameras[i].gameObject == null) { continue; }
            Destroy(cameras[i].gameObject);
        }

        if (horizontal)
        {
            cameras[0] = NewCamera(new Vector3(-320f, 0f, 0f));
            cameras[1] = NewCamera(new Vector3(320f, 0f, 0f));
            if (horizontalTwist)
            {
                FlipY(0);
                FlipY(1);
            }
        }
        if (vertical)
        {
            cameras[2] = NewCamera(new Vector3(0f, -216f, 0f));
            cameras[3] = NewCamera(new Vector3(0f, 216f, 0f));
            if (verticalTwist)
            {
                FlipX(2);
                FlipX(3);
            }
        }
        if (horizontal && vertical)
        {
            cameras[4] = NewCamera(new Vector3(-320f, 216f, 0f));
            cameras[5] = NewCamera(new Vector3(320f, 216f, 0f));
            cameras[6] = NewCamera(new Vector3(-320f, -216f, 0f));
            cameras[7] = NewCamera(new Vector3(320f, -216f, 0f));
            if (horizontalTwist)
            {
                for (int i = 4; i < 8; ++i)
                {
                    FlipY(i);
                }
            }
            if (verticalTwist)
            {
                for (int i = 4; i < 8; ++i)
                {
                    FlipX(i);
                }
            }
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < cameras.Length; ++i)
        {
            if (cameras[i] == null || cameras[i].gameObject == null) { continue; }
            Destroy(cameras[i].gameObject);
        }
    }

    // this assumes the player has no other colliders than the exactly four of edgecollider type.
    private void SetColliderCopies(GameObject plr)
    {
        EdgeCollider2D[] plrEdges = plr.GetComponents<EdgeCollider2D>();

        for (int ci = 0; ci < 8; ++ci)
        {
            Camera c = cameras[ci]; if (c == null) { continue; }

            if (colliderCopies.Count <= currColliderCopy) {
                GameObject newFakeCollider = new GameObject("Wrapping Collider Copy");
                newFakeCollider.layer = 11;
                colliderCopies.Add(newFakeCollider);
            }
            GameObject o = colliderCopies[currColliderCopy];
            EdgeCollider2D[] thisEdges = o.GetComponents<EdgeCollider2D>();
            if (thisEdges.Length == 0)
            {
                thisEdges = new EdgeCollider2D[4];
                for (int i = 0; i < 4; ++i)
                {
                    EdgeCollider2D newEdge = o.AddComponent<EdgeCollider2D>();
                    thisEdges[i] = newEdge;
                }
            }

            o.transform.position = c.transform.TransformPoint(plr.transform.position - mainCamTr.position);
            float zRot = plr.transform.eulerAngles.z;
            if (camFlippedX[ci]) { zRot *= -1; } if (camFlippedY[ci]) { zRot *= -1; }
            o.transform.eulerAngles = Vector3.forward * zRot;
            o.transform.localScale = plr.transform.localScale;
            for (int i = 0; i < 4; ++i)
            {
                thisEdges[i].offset = plrEdges[i].offset;
                thisEdges[i].points = new Vector2[2] { plrEdges[i].points[0], plrEdges[i].points[1] };
            }
            ++currColliderCopy;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (ftp.target == null) { return; }
        for (int i = 0; i < LevelInfoContainer.allPlayersInLevel.Count; ++i)
        {
            GameObject p = LevelInfoContainer.allPlayersInLevel[i];
            if (p == null || p.GetComponent<PrimPlayableCharacter>() == null) { continue; }
            // reposition the player when they wrap around.
            float xdif = p.transform.position.x - mainCamTr.position.x;
            float ydif = p.transform.position.y - mainCamTr.position.y;
            if (horizontal && Mathf.Abs(xdif) > 160f)
            {
                p.transform.position += new Vector3(320f, 0f, 0f) * (-Mathf.Sign(xdif));
                if (horizontalTwist)
                {
                    p.transform.position = new Vector3(p.transform.position.x, mainCamTr.position.y - ydif, p.transform.position.z);
                    p.transform.localScale = new Vector3(p.transform.localScale.x, -p.transform.localScale.y, p.transform.localScale.z);
                    ydif = p.transform.position.y - mainCamTr.position.y;
                }
            }
            if (vertical && Mathf.Abs(ydif) > 108f)
            {
                p.transform.position += new Vector3(0f, 216f, 0f) * (-Mathf.Sign(ydif));
                if (verticalTwist)
                {
                    p.transform.position = new Vector3(mainCamTr.position.x - xdif, p.transform.position.y, p.transform.position.z);
                    p.transform.localScale = new Vector3(-p.transform.localScale.x, p.transform.localScale.y, p.transform.localScale.z);
                }
            }

            if (createCollisionCopies) { SetColliderCopies(p); }
        }

        currColliderCopy = 0;
    }
}
