using UnityEngine;

// returns true if the sprite is facing in a direction
public class InGameConditionalFacingDirection : MonoBehaviour, InGameConditional
{
    public enum Direction
    {
        Right, Left
    }

    public Direction dir;

    public bool Evaluate()
    {
        SpriteRenderer sr = LevelInfoContainer.GetActiveControl()?.GetComponent<SpriteRenderer>();
        if (!sr) { return false; }
        return sr.flipX == (dir == Direction.Left);
    }

    public string GetInfo()
    {
        return "Facing wrong direction";
    }
}
