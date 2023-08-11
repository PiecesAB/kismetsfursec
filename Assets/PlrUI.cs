using UnityEngine;
using System.Collections;

public class PlrUI : MonoBehaviour {

    public float desireY;
    public float size;
    public bool destroyHandleMyself = false;

    public static GameObject MakeStatusBox(GameObject orig, Transform par)
    {
        GameObject newBox = Instantiate(orig, Vector3.zero, Quaternion.identity);
        newBox.transform.SetParent(par, false);
        newBox.GetComponent<PlrUI>().desireY += (19f + newBox.GetComponent<PlrUI>().size / 2);
        foreach (Transform ii in par)
        {
            if (ii.GetComponent<PlrUI>() != null && ii.gameObject != newBox)
            {
                ii.GetComponent<PlrUI>().desireY += (newBox.GetComponent<PlrUI>().size + 1);
            }
        }
        return newBox;
    }

    public static void DestroyStatusBox(GameObject del, Transform par)
    {
        PlrUI obui = del.GetComponent<PlrUI>();
        foreach (Transform ii in par)
        {
            PlrUI pui = ii.GetComponent<PlrUI>();
            if (pui != null && pui.desireY > obui.desireY)
            {
                pui.desireY -= (obui.size + 1);
            }
        }
        Destroy(del);
    }

    private void OnDestroy()
    {
        if (destroyHandleMyself)
        {
            foreach (Transform ii in transform.parent)
            {
                PlrUI pui = ii.GetComponent<PlrUI>();
                if (pui != null && pui.desireY > desireY)
                {
                    pui.desireY -= (size + 1);
                }
            }
        }
    }

    void Awake () {
        transform.localScale = new Vector3(1f, 0f, 1f);
        transform.localPosition = Vector3.zero;
	}

    void Update () {
        Vector3 v = transform.localPosition;
        transform.localScale = new Vector3(1f, Mathf.Lerp(transform.localScale.y, 1f, 0.2f), 1f);
        transform.localPosition = new Vector3(v.x, Mathf.Lerp(v.y, desireY, 0.2f), -100f);
	}
}
