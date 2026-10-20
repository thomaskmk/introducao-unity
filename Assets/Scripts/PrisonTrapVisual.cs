using UnityEngine;

namespace ObstacleCourse
{
    // Reads the same clock as the damage component, without changing its timing or collision.
    public sealed class PrisonTrapVisual : MonoBehaviour
    {
        public CycleTrap trap;
        public SpriteRenderer prototype;
        public SpriteRenderer surface;
        public GameObject safeMarks;
        public GameObject warningMarks;
        public GameObject electricity;
        public SpriteRenderer[] lamps;
        public SpriteRenderer[] arcs;

        private void LateUpdate() { Refresh(); }
        public void Refresh()
        {
            prototype.enabled = false;
            float elapsed = trap.game.Session == null ? 0 : trap.game.Session.Elapsed;
            var phase = trap.PhaseAt(elapsed);
            surface.color = ColorFor(phase);
            safeMarks.SetActive(phase == TrapPhase.Safe);
            warningMarks.SetActive(phase == TrapPhase.Warning);
            electricity.SetActive(phase == TrapPhase.Active);
            var signal = phase == TrapPhase.Safe ? new Color(.35f,1,.65f)
                : phase == TrapPhase.Warning ? new Color(1,.8f,.2f) : new Color(1,.24f,.14f);
            foreach (var lamp in lamps) lamp.color = signal;
            // A gentle pulse tied to the game clock; never flashes off and freezes on pause.
            float pulse = .8f + .2f * Mathf.Sin(elapsed * Mathf.PI * 4);
            foreach (var arc in arcs) arc.color = new Color(.82f, .96f, 1, pulse);
        }

        public static Color ColorFor(TrapPhase phase)
        {
            return phase == TrapPhase.Safe ? new Color(.70f, .77f, .81f)
                : phase == TrapPhase.Warning ? new Color(1, .78f, .22f)
                : new Color(1, .27f, .22f);
        }
    }
}
