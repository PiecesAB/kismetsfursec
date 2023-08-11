using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extension4DSpace;

public class TesseractPuzzle : MonoBehaviour
{
    [SerializeField]
    private Sprite[] axisSprites = new Sprite[8];
    private static Vector4[] axes = new Vector4[8] {
        Extension4D.kata, Extension4D.ana,
        Extension4D.back, Extension4D.forward,
        Extension4D.down, Extension4D.up,
        Extension4D.left, Extension4D.right };
    public SpriteRenderer rightSprite;
    public SpriteRenderer upSprite;
    public SpriteRenderer frontSprite;
    public SpriteRenderer anaSprite;
    public SpriteRenderer rightSpriteCorr;
    public SpriteRenderer upSpriteCorr;
    public SpriteRenderer frontSpriteCorr;
    public SpriteRenderer anaSpriteCorr;
    public bool isCorrect;
    public Transform leftGatePart;
    public Transform rightGatePart;

    public SpriteRenderer backdropSprite;

    private Vector3 leftGatePos;
    private Vector3 rightGatePos;

    private Color backdropMainColor;
    private Color backdropOneColor = new Color(0f, 0.2f, 0.6f);
    private Color backdropErrorColor = new Color(0.65f, 0f, 0f);

    private const float rotSpeed = 0.02f;

    private int switchMaskLong;
    private int switchMaskShort;

    private Matrix4x4[] rotors = new Matrix4x4[6] {
    new Matrix4x4(
        new Vector4(0, -1, 0, 0),
        new Vector4(1, 0, 0, 0),
        Extension4D.forward,
        Extension4D.ana
        ),
    new Matrix4x4(
        new Vector4(0, 0, -1, 0),
        Extension4D.up,
        new Vector4(1, 0, 0, 0),
        Extension4D.ana
        ),
    new Matrix4x4(
        new Vector4(0, 0, 0, -1),
        Extension4D.up,
        Extension4D.forward,
        new Vector4(1, 0, 0, 0)
        ),
    new Matrix4x4(
        Extension4D.right,
        new Vector4(0, 0, -1, 0),
        new Vector4(0, 1, 0, 0),
        Extension4D.ana
        ),
    new Matrix4x4(
        Extension4D.right,
        new Vector4(0, 0, 0, -1),
        Extension4D.forward,
        new Vector4(0, 1, 0, 0)
        ),
    new Matrix4x4(
        Extension4D.right,
        Extension4D.up,
        new Vector4(0, 0, 0, -1),
        new Vector4(0, 0, 1, 0)
        )
    };

    private MeshRend4DExtension mr4;

    [SerializeField]
    private MeshRend4DExtension correctSample;
    
    void Start()
    {
        backdropMainColor = backdropSprite.color;
        switchMaskLong = switchMaskShort = 0;
        mr4 = GetComponent<MeshRend4DExtension>();
        Matrix4x4 correctMatrix = Matrix4x4.identity;

        while (correctMatrix == Matrix4x4.identity)
        {
            int rotWays = Fakerand.Int(12, 15);
            int[] weights = new int[rotors.Length];
            for (int i = 0; i < rotWays; ++i)
            {
                ++weights[Fakerand.Int(0, rotors.Length)];
            }

            for (int i = 0; i < rotors.Length; ++i)
            {
                for (int j = 0; j < weights[i]; ++j)
                {
                    correctMatrix = rotors[i] * correctMatrix;
                }
            }
        }

        isCorrect = false;
        if (correctSample)
        {
            correctSample.extraRot = correctMatrix;
        }
        else
        {
            print(correctMatrix);
        }
        leftGatePos = leftGatePart.position;
        rightGatePos = rightGatePart.position;
    }

    // retrun true if change occured.
    private bool UpdateText(SpriteRenderer spr, Vector4 dir, MeshRend4DExtension mr)
    {
        float maxDot = -10f;
        int maxFace = 0;
        for (int i = 0; i < axes.Length; ++i)
        {
            float currDot = Vector4.Dot(dir, mr.extraRot*axes[i]);
            if (currDot > maxDot)
            {
                maxDot = currDot;
                maxFace = i;
            }
        }
        spr.color = maxDot * Color.white;
        float scala = Mathf.MoveTowards(spr.transform.localScale.x, 1f, 0.05f);
        spr.transform.localScale = new Vector3(scala, scala, 1f);
        if (spr.sprite != axisSprites[maxFace])
        {
            spr.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            spr.sprite = axisSprites[maxFace];
            return true;
        }
        return false;
    }

