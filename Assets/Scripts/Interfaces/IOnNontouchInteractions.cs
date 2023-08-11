using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// it is possible for the player to jump off the corner of a block in coyote time without touching it (this recovers the jump)
// and also perform a wall jump up to 8 units from a wall (this makes spike jumping possible lol).
// but sometimes, this non-touch matters; we still want something to happen.
// warning: the player may still be touching the object while these events fire.
public interface IOnNontouchInteractions
{
    void NormalJumpOff(GameObject plr);
    void WallJumpOff(GameObject plr);
}
