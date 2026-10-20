using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ObstacleCourse.Tests
{
    public sealed class CourseIntegrationTests
    {
        private GameController game;
        private readonly string[] savedKeys = { "ObstacleCourse.ChallengeUnlocked.v2", "ObstacleCourse.BestTime.v6.0", "ObstacleCourse.BestTime.v6.1" };
        private readonly bool[] existed = new bool[3];
        private readonly float[] saved = new float[3];
        [UnitySetUp] public IEnumerator LoadCourse()
        {
            for (int i=0;i<savedKeys.Length;i++)
            {
                existed[i]=PlayerPrefs.HasKey(savedKeys[i]);
                saved[i]=i==0 ? PlayerPrefs.GetInt(savedKeys[i]) : PlayerPrefs.GetFloat(savedKeys[i]);
                PlayerPrefs.DeleteKey(savedKeys[i]);
            }
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("Percurso",LoadSceneMode.Single);
            yield return null;
            game=Object.FindFirstObjectByType<GameController>();
            Assert.NotNull(game);
        }
        [UnityTearDown] public IEnumerator CleanUp()
        {
            for(int i=0;i<savedKeys.Length;i++)
            {
                if (!existed[i]) PlayerPrefs.DeleteKey(savedKeys[i]);
                else if(i==0) PlayerPrefs.SetInt(savedKeys[i],(int)saved[i]);
                else PlayerPrefs.SetFloat(savedKeys[i],saved[i]);
            }
            PlayerPrefs.Save(); Time.timeScale=1; yield return null;
        }

        [UnityTest] public IEnumerator StartsAtMenuAndRestartRestoresPlayerAndRules()
        {
            Assert.AreEqual(RunState.Menu,game.Session.State);
            foreach(var level in game.levels)
            {
                Assert.Greater(level.threeStarSeconds,0);
                Assert.Greater(level.twoStarSeconds,level.threeStarSeconds);
                Assert.Greater(level.timeLimit,level.twoStarSeconds);
            }
            Assert.IsTrue(game.hud.overlay.activeSelf);
            game.StartRun(); yield return null;
            game.player.GetComponent<Rigidbody2D>().position=Vector2.zero;
            game.TakeDamage(); game.Session.Tick(100); yield return null;
            Assert.AreEqual(RunState.Lost,game.Session.State);
            Assert.IsTrue(game.hud.overlay.activeSelf);
            game.StartRun(); yield return null;
            Assert.AreEqual(RunState.Playing,game.Session.State);
            Assert.Greater(game.Session.Remaining,89);
            Assert.Less(Vector2.Distance(game.player.Position,game.player.spawn),.01f);
            Assert.IsFalse(game.hud.overlay.activeSelf);
        }
        [UnityTest] public IEnumerator ARealWallStopsThePlayer()
        {
            game.StartRun(); game.player.enabled=false;
            var body=game.player.GetComponent<Rigidbody2D>();
            body.position=new Vector2(-8,-1.8f); body.linearVelocity=new Vector2(5,0);
            for(int i=0;i<30;i++) yield return new WaitForFixedUpdate();
            Assert.LessOrEqual(body.position.x,-6.7f,"Player crossed the wall.");
            Assert.Greater(body.position.x,-7.1f,"Player never reached the wall.");
        }
        [UnityTest] public IEnumerator HazardCollisionImmediatelyEndsAttempt()
        {
            game.StartRun(); game.player.enabled=false;
            game.player.GetComponent<Rigidbody2D>().position=game.Level.blocks[0].GetComponent<Rigidbody2D>().position;
            float deadline=Time.realtimeSinceStartup+2;
            while(game.IsPlaying && Time.realtimeSinceStartup<deadline) yield return null;
            Assert.AreEqual(RunState.Lost,game.Session.State);
            Assert.IsTrue(game.hud.overlay.activeSelf);
            game.StartRun(); yield return null;
            Assert.AreEqual(RunState.Playing,game.Session.State);
        }
        [UnityTest] public IEnumerator TutorialLowerPassageWorksOnlyWhenBlockLeavesAnOpening()
        {
            game.StartRun(); game.player.enabled=false;
            var player=game.player.GetComponent<Rigidbody2D>();
            var block=game.Level.blocks[0]; block.enabled=false;
            var obstacle=block.GetComponent<Rigidbody2D>();
            obstacle.position=new Vector2(-9,4.65f);
            player.position=new Vector2(-7,3.95f); player.linearVelocity=Vector2.right*5;
            Physics2D.SyncTransforms();
            float deadline=Time.realtimeSinceStartup+2;
            while(game.IsPlaying && player.position.x<-5.1f && Time.realtimeSinceStartup<deadline) yield return null;
            Assert.IsTrue(game.IsPlaying);
            Assert.Greater(player.position.x,-5.1f,"A passagem inferior deve contornar o topo da parede.");
            game.StartRun();
            obstacle.position=new Vector2(-6,4.65f);
            player.position=new Vector2(-7,3.95f); player.linearVelocity=Vector2.right*5;
            Physics2D.SyncTransforms();
            deadline=Time.realtimeSinceStartup+2;
            while(game.IsPlaying && Time.realtimeSinceStartup<deadline) yield return null;
            Assert.AreEqual(RunState.Lost,game.Session.State,"Sem abertura, a passagem inferior deve causar impacto.");
        }
        [UnityTest] public IEnumerator FinishTriggerWinsAndFreezesTheClock()
        {
            game.StartRun();
            game.player.GetComponent<Rigidbody2D>().position=new Vector2(13,4.6f);
            float until=Time.realtimeSinceStartup+2;
            while(game.IsPlaying && Time.realtimeSinceStartup<until) yield return null;
            Assert.AreEqual(RunState.Won,game.Session.State);
            Assert.IsTrue(game.hud.overlay.activeSelf);
            Assert.Greater(game.Session.Score,0);
            float elapsed=game.Session.Elapsed;
            yield return null;
            Assert.AreEqual(elapsed,game.Session.Elapsed);
        }
        [UnityTest] public IEnumerator PauseStopsPhysicsAndAllowsResume()
        {
            game.StartRun(); yield return null;
            game.TogglePause(); float remaining=game.Session.Remaining;
            yield return new WaitForSecondsRealtime(.1f);
            Assert.AreEqual(remaining,game.Session.Remaining);
            Assert.AreEqual(0,Time.timeScale);
            game.PrimaryAction(); yield return null;
            Assert.AreEqual(RunState.Playing,game.Session.State);
            Assert.AreEqual(1,Time.timeScale);
        }

        [UnityTest] public IEnumerator CompactPauseAndDefeatsShowTheCorrectStateWithoutAwardingStars()
        {
            float fullHeight=game.hud.card.rect.height;
            game.StartRun(); game.TogglePause(); yield return null;
            Assert.Less(game.hud.card.rect.height,fullHeight*.6f);
            Assert.IsFalse(game.hud.description.gameObject.activeSelf);
            Assert.IsFalse(game.hud.help.gameObject.activeSelf);
            Assert.IsFalse(game.hud.stars.gameObject.activeSelf);
            Assert.AreEqual(0,game.EarnedStars);
            game.PrimaryAction(); game.TakeDamage(); yield return null;
            Assert.AreEqual(DefeatCause.Contact,game.Session.Defeat);
            Assert.AreEqual("FUGA\nINTERROMPIDA",game.hud.title.text);
            AssertTitleFits();
            Assert.AreEqual("Você foi pego durante a fuga. Tente novamente.",game.hud.description.text);
            Assert.IsFalse(game.hud.stars.gameObject.activeSelf);
            Assert.AreEqual(0,game.EarnedStars);
            game.StartRun(); game.Session.Tick(game.Session.Duration); yield return null;
            Assert.AreEqual("TEMPO\nESGOTADO",game.hud.title.text);
            AssertTitleFits();
            Assert.AreEqual("Você não alcançou a saída a tempo.",game.hud.description.text);
            Assert.AreEqual(0,game.EarnedStars);
            Assert.AreEqual(0,game.BestStars);
            Assert.IsFalse(game.hud.stars.gameObject.activeSelf);
            Assert.IsFalse(game.hud.subtitle.gameObject.activeSelf);
        }

        [UnityTest] public IEnumerator StarsFollowTheCurrentEscapeAndBestRatingSurvivesRetriesAndReload()
        {
            var times=new[] {24f,30f,50f};
            var ratings=new[] {3,2,1};
            for(int i=0;i<times.Length;i++)
            {
                game.StartRun();
                Assert.AreEqual(0,game.EarnedStars);
                game.Session.Tick(times[i]); game.ReachFinish();
                Assert.AreEqual(ratings[i],game.EarnedStars);
                Assert.AreEqual(ratings[i],game.hud.stars.Value);
                Assert.IsTrue(game.hud.stars.gameObject.activeSelf);
                AssertTitleFits();
                Assert.AreEqual(3,game.BestStars,"A slower replay must not erase the best rating.");
                Assert.AreEqual(24,game.BestTime);
            }
            game.ReturnToMenu(); yield return null;
            Assert.IsFalse(game.hud.stars.gameObject.activeSelf);
            Assert.AreEqual(0,game.EarnedStars);
            yield return SceneManager.LoadSceneAsync("Percurso",LoadSceneMode.Single); yield return null;
            game=Object.FindFirstObjectByType<GameController>();
            Assert.AreEqual(24,game.BestTime);
            Assert.AreEqual(3,game.BestStars);
            Assert.AreEqual(0,game.EarnedStars);
        }

        [UnityTest] public IEnumerator ChallengeUsesItsOwnStarTargetsAndKeepsRecordsSeparate()
        {
            game.StartRun(); game.Session.Tick(20); game.ReachFinish();
            Assert.AreEqual(3,game.EarnedStars);
            game.PrimaryAction();
            Assert.AreEqual(1,game.LevelIndex);
            game.Session.Tick(24.5f); game.ReachFinish();
            Assert.AreEqual(2,game.EarnedStars,"24.5s earns three stars only in the tutorial.");
            Assert.AreEqual(2,game.BestStars);
            Assert.AreEqual("JOGAR NOVAMENTE",game.hud.primaryLabel.text);
            Assert.AreEqual("VOCÊ\nESCAPOU!",game.hud.title.text);
            AssertTitleFits();
            game.ReturnToMenu();
            Assert.AreEqual(3,game.BestStars);
            game.StartChallenge(); yield return null;
            Assert.AreEqual(2,game.BestStars);
            Assert.AreEqual(0,game.EarnedStars);
            Assert.IsFalse(game.hud.stars.gameObject.activeSelf);
            Assert.IsTrue(game.hud.ratingGuide.gameObject.activeSelf);
        }

        private void AssertTitleFits()
        {
            Canvas.ForceUpdateCanvases();
            Assert.LessOrEqual(game.hud.title.preferredHeight,game.hud.title.rectTransform.rect.height,
                "The complete result title must fit without clipping its second line.");
        }
    }
}
