using UnityEngine;

namespace ObstacleCourse
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
    public sealed class ChaserEnemy : MonoBehaviour
    {
        public GameController game;
        public Vector2 pointA;
        public Vector2 pointB;
        public Rect territory;
        [Min(.1f)] public float patrolSpeed = 1.3f;
        [Min(.1f)] public float chaseSpeed = 3.5f;
        [Min(.1f)] public float detectionRadius = 3.8f;
        public LayerMask walls;
        public TextMesh label;
        public bool IsChasing { get; private set; }
        private Rigidbody2D body;
        private SpriteRenderer visual;
        private bool towardB = true;
        private Vector2 hitSize;

        private void Awake() { Prepare(); }
        private void Prepare()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            hitSize = Vector2.Scale(GetComponent<BoxCollider2D>().size, transform.lossyScale);
        }
        private void FixedUpdate()
        {
            if (!game.IsPlaying) return;
            Vector2 player = game.player.Position;
            bool inRange = Vector2.Distance(body.position, player) <= detectionRadius + (IsChasing ? 1.2f : 0);
            bool visible = !Physics2D.Linecast(body.position, player, walls);
            IsChasing = inRange && territory.Contains(player) && visible;
            Vector2 target = IsChasing ? player : towardB ? pointB : pointA;
            if (!IsChasing && Vector2.Distance(body.position, target) < .1f) towardB = !towardB;
            Vector2 next = Vector2.MoveTowards(body.position, target,
                (IsChasing ? chaseSpeed : patrolSpeed) * Time.fixedDeltaTime);
            next.x = Mathf.Clamp(next.x, territory.xMin, territory.xMax);
            next.y = Mathf.Clamp(next.y, territory.yMin, territory.yMax);

            // A patrulha também respeita paredes adicionadas pelo editor.
            Vector2 delta = next - body.position;
            float distance = delta.magnitude;
            if (distance > 0)
            {
                var hit = Physics2D.BoxCast(body.position, hitSize, 0, delta / distance, distance + .02f, walls);
                if (hit) distance = Mathf.Max(0, hit.distance - .02f);
                body.MovePosition(body.position + delta.normalized * distance);
            }
            visual.color = IsChasing ? new Color(1, .2f, .44f) : new Color(.75f, .33f, .65f);
            if (label != null) label.text = IsChasing ? "!" : "?";
        }
        public void ResetEnemy()
        {
            Prepare();
            body.position = pointA;
            body.linearVelocity = Vector2.zero;
            towardB = true;
            IsChasing = false;
            visual.color = new Color(.75f, .33f, .65f);
            if (label != null) label.text = "?";
        }
        private void OnTriggerEnter2D(Collider2D other) { Hit(other); }
        private void OnTriggerStay2D(Collider2D other) { Hit(other); }
        private static void Hit(Collider2D other)
        {
            if (other.TryGetComponent<PlayerController2D>(out var player)) player.TouchHazard();
        }
    }
}
