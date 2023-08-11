using UnityEngine;
using System.Collections;

public class CurrentPlrActivities : MonoBehaviour {

    public Encontrolmentation controls;
    public GameObject UIObjectToClone;
    public GameObject UIObjectSpecial;
    public GameObject UIObjectSpecial2;
    public GameObject UIObjectDeathEE;
    public bool situateAllowed;
    public bool weaponAllowed;

    private int times = 0;

    private GameObject made;

    void Start()
    {
        times = 0;
    }

	// Update is called once per frame
	void Update () {
	
        if (controls.eventBbutton == Encontrolmentation.ActionButton.Nothing)
        {
            if (weaponAllowed)
            {
                controls.eventAbutton = Encontrolmentation.ActionButton.BButton;
                controls.eventAName = "Weapon";
            }
            else
            {
                controls.eventAbutton = Encontrolmentation.ActionButton.BButton;
                controls.eventAName = "...";
            }

            if (situateAllowed)
            {
                controls.eventBbutton = Encontrolmentation.ActionButton.XButton;
                controls.eventBName = "Situate";
            }
            else
            {
                controls.eventBbutton = Encontrolmentation.ActionButton.XButton;
                controls.eventBName = "...";
            }
        }

        if (controls.eventBName == "Situate" && (controls.flags & 64UL) == 64UL && made == null)
        {
            times++;
            if(KHealth.health <= 0f)
                {
                made = Instantiate(UIObjectDeathEE);
                made.SetActive(true);
                made.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform);
            }
            else
            {
                if (times >= 7)
                {
                    made = Instantiate(UIObjectSpecial2);
                    made.SetActive(true);
                    made.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform);
                }
                else if (times >= 3)
                {
                    made = Instantiate(UIObjectSpecial);
                    made.SetActive(true);
                    made.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform);
                }
                else
                {
                    made = Instantiate(UIObjectToClone);
                    made.SetActive(true);
                    made.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform);
                }
            }
        }

	}
}
