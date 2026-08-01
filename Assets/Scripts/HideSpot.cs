using UnityEngine;

namespace StarterAssets
{
    public class HideSpot : MonoBehaviour
    {
        public Transform hidePoint;

        public void Interact(PlayerHide player)
        {
            Debug.Log("HideSpot -> Interact");
            player.ToggleHide(hidePoint);
        }
    }
}