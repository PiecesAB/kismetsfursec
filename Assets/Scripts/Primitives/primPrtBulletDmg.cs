using UnityEngine;
using System.Collections;

public class primPrtBulletDmg : MonoBehaviour {

    public float damage;
    public string damageReason = "energy bullet";

	// Use this for initialization
	void Start () {
	
	}

    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<KHealth>())
            {
                other.GetComponent<KHealth>().ChangeHealth(-damage,damageReason);
                other.GetComponent<BasicMove>().AddBlood(other.transform.position, Quaternion.AngleAxis(Fakerand.Single() * 360f, Vector3.forward));

            }
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
