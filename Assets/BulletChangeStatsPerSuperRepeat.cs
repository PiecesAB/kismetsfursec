using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletChangeStatsPerSuperRepeat : MonoBehaviour, IBulletMakerResetOnSuperRepeat
{
    private BulletHellMakerFunctions maker;

    public int numberInCircle;
    public float bulletSpeed;
    public float hueShift;
    public float waitMultiplier = 1f;
    public float minWait = 0f;
    public int numberOfRepeats = 0;

    private BulletRandomnessModifier rm;

    public void MakerReset()
    {
        if (rm) { rm.PreResetOnSuperRepeatMod(); }
        maker.bulletShooterData.numberInCircle += numberInCircle;
        maker.setNumberOfRepeats += numberOfRepeats;
        maker.origNumberOfRepeats += numberOfRepeats;
        maker.bulletData.speed += bulletSpeed;
        if (hueShift != 0f)
        {
            float ch, cs, cv; Color.RGBToHSV(maker.bulletData.color, out ch, out cs, out cv);
            maker.bulletData.color = Color.HSVToRGB((ch + hueShift)%1f, cs, cv);
        }
        maker.waitTime *= waitMultiplier;
        if (maker.waitTime < minWait)
        {
            maker.waitTime = minWait;
        }
        if (rm) { rm.ResetOnSuperRepeatMod(); }
    }

    private void Start()
    {
        maker = GetComponent<BulletHellMakerFunctions>();
        rm = GetComponent<BulletRandomnessModifier>();
    }


}
