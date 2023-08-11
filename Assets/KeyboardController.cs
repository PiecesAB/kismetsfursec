using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Text.RegularExpressions;

public class KeyboardController : InSaveMenuBase {

    public RawImage cursor;
    public Transform cursorCollider;
    public Vector2 cursorSize;
    public float sizeMultiplier = 16f;
    public float originalSpeed;
    public float velocity;
    public Transform keyboard;
    private float currentSpeed;
    public string letterToNextInput;
    public Text nameBox;
    public Text message;
    public GameObject registerButton;
    private string origMsg;
    private Vector2 cursColOrigPos;
    public bool masterCensorList = false;
    public string[] filterACensored;
    public string[] filterBCensored;

    public AudioSource letterChangeSound;
    public AudioSource pressSound;
    public AudioSource submitSound;

    public static string defaultText = "Default message";
    public static bool censor = false;
    public static int minLength = 3;
    public static int maxLength = 32;

    private string lastLetter = "";

    public static string result;
    public static bool canClose = false;

	protected override void ChildOpen () {
        canCloseByPlayer = canClose;
        currentSpeed = originalSpeed;
        cursColOrigPos = cursorCollider.transform.localPosition;
        origMsg = defaultText;
        if (masterCensorList)
        {
            foreach (KeyboardController i in Resources.FindObjectsOfTypeAll<KeyboardController>())
            {
                i.filterACensored = filterACensored;
            }
        }
	}

    string RemoveDiacritic(string s)
    {
        s = s.Normalize(NormalizationForm.FormD);
        var sb0 = new StringBuilder();
        foreach (char c in s)
        {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
                sb0.Append(c);
        }
        return sb0.ToString();
    }

    string Simplify4Filter(string s)
    {
        string s1 = s;

        if (s1.Length > 2)
        {
            s1 = RemoveDiacritic(s1);
            s1 = s1.Replace("ẞ", "b").Replace("Þ", "d");
            s1 = s1.Replace("0", "o").Replace("1", "i").Replace("2", "z").Replace("3", "e").Replace("4", "a");
            s1 = s1.Replace("5", "s").Replace("6", "g").Replace("7", "t").Replace("8", "b").Replace("9", "g");
            s1 = s1.Replace("l", "i").Replace("j", "i").Replace("y", "i").Replace("kh", "c");
            s1 = s1.Replace("!", "i").Replace("@", "a").Replace("$", "s").Replace("&", "b").Replace("k", "c").Replace("q", "c").Replace("z", "s");
            s1 = s1.Replace("vv", "w").Replace("w", "u").Replace("v", "u").ToLowerInvariant();
            StringBuilder sb1 = new StringBuilder();
            sb1.Append(s1[0]);
            for (int i = 1; i < s1.Length; i++)
            {
                if (s1[i] != s1[i-1])
                {
                    sb1.Append(s1[i]);
                }
            }
            s1 = sb1.ToString();
            s1 = s1.Replace("ph", "f");
            s1 = s1.Replace("æ", "ae").Replace("ø", "o").Replace("œ", "oe").Replace("ß", "b").Replace("ſ", "f").Replace("þ", "b").Replace("ð", "d").Replace("µ", "u");
            s1 = s1.Replace(".", "").Replace(",", "").Replace("-", "").Replace("'", "").Replace(":", "").Replace(";", "").Replace("\"", "").Replace("_", "");
            s1 = s1.Replace("^", "").Replace("*", "").Replace("`", "").Replace(" ", "");
        }
        return s1;
    }

    string MakeRandomName()
    {
        return "random";
    }

    int AllLettersSame(string s)
    {
        if (s.Length > 2)
        {
            int ret = 0;
            for (int te = 0; te <= 1; te++)
            {
                for (int i = 1; i < s.Length; i++)
                {
                    if (s[i] != s[0])
                    {
                        ret = te + 1;
                        break;
                    }
                }
                if (ret == 1)
                {
                    s = Simplify4Filter(s);
                }
                else
                {
                    break;
                }
            }
            return ret;
        }
        else
        {
            return 2;
        }
    }

    int SimpleCheckB(string s)
    {
        if (s.Length > 2)
        {
            s = RemoveDiacritic(s.ToLowerInvariant());
            if ("1234567890".Contains(s) || "0123456789.-,'".Contains(s) || "abcdefghijklmnopqrstuvwxyz".Contains(s)
                || "',-.9876543210".Contains(s) || "zyxwvutsrqponmlkjihgfedcba".Contains(s))
            {
                if (s.Length >= 7)
                {
                    return 1;
                }
                return 0;
            }
        }
        return 2;
    }

    int FilterA(string s, string[] blacklist)
    {
        if (s.Length > 2)
        {
            s = s.ToLowerInvariant();
            for (int te = 0; te <= 1; te++)
            {
                for (int i = 0; i < blacklist.Length; i++)
                {
                    if (s.Contains((te == 1) ? Simplify4Filter(blacklist[i]) : blacklist[i]))
                    {
                        return te;
                    }
                }
                if (te == 0)
                {
                    s = Simplify4Filter(s);
                }
            }
        }
        return 2;
    }
	
