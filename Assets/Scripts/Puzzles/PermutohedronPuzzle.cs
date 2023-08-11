using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PermutohedronPuzzle : MonoBehaviour
{
    [SerializeField]
    private Sprite[] sprites = new Sprite[24];

    [SerializeField]
    private SpriteRenderer backSprite;
    private Color correctColor = new Color(0f, 0.5f, 1f);
    private Color failColor = Color.red;


    private int[] spritePermutes = {
        1234,1243,1324,1342,1423,1432,
        2134,2143,2314,2341,2413,2431,
        3124,3142,3214,3241,3412,3421,
        4123,4132,4213,4231,4312,4321,
    };
    [SerializeField]
    private GameObject sample;

    [SerializeField]
    private Door1 levelDoor;

    private Color backSpriteMainColor;

    [SerializeField]
    private int currPermute = 1234;
    private int total = 24;
    private float[] radii = new float[3] { 22f, 44f, 66f };
    private int[] ringCounts = new int[3] { 4, 8, 12 };

    private GameObject[] objects = new GameObject[24];
    private GameObject[] invariantObjects;

    private List<int>[] objectRings = { new List<int>(), new List<int>(), new List<int>()};

    private HashSet<int> chosenPermutes = new HashSet<int>() { };

    private static bool clear = false;

    private void InitObjects()
    {
        for (int i = 0; i < total; ++i)
        {
            objects[i] = (i > 0)?Instantiate(sample):sample;
            SpriteRenderer newSamp = objects[i].GetComponent<SpriteRenderer>();
            newSamp.sprite = sprites[i];
            Transform newTr = newSamp.transform;
            newTr.SetParent(transform, false);
            newTr.localPosition = Vector3.zero;
        }
        invariantObjects = objects.ToArray();
    }

    private void UpdateRings()
    {
        for (int a = 0; a < ringCounts.Length; ++a)
        {
            int rc = ringCounts[a];
            float r = radii[a];
            float speed = 8f + 12f*Mathf.PerlinNoise((float)((DoubleTime.ScaledTimeSinceLoad*0.4f) % 1000000.0), a * 100);
            for (int i = 0; i < rc; ++i)
            {
                float angle = Mathf.PI * 2f * i / rc;
                
                angle += (((a % 2) == 0)?speed:-speed) * (float)(DoubleTime.ScaledTimeSinceLoad % (10f - a)) / r;
                objects[objectRings[a][i]].transform.localPosition = 
                    Vector3.MoveTowards(objects[objectRings[a][i]].transform.localPosition,
                    new Vector3(r * Mathf.Cos(angle), r * Mathf.Sin(angle)),
                    1f);
            }
        }
    }

    private void InitRings()
    {
        int[] shuffled = Enumerable.Range(0,total).OrderBy(x => { return Fakerand.Int(0, 1000); }).ToArray();
        for (int i = 0; i < ringCounts[0]; ++i) { objectRings[0].Add(shuffled[i]); }
        for (int i = ringCounts[0]; i < ringCounts[0] + ringCounts[1]; ++i) { objectRings[1].Add(shuffled[i]); }
        for (int i = ringCounts[0] + ringCounts[1]; i < total; ++i) { objectRings[2].Add(shuffled[i]); }
        UpdateRings();
    }

    private void Fail()
    {
        chosenPermutes.Clear();
        for (int i = 0; i < invariantObjects.Length; ++i)
        {
            invariantObjects[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
        backSprite.color = failColor;
    }

    private void PermuteChanged(bool failed = false)
    {
        if (chosenPermutes.Contains(currPermute)) { Fail(); PermuteChanged(true); return; }

        chosenPermutes.Add(currPermute);
        for (int i = 0; i < spritePermutes.Length; ++i)
        {
            if (currPermute == spritePermutes[i]) { invariantObjects[i].GetComponent<SpriteRenderer>().color *= 0.55f; }
        }

        if (!failed) { backSprite.color = correctColor; }

        levelDoor.SetNumber(spritePermutes.Length - chosenPermutes.Count);

        if (chosenPermutes.Count == spritePermutes.Length) { clear = true; levelDoor.Open(); }
    }

    public void Swap(int a, int b)
    {
        if (clear) { print("stop! you already won"); }
        if (a == b) { print("ooo ghost!!"); }
        if (a < 1 || a > 4 || b < 1 || b > 4) { throw new System.Exception("can't swap out of range"); }
        List<int> permToList = new List<int>{ currPermute / 1000, (currPermute / 100) % 10, (currPermute / 10) % 10, currPermute % 10 };
        int idxa = permToList.IndexOf(a); if (idxa == -1) { throw new System.Exception("number isn't there??"); }
        int idxb = permToList.IndexOf(b); if (idxb == -1) { throw new System.Exception("number isn't there??"); }
        int temp = permToList[idxa];
        permToList[idxa] = permToList[idxb];
        permToList[idxb] = temp;
        currPermute = 1000 * permToList[0] + 100 * permToList[1] + 10 * permToList[2] + permToList[3];
        PermuteChanged();
    }

    void Start()
    {
        backSpriteMainColor = backSprite.color;
        clear = false;
        InitObjects();
        InitRings();
        PermuteChanged();
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }
        UpdateRings();

        float minRat = 1f / (total * 180f);
        for (int i = 0; i < total; ++i)
        {
            int rand1 = Fakerand.Int(0, total);
            if (Fakerand.Single() < minRat && rand1 != i)
            {
                GameObject temp = objects[i];
                objects[i] = objects[rand1];
                objects[rand1] = temp;
            }
        }

        if (clear) { backSprite.color = correctColor; }
        else { backSprite.color = Color.Lerp(backSprite.color, backSpriteMainColor, 0.05f); }
    }
}
