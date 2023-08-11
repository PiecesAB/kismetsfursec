using UnityEngine;

public class PrimPlayableCharacter : MonoBehaviour
{
    public bool playable = true;
    public string myName;

    private void Awake()
    {
        if (!playable) { DestroyImmediate(this); }
    }
}
