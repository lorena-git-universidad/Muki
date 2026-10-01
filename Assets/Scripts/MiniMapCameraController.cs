using UnityEngine;

namespace StarterAssets
{
    public class MiniMapCameraController : MonoBehaviour
    {
        [Header("Player")]
        public Transform player;

        [Header("Camera")]
        public float height = 20f;

        private void LateUpdate()
        {
            if (player == null)
                return;

            transform.position = new Vector3(
                player.position.x,
                player.position.y + height,
                player.position.z
            );

            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}