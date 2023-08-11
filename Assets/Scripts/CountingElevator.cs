using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CountingElevator : MonoBehaviour
{
    // go in right = move up
    // go in left = move down
    // it should not be possible to walk out of the elevator before it starts moving

    // possibly put the elevator at a different height if the player spawns somewhere else
    [System.Serializable]
    public struct AltStartHeight
    {
        // if any is true, then it will match all nonempty level start strings.
        public bool any;
        public string s;
        public int idx;
    }

    public float[] heights = new float[3] { 0, 80, 160 };
    [SerializeField]
    private GameObject backPanelSample;
    [SerializeField]
    private GameObject backBeamSample;
    [SerializeField]
    private GameObject mainBox;
    [SerializeField]
    private SpriteRenderer chain;
    [SerializeField]
    private SpriteRenderer leftLight;
    [SerializeField]
    private SpriteRenderer rightLight;
    [SerializeField]
    private SkinnedMeshRenderer doorControl;
    [SerializeField]
    private Collider2D doorColL;
    [SerializeField]
    private Collider2D doorColR;
    [SerializeField]
    private primDecorationMoving mover;
    public AudioSource moveSound;
    public AudioSource startSound;
    public AudioSource stopSound;
    public AudioSource doorSound;
    public AltStartHeight[] altStartHeights;

    private BoxCollider2D myCollider;

    public enum State
    {
        GetIn, Moving, GetOut
    }

    public enum Direction
    {
        None = 0, Up = 1, Down = -1
    }

    public float moveSpeed; // about how much distance to cover in one second
    public int heightIndex = 0;
    [HideInInspector]
    public bool leftOpen = false;
    [HideInInspector]
    public bool rightOpen = false;
    [HideInInspector]
    public State state = State.GetIn;
    [HideInInspector]
    public Direction dir = Direction.None;
    private bool finishedMoving = false;

    private const float elevatorSelfHeight = 88;
    private const float panelHeight = 144;
    private const float extraBackHeight = 32;
    private const float chainPosition = 96;

    private float totalBackHeight;

    private void GenerateGraphics()
    {
        totalBackHeight = heights[heights.Length - 1] + elevatorSelfHeight + extraBackHeight;
        float bz = backPanelSample.transform.position.z;
        Transform bp = backPanelSample.transform.parent;
        foreach (Transform c in bp)
        {
            GameObject g = c.gameObject;
            if (c != bp && g != backPanelSample && g != backBeamSample) { DestroyImmediate(g); }
        }

        for (float h = totalBackHeight; h > 0f; h -= panelHeight)
        {
            float nh = Mathf.Max(h - panelHeight, 0f);
            GameObject newPanel = Instantiate(backPanelSample, bp);
            newPanel.SetActive(true);
            newPanel.transform.localPosition = new Vector3(0, nh, bz);
            if (h < panelHeight)
            {
                newPanel.transform.localScale = new Vector3(1f, h / panelHeight, 1f);
            }
        }
        for (int i = 0; i < heights.Length; ++i)
        {
            GameObject newBeam = Instantiate(backBeamSample, bp);
            newBeam.SetActive(true);
            newBeam.transform.localPosition = new Vector3(0, heights[i], bz);
        }
    }

    private void UpdateChain()
    {
        float currHeight = mainBox.transform.localPosition.y;
        float h = heights[heights.Length - 1] - currHeight;
        chain.size = new Vector2(chain.size.x, h);
        chain.transform.localPosition = new Vector3(0, 8, chainPosition + h / 2);
    }  

    // because the door is hard to see when the camera is close to the axis
    private void UpdateLights()
    {
        if (!FollowThePlayer.main) { return; }
        float x = FollowThePlayer.main.transform.position.x;
        if (Mathf.Abs(x - transform.position.x) > 104f)
        {
            leftLight.color = Color.Lerp(leftLight.color, Color.clear, 0.3f);
            rightLight.color = Color.Lerp(rightLight.color, Color.clear, 0.3f);
        }
        else
        {
            leftLight.color = Color.Lerp(leftLight.color, leftOpen ? Color.white : Color.clear, 0.3f);
            rightLight.color = Color.Lerp(rightLight.color, rightOpen ? Color.white : Color.clear, 0.3f);
        }
    }

    private void UpdateDoors()
    {
        doorControl.SetBlendShapeWeight(0, Mathf.MoveTowards(doorControl.GetBlendShapeWeight(0), leftOpen ? 100f : 0f, 8f));
        doorControl.SetBlendShapeWeight(1, Mathf.MoveTowards(doorControl.GetBlendShapeWeight(1), rightOpen ? 100f : 0f, 8f));
        doorColL.enabled = !leftOpen;
        doorColR.enabled = !rightOpen;
    }

    private void SetHeightToIndex()
    {
        float h = 0;
        if (heightIndex >= 0 && heightIndex < heights.Length) { h = heights[heightIndex]; }
        mainBox.transform.localPosition = new Vector3(0, h, 0);
    }

    private void Start()
    {
        totalBackHeight = heights[heights.Length - 1] + elevatorSelfHeight + extraBackHeight;
        myCollider = GetComponent<BoxCollider2D>();
        if (Application.isPlaying)
        {
            foreach (AltStartHeight ash in altStartHeights)
            {
                if ((Utilities.loadedSaveData.levelInInfo == ash.s && !ash.any)
                    || (Utilities.loadedSaveData.levelInInfo != "" && ash.any))
                {
                    heightIndex = ash.idx;
                    SetHeightToIndex();
                }
            }
        }
    }

    private int collisionCount = 0;
    private Vector2 lastCollisionPoint;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != 20) { return; }
        ++collisionCount;
        lastCollisionPoint = col.transform.position;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer != 20) { return; }
        --collisionCount;
    }

    private IEnumerator Move()
    {
        float origHeight = heights[heightIndex];
        float targetHeight = heights[heightIndex + (int)dir];
        double timeToGetThere = Mathf.Abs(targetHeight - origHeight) / moveSpeed;
        double startTime = DoubleTime.ScaledTimeSinceLoad;
        while (DoubleTime.ScaledTimeSinceLoad - startTime < timeToGetThere)
        {
            if (Time.timeScale != 0)
            {
                float prog = (float)((DoubleTime.ScaledTimeSinceLoad - startTime) / timeToGetThere);
                float frameTarget = Mathf.Lerp(origHeight, targetHeight, EasingOfAccess.SineSmooth(prog));
                mover.v = new Vector3(0, 60f * (frameTarget - mainBox.transform.localPosition.y), 0) / Time.timeScale;
            }
            yield return new WaitForEndOfFrame();
        }
        mover.v = Vector3.zero;
        finishedMoving = true;
        yield return null;
    }

    private void Update()
    {
        if (!Application.isPlaying) { GenerateGraphics(); UpdateChain(); SetHeightToIndex(); return; }

        myCollider.offset = (mainBox.transform.localPosition.y + 0.5f * elevatorSelfHeight) * Vector3.up;
        bool oldLeftOpen = leftOpen;
        bool oldRightOpen = rightOpen;
        switch (state)
        {
            case State.GetIn:
                leftOpen = (heightIndex < heights.Length - 1);
                rightOpen = (heightIndex > 0);
                if (collisionCount > 0)
                {
                    myCollider.size += new Vector2(36, 0);
                    state = State.Moving;
                    finishedMoving = false;
                    if (heightIndex == 0) { dir = Direction.Up; }
                    else if (heightIndex == heights.Length - 1) { dir = Direction.Down; }
                    else { dir = (mainBox.transform.InverseTransformPoint(lastCollisionPoint).x < 0) ? Direction.Up : Direction.Down; }
                    StartCoroutine(Move());
                    moveSound.Play();
                    startSound.Play();
                }
                break;
            case State.Moving:
                leftOpen = rightOpen = false;
                if (finishedMoving)
                {
                    state = State.GetOut;
                    heightIndex += (int)dir;
                    moveSound.Stop();
                    stopSound.Play();
                }
                break;
            case State.GetOut:
                leftOpen = rightOpen = false;
                if (dir == Direction.Down) { leftOpen = true; }
                if (dir == Direction.Up) { rightOpen = true; }
                if (collisionCount == 0)
                {
                    myCollider.size -= new Vector2(36, 0);
                    state = State.GetIn;
                }
                break;
        }

        UpdateChain();
        UpdateLights();
        UpdateDoors();

        if (oldLeftOpen != leftOpen || oldRightOpen != rightOpen)
        {
            doorSound.Stop();
            doorSound.Play();
        }
    }
}
