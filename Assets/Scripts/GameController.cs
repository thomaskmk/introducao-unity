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
        public bool ChallengeUnlocked => PlayerPrefs.GetInt("ObstacleCourse.ChallengeUnlocked.v2", 0) == 1;
        public float BestTime => PlayerPrefs.GetFloat(BestTimeKey, 0);
        public string Hint => IsPlaying && Time.time < noticeUntil ? notice : Level.Hint(player.Position);
        private string BestTimeKey => "ObstacleCourse.BestTime.v2." + LevelIndex;
        private RunState displayedState = (RunState)(-1);
        private string notice;
        private float noticeUntil;

        private void Awake()
        {
            SelectLevel(0);
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
        private void SelectLevel(int index)
        {
            LevelIndex = index;
            Session = new RunSession(Level.timeLimit, 1, 2);
            for (int i = 0; i < levels.Length; i++) levels[i].gameObject.SetActive(i == index);
            player.spawn = Level.spawn;
            noticeUntil = 0;
        }
        public void PrimaryAction()
        {
            if (Session.State == RunState.Paused) { Session.Resume(); PresentState(); }
            else if (Session.State == RunState.Won && HasNextLevel)
            {
                SelectLevel(LevelIndex + 1);
                StartRun();
            }
            else StartRun();
        }
        public void StartChallenge()
        {
            if (!ChallengeUnlocked || Session.State != RunState.Menu) return;
            SelectLevel(1);
            StartRun();
        }
        public void StartRun()
        {
            Session.Begin();
            noticeUntil = 0;
            player.ResetPlayer();
            Level.ResetLevel();
            Physics2D.SyncTransforms();
            PresentState();
            Debug.Log($"PARTIDA: fase {LevelIndex + 1} — início — {Session.Duration}s, {Session.Lives} vida.");
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
            if (Session.State == RunState.Playing) Session.Pause();
            else if (Session.State == RunState.Paused) Session.Resume();
            PresentState();
        }
        public void TakeDamage()
        {
            if (Session.TakeDamage())
            {
                Debug.Log("PARTIDA: dano — vidas: " + Session.Lives);
                Notify("IMPACTO ABSORVIDO! Você pode avançar durante a breve invulnerabilidade.");
            }
            if (Session.State != displayedState) PresentState();
        }
        public bool CollectLife()
        {
            if (!Session.CollectLife()) return false;
            Notify("+1 VIDA! Você pode absorver um impacto. O personagem pisca durante a proteção.");
            Debug.Log("PARTIDA: vida extra coletada — vidas: " + Session.Lives);
            hud.Refresh(this);
            return true;
        }
        private void Notify(string text) { notice = text; noticeUntil = Time.time + 4; }
        public void ReachFinish()
        {
            if (!IsPlaying) return;
            Session.Win();
            if (BestTime <= 0 || Session.Elapsed < BestTime) PlayerPrefs.SetFloat(BestTimeKey, Session.Elapsed);
            if (HasNextLevel) PlayerPrefs.SetInt("ObstacleCourse.ChallengeUnlocked.v2", 1);
            PlayerPrefs.Save();
            PresentState();
        }
        private void PresentState()
        {
            displayedState = Session.State;
            Time.timeScale = IsPlaying ? 1 : 0;
            hud.ShowState(this);
            if (Session.State == RunState.Won || Session.State == RunState.Lost)
                Debug.Log($"PARTIDA: fase {LevelIndex + 1}; {Session.State}; tempo: {Session.Elapsed:F2}; vidas: {Session.Lives}; pontos: {Session.Score}");
        }
        private void OnApplicationFocus(bool focused)
        {
            if (!focused && IsPlaying) { Session.Pause(); PresentState(); }
        }
        private void OnDestroy() { Time.timeScale = 1; }
    }
}