    void Update()
    {
        if (Time.timeScale == 0) { return; }

        UpdateText(rightSprite, Extension4D.right, mr4);
        UpdateText(upSprite, Extension4D.up, mr4);
        UpdateText(frontSprite, Extension4D.back, mr4);
        UpdateText(anaSprite, Extension4D.ana, mr4);
        UpdateText(rightSpriteCorr, Extension4D.right, correctSample);
        UpdateText(upSpriteCorr, Extension4D.up, correctSample);
        UpdateText(frontSpriteCorr, Extension4D.back, correctSample);
        UpdateText(anaSpriteCorr, Extension4D.ana, correctSample);
        isCorrect = (rightSprite.sprite == rightSpriteCorr.sprite 
            && frontSprite.sprite == frontSpriteCorr.sprite 
            && upSprite.sprite == upSpriteCorr.sprite
            && anaSprite.sprite == anaSpriteCorr.sprite);

        Vector3 newLeftPos = Vector3.MoveTowards(leftGatePart.position, leftGatePos, 1f);
        Vector3 newRightPos = Vector3.MoveTowards(rightGatePart.position, rightGatePos, 1f);
        if (isCorrect)
        {
            newLeftPos = Vector3.MoveTowards(leftGatePart.position, leftGatePos + new Vector3(48, 0, 0), 1f);
            newRightPos = Vector3.MoveTowards(rightGatePart.position, rightGatePos + new Vector3(-48, 0, 0), 1f);
        }

        Vector3 oldLeftPos = leftGatePart.position;
        leftGatePart.GetComponent<Rigidbody2D>().MovePosition(newLeftPos);
        leftGatePart.GetComponent<Rigidbody2D>().velocity = (newLeftPos - oldLeftPos) / Time.deltaTime;
        Vector3 oldRightPos = rightGatePart.position;
        rightGatePart.GetComponent<Rigidbody2D>().MovePosition(newRightPos);
        rightGatePart.GetComponent<Rigidbody2D>().velocity = (newRightPos - oldRightPos) / Time.deltaTime;

        switchMaskLong = (int)(Utilities.loadedSaveData.switchMask & 255u);
        switchMaskShort = (switchMaskLong & 15) ^ (switchMaskLong >> 4);

        for (int i = 0; i < mr4.rotVels.Length; ++i)
        {
            mr4.rotVels[i] = 0;
        }

        bool colChange = false;
        bool reverse = false;
        switch (switchMaskShort)
        {
            case 3: reverse = (switchMaskLong & 1) == 1; mr4.rotVels[0] = rotSpeed * (reverse ? 1f : -1f); break; //xy
            case 5: reverse = (switchMaskLong & 1) == 1; mr4.rotVels[1] = rotSpeed * (reverse ? 1f : -1f); break; //xz
            case 9: reverse = (switchMaskLong & 1) == 1; mr4.rotVels[2] = rotSpeed * (reverse ? 1f : -1f); break; //xw
            case 6: reverse = (switchMaskLong & 2) == 2; mr4.rotVels[3] = rotSpeed * (reverse ? 1f : -1f); break; //yz
            case 10: reverse = (switchMaskLong & 2) == 2; mr4.rotVels[4] = rotSpeed * (reverse ? 1f : -1f); break; //yw
            case 12: reverse = (switchMaskLong & 4) == 4; mr4.rotVels[5] = rotSpeed * (reverse ? 1f : -1f); break; //zw
            case 1: case 2: case 4: case 8:
                colChange = true;
                backdropSprite.color = Color.Lerp(backdropSprite.color, backdropOneColor, 0.2f);
                break;
            case 0: default:
                if (switchMaskLong != 0)
                {
                    colChange = true;
                    backdropSprite.color = Color.Lerp(backdropSprite.color, backdropErrorColor, 0.2f);
                }
                break;
        }

        if (!colChange)
        {
            backdropSprite.color = Color.Lerp(backdropSprite.color, backdropMainColor, 0.2f);
        }
    }
}
