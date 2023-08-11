using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisplayALevelInfo : MonoBehaviour {

    // enum soon for different text types
    public Text txtTarget;
    private bool givenTitle = false;
    private bool bossMode = false;

    private int rainbowTextEffect = 0;

    private int lastBossHealth = -1;

	void Start () {
        givenTitle = false;
        Change();
	}

	public void Change () {

        if (LevelInfoContainer.main == null) { return; }

        txtTarget.text = LevelInfoContainer.main.levelName;

        givenTitle = true;
        bossMode = (BossController.main != null && BossController.main.changeLevelName);
	}

    private void Update()
    {
        if (!givenTitle)
        {
            Change();
            if (!bossMode) { Destroy(this); return; }
        }

        // boss mode
        if (BossController.main != null && BossController.main.changeLevelName)
        {
            if (BossController.main.currentBar != lastBossHealth && BossController.main.currentBar < BossController.main.barCount && !BossController.main.defeated)
            {
                lastBossHealth = BossController.main.currentBar;
                rainbowTextEffect = 36;
                txtTarget.text = BossController.main.barInfos[BossController.main.currentBar].name;
            }

            if (BossController.main.defeated)
            {
                txtTarget.text = "";
            }
        }

        if (rainbowTextEffect >= 0)
        {
            --rainbowTextEffect;
            if (rainbowTextEffect%3 == 2)
            {
                txtTarget.color = Color.HSVToRGB(Fakerand.Single(), 0.3f, 1f);
            }
        }
        else
        {
            txtTarget.color = Color.white;
        }
    }
}
