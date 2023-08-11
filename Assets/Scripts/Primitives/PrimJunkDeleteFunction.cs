using UnityEngine;
using System.Collections;

public class PrimJunkDeleteFunction : MonoBehaviour {

    public bool deleteImmediately = false;
    public float chance = 1;

	public void Delete()
    {
        if (chance < 1)
        {
            if (Fakerand.Single() >= chance)
            {
                return;
            }
        }
        Destroy(gameObject);
    }

    private void Start()
    {
        if (deleteImmediately) { Delete(); }
    }
}
