using System;
using UnityEngine;

public abstract class InSaveMenuList : InSaveMenuBase
{
    public InSaveMenuMain mainMenu;
    public int selection;
    public Transform[] choiceTransforms;
    public Transform arrow;
    public AudioSource changeSound;
    public AudioSource submitSound;

    protected override void ChildOpen()
    {
        selection = 0;
    }

    protected abstract void Fire(); // implemented in child for specific things to happen when item is chosen.

    protected override void ChildUpdate()
    {
        if (myControl.ButtonDown(4UL, 12UL))
        {
            --selection;
            if (selection < 0) { selection = choiceTransforms.Length - 1; }
            if (changeSound) { changeSound.Stop(); changeSound.Play(); }
        }
        if (myControl.ButtonDown(8UL, 12UL))
        {
            ++selection;
            if (selection >= choiceTransforms.Length) { selection = 0; }
            if (changeSound) { changeSound.Stop(); changeSound.Play(); }
        }
        if (myControl.ButtonDown(16UL, 16UL)) { if (submitSound) { submitSound.Stop(); submitSound.Play(); } Fire(); }

        arrow.localPosition = new Vector3(-60 + 3 * (float)Math.Sin(DoubleTime.UnscaledTimeSinceLoad * 4.0), choiceTransforms[selection].localPosition.y, arrow.localPosition.z);
    }
}
