using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace MAP
{
    public class MAPContainer : MonoBehaviour, IMAPContainer
    {
        public virtual void OnDrawGizmos()
        {

        }

        public virtual void OnDrawInspector()
        {

        }

        public virtual void OnMoveElementToGround()
        {

        }

        public virtual void OnGainElementOnGround(Vector3 pos, Vector3 eulerAngles)
        {

        }
    }
}