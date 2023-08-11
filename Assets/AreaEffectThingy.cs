using UnityEngine;
using System.Collections;

public class AreaEffectThingy : MonoBehaviour {

    public enum RegType
    {
        Toxic, RebuffShower
    }

    public RegType regtype;
    private Color c;
    private Color c2;
    private bool succ;
    private bool osc;
    public float rebuffTime;
    public GameObject prefabRebuffUI;
    public AudioSource rebufSound;
    //public bool blockersNeverMove = false;
    private Vector3 fakeRight;

	void Start () {
        if (regtype == RegType.Toxic)
        {
            succ = false;
            osc = false;
            c = GetComponent<SpriteRenderer>().color;
            c2 = Color.red;
        }

        fakeRight = transform.right * ((transform.localScale.x < 0) ? -1 : 1);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        KHealth okh = other.GetComponent<KHealth>();
        if (regtype == RegType.Toxic)
        {
            if (okh && okh.nontoxic == 0)
            {
                okh.addToxicity = 3;
                succ = true;
                GetComponent<SpriteRenderer>().color = new Color(c2.r, c2.g, c2.b, osc?1f:0f);
                osc = !osc;
            }
        }

        if (regtype == RegType.RebuffShower)
        {
            if (okh && okh.nontoxic <= rebuffTime)
            {
                okh.nontoxic = okh.previousnontoxmax = rebuffTime;
                if (!rebufSound.isPlaying)
                {
                    rebufSound.Play();
                }
                if (other.transform.Find("PlrUI Rebuff") == null)
                {
                    GameObject ne = PlrUI.MakeStatusBox(prefabRebuffUI, other.transform);
                    ne.GetComponent<HealthBarChange>().healthObj = other.GetComponent<KHealth>();
                    ne.name = "PlrUI Rebuff";
                }
                
            }
        }
    }

    void Update () {
        if (regtype == RegType.Toxic)
        {
            if (succ)
            {
                succ = false;
            }
            else
            {
                GetComponent<SpriteRenderer>().color = new Color(c.r, c.g, c.b, 0.3137f + 0.12f * (float)System.Math.Sin(2.25f * Mathf.PI * DoubleTime.UnscaledTimeRunning));
            }
        }

        if (regtype == RegType.RebuffShower)
        {
            RaycastHit2D r = Physics2D.BoxCast(transform.position + fakeRight*11- transform.up*7, new Vector2(12, 1), transform.eulerAngles.z, -transform.up, 800f, 256, transform.position.z - 256, transform.position.z + 256);
            if (r.collider != null)
            {
                GetComponent<BoxCollider2D>().offset = new Vector2(11f, -7 - (r.distance / 2f));
                GetComponent<BoxCollider2D>().size = new Vector2(12f, r.distance);
            }
            else
            {
                GetComponent<BoxCollider2D>().offset = new Vector2(11f, -4007);
                GetComponent<BoxCollider2D>().size = new Vector2(12f, 800);
            }
        }
    }
}
