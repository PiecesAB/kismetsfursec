using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

// some things were too virus and too windows. so i made them normal
public static class MetaBehaviour
{
    public static void Shutdown()
    {

        Application.Quit(1);
    }

    public static void Crash()
    {
        UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.AccessViolation);

        /*while (true)
        {
            int a = 0;
            if (a < -1) { break; }
        }*/
    }

    public static bool IsSafeMode()
    {
        return false;
    }
}
