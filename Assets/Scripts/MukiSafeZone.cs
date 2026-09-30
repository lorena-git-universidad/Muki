
using UnityEngine;
using UnityEngine.AI;

namespace StarterAssets
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(NavMeshObstacle))]
    public class MukiSafeZone : MonoBehaviour
    {
        private BoxCollider safeCollider;

        public bool Contains(Vector3 position)
        {
            if (safeCollider == null)
                safeCollider = GetComponent<BoxCollider>();

            Vector3 localPosition =
                transform.InverseTransformPoint(position);

            Vector3 localCenter = safeCollider.center;
            Vector3 halfSize = safeCollider.size * 0.5f;

            return
                Mathf.Abs(localPosition.x - localCenter.x) <= halfSize.x &&
                Mathf.Abs(localPosition.y - localCenter.y) <= halfSize.y &&
                Mathf.Abs(localPosition.z - localCenter.z) <= halfSize.z;
        }

        private void Awake()
        {
            safeCollider = GetComponent<BoxCollider>();
            safeCollider.isTrigger = true;

            NavMeshObstacle obstacle =
                GetComponent<NavMeshObstacle>();

            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.carving = true;
        }
    }
}