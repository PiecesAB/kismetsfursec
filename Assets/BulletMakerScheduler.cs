using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This allows for replacing bullets with multiple others variants in a pattern.
public class BulletMakerScheduler : MonoBehaviour, IBulletMakerOnShot
{
    private BulletHellMakerFunctions maker;

    public List<BulletData> bullets = new List<BulletData>();
    public bool preserveSpeed = false;

    private int currentIndex;

    private void Start()
    {
        maker = GetComponent<BulletHellMakerFunctions>();
        currentIndex = 0;
    }

    public void BeforeShot()
    {
        float oldSpeed = maker.bulletData.speed;
        maker.bulletData = bullets[currentIndex];
        if (preserveSpeed) { maker.bulletData.speed = oldSpeed; }
        ++currentIndex;
        if (currentIndex >= bullets.Count) { currentIndex = 0; }
    }

    public void OnShot()
    {
    }
}
