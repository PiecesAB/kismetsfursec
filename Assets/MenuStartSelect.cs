using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuStartSelect : InSaveMenuBase
{
    protected override void ChildOpen()
    {
    }

    protected override void ChildUpdate()
    {
        if (myControl.ButtonHeld(1024UL, 1024UL, 0f, out _) && myControl.ButtonHeld(2048UL, 2048UL, 0f, out _))
        {
            Utilities.toSaveData.needsSetup = Utilities.loadedSaveData.needsSetup = false;
            Utilities.SaveGame(Utilities.currentSaveNumber);
            // go straight to the first cutscene; there's nothing else to do until a level is unlocked.
            MenuStory.LoadCurrentLevelInStoryMode();
        }
    }
}
