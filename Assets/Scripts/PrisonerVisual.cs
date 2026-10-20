using UnityEngine;

namespace ObstacleCourse
{
    // Presentation only: the original body, collider, movement and damage remain on Player.
    public sealed class PrisonerVisual : MonoBehaviour
    {
        public GameController game;
        public SpriteRenderer prototype;
        public SpriteRenderer character;
        public Sprite[] frames;
        private Vector2 previous;
        private int direction;
        private float animationTime;
        private float previousElapsed;

        private void OnEnable()
        {
            previous = transform.position;
            direction = 0;
            animationTime = 0;
        }

        private void LateUpdate()
        {
            bool show = game != null && game.LevelIndex == 0;
            character.enabled = show;
            prototype.enabled = !show;
            Vector2 position = transform.position;
            Vector2 delta = position - previous;
            previous = position;
            if (!show || frames == null || frames.Length != 16) return;

            float elapsed = game.Session != null ? game.Session.Elapsed : 0;
            if (elapsed < previousElapsed || delta.sqrMagnitude > 1)
            {
                direction = 0;
                animationTime = 0;
                delta = Vector2.zero;
            }
            previousElapsed = elapsed;
            if (!game.IsPlaying) return;
            bool moving = delta.sqrMagnitude > .00001f;
            if (moving)
            {
                direction = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                    ? (delta.x > 0 ? 2 : 1) : (delta.y > 0 ? 3 : 0);
                animationTime += Time.deltaTime;
            }
            else animationTime = 0;
            int frame = moving ? 1 + (Mathf.FloorToInt(animationTime * 9) % 3) : 0;
            character.sprite = frames[direction * 4 + frame];
        }
    }
}
