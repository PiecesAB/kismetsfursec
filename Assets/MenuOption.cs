using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOption : InSaveMenuList
{
    public InSaveMenuBase deleteStoryMenu;
    public InSaveMenuBase deleteAllMenu;

    protected override void Fire()
    {
        switch (selection)
        {
            case 0: mainMenu.KeyboardOpenForNaming("Register a valid name.", true, myControl, Close); break;
            case 1: mainMenu.IconSelectOpen(true, myControl, Close); break;
            case 2: deleteStoryMenu.Open(myControl); break;
            case 3: deleteAllMenu.Open(myControl); break;
            case 4:
                Close();
                if (backSound)
                {
                    GameObject bs2 = Instantiate(backSound.gameObject, null);
                    Destroy(bs2, 1f);
                    bs2.GetComponent<AudioSource>().Play();
                }
                break;
            default: break;
        }
    }
}
