using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ObstacleCourse.Tests
{
    public sealed class TutorialIntroductionTests
    {
        private GameController game;
        private readonly string[] recordKeys = { "ObstacleCourse.BestTime.v6.0", "ObstacleCourse.BestTime.v7.1" };
        private readonly bool[] recordExisted = new bool[2];
        private readonly float[] recordValues = new float[2];

        [UnitySetUp] public IEnumerator LoadCourse()
        {
            for (int i = 0; i < recordKeys.Length; i++)
            {
                recordExisted[i] = PlayerPrefs.HasKey(recordKeys[i]);
                recordValues[i] = PlayerPrefs.GetFloat(recordKeys[i]);
            }
            Time.timeScale = 1;
            yield return SceneManager.LoadSceneAsync("Percurso", LoadSceneMode.Single);
            yield return null;
            game = Object.FindFirstObjectByType<GameController>();
            Assert.NotNull(game);
        }

        [UnityTearDown] public IEnumerator RestoreRecords()
        {
            for (int i = 0; i < recordKeys.Length; i++)
            {
                if (recordExisted[i]) PlayerPrefs.SetFloat(recordKeys[i], recordValues[i]);
                else PlayerPrefs.DeleteKey(recordKeys[i]);
            }
            PlayerPrefs.Save();
            Time.timeScale = 1;
            yield return null;
        }

        [UnityTest] public IEnumerator ThreeHighlightsExplainTheCourseBeforeTimeAndPhysicsStart()
        {
            var interpolation=game.player.GetComponent<Rigidbody2D>().interpolation;
            game.StartTutorial();
            yield return null;
            yield return null;
            var intro = game.hud.introduction;
            var audio = game.GetComponent<CourseAudio>();
            var bodies = Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
            var positions = new Vector2[bodies.Length];
            var angles = new float[bodies.Length];
            for (int i = 0; i < bodies.Length; i++)
            {
                positions[i] = bodies[i].position;
                angles[i] = bodies[i].rotation;
            }

            for (int step = 0; step < 3; step++)
            {
                Assert.AreEqual(step, intro.StepIndex);
                Assert.AreEqual(RunState.Introduction, game.Session.State);
                Assert.IsFalse(game.IsPlaying);
                Assert.AreEqual(0, Time.timeScale);
                Assert.IsTrue(intro.gameObject.activeInHierarchy);
                Assert.IsTrue(game.hud.gameplayUI.activeInHierarchy);
                Assert.IsFalse(game.hud.overlay.activeSelf, "The introduction must not show a pause or defeat modal.");
                Assert.IsFalse(game.hud.guide.card.gameObject.activeSelf);
                Assert.AreEqual(3, game.hud.liveStars.Value);
                Assert.IsFalse(audio.MusicSource.isPlaying, "Gameplay music begins after the explanation.");
                Assert.AreSame(intro.next.gameObject, EventSystem.current.currentSelectedGameObject);
                Assert.IsTrue(intro.spotlight.raycastTarget, "The dimmed layer must intercept underlying HUD clicks.");
                Canvas.ForceUpdateCanvases();
                Assert.LessOrEqual(intro.message.preferredHeight, intro.message.rectTransform.rect.height,
                    "The entire explanation must fit inside its card.");
                Assert.LessOrEqual(intro.title.preferredHeight, intro.title.rectTransform.rect.height);

                if (step == 0)
                {
                    Assert.IsTrue(intro.spotlight.Circular);
                    AssertOpeningContainsWorldPoint(game.player.GetComponent<Collider2D>().bounds.center);
                }
                else if (step == 1)
                {
                    var finish = game.Level.GetComponentInChildren<FinishZone>();
                    AssertOpeningContainsWorldPoint(finish.GetComponent<Collider2D>().bounds.center);
                }
                else
                {
                    Assert.IsFalse(intro.spotlight.Circular);
                    AssertOpeningContainsScreenPoint(RectTransformUtility.WorldToScreenPoint(null, game.hud.timer.transform.position));
                    AssertOpeningContainsScreenPoint(RectTransformUtility.WorldToScreenPoint(null, game.hud.liveStars.transform.position));
                    StringAssert.Contains(game.Level.threeStarSeconds.ToString("0.#"), intro.message.text);
                    StringAssert.Contains(game.Level.twoStarSeconds.ToString("0.#"), intro.message.text);
                }

                game.Session.Tick(1000);
                game.TakeDamage();
                game.ReachFinish();
                game.TogglePause();
                yield return new WaitForSecondsRealtime(.12f);
                Assert.AreEqual(RunState.Introduction, game.Session.State);
                Assert.AreEqual(0, game.Session.Elapsed);
                Assert.AreEqual(game.Level.timeLimit, game.Session.Remaining);
                for (int i = 0; i < bodies.Length; i++)
                {
                    Assert.Less(Vector2.Distance(positions[i], bodies[i].position), .0001f,
                        "Player and obstacles must remain still while reading.");
                    Assert.Less(Mathf.Abs(Mathf.DeltaAngle(angles[i], bodies[i].rotation)), .0001f);
                }

                intro.next.onClick.Invoke();
                // A click and submit in the same frame must not skip a second explanation.
                intro.Advance();
                if (step < 2) Assert.AreEqual(step + 1, intro.StepIndex);
                else
                {
                    Assert.AreEqual(RunState.Playing, game.Session.State);
                    Assert.AreEqual(0, game.Session.Elapsed);
                }
                yield return null;
                yield return null;
            }
            Assert.IsFalse(intro.gameObject.activeSelf);
            Assert.AreEqual(interpolation,game.player.GetComponent<Rigidbody2D>().interpolation);
            Assert.IsTrue(game.IsPlaying);
            Assert.AreEqual(1, Time.timeScale);
            Assert.IsTrue(audio.MusicSource.isPlaying);
            Assert.GreaterOrEqual(game.hud.guide.CurrentTip, 0, "The original proximity tips must resume unchanged.");
            Assert.IsTrue(game.hud.guide.card.gameObject.activeSelf);
            Assert.AreEqual(3, game.hud.liveStars.Value);
        }

        [UnityTest] public IEnumerator ChallengeAndRetriesStayDirectWhileMenuTutorialReplaysTheIntroduction()
        {
            var intro = game.hud.introduction;
            game.StartChallenge();
            yield return null;
            Assert.AreEqual(1, game.LevelIndex);
            Assert.IsTrue(game.IsPlaying);
            Assert.IsFalse(intro.gameObject.activeSelf);
            game.TakeDamage();
            game.PrimaryAction();
            yield return null;
            Assert.AreEqual(1, game.LevelIndex);
            Assert.IsTrue(game.IsPlaying);
            Assert.IsFalse(intro.gameObject.activeSelf);

            game.ReturnToMenu();
            game.StartTutorial();
            yield return null;
            Assert.AreEqual(0, intro.StepIndex);
            for (int i = 0; i < 3; i++)
            {
                intro.Advance();
                yield return null;
            }
            Assert.IsTrue(game.IsPlaying);
            game.TakeDamage();
            game.PrimaryAction();
            yield return null;
            Assert.AreEqual(0, game.LevelIndex);
            Assert.IsTrue(game.IsPlaying);
            Assert.IsFalse(intro.gameObject.activeSelf, "Retrying the tutorial should not repeat three explanations.");

            var body=game.player.GetComponent<Rigidbody2D>();
            body.position=game.Level.spawn+Vector2.right*1.5f;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.Greater(Vector2.Distance((Vector2)body.transform.position,game.Level.spawn),1);
            game.TogglePause();
            game.ReturnToMenu();
            game.StartTutorial();
            yield return null;
            yield return null;
            Assert.AreEqual(RunState.Introduction, game.Session.State);
            Assert.AreEqual(0, intro.StepIndex);
            Assert.IsTrue(intro.gameObject.activeSelf);
            Assert.Less(Vector2.Distance((Vector2)body.transform.position,game.Level.spawn),.001f,
                "The rendered prisoner must return to the spotlight even when interpolation is frozen.");
            AssertOpeningContainsWorldPoint(game.player.GetComponent<PrisonCharacterVisual>().character.bounds.center);
            foreach(var frozenBody in Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None))
            {
                Assert.Less(Vector2.Distance((Vector2)frozenBody.transform.position,frozenBody.position),.001f);
                Assert.Less(Mathf.Abs(Mathf.DeltaAngle(frozenBody.transform.eulerAngles.z,frozenBody.rotation)),.001f);
            }
        }

        private void AssertOpeningContainsWorldPoint(Vector3 world)
        {
            AssertOpeningContainsScreenPoint(Camera.main.WorldToScreenPoint(world));
        }

        private void AssertOpeningContainsScreenPoint(Vector2 screen)
        {
            var spotlight = game.hud.introduction.spotlight;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(spotlight.rectTransform, screen, null, out var local);
            Assert.IsTrue(spotlight.Opening.Contains(local), "The highlighted object must remain inside the clear opening.");
        }
    }
}
