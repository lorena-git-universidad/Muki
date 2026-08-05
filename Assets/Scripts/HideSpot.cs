using UnityEngine;

namespace StarterAssets
{
    public class HideSpot : MonoBehaviour
    {
        public Transform hidePoint;

        public void Interact(PlayerHide player)
        {
            if (player == null)
                return;

            player.ToggleHide(hidePoint);
        }
    }
}