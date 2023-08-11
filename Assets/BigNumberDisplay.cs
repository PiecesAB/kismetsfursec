using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigNumberDisplay : MonoBehaviour, IDialSetter
{
    [Range(0,9999)]
    public int number;
    public Sprite[] numSprites = new Sprite[10];
    [Header("lower places first")]
    public SpriteRenderer[] places = new SpriteRenderer[4];

    private int lastNumber;

    public void DialChanged(float val)
    {
        ChangeNumber(val);
    }

    public void UpdateNumber()
    {
        int mul = 1;
        for (int p = 0; p < 4; ++p)
        {
            int sub = number / mul;
            if (p > 0 && sub == 0)
            {
                places[p].enabled = false;
                continue;
            }
            int digit = sub % 10;
            places[p].enabled = true;
            places[p].sprite = numSprites[digit];
            places[p].color = Color.Lerp(Utilities.colorCycle[digit], Color.yellow, 0.8f);

            mul *= 10;
        }
    }

    public void ChangeNumber(int n)
    {
        number = n;
    }

    public void ChangeNumber(float n)
    {
        number = Mathf.RoundToInt(n);
    }

    void Start()
    {
        UpdateNumber();   
    }

    void Update()
    {
        if (lastNumber != number)
        {
            if (number < 0) { number = 0; }
            UpdateNumber();
        }
        lastNumber = number;
    }
}
