using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiteratureMenu : InSaveMenuBase
{
    [HideInInspector]
    public string textLink = "";
    [HideInInspector]
    public string imageLink = "";
    [HideInInspector]
    public string titleText = "";

    private TextAsset fullText;
    private Sprite coverSprite;

    [SerializeField]
    private Image[] pageImages; // 0 is left, 1 is right
    [SerializeField]
    private Text[] pageMainTexts;
    [SerializeField]
    private Text[] pageNumbers;
    [SerializeField]
    private Text title;

    [SerializeField]
    private int currPage;
    private int bookLength;

    private int fastTurnCooldown = 0;

    private List<string> pageStrings;

    private const int maxHeight = 170;

    public AudioSource pageTurnSound;

    private void RenderBook()
    {
        int leftPage = currPage;
        int rightPage = currPage + 1;

        if (leftPage == 0)
        {
            pageMainTexts[0].text = "";
            pageImages[0].sprite = coverSprite;
            pageImages[0].color = Color.white;
            pageNumbers[0].text = "";
        }
        else
        {
            pageMainTexts[0].text = pageStrings[leftPage - 1];
            pageImages[0].sprite = null;
            pageImages[0].color = new Color(0.93f, 0.93f, 0.93f);
            pageNumbers[0].text = leftPage.ToString();
        }

        if (rightPage >= bookLength)
        {
            pageMainTexts[1].text = "";
            pageImages[1].color = Color.clear;
            pageNumbers[1].text = "";
        }
        else
        {
            pageMainTexts[1].text = pageStrings[rightPage - 1];
            pageImages[1].color = new Color(0.93f, 0.93f, 0.93f);
            pageNumbers[1].text = rightPage.ToString();
        }
    }

    protected override void ChildOpen()
    {
        fullText = Resources.Load<TextAsset>(textLink);
        coverSprite = Resources.Load<Sprite>(imageLink);
        title.text = titleText;

        pageStrings = new List<string>();
        string fullTextStr = fullText.text;
        string lastThisPageText = "";
        string thisPageText = "";
        string thisToAddText = "";
        for (int c = 0; c < fullTextStr.Length; ++c)
        {
            thisToAddText += fullTextStr[c];
            if (fullTextStr[c] != ' ' && fullTextStr[c] != '\n') { continue; }
            thisPageText += thisToAddText;
            pageMainTexts[0].text = thisPageText;
            if (pageMainTexts[0].preferredHeight >= maxHeight)
            {
                pageStrings.Add(lastThisPageText);
                lastThisPageText = "";
                thisPageText = thisToAddText;
                thisToAddText = "";
            }
            else
            {
                thisToAddText = "";
                lastThisPageText = thisPageText;
            }
        }
        pageStrings.Add(thisPageText + thisToAddText);
        pageMainTexts[0].text = "";

        currPage = 0;
        bookLength = 1 + pageStrings.Count;

        RenderBook();
    }

    private bool TurnLeft()
    {
        double a;
        return myControl.ButtonDown(256UL, 512UL + 256UL) || (myControl.ButtonHeld(256UL, 512UL + 256UL, 0.5f, out a) && fastTurnCooldown == 0);
    }

    private bool TurnRight()
    {
        double a;
        return myControl.ButtonDown(512UL, 512UL + 256UL) || (myControl.ButtonHeld(512UL, 512UL + 256UL, 0.5f, out a) && fastTurnCooldown == 0);
    }

    protected override void ChildUpdate()
    {
        --fastTurnCooldown;
        if (fastTurnCooldown < 0) { fastTurnCooldown = 0; }

        if (TurnRight())
        {
            if (currPage + 2 < bookLength)
            {
                pageTurnSound.Stop(); pageTurnSound.Play();
                currPage += 2;
                fastTurnCooldown = 10;
                RenderBook();
                return;
            }
        }

        if (TurnLeft())
        {
            if (currPage - 2 >= 0)
            {
                pageTurnSound.Stop(); pageTurnSound.Play();
                currPage -= 2;
                fastTurnCooldown = 10;
                RenderBook();
                return;
            }
        }

        //RenderBook();
    }
}
