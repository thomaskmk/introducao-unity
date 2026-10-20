using UnityEngine;

namespace ObstacleCourse
{
    // Audio follows the same run state and clock as gameplay. Nothing autoplays in the menu.
    [DefaultExecutionOrder(100)]
    public sealed class CourseAudio : MonoBehaviour
    {
        public GameController game;
        public AudioClip musicLoop;
        public AudioClip uiClick;
        public AudioClip capture;
        public AudioClip victory;
        public AudioClip warning;
        [Range(0, 1)] public float musicVolume = .2f;
        [Range(0, 1)] public float effectsVolume = .55f;
        public AudioSource MusicSource { get; private set; }
        public AudioSource EffectsSource { get; private set; }
        public bool MusicPaused { get; private set; }
        private RunSession lastSession;
        private RunState lastState = RunState.Menu;
        private float lastElapsed;
        private float lastRemaining;
        private float nextWarningAt;
        private TrapPhase[] lastTrapPhases = System.Array.Empty<TrapPhase>();

        private void Awake()
        {
            MusicSource = CreateSource(true, musicVolume);
            EffectsSource = CreateSource(false, effectsVolume);
        }

        private AudioSource CreateSource(bool loop, float volume)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0;
            source.volume = volume;
            return source;
        }

        private void LateUpdate()
        {
            if (game == null || game.Session == null) return;
            var run = game.Session;
            bool newRun = run.State == RunState.Playing &&
                (lastSession != run || lastState == RunState.Menu ||
                 lastState == RunState.Won || lastState == RunState.Lost || run.Elapsed < lastElapsed);
            if (newRun)
            {
                EffectsSource.Stop();
                Play(uiClick, .65f);
                MusicSource.Stop();
                MusicSource.clip = musicLoop;
                MusicSource.volume = musicVolume;
                if (musicLoop != null) MusicSource.Play();
                MusicPaused = false;
                lastTrapPhases = new TrapPhase[game.Level.traps.Length];
                for (int i = 0; i < lastTrapPhases.Length; i++)
                    lastTrapPhases[i] = game.Level.traps[i].PhaseAt(run.Elapsed);
                nextWarningAt = 0;
            }
            else if (run.State != lastState)
            {
                EffectsSource.Stop();
                if (run.State == RunState.Paused)
                {
                    MusicSource.Pause();
                    MusicPaused = true;
                    Play(uiClick, .65f);
                }
                else if (run.State == RunState.Playing)
                {
                    MusicSource.UnPause();
                    MusicPaused = false;
                    Play(uiClick, .65f);
                }
                else
                {
                    MusicSource.Stop();
                    MusicPaused = false;
                    Play(run.State == RunState.Won ? victory : run.State == RunState.Lost ? capture : uiClick);
                }
            }
            if (run.State == RunState.Playing)
            {
                if (!newRun && lastRemaining > 10 && run.Remaining <= 10) Warn();
                for (int i = 0; i < game.Level.traps.Length && i < lastTrapPhases.Length; i++)
                {
                    var trap = game.Level.traps[i];
                    var phase = trap.PhaseAt(run.Elapsed);
                    if (phase == TrapPhase.Warning && phase != lastTrapPhases[i])
                    {
                        var area = trap.GetComponent<Collider2D>();
                        if (Vector2.Distance(game.player.Position, area.ClosestPoint(game.player.Position)) < 2.5f)
                            Warn();
                    }
                    lastTrapPhases[i] = phase;
                }
            }
            lastSession = run;
            lastState = run.State;
            lastElapsed = run.Elapsed;
            lastRemaining = run.Remaining;
        }

        private void Warn()
        {
            if (Time.unscaledTime < nextWarningAt) return;
            Play(warning, .75f);
            nextWarningAt = Time.unscaledTime + .8f;
        }

        private void Play(AudioClip clip, float volume = 1)
        {
            if (clip != null) EffectsSource.PlayOneShot(clip, volume);
        }

        private void OnDisable()
        {
            if (MusicSource != null) MusicSource.Stop();
            if (EffectsSource != null) EffectsSource.Stop();
            MusicPaused = false;
        }
    }
}
