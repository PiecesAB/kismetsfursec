using UnityEngine;
using System.Collections;

public class BladeCollisionTrigger : MonoBehaviour {

    private int debounce = 0;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (debounce > 0) { return; }
        debounce = 20;
        if (other.gameObject.GetComponent<KHealth>() && other.gameObject.GetComponent<BasicMove>())
        {
            other.gameObject.GetComponent<KHealth>().ChangeHealth(-3.5f*LevelInfoContainer.GetScalingSpikeDamage(),"razor");
            Vector3 hi = transform.up * (-GetComponent<NormalGuillotine1>().speed / 10);
            other.gameObject.GetComponent<BasicMove>().AddBlood(other.gameObject.transform.position, Quaternion.LookRotation(hi, Vector3.up));
            
            other.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector3(hi.x,hi.y));
            other.gameObject.GetComponent<AudioSource>().PlayOneShot(other.gameObject.GetComponent<BasicMove>().spikeTouchSound);
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0) { return; }
        if (debounce > 0) { --debounce; }
    }
}
