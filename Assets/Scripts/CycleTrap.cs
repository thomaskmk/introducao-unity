using UnityEngine;

namespace ObstacleCourse
{
    public enum TrapPhase { Safe, Warning, Active }

    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class CycleTrap : MonoBehaviour
    {
        public GameController game;
        [Min(.1f)] public float safeSeconds = 3;
        [Min(.1f)] public float warningSeconds = 1;
        [Min(.1f)] public float activeSeconds = 2;
        public float phaseOffset;
        public TextMesh label;
        public TrapPhase Phase { get; private set; }
        private SpriteRenderer visual;
        private Collider2D area;
        private Collider2D playerCollider;

        public TrapPhase PhaseAt(float elapsed)
        {
            float time = Mathf.Repeat(elapsed + phaseOffset, safeSeconds + warningSeconds + activeSeconds);
            return time < safeSeconds ? TrapPhase.Safe
                : time < safeSeconds + warningSeconds ? TrapPhase.Warning : TrapPhase.Active;
        }
        private void Update() { Refresh(); }
        private void FixedUpdate()
        {
            if (!game.IsPlaying || PhaseAt(game.Session.Elapsed) != TrapPhase.Active) return;
            if (area == null) area = GetComponent<Collider2D>();
            if (playerCollider == null) playerCollider = game.player.GetComponent<Collider2D>();
            // Sleeping bodies can stop sending trigger-stay events. The electric floor
            // must still hit a stationary prisoner when the current switches on.
            if (area.enabled && playerCollider.enabled && area.Distance(playerCollider).isOverlapped)
                game.player.TouchHazard();
        }
        public void Refresh()
        {
            if (visual == null) visual = GetComponent<SpriteRenderer>();
            Phase = PhaseAt(game.Session == null ? 0 : game.Session.Elapsed);
            visual.color = Phase == TrapPhase.Safe ? new Color(.22f, .31f, .36f)
                : Phase == TrapPhase.Warning ? new Color(1, .78f, .22f) : new Color(.96f, .25f, .25f);
            if (label != null)
            {
                label.text = Phase == TrapPhase.Safe ? "SEM CORRENTE · PODE PASSAR"
                    : Phase == TrapPhase.Warning ? "LIGANDO · AGUARDE" : "ELETRIFICADO · NÃO PISE";
                label.color = Phase == TrapPhase.Safe ? new Color(.48f, 1, .78f)
                    : Phase == TrapPhase.Warning ? new Color(1, .84f, .35f) : new Color(1, .60f, .53f);
            }
        }
        private void OnTriggerEnter2D(Collider2D other) { Hit(other); }
        private void OnTriggerStay2D(Collider2D other) { Hit(other); }
        private void Hit(Collider2D other)
        {
            // A fase deriva do mesmo relógio do HUD, inclusive no instante da colisão.
            if (game.IsPlaying && PhaseAt(game.Session.Elapsed) == TrapPhase.Active
                && other.TryGetComponent<PlayerController2D>(out var player)) player.TouchHazard();
        }
    }
}
