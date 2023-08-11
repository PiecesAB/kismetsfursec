using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// note: LevelInfoContainer must allow player rotation in this level.
public class RotationMachine : MonoBehaviour, IExaminableAction
{

    //public float clockwise;
    public float counterclockwise;

    private bool hi;

    public /*static*/ bool rotating;
    private Transform player;
    public GameObject arrow;
    public GameObject handle;
    public bool autoTrigger;
    public bool toggleOffscreenDownwards = true;
    private bool counteris;
    private float needed;
    private float ts;
    private List<Encontrolmentation> objects = new List<Encontrolmentation>();

    public void OnExamine(Encontrolmentation plr)
    {
        autoTrigger = true;
        if (toggleOffscreenDownwards)
        {
            FollowThePlayer ftp = Camera.main.GetComponent<FollowThePlayer>();
            ftp.mustBeOffscreenDownwardsToDie = !ftp.mustBeOffscreenDownwardsToDie;
        }
    }
    
    void Update () {

        arrow.transform.eulerAngles = new Vector3(0, 0, Camera.main.transform.eulerAngles.z);

        if (autoTrigger && !rotating)
        {
            autoTrigger = false;
            rotating = true;
            counteris = true;
            player = LevelInfoContainer.GetActiveControl().transform;
            needed = 0;
            Utilities.canPauseGame = false;
            Utilities.canUseInventory = false;
            ts = Time.timeScale;
            Time.timeScale = 0;

        }

        if (rotating && player)
        {

            if ((counteris && counterclockwise - needed >= 2f) /*|| (!counteris && clockwise - needed >= 2f)*/)
            {
                needed += 2f;
                player.RotateAround(player.position + (3f * Vector3.down), Vector3.forward, (counteris) ? 2f : -2f);
            }
            else
            {
                float deutsch = (counteris) ? (counterclockwise - needed) : 0f/*(needed - clockwise)*/;
                player.RotateAround(player.position, Vector3.forward, (counteris) ? deutsch : deutsch);
                //tr.goalRotation += deutsch;
                rotating = false;
                Utilities.canPauseGame = true;
                Utilities.canUseInventory = true;
                Time.timeScale = ts;
            }
        }
        hi = false;
        objects.Clear();
	}
}
