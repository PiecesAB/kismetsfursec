using UnityEngine;
using System.Collections;

public class BGWallCrush : MonoBehaviour {

    public bool setMatVector;
    public Vector4 fallMatVectorSet;
    public float gravityMult;
    public bool fallEnabled;
    public float damage;
    public AudioClip creak;
    public AudioClip crush;
    public Vector2 wallSizeForRaycast;

	void Start () {
        transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        if (setMatVector)
        {
            foreach (Transform wall in transform)
            {
                if (wall.GetComponent<Renderer>() != null)
                {
                    wall.GetComponent<Renderer>().material.SetVector("_FakeZ", fallMatVectorSet);
                }
            }
        }

	}

    void OnTriggerEnter2D(Collider2D man)
    {
        if (man.GetComponent<KHealth>() != null)
        {
            Destroy(GetComponent<Collider2D>());
            transform.rotation = Quaternion.AngleAxis(-1f,Vector3.right);
            fallEnabled = true;
            AudioSource aso = GetComponent<AudioSource>();
            aso.clip = creak;
            aso.Play();
        }
    }
	
	void Update () {
        
        if (fallEnabled && Time.timeScale > 0f)
        {
            float newAngle = transform.localEulerAngles.x + Time.timeScale * gravityMult * 0.14166666f * Mathf.Sin(transform.localEulerAngles.x * Mathf.Deg2Rad);
            transform.rotation = Quaternion.AngleAxis(newAngle, Vector3.right);
            bool hit = false;
            if (newAngle <=315f)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll((Vector2)transform.position + new Vector2(-wallSizeForRaycast.x / 2f, wallSizeForRaycast.y * Mathf.Cos(newAngle * Mathf.Deg2Rad)), Vector2.right, wallSizeForRaycast.x, 1049344);
                if (hits.Length > 0)
                {
                    hit = true;
                    foreach (RaycastHit2D man in hits)
                    {
                        if (man.transform.GetComponent<KHealth>() != null)
                        {
                            man.transform.GetComponent<KHealth>().ChangeHealth(-damage,"wall");
                            man.transform.GetComponent<BasicMove>().AddBlood(man.point, Quaternion.AngleAxis(-90f, Vector3.forward));
                        }
                    }
                }
            }

            if (newAngle <= 270f || hit)
            {
                transform.rotation = Quaternion.AngleAxis(Mathf.Max(270f,newAngle), Vector3.right);
                fallEnabled = false;
                AudioSource aso = GetComponent<AudioSource>();
                aso.Stop();
                aso.clip = crush;
                aso.Play();
                FindObjectOfType<FollowThePlayer>().vibSpeed = 2.4f;
            }
        }
	}
}
