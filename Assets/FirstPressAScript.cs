using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;


public class FirstPressAScript : MonoBehaviour {

    public string nextSceneName;
    public float waitTime;

    public Encontrolmentation control;

    private bool on;

    public IEnumerator Succ()
    {
        yield return new WaitForSeconds(waitTime);
        on = true;
    }


	// Use this for initialization
	void Start () {
        on = false;
        GetComponent<Text>().color = new Color(0, 0, 0, 0);
        StartCoroutine(Succ());
	}
	
	// Update is called once per frame
	void Update () {
	if (on)
        {
            GetComponent<Text>().color = new Color(0.6f+(0.25f*(float)System.Math.Sin(2*Mathf.PI*DoubleTime.UnscaledTimeRunning)), 0, 0, 1);
            if (control.AnyButtonDown())
                {
                    print("accepted");
                    SceneManager.LoadSceneAsync(nextSceneName);
                }
            }

        }

	}
