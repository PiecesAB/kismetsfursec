using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotatingGun : MonoBehaviour {


    public GameObject bulletPrefab;

    public float howManyShotsPerRotation;
    public int howManyInACircle = 4;
    public bool sineMovement;
    public float cycleSpeed;
    public float bulletSpeed;
    public float bulletDamageMultiplier;

    public List<float> shotPlaces;
    public int indx;
    public float nextPlace;


	// Use this for initialization
	void Start () {
        for (float i = 0; i < 360; i+=360/howManyShotsPerRotation)
        {
            shotPlaces.Add(i);
        }
        indx = 0;
        nextPlace = shotPlaces[indx];
        print(shotPlaces[shotPlaces.Count-1]);
	}
	
    public void Shoot(float rot)
    {
        
        for (float i = rot; i < 360+rot; i += 360/howManyInACircle)
        {
            GameObject b = (GameObject) Instantiate(bulletPrefab,transform.position,Quaternion.AngleAxis((i+90)%360,Vector3.forward));
            if (b.GetComponent<NormalBulletBehavior>())
            {
                NormalBulletBehavior n = b.GetComponent<NormalBulletBehavior>();
                b.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(Fakerand.Single(), 1,1);
                n.speed = bulletSpeed;
                n.damageMultiplier = bulletDamageMultiplier;
            }
            if (b.GetComponent<ShootingStarBehavior>())
            {
                ShootingStarBehavior n = b.GetComponent<ShootingStarBehavior>();
                b.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(0, 0, 1);
                n.speed = bulletSpeed;
                n.damageMultiplier = bulletDamageMultiplier;
            }
        }
    }

	// Update is called once per frame
	void Update () {
        
        transform.rotation = Quaternion.identity;
        transform.Rotate(new Vector3(0, 0, (float)((cycleSpeed * DoubleTime.ScaledTimeSinceLoad)%360)));
        float rot = (transform.rotation.eulerAngles.z+12)%360;
        if ((rot+720)%360 >= shotPlaces[indx] && rot % 360 < shotPlaces[indx]+90)
        {
            bool okee = true;
            if (shotPlaces.Count - 2 < indx && okee)
            {
                okee = false;
                indx = 0;
                Shoot(shotPlaces[shotPlaces.Count - 1]);
            }
            if (shotPlaces.Count - 2 >= indx && okee)
            {
                okee = false;
                indx += 1;
                Shoot(shotPlaces[indx-1]);
            }
            
        }
	}
}
