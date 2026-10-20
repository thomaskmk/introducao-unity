using UnityEngine;

namespace ObstacleCourse
{
    // Presentation only: movement, detection, damage and colliders stay on the original objects.
    public sealed class PrisonCharacterVisual : MonoBehaviour
    {
        public GameController game;
        public SpriteRenderer prototype;
        public SpriteRenderer character;
        public Sprite[] frames;
        public ChaserEnemy guard;
        private Vector2 previous;
        private int direction;
        private float animationTime;
        private float previousElapsed;
        private Rigidbody2D body;
        private float lastMovementTime;

        private void OnEnable()
        {
            body = GetComponent<Rigidbody2D>();
            previous = body.position;
            direction = 0;
            animationTime = 0;
            previousElapsed = 0;
            lastMovementTime = float.NegativeInfinity;
            if (frames != null && frames.Length == 16) character.sprite = frames[0];
        }

        private void LateUpdate()
        {
            character.enabled = true;
            prototype.enabled = false;
            Vector2 position = body.position;
            Vector2 delta = position - previous;
            previous = position;
            if (game == null || frames == null || frames.Length != 16) return;

            float elapsed = game.Session != null ? game.Session.Elapsed : 0;
            if (elapsed < previousElapsed || delta.sqrMagnitude > 1)
            {
                direction = 0;
                animationTime = 0;
                lastMovementTime = float.NegativeInfinity;
                delta = Vector2.zero;
                character.sprite = frames[0];
            }
            previousElapsed = elapsed;
            if (guard != null && guard.label != null)
                guard.label.color = guard.IsChasing ? new Color(1, .31f, .21f) : new Color(.96f, .88f, .60f);
            if (!game.IsPlaying) return;
            if (delta.sqrMagnitude > .00001f)
            {
                direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                    ? (delta.x > 0 ? 2 : 1) : (delta.y > 0 ? 3 : 0);
                lastMovementTime = Time.time;
            }
            // Render frames can fall between physics steps; keep the walk cycle across that gap.
            bool moving = Time.time - lastMovementTime < .08f;
            if (moving) animationTime += Time.deltaTime;
            else animationTime = 0;
            int frame = moving ? 1 + (Mathf.FloorToInt(animationTime * 9) % 3) : 0;
            character.sprite = frames[direction * 4 + frame];
        }
    }
}
