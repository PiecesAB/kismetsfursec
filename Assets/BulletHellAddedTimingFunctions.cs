using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class BulletDataAdtl
{
    public float deletTime;
    public Color color;
    public float speed;
    public float torque;
    public float acceleration;
    public float changeInTorque;
    public bool isAccelerationMultiplicative;
    public bool isTorqueSineWave;
    public float damage;
}

public class BulletHellAddedTimingFunctions : MonoBehaviour {

    public enum AdditionalBehaviors
    {
        MakeBulletsHomeEveryXSeconds,
        MakeBulletsSplitEveryXSecondsInDirectionTable,
        MakeBulletsMoveForXAndPauseForY,
        None
    };

    public GameObject testBullet;
    public BulletDataAdtl bulletData;
    public AdditionalBehaviors adtlBehavior;
    public bool startsItself;
    public bool keepOldBullets;
    public bool recursive;
    public float X;
    public float Y;
    public float[] directionTable;

    /*public IEnumerator Succ()
    {
            bool a = false;
            if (adtlBehavior == AdditionalBehaviors.MakeBulletsHomeEveryXSeconds)
            {
                yield return new WaitForSeconds(X);
                a = true;
                Transform[] ts = gameObject.GetComponentsInChildren<Transform>();
                if (ts != null && ts.Length > 0)
                {
                    foreach (Transform t in ts)
                    {
                        if (t != transform)
                        {
                            CloneHomings(testBullet, t.position);
                            if (!keepOldBullets)
                            {
                                Destroy(t.gameObject);
                            }
                        }
                    }
                }
            }

            if (adtlBehavior == AdditionalBehaviors.MakeBulletsSplitEveryXSecondsInDirectionTable)
            {
                yield return new WaitForSeconds(X);
                a = true;
                Transform[] ts1 = gameObject.GetComponentsInChildren<Transform>();
            if (recursive)
            {
                Succ();
            }
                if (ts1 != null)
                {
                    foreach (Transform t in ts1)
                    {
                        if (t != transform)
                        {
                            bool didThat1 = false;
                            if (t.gameObject.GetComponent<BulletHellObject1>())
                            {
                                foreach (float f in directionTable)
                                {
                                    CloneNormal(testBullet, t.position, t.gameObject.GetComponent<BulletHellObject1>().startingDirection+f);
                                }
                                didThat1 = true;
                            }
                            if (!didThat1)
                            {
                                foreach (float f in directionTable)
                                {
                                    CloneNormal(testBullet, t.position, f);
                                }
                            }

                            if (!keepOldBullets)
                            {
                                Destroy(t.gameObject);
                            }
                        }
                    }
                }
            }

            if (adtlBehavior == AdditionalBehaviors.MakeBulletsMoveForXAndPauseForY)
            {
                yield return new WaitForSeconds(X);
                a = true;
                Transform[] ts2 = gameObject.GetComponentsInChildren<Transform>();
                if (ts2 != null)
                {
                    foreach (Transform t in ts2)
                    {
                        if (t != transform)
                        {
                            if (t.gameObject.GetComponent<BulletHellObject1>())
                            {
                                CloneNoSpeed(testBullet, t.position, t.gameObject.GetComponent<BulletHellObject1>().startingDirection);
                            }
                            if (!keepOldBullets)
                            {
                                Destroy(t.gameObject);
                            }
                        }
                    }
                }
                if (directionTable[0] != 0)
                {
                    GetComponent<BulletHellMakerFunctions>().thisMakesItsOwnBullets = false;
                }
                yield return new WaitForSeconds(Y);
                a = true;
                ts2 = gameObject.GetComponentsInChildren<Transform>();
                if (ts2 != null)
                {
                    foreach (Transform t in ts2)
                    {
                        if (t != transform)
                        {
                            if (t.gameObject.GetComponent<BulletHellObject1>())
                            {
                                CloneNormal(testBullet, t.position, t.gameObject.GetComponent<BulletHellObject1>().startingDirection);
                            }
                            if (!keepOldBullets)
                            {
                                Destroy(t.gameObject);
                            }
                        }
                    }
                }
                GetComponent<BulletHellMakerFunctions>().thisMakesItsOwnBullets = true;
            }



            if (!a)
            {
                yield return new WaitForFixedUpdate();
            }
            yield return 1;
    }


    // Use this for initialization
    void Start () {
	if (startsItself)
        {
            StartCoroutine(Succ());
        }
	}

    public void ExtraFire()
    {
       StartCoroutine(Succ());
    }

    // Update is called once per frame
    void Update () {
	
	}


    public void CloneHomings(GameObject bulletObj, Vector3 pos)
    {
       

            BulletHellObject1 b = bulletObj.GetComponent<BulletHellObject1>();
            b.originPosition = pos;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        float t1 = Mathf.Atan2(player.transform.position.y - pos.y, player.transform.position.x - pos.x);
        float rot = (t1 * Mathf.Rad2Deg) % 360;


        b.startingDirection = rot;
            b.startingVelocity = bulletData.speed;
            b.startingTorque = bulletData.torque;
            b.acceleration = bulletData.acceleration;
            b.changeInTorque = bulletData.changeInTorque;
            b.deletTime = bulletData.deletTime;
            b.isTorqueSineWave = bulletData.isTorqueSineWave;
            b.isAccelerationMultiplicative = bulletData.isAccelerationMultiplicative;
            b.atWhatDistanceFromCenterIsAHit = 8;
            b.damage = bulletData.damage;
            bulletObj.GetComponent<SpriteRenderer>().color = bulletData.color;
            GameObject b2 = (GameObject)Instantiate(bulletObj, pos, Quaternion.identity);
        if (!recursive)
        {
            b2.transform.parent = transform.parent;
        }
        else
        {
            b2.transform.parent = transform;
        }
    }

    public void CloneNormal(GameObject bulletObj, Vector3 pos, float rot)
    {


        BulletHellObject1 b = bulletObj.GetComponent<BulletHellObject1>();
        b.originPosition = pos;
        b.startingDirection = rot;
        b.startingVelocity = bulletData.speed;
        b.startingTorque = bulletData.torque;
        b.acceleration = bulletData.acceleration;
        b.changeInTorque = bulletData.changeInTorque;
        b.deletTime = bulletData.deletTime;
        b.isTorqueSineWave = bulletData.isTorqueSineWave;
        b.isAccelerationMultiplicative = bulletData.isAccelerationMultiplicative;
        b.atWhatDistanceFromCenterIsAHit = 8;
        b.damage = bulletData.damage;
        bulletObj.GetComponent<SpriteRenderer>().color = bulletData.color;
        GameObject b2 = (GameObject)Instantiate(bulletObj, pos, Quaternion.identity);
        if (!recursive)
        {
            b2.transform.parent = transform.parent;
        }
        else
        {
            b2.transform.parent = transform;
        }
    }

    public void CloneNoSpeed(GameObject bulletObj, Vector3 pos, float rot)
    {

        BulletHellObject1 b = bulletObj.GetComponent<BulletHellObject1>();
        b.originPosition = pos;
        b.startingDirection = rot;
        b.startingVelocity = 0;
        b.startingTorque = bulletData.torque;
        b.acceleration = bulletData.acceleration;
        b.changeInTorque = bulletData.changeInTorque;
        b.deletTime = bulletData.deletTime;
        b.isTorqueSineWave = bulletData.isTorqueSineWave;
        b.isAccelerationMultiplicative = bulletData.isAccelerationMultiplicative;
        b.atWhatDistanceFromCenterIsAHit = 8;
        b.damage = bulletData.damage;
        bulletObj.GetComponent<SpriteRenderer>().color = bulletData.color;
        GameObject b2 = (GameObject)Instantiate(bulletObj, pos, Quaternion.identity);

            b2.transform.parent = transform;

    }
    */
}
