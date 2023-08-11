using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Obsolete("Old", true)]
public class ChestType1 : MonoBehaviour {

    public Encontrolmentation player;
    public bool item;
    public string noItemMsg;
    public string yesItemMsg;
    public string alreadyOpenedMsg;
    public string inventoryFullMsg;
    public string containerName;
    public Vector2 colOffset;
    public float colRadius;
    public bool con;
    public bool requireCategory;
    public string requiredCategory;
    public Dictionary<string, float> secondCategoryWeight = new Dictionary<string, float>();
    public GameObject textBox;
    public bool opened;

    void Start () {
        opened = false;
	}

	void Update () {
        try
        {
            if (Fastmath.FastV2Dist((Vector2)player.transform.position+colOffset, transform.position) < colRadius)
            {
                player.eventBbutton = Encontrolmentation.ActionButton.XButton;
                player.eventBName = "Open "+containerName;
                player.givenObjIdentifier = gameObject.GetInstanceID();
                con = true;
            }
            else if (player.eventBName == "Open " + containerName)
            {
                if (con)
                {
                    player.eventBbutton = Encontrolmentation.ActionButton.Nothing;
                    player.eventBName = "";
                    player.givenObjIdentifier = 0;
                    con = false;
                }
            }
        }
        catch
        {
            //   :<
        }

        if (con && ((player.flags & 64UL) == 64UL) && ((player.currentState & 64UL) == 64UL) && player.givenObjIdentifier == gameObject.GetInstanceID() && Time.timeScale > 0)
        {
            string itemName = "nothing... Sorry";

            GameObject ne = Instantiate(textBox, Vector3.zero, Quaternion.identity) as GameObject;
            ne.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform, false);
            string zz = (opened)?(alreadyOpenedMsg):((item) ? yesItemMsg : noItemMsg);
            zz = zz.Replace("<contname>", containerName).Replace("<gotitem>", itemName);
            ne.transform.GetChild(0).GetComponent<MainTextsStuff>().messages = new List<MainTextsStuff.MessageData>() { new MainTextsStuff.MessageData(zz, zz.Length*0.1f) };
            if (item)
            {
                opened = true;
            }
        }
    }
}
