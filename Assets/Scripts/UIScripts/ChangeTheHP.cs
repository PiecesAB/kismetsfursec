using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChangeTheHP : MonoBehaviour {

    public GameObject theHealthBarReference;
    private float delta;
    private bool colChanged;
	
	// Update is called once per frame
	void Update () {

        delta = KHealth.health;

        float newSize = delta;
        //GetComponent<Text>().text = "HP: " + delta;
        if (delta > 0)
        {
            float c1 = Mathf.Pow(10, 0.2f) * 15;
            newSize = Mathf.Pow(delta / c1, 2.5f);
            delta = newSize;
        }
        theHealthBarReference.GetComponent<RectTransform>().sizeDelta = new Vector2(newSize,16);

        colChanged = false;

        if (delta >= 60)
        {
            theHealthBarReference.GetComponent<Image>().color = new Color((100f-delta)/53.3f,0.75f,0f);
            colChanged = true;
        }

        if (delta >= 20 && delta < 60)
        {
            theHealthBarReference.GetComponent<Image>().color = new Color(0.75f, (delta-20f) / 53.3f, 0f);
            colChanged = true;
        }
        if (delta < 20)
        {
            theHealthBarReference.GetComponent<Image>().color = new Color(0.75f, 0f, 0f);
            colChanged = true;
        }

        if (delta < 25)
        {
           
           // GetComponent<RectTransform>().rotation = Quaternion.AngleAxis(Fakerand.Single() * 3, Vector3.forward);
            GetComponent<Text>().text = ""+Mathf.Round(delta);
            GetComponent<Text>().color = new Color(1, delta/25, delta/25, 1);
            colChanged = true;
        }
        if (delta >= 25)
        {

            // GetComponent<RectTransform>().rotation = Quaternion.AngleAxis(Fakerand.Single() * 3, Vector3.forward);
            GetComponent<Text>().text = "" + Mathf.Round(delta);
            GetComponent<Text>().color = new Color(1, 1, 1, 1);
            colChanged = true;
        }
      /*  if (!colChanged || thePlayerReference == null)
        {
            GetComponent<Text>().text = "-INFINITY";
            GetComponent<Text>().color = new Color(1, 0, 0, 1);
        }*/
    }
}
