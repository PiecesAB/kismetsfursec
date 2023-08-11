using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleMenuLogo : MonoBehaviour
{
    void Update()
    {
        transform.localEulerAngles = Vector3.forward * (Camera.main?.transform.eulerAngles.y ?? 0);
    }
}
