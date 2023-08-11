using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretMoveUpandDown : MonoBehaviour {

    public float rotateSpeed;
    public float movementWidth;
    public GameObject bullet;
    public int bulletNumber;
    public float bulletSpeed;
    public Color bulletColor;
    public int ShootTimesPerSpin;
    public float startPos;
    public float bulletTimingOffset;

    private Vector3 origPos;
    private float currentRot;
    private List<float> shootTimes = new List<float> { };
    // Use this for initialization
    void Start() {
        currentRot = startPos;
        origPos = transform.position;
        for (float i = 0; i < 360; i = i + 360 / ShootTimesPerSpin)
        {
            shootTimes.Add((i + bulletTimingOffset) % 360);
        }
    }

    void Fire()
    {
        for (float i = 0; i < 360; i = i + 360 / bulletNumber)
        {
            GameObject newBullet = Instantiate(bullet, transform.position, Quaternion.AngleAxis(i + transform.rotation.eulerAngles.z, Vector3.forward)) as GameObject;
            if (newBullet.GetComponent<NormalBulletBehavior>())
            {
                newBullet.GetComponent<NormalBulletBehavior>().speed = bulletSpeed;
            }
            if (newBullet.GetComponent<HomingBulletBehavior>())
            {
                newBullet.GetComponent<HomingBulletBehavior>().speed = bulletSpeed;
            }
            Destroy(newBullet, 30f);
        }
    }


    // Update is called once per frame
    void Update() {
        if (!(Time.timeScale == 2))
        {
            transform.position = origPos + new Vector3(0, (Mathf.PingPong((currentRot / 11.25f), 180f / 11.25f) * 2 * movementWidth) - movementWidth, 0);
        currentRot += rotateSpeed;

        currentRot = (currentRot + 720) % 360;
        if (shootTimes.Contains(currentRot))
        {
            Fire();
        }
    }
    }


}
