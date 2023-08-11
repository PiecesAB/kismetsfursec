using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericProjectileShooter : MonoBehaviour
{
    [System.Serializable]
    public enum MovementType
    {
        Linear
    }

    [Header("If this script's name is GenericProjectileShooter, use a subclass!")]
    public float[] angularPositions;
    public float[] angularVelocities;
    public MovementType[] movementTypes;
    public GameObject[] bulletObjects;

    public float offsetFromCenter;
    public float delayBetweenShots;

    public void Fire()
    {
        StartCoroutine(FireReal());
    }

    private IEnumerator FireReal()
    {
        for (int i = 0; i < bulletObjects.Length; ++i)
        {
            GameObject newbullet = Instantiate(bulletObjects[i],
                                               transform.position + offsetFromCenter*(new Vector3(Mathf.Cos(angularPositions[i] * Mathf.Deg2Rad),Mathf.Sin(angularPositions[i] * Mathf.Deg2Rad))),
                                               Quaternion.AngleAxis(angularPositions[i], Vector3.forward),
                                               transform.parent);
            if (delayBetweenShots > 0f)
            {
                yield return new WaitForSeconds(delayBetweenShots);
            }
        }
        yield return null;
    }

    private void Update()
    {
        if (Time.timeScale > 0)
        {
            for (int i = 0; i < bulletObjects.Length; ++i)
            {
                if (movementTypes[i] == MovementType.Linear)
                {
                    angularPositions[i] += angularVelocities[i] * Time.deltaTime;
                }
            }
        }
        ExtraUpdate();
    }

    public virtual void ExtraUpdate()
    {
        //default
    }
}
