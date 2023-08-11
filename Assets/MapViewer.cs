using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapViewer : MonoBehaviour, IExaminableAction
{
    private Encontrolmentation e;
    private BoxCollider2D myCol;
    private bool justOpened = false;

    [SerializeField]
    private Transform viewpoint;

    private const float speed = 6f;

    private Transform origTarget;
    private Vector3 origPos;

    public static void HideShowUIs(bool on)
    {
        Vector3 move = on ? Vector2.down * 1000 : Vector2.up * 1000;
        PrimCollectible.mainScoreCounter.transform.parent.parent.position += move;
        TimeWarnerGUI.main.transform.position += move;
        GravWarnerGUI.main.transform.position += move;
        CharacterSquaresUI.main.transform.position += move;
        
    }

    private void ClampViewPointPosition()
    {
        FollowThePlayer ftp = FollowThePlayer.main;
        viewpoint.position = new Vector3(
            Mathf.Clamp(viewpoint.position.x, ftp.cameraBounds.x, ftp.cameraBounds.z),
            Mathf.Clamp(viewpoint.position.y, ftp.cameraBounds.y, ftp.cameraBounds.w), 
            viewpoint.position.z);
    }

    public void OnExamine(Encontrolmentation plr)
    {
        e = plr;
        Utilities.DisablePlayerActions(e.gameObject);
        myCol.offset += new Vector2(0, 100000);
        justOpened = true;
        HideShowUIs(false);
        viewpoint.position = FollowThePlayer.main.transform.position;
        ClampViewPointPosition();
        origTarget = FollowThePlayer.main.target;
        FollowThePlayer.main.target = viewpoint;
        origPos = FollowThePlayer.main.transform.position;
        MapViewerUI.main.Open();
    }

    private void Close()
    {
        Utilities.ReEnablePlayerActions(e.gameObject);
        e = null;
        myCol.offset -= new Vector2(0, 100000);
        HideShowUIs(true);
        FollowThePlayer.main.target = origTarget;
        FollowThePlayer.SetCameraPosition(origPos);
        MapViewerUI.main.Close();
    }

    private void Start()
    {
        myCol = GetComponent<BoxCollider2D>();
        e = null;
    }

    private void Update()
    {
        if (Time.timeScale == 0 || !e) { return; }
        e.eventAbutton = Encontrolmentation.ActionButton.DPad;
        e.eventAName = "Move";
        e.eventBbutton = Encontrolmentation.ActionButton.XButton;
        e.eventBName = "Cancel";
        if (!justOpened) {
            double a;
            if (e.ButtonDown(64UL, 64UL)
                || ((Vector2)e.transform.position - (Vector2)transform.position).magnitude > 52f) {
                Close(); return;
            }
            Vector3 oldVP = viewpoint.position;
            if (e.ButtonHeld(1UL, 3UL, 0f, out a)) { viewpoint.position += Vector3.left * speed; }
            if (e.ButtonHeld(2UL, 3UL, 0f, out a)) { viewpoint.position += Vector3.right * speed; }
            if (e.ButtonHeld(4UL, 12UL, 0f, out a)) { viewpoint.position += Vector3.up * speed; }
            if (e.ButtonHeld(8UL, 12UL, 0f, out a)) { viewpoint.position += Vector3.down * speed; }
            ClampViewPointPosition();
            if ((oldVP - viewpoint.position).magnitude < speed)
            {
                MapViewerUI.main.aso.Stop();
            }
            else
            {
                if (MapViewerUI.main.aso.clip != MapViewerUI.main.scrollSound || !MapViewerUI.main.aso.isPlaying)
                {
                    MapViewerUI.main.aso.Stop();
                    MapViewerUI.main.aso.clip = MapViewerUI.main.scrollSound;
                    MapViewerUI.main.aso.loop = true;
                    MapViewerUI.main.aso.Play();
                }
                

            }
        }
        MapViewerUI.main.ChangeArrowDisplay();
        justOpened = false;
    }
}
