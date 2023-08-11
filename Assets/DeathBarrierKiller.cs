using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeathBarrierKiller : MonoBehaviour {

    //public GameObject player;
    public bool actuallyWrapping;
    public bool horiz = false;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	foreach (GameObject player in LevelInfoContainer.allPlayersInLevel)
        {
            if (((horiz)?player.transform.position.x:player.transform.position.y) < ((horiz) ?transform.position.x : transform.position.y) && player.GetComponent<KHealth>())
            {
                print("!");
                if (!actuallyWrapping)
                {
                    player.GetComponent<KHealth>().SetHealth(Mathf.Infinity,"below boundary");
                }
                if (actuallyWrapping)
                {
                    Vector3 a = player.transform.position;
                    Vector3 h = GameObject.FindGameObjectWithTag("Above Wrap Barrier").transform.position;
                    player.transform.position = new Vector3(((horiz) ? h.x:a.x), ((horiz)?a.y:h.y),a.z);
                }
            }
        }
	}
}
