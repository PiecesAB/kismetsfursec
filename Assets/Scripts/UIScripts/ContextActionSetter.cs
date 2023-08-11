using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;

public class ContextActionSetter : MonoBehaviour {

    private Encontrolmentation controlObj;
    public Text eventAText;
    public Image eventAImage;
    public Text eventBText;
    public Image eventBImage;

    private string oldAText;
    private string oldBText;

    public Sprite AButton;
    public Sprite BButton;
    public Sprite XButton;
    public Sprite YButton;
    public Sprite LButton;
    public Sprite RButton;
    public Sprite DPad;

    private Color origAColor;
    private Color origBColor;

    //private static ContextActionSetter main;

    Sprite ConvertEnum(Encontrolmentation.ActionButton a)
    {
        switch (a)
        {
            case Encontrolmentation.ActionButton.AButton:
                return AButton;
            case Encontrolmentation.ActionButton.BButton:
                return BButton;
            case Encontrolmentation.ActionButton.XButton:
                return XButton;
            case Encontrolmentation.ActionButton.YButton:
                return YButton;
            case Encontrolmentation.ActionButton.LButton:
                return LButton;
            case Encontrolmentation.ActionButton.RButton:
                return RButton;
            case Encontrolmentation.ActionButton.DPad:
                return DPad;
            default:
                return null;
        }
    }

    private int counterA = 0;
    private int counterB = 0;

    private IEnumerator ChangedAShow()
    {
        int currCounter = ++counterA;
        int i = 6;
        while (i > 0 && currCounter == counterA)
        {
            eventAText.color = Color.Lerp(Color.magenta, origAColor, (6 - i) / 6f);
            yield return new WaitForEndOfFrame();
            --i;
        }
        if (currCounter == counterA)
        {
            eventAText.color = origAColor;
        }
        yield return null;
    }

    private IEnumerator ChangedBShow()
    {
        int currCounter = ++counterB;
        int i = 6;
        while (i > 0 && currCounter == counterB)
        {
            eventBText.color = Color.Lerp(Color.magenta, origBColor, (6 - i) / 6f);
            yield return new WaitForEndOfFrame();
            --i;
        }
        if (currCounter == counterB)
        {
            eventBText.color = origBColor;
        }
        yield return null;
    }

    private void Awake()
    {
        //main = this;
    }

    private void Start()
    {
        origAColor = eventAText.color;
        origBColor = eventBText.color;
        oldAText = oldBText = "";
    }

    private void Update () {
        controlObj = LevelInfoContainer.GetActiveControl();
        if (controlObj)
        {
            eventAImage.sprite = ConvertEnum(controlObj.eventAbutton);
            eventAText.text = controlObj.eventAName;
            if (controlObj.eventAName != oldAText)
            {
                oldAText = controlObj.eventAName;
                StartCoroutine(ChangedAShow());
            }
            eventBImage.sprite = ConvertEnum(controlObj.eventBbutton);
            eventBText.text = controlObj.eventBName;
            if (controlObj.eventBName != oldBText)
            {
                oldBText = controlObj.eventBName;
                StartCoroutine(ChangedBShow());
            }
        }
    }
}
