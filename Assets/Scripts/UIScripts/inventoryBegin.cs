using UnityEngine;
using System.Collections;

public class inventoryBegin : MonoBehaviour {

    public Encontrolmentation input;
    public bool on;
    //public InventoryListScroll landingList;

    void Start()
    {
        on = false;
    }

        void Update () {
	if ((input.flags&16UL)==16UL)
        {
            on = true;
            /*landingList.gameObject.SetActive(true);
            landingList.showing = true;
            landingList.Change(0); //LOL!
            */
        }
	}
}
