using System;

namespace ObstacleCourse
{
    public enum RunState { Menu, Playing, Paused, Won, Lost, Introduction }
    public enum DefeatCause { None, Contact, Timeout }

    // Regras da partida separadas da cena para poder testar tempo, dano e reinício.
    public sealed class RunSession
    {
        public RunState State { get; private set; } = RunState.Menu;
        public float Duration { get; }
        public float Remaining { get; private set; }
        public float Elapsed => Duration - Remaining;
        public string EndReason { get; private set; } = "";
        public DefeatCause Defeat { get; private set; } = DefeatCause.None;

        public RunSession(float duration)
        {
            if (duration <= 0) throw new ArgumentOutOfRangeException();
            Duration = Remaining = duration;
        }

        public void ShowIntroduction()
        {
            Begin();
            State = RunState.Introduction;
        }

        public void Begin()
        {
            Remaining = Duration;
            EndReason = "";
            Defeat = DefeatCause.None;
            State = RunState.Playing;
        }

        public void Tick(float seconds)
        {
            if (State != RunState.Playing || seconds <= 0) return;
            Remaining = Math.Max(0, Remaining - seconds);
            if (Remaining <= 0) Lose(DefeatCause.Timeout, "Você não alcançou a saída a tempo.");
        }

        public bool TakeDamage()
        {
            if (State != RunState.Playing) return false;
            Lose(DefeatCause.Contact, "Você foi pego durante a fuga. Tente novamente.");
            return true;
        }

        public void Win()
        {
            if (State == RunState.Playing && Remaining > 0) State = RunState.Won;
        }

        public void Pause() { if (State == RunState.Playing) State = RunState.Paused; }
        public void Resume() { if (State == RunState.Paused) State = RunState.Playing; }
        public void ReturnToMenu() { State = RunState.Menu; }
        private void Lose(DefeatCause cause, string reason) { Defeat = cause; EndReason = reason; State = RunState.Lost; }
    }
}
