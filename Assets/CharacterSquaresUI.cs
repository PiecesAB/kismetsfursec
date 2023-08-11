using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSquaresUI : MonoBehaviour
{
    public bool forceDisplay = false;
    public Sprite iconInactive;
    public Sprite iconActive;
    public Sprite iconDefunct;
    [Header("Extra icons is a special effect per level. For ghosts")]
    public int extraIcons = 0;
    
    [Header("Khal Tet Myst Rav")]
    public Color[] colors = new Color[4];

    private string[] names =
    { "Khal", "Tetra", "Myst", "Ravel" };

    private List<Image> icons = new List<Image>();
    public GameObject iconSample;

    private int iconsCounter;
    private const int iconsPerRow = 8;
    private const float iconSize = 12f;

    private static bool trigger = false;

    public static CharacterSquaresUI main;

    public static void Trigger()
    {
        trigger = true;
    }

    private void ClearIconsAndDisableThis()
    {
        for (int i = 0; i < icons.Count; ++i)
        {
            Destroy(icons[i].gameObject);
        }
        gameObject.SetActive(false);
    }

    private void MakeIcon(Sprite sp, Color col)
    {
        GameObject iconNew = Instantiate(iconSample, transform);
        iconNew.GetComponent<RectTransform>().localPosition = 
            new Vector3(iconSize * (iconsCounter % iconsPerRow), -iconSize * (iconsCounter / iconsPerRow));
        Image iconNewImage = iconNew.GetComponent<Image>();
        iconNewImage.sprite = sp;
        iconNewImage.color = col;

        icons.Add(iconNewImage);
        ++iconsCounter;
    }

    void Start()
    {
        main = this;
        iconsCounter = 0;

        if (!forceDisplay && LevelInfoContainer.allPlayersInLevel.Count < 2) { return; }

        int playables = 0;
        for (int i = 0; i < LevelInfoContainer.allPlayersInLevel.Count; ++i)
        {
            string n = LevelInfoContainer.allPlayersNames[i];
            Color thisColor = Color.Lerp(Color.white, Fakerand.Color(128), 0.5f);
            Sprite sp = iconDefunct;
            if (n != null)
            {
                for (int j = 0; j < names.Length; ++j)
                {
                    if (names[j] == n)
                    {
                        thisColor = colors[j];
                        sp = iconInactive;
                    }
                }
            }
            if (sp != iconDefunct) { ++playables; }
            MakeIcon(sp, thisColor);
        }
        if (!forceDisplay && playables < 2) { ClearIconsAndDisableThis(); return; }

        for (int i = 0; i < extraIcons; ++i)
        {
            MakeIcon(iconDefunct, Color.Lerp(Color.white, Fakerand.Color(128), 0.5f));
        }

        RealUpdate();
    }

    private void RealUpdate()
    {
        for (int i = 0; i < LevelInfoContainer.allCtsInLevel.Length; ++i)
        {
            Encontrolmentation e = LevelInfoContainer.allCtsInLevel[i];
            if (e) {
                if (e.allowUserInput)
                    icons[i].sprite = iconActive;
                else
                    icons[i].sprite = iconInactive;
            } else
                icons[i].sprite = iconDefunct;
        }
    }

    void Update()
    {
        if (trigger)
        {
            trigger = false;
            RealUpdate();
        }
    }
}
