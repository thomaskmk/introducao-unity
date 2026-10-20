using UnityEngine;
using UnityEngine.InputSystem;

namespace ObstacleCourse
{
    public sealed class GameController : MonoBehaviour
    {
        [Min(1)] public float timeLimit = 60;
        [Min(1)] public int initialLives = 3;
        public PlayerController2D player;
        public PatrolHazard[] hazards;
        public CourseHud hud;
        public RunSession Session { get; private set; }
        public bool IsPlaying => Session != null && Session.State == RunState.Playing;
        private RunState displayedState = (RunState)(-1);
        private const string BestTimeKey = "ObstacleCourse.BestTime.v1";
        public float BestTime => PlayerPrefs.GetFloat(BestTimeKey, 0);

        private void Awake()
        {
            Session = new RunSession(timeLimit, initialLives);
            Application.targetFrameRate = 60;
        }
        private void Start() { PresentState(); }
        private void Update()
        {
            var keys = Keyboard.current;
            var pad = Gamepad.current;
            if ((keys != null && (keys.escapeKey.wasPressedThisFrame || keys.pKey.wasPressedThisFrame))
                || (pad != null && pad.startButton.wasPressedThisFrame)) TogglePause();
            if (keys != null && keys.rKey.wasPressedThisFrame && Session.State != RunState.Menu) StartRun();
            Session.Tick(Time.deltaTime);
            if (Session.State != displayedState) PresentState();
            hud.Refresh(this);
        }
        public void PrimaryAction()
        {
            if (Session.State == RunState.Paused) Session.Resume();
            else StartRun();
            PresentState();
        }
        public void StartRun()
        {
            Session.Begin();
            player.ResetPlayer();
            foreach (var hazard in hazards) hazard.ResetHazard();
            PresentState();
            Debug.Log($"PARTIDA: início — {Session.Duration} segundos, {Session.MaximumLives} vidas.");
        }
        public void ReturnToMenu()
        {
            Session.ReturnToMenu();
            player.ResetPlayer();
            foreach (var hazard in hazards) hazard.ResetHazard();
            PresentState();
        }
        public void TogglePause()
        {
            if (Session.State == RunState.Playing) Session.Pause();
            else if (Session.State == RunState.Paused) Session.Resume();
            PresentState();
        }
        public void TakeDamage()
        {
            if (Session.TakeDamage()) Debug.Log("PARTIDA: dano — vidas: " + Session.Lives);
            if (Session.State != displayedState) PresentState();
        }
        public void ReachFinish()
        {
            if (!IsPlaying) return;
            Session.Win();
            if (BestTime <= 0 || Session.Elapsed < BestTime)
            {
                PlayerPrefs.SetFloat(BestTimeKey, Session.Elapsed);
                PlayerPrefs.Save();
            }
            PresentState();
        }
        private void PresentState()
        {
            displayedState = Session.State;
            Time.timeScale = IsPlaying ? 1 : 0;
            hud.ShowState(this);
            if (Session.State == RunState.Won || Session.State == RunState.Lost)
                Debug.Log($"PARTIDA: {Session.State}; tempo: {Session.Elapsed:F2}; vidas: {Session.Lives}; pontos: {Session.Score}");
        }
        private void OnApplicationFocus(bool focused)
        {
            if (!focused && IsPlaying) { Session.Pause(); PresentState(); }
        }
        private void OnDestroy() { Time.timeScale = 1; }
    }
}
