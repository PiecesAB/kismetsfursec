using UnityEngine;
using System.Collections;

public class BossChp2SpikeCrucherScript : MonoBehaviour {

    /*public float delay;
    public float rechargeWhileUp;

    private bool raising;
	// Use this for initialization
	void Start () {
        raising = false;
	}
	
    public void DropSpike()
    {
        float plrX = GameObject.FindGameObjectWithTag("Player").transform.position.x;
        transform.parent.position = new Vector3(Mathf.Clamp(plrX, 144, 672), transform.parent.position.y, transform.parent.position.z);
        GetComponent<ConstantForce2D>().enabled = false;
    }


    public IEnumerator Raise()
    {
        
        raising = true;
        yield return new WaitForSeconds(delay);
        GetComponent<ConstantForce2D>().enabled = true;
        yield return new WaitForSeconds(rechargeWhileUp);
        raising = false;
    }

     private IEnumerator FFOnPriv()
    {
        FindObjectOfType<BossStats>().defense = 99;
        Rigidbody2D plrBody = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        Vector2 vc = plrBody.gameObject.transform.position - (transform.position + new Vector3(0, 20, 0));
        if (Vector3.Distance(GameObject.FindGameObjectWithTag("Player").transform.position, transform.position + new Vector3(0, 20, 0)) < 200)
        {
            plrBody.velocity = vc.normalized * 3000;
        }
        yield return new WaitForSeconds(0.6f);
        transform.parent.gameObject.GetComponent<CircleCollider2D>().enabled = true;
        transform.parent.gameObject.GetComponent<BossChp2ForceField>().on = true;
    }
    public void TurnFFOn()
    {
        
        StartCoroutine(FFOnPriv());
    }
    public void TurnFFOff()
    {
        FindObjectOfType<BossStats>().defense = 25;
        transform.parent.gameObject.GetComponent<CircleCollider2D>().enabled = false;
        transform.parent.gameObject.GetComponent<BossChp2ForceField>().on = false;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (!raising && other.isTrigger == false)
        {
            raising = true;
            StartCoroutine(Raise());
        }

    }


	// Update is called once per frame
	void Update () {
	if (GetComponentInChildren<BossChp2ChainScript>())
        {
            GetComponentInChildren<BossChp2ChainScript>().posStart = new Vector3(transform.position.x,-600);
            GetComponentInChildren<BossChp2ChainScript>().posEnd = transform.position;
        }
	}*/
}
