using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
//using UnityEditor;
//using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class LevelInfoAutoTheme : MonoBehaviour
{
    #if UNITY_EDITOR
    private static Regex regex = new Regex(@"/");

    private void Awake()
    {
        //if (Application.isPlaying) { return; }
        string path = SceneManager.GetActiveScene().path;
        var rmatches = regex.Matches(path);
        if (rmatches.Count < 2) { return; }
        int begin = rmatches[rmatches.Count - 2].Index;
        int len = rmatches[rmatches.Count - 1].Index - begin;
        string themeName = path.Substring(begin + 1, len - 1);
        LevelInfoContainer.Theme themeReal = (LevelInfoContainer.Theme)Enum.Parse(typeof(LevelInfoContainer.Theme), themeName, true);
        GetComponent<LevelInfoContainer>().levelTheme = themeReal;
        //EditorUtility.SetDirty(GetComponent<LevelInfoContainer>());
        //EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
    #endif
}
