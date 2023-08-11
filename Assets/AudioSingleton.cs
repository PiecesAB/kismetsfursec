using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSingleton : MonoBehaviour
{
    [Header("Only one object with this ID can play sounds in the scene.")]
    [Header("The only such AudioSource is also the latest one created.")]
    public string ID;

    public static Dictionary<string, AudioSource[]> map = new Dictionary<string, AudioSource[]>();

    private void Start()
    {
        AudioSource[] a = GetComponents<AudioSource>();
        if (map.ContainsKey(ID))
        {
            for (int i = 0; i < map[ID].Length; ++i)
            {
                if (map[ID][i] == null) { continue; }
                map[ID][i].Stop();
                map[ID][i].enabled = false;
            }
        }
        map[ID] = a ?? throw new System.Exception("AudioSingleton is not in an object with AudioSource!");
    }
}
