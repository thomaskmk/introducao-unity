using UnityEngine;

namespace ObstacleCourse
{
    public sealed class FinishZone : MonoBehaviour
    {
        public GameController game;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController2D>(out _)) game.ReachFinish();
        }
    }
}
