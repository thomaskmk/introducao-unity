using UnityEngine;

namespace ObstacleCourse
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class ExtraLife : MonoBehaviour
    {
        public GameController game;
        public bool Collected { get; private set; }

        private void OnTriggerEnter2D(Collider2D other) { TryCollect(other); }
        private void OnTriggerStay2D(Collider2D other) { TryCollect(other); }
        private void TryCollect(Collider2D other)
        {
            if (Collected || !other.TryGetComponent<PlayerController2D>(out _)) return;
            if (!game.CollectLife()) return;
            Collected = true;
            SetVisible(false);
        }
        public void ResetPickup() { Collected = false; SetVisible(true); }
        private void SetVisible(bool visible)
        {
            GetComponent<Collider2D>().enabled = visible;
            foreach (var visual in GetComponentsInChildren<Renderer>()) visual.enabled = visible;
        }
    }
}
