using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MenuScrollHandler : InSaveMenuBase {

    public enum ScrollType
    {
    Vertical,Horizontal
    }

    public ScrollType scrollDirection;
    public bool looping;
    public int index;
    public int count;
    public float holdKeyStartDelay;
    public float holdKeyScrollWait;
    public int startingIndex;

    public AudioSource changeSound;

    private double lastScrollTime;

    protected override void ChildOpen()
    {
        index = startingIndex;
    }

    protected override void ChildUpdate () {

        var kx = myControl.eventsTable[myControl.eventsTable.Count - 1].Item1;
        var ky = myControl.eventsTable[myControl.eventsTable.Count - 1].Item2;

        if (scrollDirection == ScrollType.Vertical)
        {
            double t = DoubleTime.UnscaledTimeRunning - lastScrollTime;
            double placeholder = 0.0;
            if (myControl.ButtonDown(4UL, 12UL) || (myControl.ButtonHeld(4UL, 12UL, holdKeyStartDelay, out placeholder) && t >= holdKeyScrollWait))
            {
                lastScrollTime = DoubleTime.UnscaledTimeRunning;
                int oldIndex = index;
                index = (index == 0) ? ((looping)?(count-1):(0)) : (index - 1); //one line madness
                if (oldIndex != index) { changeSound.Stop(); changeSound.Play(); }
            }

            if (myControl.ButtonDown(8UL, 12UL) || (myControl.ButtonHeld(8UL, 12UL, holdKeyStartDelay, out placeholder) && t >= holdKeyScrollWait))
            {
                lastScrollTime = DoubleTime.UnscaledTimeRunning;
                int oldIndex = index;
                index = (index == count-1) ? ((looping) ? (0) : (count-1)) : (index + 1); //lol
                if (oldIndex != index) { changeSound.Stop(); changeSound.Play(); }
            }

        }

	}
}
