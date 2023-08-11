using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDeleteOnBossBar : MonoBehaviour
{
    public int barToBeat;
    private void Update()
    {
        if (!BossController.main) { return; }
        if (BossController.main.currentBar <= barToBeat) { return; }
        Destroy(gameObject);
    }
}
