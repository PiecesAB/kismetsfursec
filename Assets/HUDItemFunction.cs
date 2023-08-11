using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDItemFunction : MonoBehaviour
{
    public int currItem;
    public Image itemImage;

    private GameObject[] plrs;

    void UpdateItemPic()
    {
        if(currItem >= 0)
        {
            itemImage.enabled = true;
            itemImage.sprite = InventoryItemsNew.GetItemInfo(currItem, true).imag;
        }
        else
        {
            itemImage.enabled = false;
        }
    }

    void Start()
    {
        List<int> l = Utilities.loadedSaveData.SharedPlayerItems;
        currItem = (l.Count > 0)?l[0]:-1;
        UpdateItemPic();

        plrs = GameObject.FindGameObjectsWithTag("Player");
    }

    void Update()
    {
        //use item
        if (Time.timeScale > 0 && plrs.Length > 0)
        {
            Encontrolmentation e = null;
            for (int i = 0; i < plrs.Length; i++)
            {
                Encontrolmentation etemp = plrs[i].GetComponent<Encontrolmentation>();
                if (etemp && etemp.allowUserInput)
                {
                    e = etemp;
                    break;
                }
            }

            if (e && e.ButtonDown(128UL, 128UL))
            {
                InventoryItemsNew.UseItem(0, e.gameObject);
            }
        }

        //change item picture
        List<int> l = Utilities.loadedSaveData.SharedPlayerItems;
        if (l.Count > 0)
        {
            if (l[0] != currItem)
            {
                currItem = l[0];
                UpdateItemPic();
            }
        }
        else if (itemImage.enabled)
        {
            itemImage.enabled = false;
        }
    }
}
