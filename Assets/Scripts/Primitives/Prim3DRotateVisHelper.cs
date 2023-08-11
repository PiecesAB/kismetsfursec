using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisHelper : MonoBehaviour
{
    public IVisHelperMain main;

    private void OnBecameInvisible()
    {
        if (main == null || main.GetAlwaysVisible()) { return; }
        main.Invis2();
    }

    private void OnBecameVisible()
    {
        if (main == null) { return; }
        main.Vis2();
    }
}
