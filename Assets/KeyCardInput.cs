using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCardInput : GenericBlowMeUp, IExaminableAction
{
    public GameObject failBox;
    public bool solved = false;
    public SpecialGate[] gatesToUnlock;
    private const int keyId = 300;

    private int HasKey()
    {
        int i; InventoryItemsNew.InventoryHasItem(keyId, out i); return i;
    }

    public void OnExamine(Encontrolmentation plr)
    {
        if (solved || Time.timeScale == 0) { return; }
        int i = HasKey();
        if (i >= 0)
        {
            InventoryItemsNew.DeleteItemByIndex(i);
            foreach (SpecialGate g in gatesToUnlock)
            {
                g.OnAmbushComplete();
            }
            BlowMeUp();
        }
        else
        {
            TextBoxGiverHandler.SpawnNewBox(ref failBox, gameObject);
        }
    }
}
