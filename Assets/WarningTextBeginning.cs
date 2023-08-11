using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class WarningTextBeginning : MonoBehaviour {

    public string[] texts;
    public string[] specialContinueMsg;
    public Text anyContinue;
    public AudioSource music;
    public AudioSource submitSound;
    public GameObject bg;
    public Image circleMask;
    public string nextLevel;
    public Text loadingText;
    public Text progressText;
    public GameObject[] delet;
    public int test;

    private bool go;
    private bool xx;
    private bool zz;
    private AsyncOperation a = new AsyncOperation();

    IEnumerator Succ()
    {
        yield return new WaitForSeconds(3f);
        anyContinue.gameObject.SetActive(true);
    }

	void Start () {
        go = false;
        xx = false;
        zz = true;
        circleMask.material.SetFloat("_Prog", 0f);
        int r = Fakerand.Int(0, texts.Length);
        if(!PlayerPrefs.HasKey("wasTheGameEverStarted"))
        {
            r = 0;
            PlayerPrefs.SetString("wasTheGameEverStarted", "Yes.");
            PlayerPrefs.Save();
        }

        if (PlayerPrefs.HasKey("gameIsOpen"))
        {
            PlayerPrefs.DeleteKey("gameIsOpen");
            GetComponent<Text>().text = "<color=red>The game was closed in an inappropriate way. Some data may have been lost.</color>";
        }
        else
        {
            if (test >= 0)
            {
                r = test;
            }
            else
            {
                r = Utilities.loadedSaveData.needsSetup ? 0 : r;
            }
            GetComponent<Text>().text = texts[r];
            if (specialContinueMsg[r] != "")
            {
                anyContinue.text = specialContinueMsg[r];
            }
        }
        
        StartCoroutine(Succ());
    }

    IEnumerator TransitionOut()
    {
        //loadingText.text = "Loading completed.";
        anyContinue.text = "";
        foreach(var aa in delet)
        {
            Destroy(aa);
        }
        transform.parent.GetComponent<Image>().color = Color.clear;
        //yield return new WaitForSeconds(1.6f);
        loadingText.gameObject.SetActive(false);
        progressText.gameObject.SetActive(false);
        while (circleMask.material.GetFloat("_Prog") < 1f)
        {
            circleMask.material.SetFloat("_Prog", circleMask.material.GetFloat("_Prog") + 0.04f);
            yield return new WaitForEndOfFrame();
        }
        Destroy(transform.parent.parent.gameObject);
    }

    void Update()
    {
        if(!go && anyContinue.gameObject.activeInHierarchy && (GetComponent<Encontrolmentation>().currentState > 0))
        {
            go = true;
        }
        else if (!go && !xx && circleMask.material.GetFloat("_Prog") < 1f)
        {
            circleMask.material.SetFloat("_Prog", circleMask.material.GetFloat("_Prog") + 0.04f);
        }

        if (go && zz)
        {
            circleMask.material.SetFloat("_Prog", circleMask.material.GetFloat("_Prog") - 0.04f);
            if (music)
            music.volume = 0.1f * circleMask.material.GetFloat("_Prog");
            {
                
                if (!xx)
                {
                    xx = true;
                    submitSound.Play();
                    loadingText.gameObject.SetActive(true);
                    loadingText.text = "";//"Loading title screen";
                    a = SceneManager.LoadSceneAsync(nextLevel);
                    a.allowSceneActivation = false;
                }

                if (xx && a != null)
                {
                    if (circleMask.material.GetFloat("_Prog") <= 0f)
                    {
                        GetComponent<Text>().text = "";
                        DontDestroyOnLoad(transform.parent.parent.gameObject);
                        a.allowSceneActivation = true;
                        if (a.isDone)
                        {
                            StartCoroutine(TransitionOut());
                            zz = false;
                        }
                    }
                   progressText.text = Mathf.Min(Mathf.Floor((a.progress/0.903f)  * 100f),100f) + "";
                }

            }
        }
    }

}
