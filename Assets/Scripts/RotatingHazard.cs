using UnityEngine;

namespace ObstacleCourse
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class RotatingHazard : MonoBehaviour
    {
        public GameController game;
        public float initialAngle;
        public float degreesPerSecond = 95;
        public bool oscillates;
        [Range(0, 180)] public float amplitude = 55;
        [Min(.1f)] public float period = 3.6f;
        private Rigidbody2D body;

        public float AngleAt(float elapsed)
        {
            return initialAngle + (oscillates
                ? amplitude * Mathf.Sin(elapsed * 2 * Mathf.PI / Mathf.Max(.1f, period))
                : elapsed * degreesPerSecond);
        }

        private void Awake() { body = GetComponent<Rigidbody2D>(); }
        private void FixedUpdate()
        {
            if (game.IsPlaying) body.MoveRotation(AngleAt(game.Session.Elapsed));
        }
        public void ResetHazard()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            body.rotation = AngleAt(0);
            body.angularVelocity = 0;
        }
        private void OnTriggerEnter2D(Collider2D other) { Hit(other); }
        private void OnTriggerStay2D(Collider2D other) { Hit(other); }
        private static void Hit(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController2D>(out var player)) player.TouchHazard();
        }
    }
}
