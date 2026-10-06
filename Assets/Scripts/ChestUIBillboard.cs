using UnityEngine;

namespace StarterAssets
{
    public sealed class ChestUIBillboard : MonoBehaviour
    {
        private Camera targetCamera;

        private void LateUpdate()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            if (targetCamera == null) return;

            Vector3 direction = targetCamera.transform.position - transform.position;
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up) * Quaternion.Euler(0f, 180f, 0f);
        }
    }
}
