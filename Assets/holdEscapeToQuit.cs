using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class holdEscapeToQuit : MonoBehaviour {

    [Header("This script is persistent throughout the game and has important setting stuff")]
    public bool on;
    public Image image;
    public Text text;

    private bool going;
    private double u;
    private bool bye;
    private double t;
    private string oldAspect;

    public Coroutine WaitForRealSeconds(float time)
    {
        return StartCoroutine(WaitForRealSecondsImpl(time));
    }

    private IEnumerator WaitForRealSecondsImpl(float time)
    {
        double startTime = DoubleTime.UnscaledTimeRunning;
        while (DoubleTime.UnscaledTimeRunning - startTime < time)
            yield return 1;
    }

    void Start () {
        DontDestroyOnLoad(gameObject.transform.root);
        u = 0f;
	}
	
    IEnumerator Quit()
    {
        yield return WaitForRealSeconds(1.6f);
        PlayerPrefs.DeleteKey("gameIsOpen");
        Application.Quit();
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteKey("Fullscreen");
    }

	// Update is called once per frame
	void Update () {
        

        if (!PlayerPrefs.HasKey("WindowVRes"))
        {
            PlayerPrefs.SetFloat("WindowVRes", 48f);
            PlayerPrefs.Save();
        }
        if (!PlayerPrefs.HasKey("AspectRatio"))
        {
            PlayerPrefs.SetString("AspectRatio", "16:9");
            oldAspect = "16:9";
            PlayerPrefs.Save();
        }
       /* if (!PlayerPrefs.HasKey("Fullscreen"))
        {
            PlayerPrefs.SetFloat("Fullscreen", (Screen.fullScreen)?0f:1f);
            PlayerPrefs.Save();
        }

        if (Screen.fullScreen != (PlayerPrefs.GetFloat("Fullscreen", 0f)==1f))
        {
            Screen.fullScreen = (PlayerPrefs.GetFloat("Fullscreen", 0f) == 1f);
        }*/

      /*      if ((Screen.fullScreen == false && Screen.height/10f != PlayerPrefs.GetFloat("WindowVRes",48f)) || oldAspect != PlayerPrefs.GetString("AspectRatio", "16:9"))
        {
            int r = (int)PlayerPrefs.GetFloat("WindowVRes", 48f);
            float t = (PlayerPrefs.GetString("AspectRatio", "16:9") == "16:9") ? (1.777779f) : ((PlayerPrefs.GetString("AspectRatio", "16:9") == "16:10") ? (1.6f) : ((PlayerPrefs.GetString("AspectRatio", "16:9") == "4:3") ? (1.333335f) : ((PlayerPrefs.GetString("AspectRatio", "16:9") == "5:4") ? (1.25f) : (1.333335f))));
            oldAspect = PlayerPrefs.GetString("AspectRatio", "16:9");
            Screen.SetResolution(Mathf.FloorToInt(r * 10f * t), r * 10, false);
        }
        */
        


	/*if (!Input.GetKey(KeyCode.Escape) && !bye)
        {
            if (going)
            {
                Utilities.canPauseGame = true;
                Utilities.canUseInventory = true;
                Time.timeScale = (float)t;
            }
            going = false;
            text.text = "";
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(image.GetComponent<RectTransform>().sizeDelta.x, 0);
            
        }
    else if (!bye)
        {
            if (!going)
            {
                going = true;
                u = DoubleTime.UnscaledTimeRunning;
                Utilities.canPauseGame = false;
                Utilities.canUseInventory = false;
                t = Time.timeScale;
                Time.timeScale = 0;
            }
            double v = DoubleTime.UnscaledTimeRunning - u;
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(image.GetComponent<RectTransform>().sizeDelta.x,(float)v*80);
            text.text = (3f - v).ToString("0.00");
            if (v > 3f)
            {
                bye = true;
                text.text = "game quit";
                image.GetComponent<RectTransform>().sizeDelta = new Vector2(image.GetComponent<RectTransform>().sizeDelta.x, 240);
                StartCoroutine(Quit());
            }
        }
        */

	}
}
