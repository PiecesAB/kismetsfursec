using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BulletHellMakerFunctions))]
[RequireComponent(typeof(BulletMakerTracker))]
public class BulletScreenWrap : MonoBehaviour
{
    private BulletHellMakerFunctions maker;
    private BulletMakerTracker tracker;

    public enum DogType
    {
        Horizontal, Vertical
    }

    public DogType dogType;
    public bool deleteAllBulletsIfRendererInvisible = true;

    private Dictionary<DogType, Vector3[]> dogPositions = new Dictionary<DogType, Vector3[]>()
    {
        {DogType.Horizontal, new Vector3[2]{new Vector3(320, 0), new Vector3(-320, 0)} },
        {DogType.Vertical, new Vector3[2]{new Vector3(0, 216), new Vector3(0, -216)} },
    };
    private Dictionary<BulletObject, BulletObject[]> dogTrackers = new Dictionary<BulletObject, BulletObject[]>();

    private void Start()
    {
        maker = GetComponent<BulletHellMakerFunctions>();
        tracker = GetComponent<BulletMakerTracker>();
    }

    private void DestroyDogTrackerEntry(BulletObject b)
    {
        if (BulletRegister.IsRegistered(b)) { BulletRegister.MarkToDestroy(b); }
        for (int i = 0; i < dogTrackers[b].Length; ++i)
        {
            if (BulletRegister.IsRegistered(dogTrackers[b][i])) { BulletRegister.MarkToDestroy(dogTrackers[b][i]); }
        }
    }

    private void WrapBulletPosition(BulletObject b)
    {
        Vector3 camPos = b.GetCameraPosition();
        switch (dogType)
        {
            case DogType.Horizontal:
                if (camPos.x < -160f) { b.originPosition += new Vector3(320f, 0); }
                else if (camPos.x > 160f) { b.originPosition += new Vector3(-320f, 0); }
                break;
            case DogType.Vertical:
                if (camPos.y < -108f) { b.originPosition += new Vector3(0, 216f); }
                else if (camPos.y > 108f) { b.originPosition += new Vector3(0, -216f); }
                break;
        }
    }

    private void Update()
    {
        for (int i = 0; i < tracker.myBullets.Count; ++i)
        {
            if (deleteAllBulletsIfRendererInvisible && maker.tangibleRenderer && !maker.tangibleRenderer.isVisible) { break; }
            BulletObject b = tracker.myBullets[i];
            if (!BulletRegister.IsRegistered(b) || b.materialInternalIdx == -1 || b.textureInternalIdx == -1) { continue; }
            WrapBulletPosition(b);

            if (!dogTrackers.ContainsKey(b))
            {
                BulletObject[] newDogs = new BulletObject[2];
                for (int j = 0; j < dogPositions[dogType].Length; ++j) {
                    newDogs[j] = b.ShallowCopy();
                    newDogs[j].simulationSpeedMult = 0;
                    newDogs[j].destroyOnLeaveScreen = false;
                    BulletRegister.Register(ref newDogs[j], BulletRegister.materials[b.materialInternalIdx], BulletRegister.textures[b.textureInternalIdx]);
                }
                dogTrackers.Add(b, newDogs);
            }
            for (int j = 0; j < dogTrackers[b].Length; ++j)
            {
                dogTrackers[b][j].UpdateTransform(b.GetPosition() + dogPositions[dogType][j], b.GetRotationDegrees(), b.GetScale());
                dogTrackers[b][j].grazed = b.grazed;
                dogTrackers[b][j].color = b.color;
                dogTrackers[b][j].renderGroup = new BulletControllerHelper.RenderGroup(b.materialInternalIdx, b.textureInternalIdx, b.color);
            }
        }

        if (dogTrackers.Count == 0) { return; }

        BulletObject[] dogKeys = dogTrackers.Keys.ToArray();

        for (int i = 0; i < dogKeys.Length; ++i)
        {
            BulletObject key = dogKeys[i];
            if ((deleteAllBulletsIfRendererInvisible && maker.tangibleRenderer && !maker.tangibleRenderer.isVisible) 
                || !BulletRegister.IsRegistered(key))
            {
                DestroyDogTrackerEntry(key);
                continue;
            }
            for (int j = 0; j < dogTrackers[key].Length; ++j)
            {
                if (!BulletRegister.IsRegistered(dogTrackers[key][j]))
                {
                    DestroyDogTrackerEntry(key);
                    continue;
                }
            }
        }
    }


}
