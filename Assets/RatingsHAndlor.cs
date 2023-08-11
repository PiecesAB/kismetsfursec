using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RatingsHAndlor : MonoBehaviour {

    public Image star;
    public Image descriptorBox;
    public Image ArabicTens;
    public Image ArabicOnes;
    public Image ArabicDecim;
    public Image KGOnes;
    public Image KGDecim;
    public Image descriptorLetter1;
    public Image descriptorLetter2;
    public Image descriptorLetter3;
    public Image descriptorLetter4;

    public Sprite[] arabicDigits;
    public Sprite[] KGdigits;

    public Sprite[] descriptors;

    [SerializeField]
    private Gradient ratingColors;

    private LevelInfoContainer it = null;

    void Start()
    {
        it = FindObjectOfType<LevelInfoContainer>();
    }

	void Update () {
        if (it != null)
        {
            star.color = descriptorBox.color = ratingColors.Evaluate(it.rating / 32f);

            //arabic number changer
            #region
            if (it.rating >= 20)
            {
                ArabicTens.sprite = arabicDigits[1];
            }
            else
            {
                ArabicTens.sprite = arabicDigits[0];
            }

            ArabicOnes.sprite = arabicDigits[(it.rating / 2) % 10];
            ArabicDecim.sprite = arabicDigits[(it.rating % 2)+10];
            #endregion

            //sub number changer
            KGOnes.sprite = KGdigits[it.rating / 2];
            KGDecim.sprite = KGdigits[(it.rating%2)*8];

            //descriptor changer
            descriptorLetter1.sprite = descriptors[(int)it.descriptors[0]];
            descriptorLetter2.sprite = descriptors[(int)it.descriptors[1]];
            descriptorLetter3.sprite = descriptors[(int)it.descriptors[2]];
            descriptorLetter4.sprite = descriptors[(int)it.descriptors[3]];


        }
    }
	
}