	protected override void ChildUpdate () {
        float dirx = 0f;
        float diry = 0f;

        Encontrolmentation e = myControl;

        if ((e.currentState & 15UL) != 0UL) //any d-pad pressed
        {
            currentSpeed += 0.01666666f * velocity;
        }
        else //no d-pad pressed
        {
            currentSpeed = originalSpeed;
        }


        if ((e.currentState & 3UL) == 1UL) //left out of left and right
        {
            dirx = currentSpeed;
        }

        if ((e.currentState & 3UL) == 2UL) //right out of left and right
        {
            dirx = -currentSpeed;
        }

        if ((e.currentState & 12UL) == 4UL) //up out of up and down
        {
            diry = -currentSpeed;
        }

        if ((e.currentState & 12UL) == 8UL) //down out of up and down
        {
            diry = currentSpeed;
        }

        if (Time.timeScale == 0)
        {
            Material m = cursor.material;
            m.SetFloat("_FakeTimeToAdd", (cursor.material.GetFloat("_FakeTimeToAdd") + 0.0166666666666f) % 1000f);
            cursor.material = m;
        }

        cursor.uvRect = new Rect (cursor.uvRect.position+new Vector2(dirx/cursorSize.x, diry/ cursorSize.y), cursor.uvRect.size);
        cursorCollider.localPosition = cursColOrigPos + new Vector2(cursorSize.x * (1f-cursor.uvRect.position.x), cursorSize.y * (1f-cursor.uvRect.position.y)) * sizeMultiplier;
        //normalize coordinates of the cursor to be in [0,1)
        cursor.uvRect = new Rect(new Vector2(Mathf.Repeat(cursor.uvRect.x,1f), Mathf.Repeat(cursor.uvRect.y, 1f)), cursor.uvRect.size);
        letterToNextInput = "";
        foreach (Transform key in keyboard)
        {
            if (key.gameObject != cursor.gameObject && key.gameObject != cursorCollider.gameObject && key != transform)
            {
                
                if (key.gameObject.activeInHierarchy && System.Math.Abs(cursorCollider.transform.localPosition.x - key.transform.localPosition.x) < key.GetComponent<RectTransform>().sizeDelta.x*0.5f && System.Math.Abs(cursorCollider.transform.localPosition.y - key.transform.localPosition.y) < key.GetComponent<RectTransform>().sizeDelta.y*0.5f)
                {
                    letterToNextInput = key.gameObject.name;
                    key.GetComponent<Image>().enabled = true;
                }
                else
                {
                    key.GetComponent<Image>().enabled = false;
                }
            }
        }
        if (lastLetter != letterToNextInput && letterToNextInput != "")
        {
            letterChangeSound.Stop();
            letterChangeSound.Play();
        }
        lastLetter = letterToNextInput;

        if (e.ButtonDown(16UL,16UL) ) //A press
        {
            if (letterToNextInput != "")
            {
                pressSound.Stop();
                pressSound.Play();
                if (letterToNextInput.Length == 1) // 1 letter. normal
                {
                    if (nameBox.text.Length < maxLength)
                    {
                        nameBox.text += letterToNextInput;
                    }
                }
                else
                {
                    switch (letterToNextInput)
                    {
                        case "space":
                        if (nameBox.text.Length < maxLength)
                        {
                            nameBox.text += " ";
                        }
                        break;
                        case "back":
                            nameBox.text = nameBox.text.Substring(0, Mathf.Max(nameBox.text.Length - 1,0));
                            break;
                        case "clear":
                            nameBox.text = "";
                            break;
                        case "random":
                            nameBox.text = MakeRandomName();
                            break;
                        case "register":
                            result = nameBox.text;
                            GameObject subSnd2 = Instantiate(submitSound.gameObject, null);
                            subSnd2.GetComponent<AudioSource>().Play();
                            Destroy(subSnd2, 1f);
                            Close();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        //more if statements above, higher priority
        /*else*/
        int filterAResult = censor ? FilterA(nameBox.text, filterACensored) : 2;
        int filterBResult = censor ? FilterA(nameBox.text, filterBCensored) : 2;
        int allSameResult = censor ? AllLettersSame(nameBox.text) : 2;
        int simpBResult = censor ? SimpleCheckB(nameBox.text) : 2;

        if (nameBox.text.Length < minLength)
        {
            message.text = origMsg + " (" + (minLength-nameBox.text.Length) + " more chars)";
            registerButton.SetActive(false);
        }
        else if (filterBResult != 2)
        {
            if (filterBResult == 0)
            {
                message.text = "<color=red>You can't have that name!</color>";
            }
            if (filterBResult == 1)
            {
                message.text = "<color=red>You can't have that name! (Filterpass?)</color>";
            }
            registerButton.SetActive(false);
        }
        else if (filterAResult != 2)
        {
            if (filterAResult == 0)
            {
                message.text = "<color=red>You can't have that name.</color>";
            }
            if (filterAResult == 1)
            {
                message.text = "<color=red>You can't have that name. (Filterpass?)</color>";
            }
            registerButton.SetActive(false);
        }
        else if (nameBox.text.ToLowerInvariant() == "claire")
        {
            message.text = "<color=red>You should stop asking about the sisters.</color>";
            registerButton.SetActive(false);
        }
        else if (nameBox.text.ToLowerInvariant() == "who is claire")
        {
            message.text = "<color=red>Claire exists now, and look what we have to deal with.</color>";
            registerButton.SetActive(false);
        }
        else if (allSameResult != 2)
        {
            if (allSameResult == 0)
            {
                message.text = "<color=red>Too simple</color>";
            }
            if (allSameResult == 1)
            {
                message.text = "<color=red>Still too simple</color>";
            }
            registerButton.SetActive(false);
        }
        else if (simpBResult != 2)
        {
            if (simpBResult == 0)
            {
                message.text = "<color=red>Too simple</color>";
            }
            if (simpBResult == 1)
            {
                message.text = "<color=red>Still too simple</color>";
            }
            registerButton.SetActive(false);
        }
        else if (nameBox.text.Length >= maxLength)
        {
            message.text = "<color=yellow>"+nameBox.text.Length + "/" + maxLength.ToString() + " chars</color>";
            registerButton.SetActive(true);
        }
        else
        {
            message.text = origMsg;
            registerButton.SetActive(true);
        }

        //filter name

    }
}
