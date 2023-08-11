using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuConfirmDeletions : MenuTwoChoice
{
    public InSaveMenuBase nextMenu;
    private bool confirmed;
    public Text replaceText2;
    public bool certaintyCheck = false;
    public bool resetStoryProgress = false;
    public GameObject keyboardMenuPrefab;

    public AudioSource wipeSound;

    private IEnumerator KeyboardListen(GameObject kmen)
    {
        while (kmen.activeSelf) { yield return new WaitForEndOfFrame(); }
        if (KeyboardController.result.ToLower() != "i hate myself") { Close(); yield break; }

        //confirmed = true;
        SceneManager.LoadScene("DeleteAllConfirmation");

        yield return null;
    }

    protected override void MakeSelection()
    {
        if (selection == 0) { backSound.Stop(); backSound.Play(); Close(); return; }
        //1

        if (keyboardMenuPrefab != null)
        {
            KeyboardController.censor = false;
            KeyboardController.defaultText = "Please input \"I hate myself\" to proceed.";
            KeyboardController.minLength = 0;
            KeyboardController.maxLength = 40;
            KeyboardController.canClose = true;
            KeyboardController.result = "";
            GameObject kmen = Instantiate(keyboardMenuPrefab);
            kmen.transform.SetParent(transform.parent);
            kmen.transform.position = transform.position;
            kmen.GetComponent<KeyboardController>().Open(myControl);
            StartCoroutine(KeyboardListen(kmen));
        }
        else if (resetStoryProgress)
        {
            Utilities.ResetStoryProgress();
            GameObject ws2 = Instantiate(wipeSound.gameObject, null);
            Destroy(ws2, 1f);
            ws2.GetComponent<AudioSource>().Play();
            Close();
        }
        else
        {
            confirmed = true;
            nextMenu.Open(myControl);
        }
        
    }

    private string GetString2()
    {
        string s = "";
        TimeSpan t = DateTime.Now - Utilities.toSaveData.dateTimeCreated;
        int hours = Mathf.FloorToInt((float)t.TotalHours);

        if (hours >= 1)
        {
            s += hours + " hour" + ((hours == 1) ? "" : "s") + " of progress";
        }

        if (Utilities.toSaveData.totalScore >= 1000)
        {
            if (s != "") { s += " and "; }
            s += "$" + Utilities.toSaveData.totalScore + " total";
        }

        if (s == "") { s = "everything you see here"; }

        return s;
    }

    protected override void ChildOpen()
    {
        selection = 0;
        UpdateArrow();
        confirmed = false;

        if (replaceText2 != null)
        {
            replaceText2.text = replaceText2.text.Replace("<A>", GetString2());
        }
    }

    protected override void ChildUpdate()
    {
        if (myControl.allowUserInput && confirmed) { Close(); return; }

        if (myControl.ButtonDown(1UL, 3UL) || myControl.ButtonDown(2UL, 3UL))
        {
            selection = (selection == 0) ? 1 : 0;
            if (changeSound) { changeSound.Stop(); changeSound.Play(); }
        }

        if (certaintyCheck && selection == 1 && Fakerand.Int(1,7) == 4)
        {
            selection = 0;
        }

        if (myControl.ButtonDown(16UL, 16UL)) {
            if (submitSound)
            {
                GameObject ss2 = Instantiate(submitSound.gameObject, null);
                Destroy(ss2, 1f);
                ss2.GetComponent<AudioSource>().Play();
            }
            MakeSelection();
        }
        UpdateArrow();
    }
}
