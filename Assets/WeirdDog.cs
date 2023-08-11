using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeirdDog : MonoBehaviour, IChoiceUIResponse
{
    public static string result;

    public AmbushController boltAmbush;
    public GameObject firstDialog;
    public GameObject secondDialog;
    public PrimExaminableItem secondDialogPEI;
    public Door1 mainDoor1;

    private Animator anim;

    private bool boltsMade = false;
    private bool solved = false;

    public GameObject ChoiceResponse(string text)
    {
        result = text;
        return null;
    }

    private void Start()
    {
        result = "";
        anim = GetComponent<Animator>();
    }

    private void Update()
    {

        if (result == "no")
        {
            anim.SetTrigger("Mad");
        }

        if (Time.timeScale != 0)
        {
            if (result != "" && !boltsMade)
            {
                boltAmbush.Activate();
                boltsMade = true;
            }

            result = "";
            anim.SetTrigger("BackToNormal");

            if (boltAmbush.statusMessage == AmbushController.WIN_AMBUSH_MESSAGE && !solved && !Door1.levelComplete)
            {
                firstDialog.transform.localPosition = new Vector3(0, 9999, 0);
                secondDialog.transform.localPosition = Vector3.zero;
                Utilities.AddAction("1337bolts");
                mainDoor1.trivialCompletion = false;
                solved = true;
            }
            if (secondDialogPEI == null)
            {
                GetComponent<ShrinkOut>().Bye();
            }
        }
    }
}
