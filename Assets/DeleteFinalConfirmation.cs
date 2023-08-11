using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeleteFinalConfirmation : MonoBehaviour
{
    public Text countdownMsgText;
    public string[] byeMsgs;
    private Encontrolmentation myControl;
    public Text mainMsgText;
    private string countdownMsg;
    private bool bye;

    private void Start()
    {
        countdownMsg = countdownMsgText.text;
        Application.targetFrameRate = 60;
        myControl = GetComponent<Encontrolmentation>();
        bye = false;
    }

    private IEnumerator Bye()
    {
        Utilities.DeleteGame(Utilities.activeGameNumber);

        countdownMsgText.text = "";
        mainMsgText.color = Color.red;
        for (int i = 0; i < byeMsgs.Length; ++i)
        {
            mainMsgText.text = byeMsgs[i];
            yield return new WaitForSecondsRealtime(Mathf.Exp(Fakerand.Single()*3 - 2));
        }

        mainMsgText.text = "All done. Bye-bye now.";
        yield return new WaitForSecondsRealtime(0.5f);

        MetaBehaviour.Crash();

        yield return new WaitForSecondsRealtime(0.5f);

        Application.Quit();

        yield return null;
    }

    private void Update()
    {
        if (bye) { return; }

        string timer = (Mathf.Max(45 - (float)DoubleTime.ScaledTimeSinceLoad,0)).ToString("000");
        countdownMsgText.text = countdownMsg.Replace("<t>", timer);

        if (DoubleTime.ScaledTimeSinceLoad >= 2 && myControl.AnyButtonDown())
        {
            LoadingScreenScript.requestedLevel = "TitleMenu";
            SceneManager.LoadScene("NeutralLoader");
        }

        if (DoubleTime.ScaledTimeSinceLoad >= 45)
        {
            bye = true;
            StartCoroutine(Bye());
        }
    }
}
