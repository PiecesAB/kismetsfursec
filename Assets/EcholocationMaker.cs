using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcholocationMaker : MonoBehaviour
{
    public GameObject echoBulletSample;
    //public float angle = 150f;
    public int amount = 12;
    public float waitTime = 2f;
    //public float baseSpeed = 96f;

    void Start()
    {
        StartCoroutine(MainLoop());
    }

    public void MakeCircle(Vector3 pos, float speed)
    {
        for (float t = 0f; t <= 359.999f; t += 360f / amount)
        {
            float rt = t * Mathf.Deg2Rad;
            Vector2 norm = new Vector2(Mathf.Cos(rt)/* *(facingLeft?-1:1) */, Mathf.Sin(rt));
            GameObject echoBullet = Instantiate(echoBulletSample, pos, Quaternion.identity);
            echoBullet.GetComponent<primDecorationMoving>().v = (speed * norm); //+ vel;
            echoBullet.GetComponent<primDeleteInTime>().t = 6.6666666f;
            echoBullet.SetActive(true);

        }
    }

    private IEnumerator MainLoop()
    {
        while (true)
        {
            Encontrolmentation plr = LevelInfoContainer.GetActiveControl();
            if (plr == null) { goto wait; }

            //Vector2 vel = plr.GetComponent<BasicMove>().fakePhysicsVel;
            bool facingLeft = plr.GetComponent<SpriteRenderer>().flipX;

            //for (float t = 90f - angle*0.5f; t <= 90f + angle *0.5f + 0.001f; t += angle/(amount - 1))
            MakeCircle(plr.transform.position + new Vector3(0f, 9f), 96f);

            foreach (BatObstacle b in BatObstacle.all)
            {
                SkinnedMeshRenderer smr = b.GetRenderer();
                if (smr != null && smr.isVisible)
                {
                    MakeCircle(b.transform.position, 64f);
                }
            }

            wait:
            yield return new WaitForSeconds(waitTime);
        }
    }
}
