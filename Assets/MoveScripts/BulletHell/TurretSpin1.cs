using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretSpin1 : MonoBehaviour {

    public float rotateSpeed;
    public GameObject bullet;
    public int bulletNumber;
    public float bulletSpeed;
    public Color bulletColor;
    public int ShootTimesPerSpin;
    public bool counterclockwise;
    public AudioClip fireSound;

    private float currentRot;
    private List<float> shootTimes = new List<float> { };
	// Use this for initialization
	void Start () {
        currentRot = 0;
        for (float i = 0; i < 360; i=i+360/ShootTimesPerSpin)
        {
                shootTimes.Add(i);
        }
	}
	
    void Fire()
    {
        for (float i = 0; i < 360; i = i+ 360/bulletNumber)
        {
            GameObject newBullet = Instantiate(bullet, transform.position, Quaternion.AngleAxis(i+currentRot, Vector3.forward)) as GameObject;
            if (newBullet.GetComponent<NormalBulletBehavior>())
            {
                newBullet.GetComponent<NormalBulletBehavior>().speed = bulletSpeed;
            }
            if (newBullet.GetComponent<HomingBulletBehavior>())
            {
                newBullet.GetComponent<HomingBulletBehavior>().speed = bulletSpeed;
            }
            if (GetComponent<AudioSource>())
            {
                GetComponent<AudioSource>().PlayOneShot(fireSound);
            }
            Destroy(newBullet, 30f);
        }
    }


	// Update is called once per frame
	void Update () {
        if (!(Time.timeScale == 2)) 
        if (counterclockwise)
        {
            transform.Rotate(new Vector3(0, 0, -rotateSpeed));
            currentRot -= rotateSpeed;
        }
        if (!counterclockwise)
        {
            transform.Rotate(new Vector3(0, 0, rotateSpeed));
            currentRot += rotateSpeed;
        }
        

        currentRot = (currentRot+720) % 360;
        if (shootTimes.Contains(currentRot))
        {
            Fire();
        }
	}
}
