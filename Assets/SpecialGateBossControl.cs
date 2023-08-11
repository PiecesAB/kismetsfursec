using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialGateBossControl : MonoBehaviour
{
    // How does it work!!!
    // If this has a trigger collider, the door will close when this is touched, and trigger the next bar.
    // (This is so the player is actually in the area of fire when the barrage starts.)
    // Else, this will open the door when the boss was defeated at a certain bar.

    [SerializeField]
    private SpecialGate gate;
    private BossController controller;
    public int barToBeat;
    private bool triggerColliderMode = false;
    private bool used = false;

    void Start()
    {
        if (!gate) { gate = GetComponent<SpecialGate>(); }
        triggerColliderMode = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer != 20) { return; }
        if (!used) {
            gate.OnAmbushBegin();
            used = true;
            BossController.StopAwaitingNextAttack();
            Destroy(this);
        }
    }

    private IEnumerator WaitAndComplete(float delay)
    {
        yield return new WaitForSeconds(delay);
        gate.OnAmbushComplete();
        Destroy(this);
    }

    void Update()
    {
        if (!controller) { controller = BossController.main; }
        if (!controller) { return; }
        if (!triggerColliderMode)
        {
            if ((controller.currentBar > barToBeat || controller.defeated) && !used)
            {
                if (controller.barCount <= barToBeat + 1)
                {
                    StartCoroutine(WaitAndComplete(3f));
                }
                else
                {
                    gate.OnAmbushComplete();
                    Destroy(this);
                }
                used = true;
            }
        }
    }
}
