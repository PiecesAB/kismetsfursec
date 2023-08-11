using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MenuTwoChoice : InSaveMenuBase
{
    public Transform arrow;
    public Transform[] iconObjects = new Transform[2];
    public int selection;
    public AudioSource changeSound;
    public AudioSource submitSound;

    protected void UpdateArrow()
    {
        arrow.localPosition = new Vector3(iconObjects[selection].localPosition.x, arrow.localPosition.y, arrow.localPosition.z);
    }

    protected abstract void MakeSelection();

    protected override void ChildOpen()
    {
        selection = 0;
        UpdateArrow();
    }

    protected override void ChildUpdate()
    {
        if (myControl.ButtonDown(1UL, 3UL) || myControl.ButtonDown(2UL, 3UL))
        {
            selection = (selection == 0) ? 1 : 0;
            if (changeSound) { changeSound.Stop(); changeSound.Play(); }
        }
        if (myControl.ButtonDown(16UL, 16UL))
        {
            if (submitSound) {
                GameObject ss2 = Instantiate(submitSound.gameObject, null);
                Destroy(ss2, 1f);
                ss2.GetComponent<AudioSource>().Play();
            }
            MakeSelection();
        }
        UpdateArrow();
    }
}
