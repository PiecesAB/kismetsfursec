using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeDoorInput : GenericBlowMeUp, IExaminableAction
{
    public string prompt;
    public string code;
    public int attempts;
    public int minLength;
    public int maxLength;
    public GameObject keyboardMenuPrefab;
    public GameObject dialogZone;
    public GameObject failDialog;
    public GameObject permFailDialog;
    public GameObject passDialog;
    public Door1 doorToUnlock;
    public bool caseSensitive = false;
    public bool solved = false;

    private IEnumerator KeyboardListen(GameObject kmen, float ts)
    {
        while (kmen.activeSelf) { yield return new WaitForEndOfFrame(); }
        Utilities.ResumeTime(ts);
        if (KeyboardController.result == "") { yield break; }

        GameObject ne = null;
        string res = KeyboardController.result;
        res = caseSensitive ? res : res.ToLower();
        code = caseSensitive ? code : code.ToLower();
        if (res == code)
        {
            ne = Instantiate(passDialog);
            solved = true;
            if (doorToUnlock)
            {
                --doorToUnlock.x;
            }
            GetComponent<BoxCollider2D>().offset = new Vector2(999999, 999999);
        }
        else
        {
            if (attempts > 1)
            {
                ne = Instantiate(failDialog);
                MainTextsStuff.insertableIntValue1 = --attempts;
            }
            else
            {
                ne = Instantiate(permFailDialog);
                GetComponent<BoxCollider2D>().offset = new Vector2(999999, 999999);
                BlowMeUp(0.05f, true);
            }
        }
        ne.transform.SetParent(GameObject.FindGameObjectWithTag("DialogueArea").transform, false);
        ne.SetActive(true);

        yield return null;
    }

    public void OnExamine(Encontrolmentation plr)
    {
        if (solved || attempts <= 0 || Time.timeScale == 0) { return; }
        float ts = Utilities.StopTime();
        KeyboardController.censor = false;
        KeyboardController.defaultText = prompt;
        KeyboardController.minLength = minLength;
        KeyboardController.maxLength = maxLength;
        KeyboardController.canClose = true;
        KeyboardController.result = "";
        GameObject kmen = Instantiate(keyboardMenuPrefab);
        kmen.transform.SetParent(dialogZone.transform);
        kmen.transform.localScale = Vector3.one;
        kmen.transform.position = new Vector3(320, 240);
        kmen.GetComponent<KeyboardController>().Open(plr);
        StartCoroutine(KeyboardListen(kmen, ts));
    }
}
