using UnityEngine;
using System.Collections;

namespace MAP
{
    public interface IMAPContainer
    {
        void OnGainElementOnGround(Vector3 pos, Vector3 eulerAngles);
        void OnMoveElementToGround();
    }
}
