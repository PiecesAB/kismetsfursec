using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class InSaveMenuBase : MonoBehaviour
{
    protected Encontrolmentation mainControl;
    protected Encontrolmentation myControl;

    public bool canCloseByPlayer = true;
    public AudioSource openSound;
    public AudioSource backSound;

    private bool lastFrameAllowInput = false;

    private void Awake()
    {
        myControl = GetComponent<Encontrolmentation>();
        myControl.allowUserInput = lastFrameAllowInput = false;
        //Open(); Testing
    }

    protected abstract void ChildOpen();

    public void Open(Encontrolmentation otherControl = null)
    {
        gameObject.SetActive(true);
        if (otherControl) { mainControl = otherControl; }
        if (mainControl) { mainControl.allowUserInput = false; }
        myControl.allowUserInput = true;
        if (openSound) { openSound.Stop(); openSound.Play(); }
        ChildOpen();
    }

    public void Close()
    {
        if (mainControl) { mainControl.allowUserInput = true; }
        myControl.allowUserInput = false;
        ChildClose();
        gameObject.SetActive(false);
    }

    protected abstract void ChildUpdate();

    protected virtual void ChildClose()
    {

    }

    public void Update()
    {
        if (lastFrameAllowInput)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0 && !(this is SecondaryTitleMenu)) // for settings or gallery in the title menu
            {
                canCloseByPlayer = false;
            }

            if ((myControl.ButtonDown(32UL, 32UL) || myControl.ButtonDown(1024UL, 1024UL)) && canCloseByPlayer)
            {
                if (this is KeyboardController)
                {
                    KeyboardController.result = "";
                }
                if (this is IconSelectController)
                {
                    IconSelectController.result = -1;
                }
                if (backSound) {
                    GameObject bs2 = Instantiate(backSound.gameObject, null);
                    Destroy(bs2, 1f);
                    bs2.GetComponent<AudioSource>().Play();
                }
                Close();
            }

            ChildUpdate();
        }
        lastFrameAllowInput = myControl.allowUserInput;
    }
}
