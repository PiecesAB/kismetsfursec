using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChangeGunValue : MonoBehaviour
{

    public GameObject theHealthBarReference;
    private float delta;
    private bool colChanged;
    private Color origCol;

    void Start()
    {
        origCol = theHealthBarReference.GetComponent<Image>().color;
    }


    // Update is called once per frame
    void Update()
    {

        delta = GameObject.FindGameObjectWithTag("Player").GetComponent<ClickToChangeTime>().gunHealth;
        //GetComponent<Text>().text = "HP: " + delta;
        theHealthBarReference.GetComponent<RectTransform>().sizeDelta = new Vector2(delta, 16);
        if (GameObject.FindGameObjectWithTag("Player").GetComponent<ClickToChangeTime>().disabled == true)
        {
            theHealthBarReference.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f);

        }

        if (GameObject.FindGameObjectWithTag("Player").GetComponent<ClickToChangeTime>().disabled == false)
        {
            theHealthBarReference.GetComponent<Image>().color = origCol;

        }


    }
}
