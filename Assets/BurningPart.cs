using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurningPart : MonoBehaviour
{
    [Header("d is the damage if applicable")]
    public float d = 0.01f;
    [Header("For flammable objects")]
    public float timeLeft = 8f;

    public BoxCollider2D[] boxFlameCollidersToCopy;
    public PolygonCollider2D[] polygonFlameCollidersToCopy;
    public GameObject flameToCopy;
    public GameObject dustBlockWhenBurnOver;
    public bool dealDamage = true;

    private static List<KHealth> healths = new List<KHealth>();
    private List<GameObject> colObjs = new List<GameObject>();

    private const float fireDelay = 0.01f;
    private const float burnTime = 3f;

    private float myBurnTime = burnTime;

    private static BurningPart main;

    public IEnumerator CatchFire(BurningPart source, bool superfast = false)
    {
        yield return new WaitForSeconds(fireDelay);

        BurningPart newFire = gameObject.GetComponent<BurningPart>(); // isn't newFire this object?
        newFire.dealDamage = false;
        newFire.d = d;
        myBurnTime = burnTime * (superfast ? 0.2f : 1f);
        newFire.timeLeft = myBurnTime;
        newFire.boxFlameCollidersToCopy = new BoxCollider2D[source.boxFlameCollidersToCopy.Length];
        for (int i = 0; i < source.boxFlameCollidersToCopy.Length; i++)
        {
            BoxCollider2D bci = source.boxFlameCollidersToCopy[i];
            BoxCollider2D nbci = gameObject.AddComponent<BoxCollider2D>();
            nbci.isTrigger = true;
            nbci.offset = bci.offset;
            nbci.size = bci.size;
            newFire.boxFlameCollidersToCopy[i] = nbci;
        }
        newFire.polygonFlameCollidersToCopy = new PolygonCollider2D[source.polygonFlameCollidersToCopy.Length];
        for (int i = 0; i < source.polygonFlameCollidersToCopy.Length; i++)
        {
            PolygonCollider2D pci = source.polygonFlameCollidersToCopy[i];
            PolygonCollider2D npci = gameObject.AddComponent<PolygonCollider2D>();
            npci.isTrigger = true;
            npci.SetPath(0, pci.GetPath(0));
            npci.offset = pci.offset;
            newFire.polygonFlameCollidersToCopy[i] = npci;
        }
        newFire.flameToCopy = source.flameToCopy;
        newFire.dustBlockWhenBurnOver = source.dustBlockWhenBurnOver;

        Transform st = gameObject.transform;
        Instantiate(source.flameToCopy, st.position, st.rotation, st);

    }

    void OnTriggerStay2D(Collider2D hi)
    {
        if (dealDamage && hi.GetComponent<KHealth>() && !healths.Contains(hi.GetComponent<KHealth>()))
        {
            KHealth kh1 = hi.GetComponent<KHealth>();
            if (kh1.overheat < 0.5f) { kh1.overheat = 0.5f; }
            kh1.overheat += d;
            healths.Add(kh1);
        }
        else
        {
            primExtraTags pet = hi.GetComponent<primExtraTags>();
            GameObject g = hi.gameObject;
            if (pet)
            {
                if (pet.tags.Contains("flammable"))
                {
                    pet.tags.Remove("flammable");
                    BurningPart newFire = g.AddComponent<BurningPart>();
                    newFire.StartCoroutine(newFire.CatchFire(this));
                }
                if (pet.tags.Contains("superflammable"))
                {
                    pet.tags.Remove("superflammable");
                    BurningPart newFire = g.AddComponent<BurningPart>();
                    newFire.StartCoroutine(newFire.CatchFire(this, true));
                }
                if (pet.tags.Contains("liquid"))
                {
                    enabled = false;
                }
            }
        }
    }

    void Update()
    {
        if (main == null) { main = this; }
        if (main == this) { healths.Clear(); }
        transform.hasChanged = false;

        if (Time.timeScale == 0) { return; }

        if (timeLeft > 0)
        {
            timeLeft -= Time.timeScale * 0.0166666f;
            if (timeLeft <= myBurnTime && GetComponent<SpriteRenderer>())
            {
                float e = (timeLeft / myBurnTime);
                GetComponent<SpriteRenderer>().color = new Color(e, e, e, 1f);
            }
        }

        if (timeLeft > -0.9999f && timeLeft <= 0f)
        {
            GameObject dus = Instantiate(dustBlockWhenBurnOver, transform.position, transform.rotation, transform.parent);
            Destroy(gameObject);
        }
    }
}
