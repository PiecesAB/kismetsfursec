using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCredits : SecondaryTitleMenu
{
    protected override void ChildOpen()
    {
        open = true;
    }

    protected override void ChildUpdate()
    {
    }

    protected override void ChildClose()
    {
        open = false;
    }
}
