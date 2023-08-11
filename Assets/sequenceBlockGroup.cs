using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class sequenceBlockGroup : MonoBehaviour
{
    [Header("Make sure this is 6 long")]
    [Range(0,10)]
    public int[] multiplicities = new int[6];
    [Header("CHECK BELOW TO STOP THE REFRESHING")]
    public bool lockRefreshEditor = false;

    public GameObject seqBlockPrefab;

    public Sprite[] pipSpritesFancy = new Sprite[6];
    public Sprite[] pipSpritesPlain = new Sprite[6];

    public int current = 0;
    public string currAsString = "0";

    public int sameTouchedNumber;

    public float timeToBeatNumber;

    public int loseFlag = 0;

    public bool won = false;

    private int[] multOld = new int[6];

    private GameObject[,] oneBlocks;

    public bool collidedThisFrame = false;

    public AudioClip correctSound;
    public AudioClip advanceSound;
    public AudioClip failSound;
    public AudioClip winSound;

    void Start()
    {
        if (Application.isPlaying)
        {
            loseFlag = 0;
            won = false;
            collidedThisFrame = false;
        }
    }

    void UpdateCurrent(int x)
    {
        AudioSource asr = GetComponent<AudioSource>();
        current = x;
        currAsString = x.ToString();
        //win condition
        if (current == 6 || multiplicities[current] == 0)
        {
            won = true;
            asr.Stop();
            asr.clip = winSound;
            asr.pitch = 1f;
            asr.Play();
        }
        else if (x > 0)
        {
            asr.Stop();
            asr.clip = advanceSound;
            asr.pitch = Mathf.Pow(2f, (current - 1f) * 0.08333333f);
            asr.Play();
        }
        else
        {
            asr.Stop();
            asr.clip = failSound;
            asr.pitch = Mathf.Pow(2f, (current - 1f) * 0.08333333f);
            asr.Play();
        }
    }

    void Lose()
    {
        UpdateCurrent(0);
        sameTouchedNumber = 0;
        loseFlag = 25;
    }

    public void ChildCollided(SequenceBlockSelf s)
    {
        if (!s.collidedAlready && !collidedThisFrame)
        {
            collidedThisFrame = true;
            if (current == s.value)
            {
                s.collidedAlready = true;
                sameTouchedNumber++;

                AudioSource asr = GetComponent<AudioSource>();
                if (sameTouchedNumber == multiplicities[current])
                {
                    UpdateCurrent(current + 1);
                    sameTouchedNumber = 0;

                    
                }
                else
                {
                    asr.Stop();
                    asr.clip = correctSound;
                    asr.pitch = 1f;
                    asr.Play();
                }

                //print("Current: " + current + " Touched: " + sameTouchedNumber);
            }
            else if (!(current == 0 && sameTouchedNumber == 0))
            {
                Lose();
            }
        }

    }

    void RegenBlocks()
    {
        List<Transform> transform2 = transform.Cast<Transform>().ToList();
        foreach (Transform child in transform2)
        {
            if (child != transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }
        oneBlocks = new GameObject[6, 10];
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < multiplicities[i]; j++)
            {
                GameObject nb = Instantiate(seqBlockPrefab, transform);
                nb.transform.localPosition = new Vector3(i * 16, j * 16, 0);
                nb.transform.localRotation = Quaternion.identity;

                SequenceBlockSelf sbs = nb.GetComponent<SequenceBlockSelf>();
                sbs.hub = this;
                sbs.defaultIcon = pipSpritesFancy[i];
                sbs.value = i;

                Transform nbicon = nb.transform.GetChild(0);
                nbicon.GetComponent<SpriteRenderer>().sprite = pipSpritesFancy[i];
                foreach (Transform bg in nbicon)
                {
                    if (bg != nbicon)
                    {
                        bg.GetComponent<SpriteRenderer>().sprite = pipSpritesPlain[i];
                    }
                }
                oneBlocks[i, j] = nb;
            }
        }
    }

    void Update()
    {
        if (loseFlag > 0)
        {
            loseFlag--;
            //print("LOSE!");
        }

        if (Application.isPlaying)
        {
            //print("Current: "+ current + " Touched: " + sameTouchedNumber);
            collidedThisFrame = false;
        }
        else
        {
            if (multiplicities.Length != 6)
            {
                print("no no, keep it 6");
                multiplicities = new int[6];
            }

            if (!multOld.SequenceEqual(multiplicities))
            {
                //regenerate blocks
                if (!lockRefreshEditor)
                {
                    RegenBlocks();
                }

                multiplicities.CopyTo(multOld, 0);
            }
        }
    }
}
