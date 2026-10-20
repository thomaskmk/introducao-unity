using UnityEngine;

namespace ObstacleCourse
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class SlowZone : MonoBehaviour
    {
        [Range(.1f, 1)] public float speedMultiplier = .45f;
        private Collider2D area;

        // Consultar a posição evita efeitos presos após teleporte, reinício ou troca de fase.
        public bool Contains(Vector2 position)
        {
            if (area == null) area = GetComponent<Collider2D>();
            return isActiveAndEnabled && area.OverlapPoint(position);
        }
    }
}
