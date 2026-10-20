using UnityEngine;

namespace ObstacleCourse
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class PatrolHazard : MonoBehaviour
    {
        public GameController game;
        public Vector2 pointA;
        public Vector2 pointB;
        [Min(0.1f)] public float speed = 3;
        public float phaseOffset;
        private Rigidbody2D body;

        private void Awake() { body = GetComponent<Rigidbody2D>(); }
        private void FixedUpdate()
        {
            if (!game.IsPlaying) return;
            float length = Vector2.Distance(pointA, pointB);
            if (length < 0.001f) return;
            float distance = Mathf.PingPong(game.Session.Elapsed * speed + phaseOffset, length);
            body.MovePosition(Vector2.Lerp(pointA, pointB, distance / length));
        }
        public void ResetHazard()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            float length = Vector2.Distance(pointA, pointB);
            body.position = length > 0 ? Vector2.Lerp(pointA, pointB, Mathf.PingPong(phaseOffset, length) / length) : pointA;
        }
        private void OnTriggerEnter2D(Collider2D other) { Hit(other); }
        private void OnTriggerStay2D(Collider2D other) { Hit(other); }
        private static void Hit(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController2D>(out var player)) player.TouchHazard();
        }
    }
}
