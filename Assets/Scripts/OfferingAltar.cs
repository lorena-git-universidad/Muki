using UnityEngine;

namespace StarterAssets
{
    public class OfferingAltar : MonoBehaviour
    {
        [Header("Altar")]
        public Transform offeringPoint;

        [Header("Effect")]
        public float activeDuration = 60f;

        [Header("State")]
        public bool isUsed = false;
        public bool isActive = false;

        private float remainingTime;

        private OfferingItem placedOffering;

        public bool CanUse()
        {
            return !isUsed && !isActive;
        }

        public bool PlaceOffering(OfferingItem offering)
        {
            if (offering == null)
                return false;

            if (!CanUse())
                return false;

            if (offeringPoint == null)
            {
                Debug.LogError(
                    "El altar no tiene un OfferingPoint asignado."
                );

                return false;
            }

            // Guardar referencia
            placedOffering = offering;

            // Colocar físicamente la ofrenda
            offering.PlaceOnAltar(offeringPoint);

            // Activar altar
            isUsed = true;
            isActive = true;

            remainingTime = activeDuration;

            Debug.Log(
                "¡Altar activado! Duración: " +
                activeDuration +
                " segundos."
            );

            return true;
        }

        private void Update()
        {
            if (!isActive)
                return;

            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                isActive = false;

                Debug.Log(
                    "La ofrenda del altar se agotó."
                );
            }
        }

        public float GetRemainingTime()
        {
            return remainingTime;
        }
    }
}