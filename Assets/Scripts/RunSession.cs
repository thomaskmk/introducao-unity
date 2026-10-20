using System;

namespace ObstacleCourse
{
    public enum RunState { Menu, Playing, Paused, Won, Lost }

    // Regras da partida separadas da cena para poder testar tempo, dano e reinício.
    public sealed class RunSession
    {
        public RunState State { get; private set; } = RunState.Menu;
        public float Duration { get; }
        public int MaximumLives { get; }
        public float Remaining { get; private set; }
        public float Elapsed => Duration - Remaining;
        public int Lives { get; private set; }
        public string EndReason { get; private set; } = "";
        public int Score => State == RunState.Won ? 1000 + (int)Math.Ceiling(Remaining * 10) + Lives * 100 : 0;

        public RunSession(float duration, int lives)
        {
            if (duration <= 0 || lives <= 0) throw new ArgumentOutOfRangeException();
            Duration = Remaining = duration;
            MaximumLives = Lives = lives;
        }

        public void Begin()
        {
            Remaining = Duration;
            Lives = MaximumLives;
            EndReason = "";
            State = RunState.Playing;
        }

        public void Tick(float seconds)
        {
            if (State != RunState.Playing || seconds <= 0) return;
            Remaining = Math.Max(0, Remaining - seconds);
            if (Remaining <= 0) Lose("O tempo acabou.");
        }

        public bool TakeDamage()
        {
            if (State != RunState.Playing) return false;
            Lives = Math.Max(0, Lives - 1);
            if (Lives == 0) Lose("Você perdeu todas as vidas.");
            return true;
        }

        public void Win()
        {
            if (State == RunState.Playing && Remaining > 0) State = RunState.Won;
        }

        public void Pause() { if (State == RunState.Playing) State = RunState.Paused; }
        public void Resume() { if (State == RunState.Paused) State = RunState.Playing; }
        public void ReturnToMenu() { State = RunState.Menu; }
        private void Lose(string reason) { EndReason = reason; State = RunState.Lost; }
    }
}
