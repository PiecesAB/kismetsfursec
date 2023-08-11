using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IovannisCage : MonoBehaviour
{
    public bool entered = false;
    public GameObject enteredDialog;
    public PrimExaminableItem iovannisPEI;
    public Transform leftGear;
    public Transform rightGear;
    public Transform mainCage;
    public PrimExaminableItem lever;

    private Vector3 maxPos;
    private Vector3 minPos;

    private void EnterFunc()
    {
        iovannisPEI.textboxesToGive = new GameObject[1] { enteredDialog };
        iovannisPEI.autoGiveMsg = true;
        lever.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponent<PrimPlayableCharacter>())
        {
            if (!entered)
            {
                EnterFunc();
                entered = true;
            }
        }
    }

    //put them back
    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.GetComponent<BasicMove>() && !col.GetComponent<PrimPlayableCharacter>()) //iovannis
        {
            Vector2 dif = (col.transform.position - mainCage.position);
            if (dif.magnitude > 80)
            {
                dif = dif.normalized * 48;
            }
            col.transform.position = mainCage.position + (Vector3)dif;
        }
    }

    void Start()
    {
        entered = false;
        maxPos = mainCage.position;
        minPos = maxPos + new Vector3(0, -80, 0);
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }

        Vector3 oldPos = mainCage.position;
        bool on = (Utilities.loadedSaveData.switchMask & 256u) != 0u;

        Vector3 newPos = Vector3.MoveTowards(mainCage.position, on ? minPos : maxPos, on ? 0.5f : 0.25f);

        mainCage.GetComponent<Rigidbody2D>().MovePosition(newPos);
        mainCage.GetComponent<Rigidbody2D>().velocity = (newPos - oldPos) / Time.deltaTime;
        float mag = (newPos - oldPos).y * 5.5f;
        leftGear.eulerAngles += Vector3.forward * mag;
        rightGear.eulerAngles += Vector3.back * mag;

        if (!lever.enabled && iovannisPEI.textboxesToGive.Length == 0)
        {
            lever.enabled = true;
        }
    }
}
