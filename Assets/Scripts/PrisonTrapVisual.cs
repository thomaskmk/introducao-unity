using UnityEngine;

namespace ObstacleCourse
{
    // Reads the same clock as the damage component, without changing its timing or collision.
    public sealed class PrisonTrapVisual : MonoBehaviour
    {
        public CycleTrap trap;
        public SpriteRenderer prototype;
        public SpriteRenderer surface;

        private void LateUpdate()
        {
            prototype.enabled = false;
            var phase = trap.PhaseAt(trap.game.Session == null ? 0 : trap.game.Session.Elapsed);
            surface.color = ColorFor(phase);
        }

        public static Color ColorFor(TrapPhase phase)
        {
            return phase == TrapPhase.Safe ? new Color(.70f, .77f, .81f)
                : phase == TrapPhase.Warning ? new Color(1, .78f, .22f)
                : new Color(1, .27f, .22f);
        }
    }
}
