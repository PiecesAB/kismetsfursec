using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowMaker : MonoBehaviour
{
    public BulletData[] snowflakes;

    private Vector2 startPos;
    private Vector2 endPos;
    private float directionDegrees;
    private Vector2 norm;
    private float length;

    public float spacing = 12f;
    public int rowCount = 5;
    public float waitBetweenRows = 0.3f;
    public float waitForNextSeries = 1.5f;
    public float activeRadius = 640f;

    private Transform camTrans;

    private List<BulletObject> myBullets = new List<BulletObject>();

    void Start()
    {
        EdgeCollider2D e = GetComponent<EdgeCollider2D>();
        startPos = e.points[0]; endPos = e.points[1];
        length = (endPos - startPos).magnitude;
        norm = (endPos - startPos).normalized;
        directionDegrees = Mathf.Atan2(-norm.x, norm.y) * Mathf.Rad2Deg;
        camTrans = Camera.main.transform;

        StartCoroutine(MainLoop());
    }

    private void MakeBullet(BulletData d, Vector3 pos)
    {
        BulletObject b = new BulletObject();

        d.TransferBasicInfo(b);

        b.UpdateTransform(pos, 0, d.scale);
        b.originPosition = pos;
        b.startingDirection = directionDegrees;

        myBullets.Add(b);
        BulletRegister.Register(ref b, d.material, d.sprite.texture);
    }

    private void MakeBulletRow(BulletData d, bool halfShift)
    {
        if ((camTrans.position - transform.position).magnitude > activeRadius) { return; }

        float factor = d.scale.x + spacing;
        for (float x = (d.scale.x*0.5f) + ((halfShift)?(factor*0.5f):0); x <= length - (d.scale.x*0.5f); x += factor)
        {
            MakeBullet(d, (Vector2)transform.position + startPos + x * norm);
        }
    }

    private IEnumerator MainLoop()
    {
        while (this != null) //?????
        {
            BulletData sf = snowflakes[Fakerand.Int(0, snowflakes.Length)];
            bool halfShift = false;
            for (int i = 0; i < rowCount; ++i)
            {
                MakeBulletRow(sf, halfShift);

                if (i == rowCount - 1) { break; }
                yield return new WaitForSeconds(waitBetweenRows);
                halfShift = !halfShift;
            }
            yield return new WaitForSeconds(waitForNextSeries);
        }
        yield return null;
    }

    private void ClearAllBullets()
    {
        foreach (BulletObject b in myBullets)
        {
            BulletRegister.MarkToDestroy(b);
        }
    }

    private void OnDestroy()
    {
        ClearAllBullets();
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (Door1.levelComplete) { ClearAllBullets(); return; }

        for (int i = 0; i < myBullets.Count; ++i)
        {
            BulletObject b = myBullets[i];
            if (!BulletRegister.IsRegistered(b)) // if it has been deleted from the register
            {
                myBullets.RemoveAt(i); --i; continue;
            }
        }
    }
}
