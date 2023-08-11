using UnityEngine;
using System.Collections;

public class primFloatingItem : MonoBehaviour {

    public int itemID;
    public AudioSource collectSound;
    public string receiveChoice;
    public GameObject prefabOfInventoryFull;

    void OnTriggerEnter2D(Collider2D col)
    {
        
        if (collectSound != null)
        {
            collectSound.Stop();
            collectSound.Play();
        }
    }

}
