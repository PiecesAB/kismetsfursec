using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectronTracker : MonoBehaviour
{
    public int amount;

    private BasicMove bm;

    public GameObject uiPrefab;
    public GameObject uiInstance;
    public GameObject lightningPrefab;

    public Queue<GameObject> respawns = new Queue<GameObject>();

    public void CopyFrom(ElectronTracker source)
    {
        amount = source.amount;
        bm = source.bm;
        uiPrefab = source.uiPrefab;
        uiInstance = source.uiInstance;
        uiInstance.transform.SetParent(transform, false);
        source.uiInstance = null;
        lightningPrefab = source.lightningPrefab;
        respawns = source.respawns;
    }

    private void MakeLightning(Vector3 target)
    {
        GameObject l = Instantiate(lightningPrefab, null);
        LineRenderer ll = l.GetComponent<LineRenderer>();
        Vector3[] p = new Vector3[9];
        p[0] = transform.position;
        Vector3 fakeUp = transform.localScale.y < 0f ? -transform.up : transform.up;
        p[8] = transform.position - fakeUp * 22f;
        for (int i = 1; i < 8; ++i)
        {
            p[i] = Vector3.Lerp(p[0], p[8], i/8f) + 32f * (Vector3)Fakerand.UnitCircle();
        }
        ll.positionCount = 9;
        ll.SetPositions(p);
    }

    public bool DecrementByDoor(Vector3 doorPos, int decAmt)
    {
        if (decAmt > amount) { return false; }
        amount -= decAmt;
        MakeLightning(doorPos);
        for (int i = 0; i < decAmt; ++i)
        {
            respawns.Dequeue();
        }
        if (amount == 0)
        {
            if (uiInstance) { PlrUI.DestroyStatusBox(uiInstance, transform); }
            Destroy(this);
        }
        return true;
    }

    public void LoseOne()
    {
        --amount;
        GameObject r = respawns.Dequeue();
        r.SetActive(true);
        Vector3 fakeUp = transform.localScale.y < 0f ? -transform.up : transform.up;
        MakeLightning(transform.position - fakeUp * 22f);
        if (amount == 0)
        {
            if (uiInstance) { PlrUI.DestroyStatusBox(uiInstance, transform); }
            Destroy(this);
        }
    }

    private void Start()
    {
        bm = GetComponent<BasicMove>();
        if (bm) { bm.electronTracker = this; }
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (!bm) { bm = GetComponent<BasicMove>(); }
        if (bm) { bm.electronTracker = this; }
        if (!uiInstance) { uiInstance = PlrUI.MakeStatusBox(uiPrefab, transform); }
        if (uiInstance)
        {
            uiInstance.GetComponentInChildren<TextMesh>().text = amount.ToString();
        }
    }
}
