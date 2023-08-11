using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KhalGunMachine : MonoBehaviour {

    public enum Setting
    {
        Additive,Subtractive
    }

    public double TimeMade = 0f;
    public float size = 0f;
    public Transform laserRender;
    public BoxCollider2D specialCollision;
    public Setting setting = Setting.Additive;
    private List<BasicMove> plrz = new List<BasicMove>();
    private List<BasicMove> plr2 = new List<BasicMove>();

    void Start () {
        size = 0f;
        TimeMade = DoubleTime.ScaledTimeSinceLoad + 0.01f;
        plrz = new List<BasicMove>(FindObjectsOfType<BasicMove>());
        print(plrz.Count);
	}
	
	
	void Update () {
        if (Time.timeScale > 0)
        {
            plr2.Clear();
            size+=2f;
            if (setting == Setting.Additive)
            {
                GetComponent<BoxCollider2D>().enabled = true;
                GetComponent<BoxCollider2D>().size = new Vector2(size, GetComponent<BoxCollider2D>().size.y);
                GetComponent<BoxCollider2D>().offset = Vector2.right * size / 2f;
                specialCollision.size = GetComponent<BoxCollider2D>().size;
                specialCollision.offset = GetComponent<BoxCollider2D>().offset;
            }
            if (setting == Setting.Subtractive)
            {
                GetComponent<BoxCollider2D>().enabled = false;
                RaycastHit2D[] plrs = Physics2D.BoxCastAll(transform.position, new Vector2(1f, transform.localScale.y * 10f), transform.eulerAngles.z, transform.right, size, 1 << 20);
                foreach (var plr in plrs)
                {
                    if (plr.transform.gameObject.GetComponent<BasicMove>() != null)
                    {
                        plr.transform.gameObject.GetComponent<BasicMove>().CanCollide = false;
                        plr2.Add(plr.transform.gameObject.GetComponent<BasicMove>());
                    }
                }
                foreach (var plr in plrz)
                {
                    if (!plr2.Contains(plr))
                    {
                        plr.CanCollide = true;
                    }
                }
                if (size < 5000)
                {
                    specialCollision.size = new Vector2(size, GetComponent<BoxCollider2D>().size.y);
                    specialCollision.offset = Vector2.right * size / 2f;
                }
            }

            RaycastHit2D[] boxs = Physics2D.BoxCastAll(transform.position, new Vector2(1f, transform.localScale.y * 10f), transform.eulerAngles.z, transform.right, size, 1 << 17);
            foreach (var i in boxs)
            {
                GameObject j = i.transform.parent.gameObject;
                if (j != gameObject && j.GetComponent<KhalGunMachine>())
                {
                    if (j.GetComponent<KhalGunMachine>().TimeMade <= TimeMade && j.GetComponent<KhalGunMachine>().TimeMade != 0)
                    {
                        Destroy(j);
                        break;
                    }
                }
            }
            if (size < 5000)
            {
                laserRender.localScale = new Vector3(size / 10f, 1f, 1f);
                laserRender.localPosition = Vector3.right * size / 2f;
            }
        }
    }
}
