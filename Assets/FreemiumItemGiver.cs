using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreemiumItemGiver : MonoBehaviour, IChoiceUIResponse, IDialogGiverSetup
{
    public GenericBlowMeUp lockIcon;
    public BoxCollider2D unlockCollider;
    public BoxCollider2D openCollider;
    public int cost;

    public GameObject[] normalDialogs;
    public GameObject[] poorDialogs;

    private PrimExaminableItem pei;

    private IEnumerator Open()
    {
        yield return new WaitUntil(() => Time.timeScale > 0f);
        lockIcon.BlowMeUp();
        unlockCollider.offset = new Vector2(0, 999999);
        openCollider.offset = Vector2.zero;
        Utilities.ChangeScore(-cost);
        Destroy(this);
    }

    public void SetupDialog()
    {
        MainTextsStuff.insertableIntValue1 = cost;
    }

    public GameObject ChoiceResponse(string message)
    {
        if (message != "")
        {
            StartCoroutine(Open());
        }
        return null;
    }

    private void Start()
    {
        pei = GetComponent<PrimExaminableItem>();
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (Utilities.loadedSaveData.score >= cost)
        {
            pei.textboxesToGive = normalDialogs;
        }
        else
        {
            pei.textboxesToGive = poorDialogs;
        }
    }
}
