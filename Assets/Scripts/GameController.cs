using UnityEngine;
using UnityEngine.InputSystem;

namespace ObstacleCourse
{
    public sealed class GameController : MonoBehaviour
    {
        public CourseLevel[] levels;
        public PlayerController2D player;
        public CourseHud hud;
        public RunSession Session { get; private set; }
        public int LevelIndex { get; private set; }
        public CourseLevel Level => levels[LevelIndex];
        public bool IsPlaying => Session != null && Session.State == RunState.Playing;
        public bool HasNextLevel => LevelIndex + 1 < levels.Length;
        public float BestTime => PlayerPrefs.GetFloat(BestTimeKey, 0);
        // Live score: the rating still available if this attempt succeeds.
        public int CurrentStars => Session.State == RunState.Introduction || Session.State == RunState.Playing || Session.State == RunState.Paused || Session.State == RunState.Won
            ? Level.StarsForTime(Session.Elapsed) : 0;
        public int EarnedStars => Session.State == RunState.Won ? Level.StarsForTime(Session.Elapsed) : 0;
        public int BestStars => BestTime > 0 ? Level.StarsForTime(BestTime) : 0;
        private string BestTimeKey => "ObstacleCourse.BestTime.v" + Level.layoutVersion + "." + LevelIndex;
        private RunState displayedState = (RunState)(-1);

        private void Awake()
        {
            SelectLevel(0);
            Application.targetFrameRate = 60;
        }
        private void Start() { PresentState(); }
        private void Update()
        {
            if (Session.State == RunState.Introduction) { hud.Refresh(this); return; }
            var keys = Keyboard.current;
            var pad = Gamepad.current;
            if ((keys != null && (keys.escapeKey.wasPressedThisFrame || keys.pKey.wasPressedThisFrame))
                || (pad != null && pad.startButton.wasPressedThisFrame)) TogglePause();
            if (keys != null && keys.rKey.wasPressedThisFrame && Session.State != RunState.Menu) StartRun();
            Session.Tick(Time.deltaTime);
            if (Session.State != displayedState) PresentState();
            hud.Refresh(this);
        }
        private void SelectLevel(int index)
        {
            LevelIndex = index;
            Session = new RunSession(Level.timeLimit);
            for (int i = 0; i < levels.Length; i++) levels[i].gameObject.SetActive(i == index);
            player.spawn = Level.spawn;
        }
        public void PrimaryAction()
        {
            if (Session.State == RunState.Introduction) { hud.introduction.Advance(); }
            else if (Session.State == RunState.Menu) { StartChallenge(); }
            else if (Session.State == RunState.Paused) { Session.Resume(); PresentState(); }
            else if (Session.State == RunState.Won && HasNextLevel)
            {
                SelectLevel(LevelIndex + 1);
                StartRun();
            }
            else StartRun();
        }
        public void StartChallenge()
        {
            if (Session.State != RunState.Menu) return;
            SelectLevel(1);
            StartRun();
        }
        public void StartTutorial()
        {
            if (Session.State != RunState.Menu) return;
            SelectLevel(0);
            Session.ShowIntroduction();
            player.ResetPlayer();
            Level.ResetLevel();
            Physics2D.SyncTransforms();
            hud.introduction.Begin();
            PresentState();
        }
        public void StartRun()
        {
            Session.Begin();
            player.ResetPlayer();
            Level.ResetLevel();
            Physics2D.SyncTransforms();
            PresentState();
            Debug.Log($"PARTIDA: fase {LevelIndex + 1} — início — {Session.Duration}s.");
        }
        public void ReturnToMenu()
        {
            SelectLevel(0);
            player.ResetPlayer();
            Level.ResetLevel();
            PresentState();
        }
        public void TogglePause()
        {
            if (Session.State == RunState.Introduction) return;
            if (Session.State == RunState.Playing) Session.Pause();
            else if (Session.State == RunState.Paused) Session.Resume();
            PresentState();
        }
        public void TakeDamage()
        {
            if (Session.TakeDamage())
            {
                Debug.Log("PARTIDA: tentativa encerrada por contato com um perigo.");
            }
            if (Session.State != displayedState) PresentState();
        }
        public void ReachFinish()
        {
            if (!IsPlaying) return;
            Session.Win();
            if (BestTime <= 0 || Session.Elapsed < BestTime) PlayerPrefs.SetFloat(BestTimeKey, Session.Elapsed);
            PlayerPrefs.Save();
            PresentState();
        }
        private void PresentState()
        {
            displayedState = Session.State;
            Time.timeScale = IsPlaying ? 1 : 0;
            hud.ShowState(this);
            if (Session.State == RunState.Won || Session.State == RunState.Lost)
                Debug.Log($"PARTIDA: fase {LevelIndex + 1}; {Session.State}; tempo: {Session.Elapsed:F2}; estrelas: {EarnedStars}");
        }
        private void OnApplicationFocus(bool focused)
        {
            if (!focused && IsPlaying) { Session.Pause(); PresentState(); }
        }
        private void OnDestroy() { Time.timeScale = 1; }
    }
}
