using UnityEngine;

namespace ObstacleCourse
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(SpriteRenderer))]
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
        private CapsuleCollider2D hitbox;
        private ContactFilter2D wallFilter;
        private readonly RaycastHit2D[] wallHits = new RaycastHit2D[4];

        private void Awake() { Prepare(); }
        private void Prepare()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            if (hitbox == null) hitbox = GetComponent<CapsuleCollider2D>();
            wallFilter.SetLayerMask(walls);
            wallFilter.useTriggers = false;
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

            MoveAlongWalls(next - body.position);
            visual.color = IsChasing ? new Color(1, .2f, .44f) : new Color(.75f, .33f, .65f);
            if (label != null) label.text = IsChasing ? "!" : "?";
        }
        private void MoveAlongWalls(Vector2 remaining)
        {
            Vector2 position = body.position;
            Vector2 scale = transform.lossyScale;
            Vector2 size = Vector2.Scale(hitbox.size, new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y))) + Vector2.one * .04f;
            Vector2 offset = transform.TransformVector(hitbox.offset);
            // Expand the cast by a .02 skin in every direction, including shallow approaches.
            // Sweep from the physics position, not the interpolated artwork position.
            // Keep the tangential movement when a wall blocks the diagonal approach.
            for (int step = 0; step < 3 && remaining.sqrMagnitude > .000001f; step++)
            {
                float distance = remaining.magnitude;
                Vector2 direction = remaining / distance;
                int count = Physics2D.CapsuleCast(position + offset, size, hitbox.direction,
                    body.rotation, direction, wallFilter, wallHits, distance);
                float travel = distance;
                Vector2 normal = Vector2.zero;
                for (int i = 0; i < count; i++)
                {
                    float approach = -Vector2.Dot(direction, wallHits[i].normal);
                    if (approach <= .001f) continue;
                    float allowed = Mathf.Max(0, wallHits[i].distance);
                    if (allowed < travel) { travel = allowed; normal = wallHits[i].normal; }
                }
                position += direction * travel;
                if (normal == Vector2.zero) break;
                remaining -= direction * travel;
                remaining -= normal * Mathf.Min(0, Vector2.Dot(remaining, normal));
            }
            body.MovePosition(position);
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
