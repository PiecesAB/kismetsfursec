using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapViewerUI : MonoBehaviour
{
    public Image leftArrow;
    public Image rightArrow;
    public Image upArrow;
    public Image downArrow;
    public Image border;

    public static MapViewerUI main;

    public AudioSource aso;
    public AudioClip scrollSound;
    public AudioSource aso2;
    public AudioClip openSound;
    public AudioClip closeSound;

    public bool viewing = false;

    private void Start()
    {
        main = this;
        aso = GetComponent<AudioSource>();
    }

    private void SetImageAlpha(Image im, float a)
    {
        im.color = new Color(im.color.r, im.color.g, im.color.b, a);
    }

    public void ChangeArrowDisplay()
    {
        Vector4 bounds = FollowThePlayer.main.cameraBounds;
        Vector2 currPos = FollowThePlayer.main.transform.position;
        SetImageAlpha(leftArrow, Mathf.Lerp(leftArrow.color.a, (Mathf.Abs(bounds.x - currPos.x) < 8f) ? 0f : 0.75f, 0.35f));
        SetImageAlpha(rightArrow, Mathf.Lerp(rightArrow.color.a, (Mathf.Abs(bounds.z - currPos.x) < 8f) ? 0f : 0.75f, 0.35f));
        SetImageAlpha(upArrow, Mathf.Lerp(upArrow.color.a, (Mathf.Abs(bounds.w - currPos.y) < 8f) ? 0f : 0.75f, 0.35f));
        SetImageAlpha(downArrow, Mathf.Lerp(downArrow.color.a, (Mathf.Abs(bounds.y - currPos.y) < 8f) ? 0f : 0.75f, 0.35f));
    }

    public void Open()
    {
        leftArrow.gameObject.SetActive(true);
        rightArrow.gameObject.SetActive(true);
        upArrow.gameObject.SetActive(true);
        downArrow.gameObject.SetActive(true);
        border.gameObject.SetActive(true);
        aso2.Stop();
        aso2.clip = openSound;
        aso2.Play();
        viewing = true;
    }

    public void Close()
    {
        leftArrow.gameObject.SetActive(false);
        rightArrow.gameObject.SetActive(false);
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);
        border.gameObject.SetActive(false);
        aso.Stop();
        aso2.Stop();
        aso2.clip = closeSound;
        aso2.Play();
        viewing = false;
    }
}
