using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class HideSpot : MonoBehaviour
    {
        public Transform hidePoint;

        [Header("Geometria del vagon (opcional)")]
        [Tooltip("Colliders del vagon usados para detectar el suelo interior. " +
                 "Si se deja vacio, se buscan automaticamente en los hijos " +
                 "(se ignoran triggers y colliders del propio Hide Spot).")]
        public Collider[] wagonColliders;

        [Tooltip("Margen extra por encima del collider desde donde parte el rayo hacia abajo.")]
        public float floorRayExtraHeight = 0.5f;

        public void Interact(PlayerHide player)
        {
            if (player == null)
                return;

            player.ToggleHide(hidePoint);
        }

        // Devuelve la Y del primer suelo que encuentra un rayo vertical
        // que baja desde arriba del vagon, en el XZ del HidePoint.
        // Para un vagon abierto por arriba, es el suelo interior.
        public bool TryGetFloorY(out float floorY)
        {
            floorY = 0f;

            if (hidePoint == null)
                return false;

            List<Collider> colliders = new List<Collider>();
            CollectColliders(colliders);

            if (colliders.Count == 0)
                return false;

            float top = float.NegativeInfinity;
            float bottom = float.PositiveInfinity;

            foreach (Collider c in colliders)
            {
                top = Mathf.Max(top, c.bounds.max.y);
                bottom = Mathf.Min(bottom, c.bounds.min.y);
            }

            Vector3 origin = hidePoint.position;
            origin.y = top + floorRayExtraHeight;

            Ray ray = new Ray(origin, Vector3.down);
            float maxDistance = (origin.y - bottom) + floorRayExtraHeight;

            bool found = false;
            float bestY = float.NegativeInfinity;

            foreach (Collider c in colliders)
            {
                // Collider.Raycast solo prueba contra este collider,
                // asi que nunca golpea al jugador ni a otros objetos.
                if (c.Raycast(ray, out RaycastHit hit, maxDistance) &&
                    hit.point.y > bestY)
                {
                    bestY = hit.point.y;
                    found = true;
                }
            }

            if (found)
                floorY = bestY;

            return found;
        }

        // Direccion horizontal hacia la que debe mirar el jugador al estar escondido.
        // Convencion: el eje Z (azul) del HidePoint apunta al frente del vagon.
        public Vector3 GetFacingDirection()
        {
            Vector3 forward = Vector3.zero;

            if (hidePoint != null)
            {
                forward = hidePoint.forward;
                forward.y = 0f;
            }

            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = transform.forward;
                forward.y = 0f;
            }

            if (forward.sqrMagnitude < 0.0001f)
                return Vector3.forward;

            return forward.normalized;
        }

        private void CollectColliders(List<Collider> result)
        {
            result.Clear();

            if (wagonColliders != null && wagonColliders.Length > 0)
            {
                foreach (Collider c in wagonColliders)
                {
                    if (c != null && c.enabled)
                        result.Add(c);
                }

                return;
            }

            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                if (!c.enabled || c.isTrigger)
                    continue;

                // Ignora colliders del propio Hide Spot (p. ej. el de interaccion).
                if (c.gameObject == gameObject)
                    continue;

                result.Add(c);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (hidePoint == null)
                return;

            Vector3 p = hidePoint.position;
            Vector3 dir = GetFacingDirection();

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(p, p + dir);
            Gizmos.DrawSphere(p + dir, 0.05f);

            if (TryGetFloorY(out float y))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(new Vector3(p.x, y, p.z), 0.1f);
            }
        }
    }
}