using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScrollblock : MonoBehaviour
{
    public Vector2 velocity;
    public Vector2 position;
    public Vector2 patternSize;
    public Transform positionFromObject = null;
    [Range(-1, 31)]
    public int changeDirOnSwitch;
    public Vector2 switchedVelocity;
    public float switchChangeTime = 1f;

    private Vector2 lastPos;

    private SpriteRenderer spr;
    private StaticBulletsOnVertices patternTile;
    private Bounds outerBounds;
    private Bounds innerBounds;
    private Vector2 bulletSize;

    private Color darkGray = new Color(0.3f, 0.3f, 0.3f);

    private float switchInterpol;

    private class Vector2Comp : IEqualityComparer<Vector2>
    {
        public bool Equals(Vector2 a, Vector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public int GetHashCode(Vector2 v)
        {
            return v.x.GetHashCode() + v.y.GetHashCode() << 2;
        }
    }

    private HashSet<Vector2> visiblePatternPositions = new HashSet<Vector2>(new Vector2Comp());
    private HashSet<Vector2> checkedPatternPositions = new HashSet<Vector2>(new Vector2Comp());
    private Dictionary<Vector2, Transform> patterns = new Dictionary<Vector2, Transform>();

    public bool BackCondition(Vector3 v)
    {
        return ((Vector2)v - (Vector2)innerBounds.ClosestPoint(v)).magnitude > 0.01f;
    }

    public Vector2 BackScale(Vector3 v)
    {
        Vector2 dif = (Vector2)v - (Vector2)innerBounds.ClosestPoint(v);
        return new Vector2(
            Mathf.Clamp01(1f - 2f * (Mathf.Abs(dif.x) / bulletSize.x)),
            Mathf.Clamp01(1f - 2f * (Mathf.Abs(dif.y) / bulletSize.y))
        );
    }

    private void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        if (changeDirOnSwitch >= 0)
        {
            spr.color = darkGray;
        }
        patternTile = transform.GetChild(0).GetComponent<StaticBulletsOnVertices>();
        patternTile.disabledBackside = true;
        patternTile.cancelGraphicWhenDestroyed = false;
        patternTile.deleteObjectWhenAllBulletsGone = false; // so it doesn't try to fill the holes
        outerBounds = spr.bounds;
        bulletSize = new Vector2(patternTile.bulletData.scale.x, patternTile.bulletData.scale.y);
        innerBounds = new Bounds(outerBounds.center, outerBounds.size - (Vector3)bulletSize);
        patternTile.SetBackCondition(BackCondition);
        patternTile.SetBackScale(BackScale);
        //patternTile.gameObject.SetActive(false);
        if (positionFromObject) { lastPos = positionFromObject.position; }

        switchInterpol = 0f;
    }

    private void AddPatternPositionIfVisible(Vector2 v)
    {
        if (checkedPatternPositions.Contains(v)) { return; }
        checkedPatternPositions.Add(v);
        Vector2 gv = transform.TransformPoint(v);
        Bounds vBounds = new Bounds(new Vector3(gv.x, gv.y, outerBounds.center.z), new Vector3(patternSize.x, patternSize.y, outerBounds.size.z));
        if (vBounds.Intersects(outerBounds)) {
            visiblePatternPositions.Add(v);
            AddPatternPositionIfVisible(v + Vector2.left * patternSize.x);
            AddPatternPositionIfVisible(v + Vector2.right * patternSize.x);
            AddPatternPositionIfVisible(v + Vector2.down * patternSize.y);
            AddPatternPositionIfVisible(v + Vector2.up * patternSize.y);
        }
    }

    private void AddPatternPositions()
    {
        AddPatternPositionIfVisible(Vector2.zero);
    }

    private void PopPatterns(Vector2 pop)
    {
        Dictionary<Vector2, Transform> newPatterns = new Dictionary<Vector2, Transform>();
        foreach(KeyValuePair<Vector2, Transform> kv in patterns)
        {
            newPatterns.Add(kv.Key - pop, kv.Value);
        }
        patterns = newPatterns;
    }

    private void DeleteInvisiblePatterns()
    {
        Dictionary<Vector2, Transform> newPatterns = null;
        bool dirty = false;
        foreach (KeyValuePair<Vector2, Transform> kv in patterns)
        {
            if (!visiblePatternPositions.Contains(kv.Key))
            {
                dirty = true;
                if (newPatterns == null) { newPatterns = new Dictionary<Vector2, Transform>(patterns); }
                Destroy(kv.Value.gameObject);
                newPatterns.Remove(kv.Key);
            }
        }
        if (dirty) { patterns = newPatterns; }
    }

    private void DeleteAllPatterns()
    {
        foreach (KeyValuePair<Vector2, Transform> kv in patterns)
        {
            Destroy(kv.Value.gameObject);
        }
        patterns.Clear();
    }

    private void MakeNewPatterns()
    {
        foreach (Vector2 p in visiblePatternPositions)
        {
            if (!patterns.ContainsKey(p))
            {
                GameObject newPatternTile = Instantiate(patternTile.gameObject, transform);
                StaticBulletsOnVertices sbv = newPatternTile.GetComponent<StaticBulletsOnVertices>();
                sbv.SetBackCondition(BackCondition);
                sbv.SetBackScale(BackScale);
                newPatternTile.SetActive(true);
                patterns.Add(p, newPatternTile.transform);
            }
        }
    }

    private void PlacePatterns()
    {
        foreach (KeyValuePair<Vector2, Transform> kv in patterns)
        {
            kv.Value.localPosition = kv.Key + position;
        }
    }

    private void Update()
    {
        if (positionFromObject) { position += (Vector2)positionFromObject.position - lastPos; }
        else {
            Vector2 realVelocity = velocity;
            if (changeDirOnSwitch >= 0)
            {
                switchInterpol = Mathf.MoveTowards(switchInterpol, Utilities.IsSwitchSet(changeDirOnSwitch) ? 1f : 0f, 0.016666666f * Time.timeScale / switchChangeTime);
                realVelocity = Vector2.Lerp(velocity, switchedVelocity, switchInterpol);
                spr.color = Color.Lerp(darkGray, Color.Lerp(Color.white, Utilities.colorCycle[changeDirOnSwitch], 0.5f), switchInterpol);
            }
            position += realVelocity * Time.timeScale * 0.016666666f;
        }

        if (transform.hasChanged)
        {
            position -= (Vector2)(spr.bounds.center - outerBounds.center);
            outerBounds = spr.bounds;
            innerBounds = new Bounds(outerBounds.center, outerBounds.size - (Vector3)bulletSize);
            transform.hasChanged = false;
        }

        Vector2 posPop = Vector2.zero;

        while (position.x + posPop.x >= patternSize.x * 0.5f)
        {
            posPop += Vector2.left * patternSize;
        }
        while (position.x + posPop.x < -patternSize.x * 0.5f)
        {
            posPop += Vector2.right * patternSize;
        }
        while (position.y + posPop.y >= patternSize.y * 0.5f)
        {
            posPop += Vector2.down * patternSize;
        }
        while (position.y + posPop.y < -patternSize.y * 0.5f)
        {
            posPop += Vector2.up * patternSize;
        }

        if (posPop.magnitude > 0) { PopPatterns(posPop); }
        position += posPop;

        if (positionFromObject) { lastPos = positionFromObject.position; }

        if (!spr.isVisible)
        {
            if (patterns.Count > 0) { DeleteAllPatterns(); }
            return;
        }

        visiblePatternPositions.Clear();
        checkedPatternPositions.Clear();
        AddPatternPositions();
        MakeNewPatterns();
        PlacePatterns();
        DeleteInvisiblePatterns();

        
    }
}
