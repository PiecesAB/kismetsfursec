using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SecondaryTitleMenu : InSaveMenuBase
{
    public static bool open = false;

    protected abstract override void ChildOpen();
    protected abstract override void ChildUpdate();
}
