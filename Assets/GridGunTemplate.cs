using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public abstract class GridGunTemplate : SpecialGunTemplate
{
    [System.Serializable]
    public struct Point
    {
        public int x;
        public int y;

        public Point(int inX, int inY)
        {
            x = inX; y = inY;
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.x + b.x, a.y + b.y);
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.x - b.x, a.y - b.y);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }

        public override string ToString()
        {
            return ToVector2().ToString();
        }
    }
    public List<Point> path;
    public float gridSize;
    public LineRenderer pathDisplay;
    public SpriteRenderer grid;
    public SpriteRenderer newPosSample = null;

    private SpriteRenderer mySpr;
    private Color validColor = new Color(1f, 1f, 1f, 160f / 255f);
    private Color invalidColor = new Color(1f, 0.3f, 0.3f, 160f / 255f);

    public float gravityChange = 4f;

    private Point defaultPos = new Point(0, 0);

    protected ColorCorrectionCurves ccc;

    private void CalculateAngle()
    {
        Point endpoint = path[path.Count - 1];
        if (endpoint.x == 0 && endpoint.y == 0) { nextangle = 0f; }
        else { nextangle = Mathf.Atan2(endpoint.y, endpoint.x); }
        nextangle *= Mathf.Rad2Deg;
    }

    private FollowThePlayer ftp;

    protected bool IsLegalTeleportPosition(Vector3 newPos)
    {
        // no teleporting off-screen (or somewhere that would scroll it)
        if (!ftp) { ftp = Camera.main.GetComponent<FollowThePlayer>(); }
        Vector3 wrappedPos;
        if (ftp.WouldItScrollIfIWereHere(newPos, out wrappedPos)) { return false; }

        // no teleporting into the ground
        Vector2 boxSize = GetComponent<BoxCollider2D>().size;
        RaycastHit2D[] rl = Physics2D.BoxCastAll(wrappedPos,
                                            new Vector2(Mathf.Max(boxSize.x - 4f, 2f), Mathf.Max(boxSize.y - 4f, 2f)),
                                            transform.eulerAngles.z, Vector2.up, 0.1f, 768 + 1048576);
        for (int i = 0; i < rl.Length; ++i)
        {
            if (rl[i].collider)
            {
                if (rl[i].collider.gameObject == gameObject) { continue; }
                if (rl[i].collider.GetComponent<beamBlock>()) { continue; } // teleport through tacks
                else { return false; }
            }
        }

        return true;
    }

    private void SetLocked()
    {
        mvtLocked = (e.currentState & (32UL + 256UL + 512UL)) != 0UL;
    }

    private void SimplifyPath(List<Point> oldPath)
    {
        for (int i = path.Count - 2; i >= 0; --i)
        {
            Point endpoint = path[path.Count - 1];
            Point dif = endpoint - path[i];
            if (dif.x == 0 && dif.y == 0)
            {
                path.RemoveRange(i, path.Count - 1 - i);
            }
            else if (dif.x == 0 && path.Count - 1 - i > Mathf.Abs(dif.y))
            {
                int sign = (dif.y > 0) ? 1 : -1;
                path.RemoveAt(path.Count - 2);
                path.RemoveAt(i + 1);
                for (int j = path.Count - 2; j > i; --j)
                {
                    path[j] = new Point(path[i].x, path[j].y);
                }
            }
            else if (dif.y == 0 && path.Count - 1 - i > Mathf.Abs(dif.x))
            {
                int sign = (dif.x > 0) ? 1 : -1;
                path.RemoveAt(path.Count - 2);
                path.RemoveAt(i + 1);
                for (int j = path.Count - 2; j > i; --j)
                {
                    path[j] = new Point(path[j].x, path[i].y);
                }
            }
        }

        if (path.Count == 1)
        {
            if (mvtLocked)
            {
                if (e.ButtonDown(1UL, 3UL)) { path.Add(path[path.Count - 1] + (new Point(-1, 0))); }
                if (e.ButtonDown(2UL, 3UL)) { path.Add(path[path.Count - 1] + (new Point(1, 0))); }
                if (e.ButtonDown(4UL, 12UL)) { path.Add(path[path.Count - 1] + (new Point(0, 1))); }
                if (e.ButtonDown(8UL, 12UL)) { path.Add(path[path.Count - 1] + (new Point(0, -1))); }
            }
        }
        else if (path.Count - 1 > gunHealth / gunHealthDecreaseAmount)
        {
            path = oldPath;
        }
    }

    private void SpecialOneLeftTrigger()
    {
        if ((int)gunHealth / (int)gunHealthDecreaseAmount == 1f)
        {
            path = new List<Point>() { new Point(0, 0) };
        }
    }

    protected override void ChildStart()
    {
        path = new List<Point>();
        ccc = Camera.main.GetComponent<ColorCorrectionCurves>();
        mySpr = GetComponent<SpriteRenderer>();
    }

    protected override void AimingBegin()
    {
        path = new List<Point>() { new Point(0, 0) };
        List<Point> onePath = null;
        if ((e.currentState & 15UL) != 0UL) {
            if ((e.currentState & 3UL) == 1UL) { path.Add(new Point(-1, 0)); }
            if ((e.currentState & 3UL) == 2UL) { path.Add(new Point(1, 0)); }
            if ((e.currentState & 12UL) == 4UL) { path.Add(path[path.Count - 1] + new Point(0, 1)); }
            if ((e.currentState & 12UL) == 8UL) { path.Add(path[path.Count - 1] + new Point(0, -1)); }
            if (path.Count > 2) // in case the player has energy for only one shot, no diagonals
            {
                onePath = new List<Point>(path);
                while (onePath.Count > 2) { onePath.RemoveAt(2); }
            }
        }
        else {
            if (defaultPos.x == 0 && defaultPos.y == 0) { path.Add(new Point(mySpr.flipX ? -1 : 1, 0)); }
            else { path.Add(defaultPos); }
            onePath = path;
        }
        SimplifyPath(onePath);
        CalculateAngle();
        SetLocked();
    }

    AudioSource gridSound = null;

    protected override void AimingUpdate()
    {
        SetLocked();
        List<Point> oldPath = new List<Point>(path);
        bool adjust = false;
        if (mvtLocked)
        {
            if (e.ButtonDown(1UL, 3UL)) { SpecialOneLeftTrigger(); path.Add(path[path.Count - 1] + (new Point(-1, 0))); adjust = true; }
            if (e.ButtonDown(2UL, 3UL)) { SpecialOneLeftTrigger(); path.Add(path[path.Count - 1] + (new Point(1, 0))); adjust = true; }
            if (e.ButtonDown(4UL, 12UL)) { SpecialOneLeftTrigger(); path.Add(path[path.Count - 1] + (new Point(0, 1))); adjust = true; }
            if (e.ButtonDown(8UL, 12UL)) { SpecialOneLeftTrigger(); path.Add(path[path.Count - 1] + (new Point(0, -1))); adjust = true; }
        }
        SimplifyPath(oldPath);
        if (path.Count >= 2) { defaultPos = path[1]; }
        CalculateAngle();
        if (adjust)
        {
            if (gridSound == null) { gridSound = grid.GetComponent<AudioSource>(); }
            if (gridSound) {
                gridSound.Stop();
                gridSound.pitch = 1.12f;
                gridSound.Play();
            }
        }
    }

    protected override void GraphicsUpdateWhenNotAiming()
    {
        mvtLocked = false;
        pathDisplay.gameObject.SetActive(false);
        pathDisplay.enabled = false;
        grid.enabled = false;
        if (newPosSample) { newPosSample.enabled = false; }
    }

    protected override void GraphicsUpdateWhenAiming()
    {
        pathDisplay.gameObject.SetActive(true);
        pathDisplay.enabled = true;
        Vector3[] displayVecs = new Vector3[path.Count];
        for (int i = 0; i < displayVecs.Length; ++i)
        {
            displayVecs[i] = new Vector3(path[i].x * gridSize, path[i].y * gridSize, 0);
        }
        pathDisplay.positionCount = displayVecs.Length;
        pathDisplay.SetPositions(displayVecs);
        grid.enabled = true;

        Vector2 lastGridPos = displayVecs[displayVecs.Length - 1];
        if (newPosSample)
        {
            if (lastGridPos.sqrMagnitude < 1)
            {
                newPosSample.enabled = false;
            }
            else
            {
                newPosSample.enabled = true;
                newPosSample.sprite = mySpr.sprite;
                newPosSample.flipX = mySpr.flipX;
                newPosSample.color = (IsLegalTeleportPosition(newPosSample.transform.position)) ? validColor : invalidColor;
                newPosSample.transform.localPosition = lastGridPos;
            }
        }

        if (mvtLocked)
        {
            grid.material.SetFloat("_S", gridSize * Mathf.Min((int)gunHealth / (int)gunHealthDecreaseAmount, 5f));
        }
        else
        {
            grid.material.SetFloat("_S", 0f);
        }
    }

    protected override abstract float Fire();

    protected override void ChildUpdate()
    {
        // :(
    }
}
