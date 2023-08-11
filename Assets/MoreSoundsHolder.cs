using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoreSoundsHolder : MonoBehaviour
{
    public static MoreSoundsHolder main = null;

    private void Awake()
    {
        main = this;
    }
}
