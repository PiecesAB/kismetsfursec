using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CCLBackdropController : MonoBehaviour
{
    public Color fogColor;
    public Transform sampleHolder;
    public Transform scrollHolder;
    public Sprite[] randomImages;
    public SpriteRenderer randomImageSample;
    public SpriteRenderer windowSample;
    public GameObject speedLineSample;

    private Vector2 lastCamPos = Vector2.negativeInfinity;

    private List<GameObject> grids = new List<GameObject>();

    private const double gridSpacingTime = 2.8;
    private const float scrollVelocity = 160f;

    private List<string> disableSkyboxNames = new List<string>
    {
        "SkyboxCamera1", "SkyboxCamera2", "Skybox Cube", "LowestCameraDisplaysBlack"
    };

    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogEndDistance = 1000;
        RenderSettings.fogStartDistance = 0;
        foreach (Transform t in sampleHolder)
        {
            if (t == sampleHolder) { continue; }
            grids.Add(t.gameObject);
        }
        GameObject[] rootObjs = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject g in rootObjs)
        {
            if (disableSkyboxNames.Contains(g.name)) { g.SetActive(false); }
        }
    }

    private void MakeNewGrid()
    {
        GameObject sample = grids[Fakerand.Int(0, grids.Count)];
        GameObject newGrid = Instantiate(sample, scrollHolder);
        newGrid.transform.localPosition = new Vector3(0, 0, 1000);
        newGrid.transform.localScale *= Fakerand.Single(0.8f, 1.3f);
        newGrid.SetActive(true);
    }

    private void MoveObjectsForward()
    {
        foreach (Transform t in scrollHolder)
        {
            if (t == scrollHolder) { continue; }
            float mul = 1f;
            if (t.gameObject.name.StartsWith("SpeedLine")) { mul = 4f; }
            t.localPosition += mul * scrollVelocity * Time.timeScale * 0.01666666666f * Vector3.back;
            if (t.localPosition.z < -300) { Destroy(t.gameObject, 0.03f); }
        }
    }

    private void RotateScrollHolder()
    {
        float t = (float)(DoubleTime.ScaledTimeSinceLoad % 10000.0);
        foreach (Transform r in scrollHolder)
        {
            if (r == scrollHolder) { continue; }
            float ra = rotationAmount;
            if (r.gameObject.name.StartsWith("Window"))
            {
                ra *= 0.5f;
            }
            if (r.gameObject.name.StartsWith("SpeedLine"))
            {
                ra *= 1.4f;
            }
            r.RotateAround(scrollHolder.position, Vector3.forward, ra);
        }
    }

    private void MakeNewRandomImage()
    {
        GameObject newSprite = Instantiate(randomImageSample.gameObject, scrollHolder);
        SpriteRenderer newSR = newSprite.GetComponent<SpriteRenderer>();
        newSR.sprite = randomImages[Fakerand.Int(0, randomImages.Length)];
        newSprite.transform.localPosition = (Vector3)(Fakerand.Single(25, 75) * Fakerand.UnitCircle(true)) + 1000 * Vector3.forward;
        newSprite.transform.localScale *= Fakerand.Single(0.8f, 1.3f);
        newSR.color = Color.HSVToRGB(Fakerand.Single(), 0.3f, 1f);
        newSR.color = new Color(newSR.color.r, newSR.color.g, newSR.color.b, 0.8f);
    }

    private void MakeNewWindow()
    {
        GameObject newSprite = Instantiate(windowSample.gameObject, scrollHolder);
        SpriteRenderer newSR = newSprite.GetComponent<SpriteRenderer>();
        newSR.size = new Vector2(Fakerand.Int(96, 384), Fakerand.Int(80, 240));
        newSprite.transform.localPosition = (Vector3)(Mathf.Min(newSR.size.x, newSR.size.y) * 0.05f * Fakerand.UnitCircle()) + 1000 * Vector3.forward;
        newSR.color = Color.HSVToRGB(Fakerand.Single(), 0.3f, 1f);
    }

    private void MakeNewSpeedLine()
    {
        GameObject newLine = Instantiate(speedLineSample, scrollHolder);
        newLine.transform.localPosition = Fakerand.Single(10, 25) * Fakerand.UnitCircle(true);
        newLine.transform.localScale *= Fakerand.Single(0.5f, 2.0f);
        LineRenderer lr = newLine.GetComponent<LineRenderer>();
        lr.startColor = lr.endColor = Color.HSVToRGB(Fakerand.Single(), 0.3f, 1f);
        lr.startColor = lr.endColor = Color.HSVToRGB(Fakerand.Single(), 0.3f, 1f);
        lr.startColor = lr.endColor = new Color(lr.startColor.r, lr.startColor.g, lr.startColor.b, 0.4f);
    }

    private double lastGridTime = -3.0;
    private double lastImageTime = -0.3;
    private double lastWindowTime = -2.0;
    private double lastSpeedLineTime = -0.05;
    private float rotationAmount = 0;

    void Update()
    {
        rotationAmount = 0;
        if (lastCamPos.x > -1000000 && FollowThePlayer.main)
        {
            rotationAmount = 0.3f * Vector2.Dot(FollowThePlayer.main.transform.right, (Vector2)FollowThePlayer.main.transform.position - lastCamPos);
        }
        lastCamPos = FollowThePlayer.main?.transform.position ?? Vector2.negativeInfinity;
        RotateScrollHolder();
        if (Time.timeScale == 0) { return; }
        MoveObjectsForward();
        lastImageTime += 0.016666666666666666 * Time.timeScale;
        lastWindowTime += 0.016666666666666666 * Time.timeScale;
        lastSpeedLineTime += 0.016666666666666666 * Time.timeScale;

        if (lastSpeedLineTime >= 0f)
        {
            MakeNewSpeedLine();
            lastSpeedLineTime = -0.03 * Mathf.Exp(Fakerand.Single());
        }
        if (DoubleTime.ScaledTimeSinceLoad - lastGridTime >= gridSpacingTime)
        {
            MakeNewGrid();
            lastGridTime = DoubleTime.ScaledTimeSinceLoad;
            return;
        }
        if (lastImageTime >= 0f)
        {
            MakeNewRandomImage();
            lastImageTime = -0.2 * Mathf.Exp(Fakerand.Single());
            return;
        }
        if (lastWindowTime >= 0f)
        {
            MakeNewWindow();
            lastWindowTime = -0.6 * Mathf.Exp(Fakerand.Single() * 2f);
            return;
        }
    }
}
