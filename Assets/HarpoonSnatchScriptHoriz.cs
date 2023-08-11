using UnityEngine;
using System.Collections;

public class HarpoonSnatchScriptHoriz : MonoBehaviour {

    public GameObject player;
    public float distanceOnTrigger;
    public AudioClip noticedSound;
    public float stallTime;
    public float speedWhereOneIsInstant;
    public float stall2Time;
    public float retractSpeed;
    public AudioClip downSound;


    private float yDiff;
    private byte catching;


	// Use this for initialization
	void Start () {
        catching = 0;
	while (player==null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
	}

	public IEnumerator Catch()
    {
        yield return new WaitForSeconds(stallTime);
        Vector3 a = GetComponent<HarpoonExtensionMakerHoriz>().origin;
        float stopPoint = Physics2D.Raycast(transform.position - new Vector3(0, 16, 0), Vector2.right, 240, 1 << 8).point.y;
        float add = Mathf.InverseLerp(a.x, stopPoint - 28, transform.position.x);
        for (float i = (speedWhereOneIsInstant * Time.timeScale) +add; i < 1; i+=speedWhereOneIsInstant*Time.timeScale)
        {
            transform.position = new Vector3(Mathf.Lerp(a.x, stopPoint - 28, i),a.y,a.z);
            yield return new WaitForEndOfFrame();
        }
        transform.position = new Vector3(stopPoint - 28, a.y , a.z);
        yield return new WaitForSeconds(stall2Time);
        catching = 1;
        for (float j = 1-(retractSpeed * Time.timeScale); j > 0; j -= retractSpeed * Time.timeScale)
        {
            if (catching == 2)
            {
                j = -1;
            }
            transform.position = new Vector3(a.x, Mathf.Lerp(stopPoint - 28,a.y, j), a.z);
            yield return new WaitForEndOfFrame();
        }
        if (catching == 1)
        {
            transform.position = a;
        }
        catching = 0;
    }


	// Update is called once per frame
	void Update () {

	if (System.Math.Abs(player.transform.position.y-transform.position.y)<distanceOnTrigger && catching ==0)
        {
            catching = 2;
            StartCoroutine(Catch());
        }
	}
}
