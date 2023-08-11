using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    private PlayableDirector pd;

    private void Start()
    {
        pd = GetComponent<PlayableDirector>();
    }

    void Update()
    {

    }
}
