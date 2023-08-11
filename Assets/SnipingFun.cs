using UnityEngine;
using System.Collections;

public class SnipingFun : MonoBehaviour {

    public GameObject aimTargetObject;
    public LineRenderer laserLine;
    public Sprite normalTarget;
    public Sprite closerTarget;
    public Sprite aboutToFireTarget;
    public float damage;

    public bool IWouldntChangeAnythingBelowThis;

    public Transform targetPlayer;
    public Vector3 currentLocalTargetPosition;
    public float aimDistanceFromTarget;
    public bool targetPlayerVisible;
    public bool aboutToFire;

    private Vector3 zeroVect = new Vector3(0, 0, 0);

	// Use this for initialization
	void Start () {
        currentLocalTargetPosition = Vector3.zero;
        targetPlayer = GameObject.FindGameObjectWithTag("Player").transform;
        laserLine.sortingLayerName = "FG";
	}

    void moveTarget()
    {
        aimTargetObject.transform.position = Vector3.SmoothDamp(aimTargetObject.transform.position, targetPlayer.position, ref zeroVect, 0.6f, 50000);
        Vector3 dir2 = aimTargetObject.transform.position - transform.position;
        float angle = Mathf.Atan2(dir2.y, dir2.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle+90, Vector3.forward);
        if ((angle+810)%360 < 180)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
        Vector3[] lol = { Vector3.zero, transform.parent.InverseTransformPoint(aimTargetObject.transform.position) };
        laserLine.SetPositions(lol);
    }

    public IEnumerator chargeAndFire(Vector2 dir)
    {
        yield return new WaitForSeconds(0.35f);
        RaycastHit2D ray = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), new Vector2(dir.normalized.x, dir.normalized.y), transform.parent.InverseTransformPoint(aimTargetObject.transform.position).magnitude+30,(1<<8)+(1<<20));
        
        if (ray.transform == targetPlayer)
        {
            if (targetPlayer.gameObject.GetComponent<KHealth>())
            {
                targetPlayer.gameObject.GetComponent<KHealth>().ChangeHealth(-damage,"sniper");
                BasicMove bm = targetPlayer.gameObject.GetComponent<BasicMove>();
                if (bm)
                {
                  bm.AddBlood(ray.point, Quaternion.LookRotation(new Vector3(dir.x, dir.y), Vector3.up));
                    bm.AddBlood(ray.point, Quaternion.LookRotation(new Vector3(dir.x, dir.y), Vector3.up));
                    targetPlayer.gameObject.GetComponent<AudioSource>().PlayOneShot(bm.spikeTouchSound);
                }
            }
        }
        //Camera.main.transform.position = Camera.main.transform.position + (new Vector3(dir.normalized.x,dir.normalized.y) * 30);
        RaycastHit2D ray2 = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), new Vector2(dir.normalized.x, dir.normalized.y), 10000 /*transform.parent.InverseTransformPoint(aimTargetObject.transform.position).magnitude*/, ~((1 << 20)+(1<<8)));
        Vector3[] positions = { Vector3.zero, transform.parent.InverseTransformPoint(aimTargetObject.transform.position) };
        Color omg = Color.HSVToRGB(Fakerand.Single(), 1, 1);
        laserLine.SetPositions(positions);
        laserLine.SetColors(omg,omg);
        laserLine.SetWidth(3, 20);
        for (float i = 1; i <= 24; i++)
        {
            yield return new WaitForSeconds(0.025f);
            omg = Color.HSVToRGB(Fakerand.Single(), 1,1);
            laserLine.SetColors(omg, omg);
            laserLine.SetWidth(Mathf.Lerp(3,0,i/24), Mathf.Lerp(20, 0, i / 24));
        }
        laserLine.SetWidth(1.42f,1.42f);
        aboutToFire = false;
    }


    // Update is called once per frame
    void Update() {
        if (targetPlayer && !aboutToFire)
        {
            Vector3 dir = targetPlayer.position - transform.position;
            targetPlayerVisible = false;
            if (Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y), new Vector2(dir.normalized.x, dir.normalized.y), 320).transform == targetPlayer && dir.magnitude >32)
            {
                targetPlayerVisible = true;
                aimDistanceFromTarget = (targetPlayer.position - aimTargetObject.transform.position).magnitude;
            }
            if (!targetPlayerVisible)
            {
                SpriteRenderer cool = aimTargetObject.GetComponent<SpriteRenderer>();
                cool.color = new Color(1, 1, 1, Mathf.Clamp01(cool.color.a - 0.05f));
                laserLine.SetColors(new Color(1, 1, 1, Mathf.Clamp01(cool.color.a*2 - 0.05f)/2), new Color(1, 1, 1, 0));
            }
            if (targetPlayerVisible && aimDistanceFromTarget > 50)
            {
                SpriteRenderer cool = aimTargetObject.GetComponent<SpriteRenderer>();
                cool.color = new Color(1, 1, 1, Mathf.Clamp01(cool.color.a + 0.2f));
                laserLine.SetColors(new Color(1, 1, 1, Mathf.Clamp01(cool.color.a * 2 + 0.2f) / 2), new Color(1, 1, 1, 0));
                cool.sprite = normalTarget;
                moveTarget();
                
            }
            if (targetPlayerVisible && aimDistanceFromTarget <= 50 && aimDistanceFromTarget > 14)
            {
                SpriteRenderer cool = aimTargetObject.GetComponent<SpriteRenderer>();
                cool.color = new Color(1, 1, 0, Mathf.Clamp01(cool.color.a + 0.2f));
                laserLine.SetColors(new Color(1, 1, 1, Mathf.Clamp01(cool.color.a * 2 + 0.2f) / 2), new Color(1, 1, 0, 0));

                cool.sprite = closerTarget;
                moveTarget();
                
            }
            if (targetPlayerVisible && aimDistanceFromTarget <= 14)
            {
                SpriteRenderer cool = aimTargetObject.GetComponent<SpriteRenderer>();
                cool.color = new Color(1, 0, 0, 1);
                laserLine.SetColors(new Color(1, 1, 1, Mathf.Clamp01(cool.color.a * 2 + 0.2f) / 2), new Color(1, 0, 0, 0));

                cool.sprite = aboutToFireTarget;
                if (!aboutToFire)
                {
                    aboutToFire = true;
                    StartCoroutine(chargeAndFire(new Vector2(dir.normalized.x, dir.normalized.y)));
                }
            }
        }
    }
}
