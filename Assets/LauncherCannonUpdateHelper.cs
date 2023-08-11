using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherCannonUpdateHelper : MonoBehaviour
{
    public LauncherCannon main;
    public Encontrolmentation e;

    private int frame = 0;

    void Update()
    {
        ++frame;
        if (main.state == LauncherCannon.State.Full)
        {
            e.eventAbutton = Encontrolmentation.ActionButton.BButton;
            e.eventAName = "Fire";
            if (main.fixedAngle)
            {
                e.eventBbutton = Encontrolmentation.ActionButton.XButton;
                e.eventBName = "...";
            }
            else
            {
                e.eventBbutton = frame % 40 < 20 ? Encontrolmentation.ActionButton.LButton : Encontrolmentation.ActionButton.RButton;
                e.eventBName = "Aim";
            }
        }
        else
        {
            Destroy(this);
        }
    }
}
