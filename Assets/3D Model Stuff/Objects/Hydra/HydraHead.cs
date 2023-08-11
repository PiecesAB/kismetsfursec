using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydraHead : MonoBehaviour
{
    public HydraObstacle mainPart;
    public PrimEnemyHealth health;
    public Renderer[] turnRed;
    public float honkTime = 0.7f;
    private List<Color> oldColors;
    [HideInInspector]
    public bool honking = false;

    private IEnumerator Honk()
    {
        if (honking) { yield break; }
        honking = true;
        health.enabled = false;
        float oldSpeed = mainPart.speed;
        mainPart.speed = 0.01f;
        oldColors = new List<Color>();
        for (int i = 0; i < turnRed.Length; ++i)
        {
            Renderer r = turnRed[i];
            for (int j = 0; j < r.materials.Length; ++j)
            {
                Color c = r.materials[j].color;
                oldColors.Add(c);
                r.materials[j].color = Color.red * (0.3f*c.r + 0.59f*c.g + 0.11f*c.b);
            }
        }
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(honkTime);
        GetComponent<AudioSource>().Stop();
        int k = 0;
        for (int i = 0; i < turnRed.Length; ++i)
        {
            Renderer r = turnRed[i];
            for (int j = 0; j < r.materials.Length; ++j)
            {
                r.materials[j].color = oldColors[k++];
            }
        }
        if (!HydraObstacle.commonClone) { Destroy(mainPart.gameObject); yield break; }
        GameObject leftClone = Instantiate(HydraObstacle.commonClone, mainPart.transform.position, mainPart.transform.rotation, mainPart.transform.parent);
        GameObject rightClone = Instantiate(HydraObstacle.commonClone, mainPart.transform.position, mainPart.transform.rotation, mainPart.transform.parent);
        leftClone.SetActive(true);
        rightClone.SetActive(true);
        HydraObstacle leftCloneH = leftClone.GetComponent<HydraObstacle>();
        HydraObstacle rightCloneH = rightClone.GetComponent<HydraObstacle>();
        leftCloneH.speed = rightCloneH.speed = oldSpeed;
        leftCloneH.initialAngleDegrees = mainPart.transform.eulerAngles.z - 90f;
        rightCloneH.initialAngleDegrees = mainPart.transform.eulerAngles.z + 90f;
        Destroy(mainPart.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 19)
        {
            StartCoroutine(Honk());
        }
        else if (col.gameObject.tag == "SuperRay")
        {
            col.GetComponent<SuperRay>().currentThickness = 0f;
            StartCoroutine(Honk());
        }
    }
}
