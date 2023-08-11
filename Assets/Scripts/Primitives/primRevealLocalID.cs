using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
[System.Serializable]
public class primRevealLocalID : MonoBehaviour {

    [SerializeField]
    public int ID;

#if (UNITY_EDITOR)
    private void Start()
    {
        if (!Application.isPlaying)
        {
            if (ID != GetInstanceID())
            {
                ID = GetInstanceID();
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
    }

    private void Update () {

        if (!Application.isPlaying)
        {
            if (ID != GetInstanceID())
            {
                ID = GetInstanceID();
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
        else
        {
            enabled = false;
        }
    }

#endif

}

