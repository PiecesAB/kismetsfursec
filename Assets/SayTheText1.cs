using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SayTheText1 : MonoBehaviour {

    public string toSay;
    public string toSay2;
    public GameObject tx;
    private Text text;


    // Use this for initialization
    void Start () {
        text = tx.GetComponent<Text>();
        StartCoroutine(QuestionMark());

    }


    IEnumerator QuestionMark()
    {
        
            
            yield return new WaitForSeconds(1.0f);
        
        for (int a = 0; a <= (toSay.Length-1); a++)
        {
            yield return new WaitForSeconds(0.03f);
            text.text  = string.Concat(text.text, toSay[a]);
        }

        yield return new WaitForSeconds(3.0f);
        text.text = "";

        for (int a = 0; a <= (toSay2.Length - 1); a++)
        {
            yield return new WaitForSeconds(0.03f);
            text.text = string.Concat(text.text, toSay2[a]);
        }

    }

    // Update is called once per frame
    void Update () {
	
	}
}
