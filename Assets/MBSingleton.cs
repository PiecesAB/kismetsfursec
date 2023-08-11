using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MBSingleton<T> : MonoBehaviour
{
    public static T onlyOne = default;

    protected abstract void ChildAwake();

    private void Awake()
    {
        if (!Application.isPlaying)
        {
            onlyOne = default;
            DestroyImmediate(gameObject); return;
        }

        if (onlyOne == null)
        {
            DontDestroyOnLoad(gameObject);
            onlyOne = default;
            ChildAwake();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
