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
        [UnitySetUp] public IEnumerator LoadCourse()
        {
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("Percurso",LoadSceneMode.Single);
            yield return null;
            game=Object.FindFirstObjectByType<GameController>();
            Assert.NotNull(game);
        }
        [UnityTearDown] public IEnumerator CleanUp() { Time.timeScale=1; yield return null; }

        [UnityTest] public IEnumerator StartsAtMenuAndRestartRestoresPlayerAndRules()
        {
            Assert.AreEqual(RunState.Menu,game.Session.State);
            Assert.IsTrue(game.hud.overlay.activeSelf);
            game.StartRun(); yield return null;
            game.player.GetComponent<Rigidbody2D>().position=Vector2.zero;
            game.TakeDamage(); game.Session.Tick(60); yield return null;
            Assert.AreEqual(RunState.Lost,game.Session.State);
            Assert.IsTrue(game.hud.overlay.activeSelf);
            game.StartRun(); yield return null;
            Assert.AreEqual(3,game.Session.Lives);
            Assert.Greater(game.Session.Remaining,59);
            Assert.Less(Vector2.Distance(game.player.Position,game.player.spawn),.01f);
            Assert.IsFalse(game.hud.overlay.activeSelf);
        }
        [UnityTest] public IEnumerator ARealWallStopsThePlayer()
        {
            game.StartRun(); game.player.enabled=false;
            var body=game.player.GetComponent<Rigidbody2D>();
            body.position=new Vector2(-8,0); body.linearVelocity=new Vector2(5,0);
            for(int i=0;i<30;i++) yield return new WaitForFixedUpdate();
            Assert.LessOrEqual(body.position.x,-6.7f,"Player crossed the wall.");
            Assert.Greater(body.position.x,-7.1f,"Player never reached the wall.");
        }
        [UnityTest] public IEnumerator HazardCollisionRemovesOneLifeWithProtection()
        {
            game.StartRun(); game.player.enabled=false;
            game.player.GetComponent<Rigidbody2D>().position=game.hazards[0].GetComponent<Rigidbody2D>().position;
            for(int i=0;i<5;i++) yield return new WaitForFixedUpdate();
            Assert.AreEqual(2,game.Session.Lives);
            game.player.TouchHazard(); Assert.AreEqual(2,game.Session.Lives);
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
    }
}
