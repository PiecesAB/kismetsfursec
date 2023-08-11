using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontPanel : MonoBehaviour, IPrimDismantle
{
    public MonoBehaviour[] disableScriptInsteadOfGameObject;
    private HashSet<GameObject> dontDisable = new HashSet<GameObject>();
    private HashSet<GameObject> inside = new HashSet<GameObject>();
    private HashSet<MonoBehaviour> compInside = new HashSet<MonoBehaviour>();
    private HashSet<CircleCollider2D> screwColliders = new HashSet<CircleCollider2D>();
    public AmbushController induceWinOnDismantle = null;

    public void OnDismantle()
    {
        if (induceWinOnDismantle != null)
        {
            induceWinOnDismantle.InduceWin();
        }

        foreach (MonoBehaviour g in compInside)
        {
            g.enabled = true;
        }

        foreach (CircleCollider2D c in screwColliders)
        {
            c.enabled = true;
            c.GetComponentInChildren<MeshRenderer>().enabled = true;
            c.transform.SetParent(transform.parent, true);
        }

        foreach (GameObject g in inside)
        {
            g.SetActive(true);
            g.transform.SetParent(transform.parent, true);
        }
        foreach (GameObject g in dontDisable)
        {
            g.transform.SetParent(transform.parent, true);
        }
        transform.position += Vector3.forward * (-80);
    }

    void Start()
    {
        foreach (MonoBehaviour g in disableScriptInsteadOfGameObject)
        {
            dontDisable.Add(g.gameObject);
            compInside.Add(g);
            g.enabled = false;
        }

        foreach (Transform t in transform)
        {
            if (t == transform) { continue; }
            if (dontDisable.Contains(t.gameObject)) { continue; }
            if (t.GetComponent<DialScrew>())
            {
                screwColliders.Add(t.GetComponent<CircleCollider2D>());
                t.GetComponent<CircleCollider2D>().enabled = false;
                t.GetComponentInChildren<MeshRenderer>().enabled = false;
                continue;
            }
            inside.Add(t.gameObject);
            t.gameObject.SetActive(false);
        }
    }
}
