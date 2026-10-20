using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ObstacleCourse.Tests
{
    public sealed class NewMechanicsTests
    {
        private GameController game;
        private readonly string[] keys = { "ObstacleCourse.ChallengeUnlocked.v2", "ObstacleCourse.BestTime.v2.0", "ObstacleCourse.BestTime.v2.1" };
        private readonly bool[] existed = new bool[3];
        private readonly float[] values = new float[3];
        [UnitySetUp] public IEnumerator Load()
        {
            for(int i=0;i<keys.Length;i++)
            {
                existed[i]=PlayerPrefs.HasKey(keys[i]);
                values[i]=i==0?PlayerPrefs.GetInt(keys[i]):PlayerPrefs.GetFloat(keys[i]);
                PlayerPrefs.DeleteKey(keys[i]);
            }
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("Percurso"); yield return null;
            game=Object.FindFirstObjectByType<GameController>(); game.StartRun();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            for(int i=0;i<keys.Length;i++)
            {
                if(!existed[i]) PlayerPrefs.DeleteKey(keys[i]);
                else if(i==0) PlayerPrefs.SetInt(keys[i],(int)values[i]);
                else PlayerPrefs.SetFloat(keys[i],values[i]);
            }
            PlayerPrefs.Save(); Time.timeScale=1; yield return null;
        }
        private void Place(Vector2 position)
        {
            game.player.GetComponent<Rigidbody2D>().position=position;
            Physics2D.SyncTransforms();
        }
        [UnityTest] public IEnumerator PickupIsOptionalSingleUseAndReturnsOnRetry()
        {
            var pickup=game.Level.pickups[0];
            Place(pickup.transform.position);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.AreEqual(2,game.Session.Lives); Assert.IsTrue(pickup.Collected);
            game.player.TouchHazard(); Assert.AreEqual(1,game.Session.Lives);
            yield return new WaitForFixedUpdate(); Assert.AreEqual(1,game.Session.Lives);
            game.StartRun(); yield return null;
            Assert.IsFalse(pickup.Collected); Assert.IsTrue(pickup.GetComponent<Collider2D>().enabled);
            Assert.AreEqual(1,game.Session.Lives);
            game.ReachFinish(); Assert.AreEqual(RunState.Won,game.Session.State);
        }
        [UnityTest] public IEnumerator DamageProtectionAllowsMovementThenExpires()
        {
            game.Session.CollectLife();
            game.player.TouchHazard(); game.player.TouchHazard();
            Assert.AreEqual(1,game.Session.Lives); Assert.IsTrue(game.player.IsProtected);
            Assert.AreEqual(RunState.Playing,game.Session.State);
            yield return new WaitForSeconds(game.player.protectionSeconds+.05f);
            Assert.IsFalse(game.player.IsProtected);
            game.player.TouchHazard(); Assert.AreEqual(RunState.Lost,game.Session.State);
        }
        [UnityTest] public IEnumerator TrapWarnsBeforeDamageAndHurtsPlayerAlreadyInside()
        {
            var trap=game.Level.traps[0];
            Assert.AreEqual(TrapPhase.Safe,trap.PhaseAt(0));
            Assert.AreEqual(TrapPhase.Warning,trap.PhaseAt(trap.safeSeconds));
            Assert.AreEqual(TrapPhase.Active,trap.PhaseAt(trap.safeSeconds+trap.warningSeconds));
            Assert.AreEqual(TrapPhase.Safe,trap.PhaseAt(trap.safeSeconds+trap.warningSeconds+trap.activeSeconds));
            Place(trap.transform.position); yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.AreEqual(1,game.Session.Lives);
            game.Session.Tick(trap.safeSeconds); yield return new WaitForFixedUpdate();
            Assert.AreEqual(RunState.Playing,game.Session.State);
            game.Session.Tick(trap.warningSeconds); yield return new WaitForFixedUpdate(); yield return null;
            Assert.AreEqual(RunState.Lost,game.Session.State);
        }
        [UnityTest] public IEnumerator SlowFloorChangesSpeedAndClearsOnExitAndRestart()
        {
            var zone=game.Level.slowZones[0];
            Place(zone.transform.position); yield return null;
            Assert.AreEqual(zone.speedMultiplier,game.player.SpeedMultiplier);
            Place(game.player.spawn); yield return null;
            Assert.AreEqual(1,game.player.SpeedMultiplier);
            Place(zone.transform.position); game.StartRun(); yield return null;
            Assert.AreEqual(1,game.player.SpeedMultiplier);
        }
        [UnityTest] public IEnumerator EnemyDetectsPlayerAndStopsPursuingOutsideTerritory()
        {
            var enemy=game.Level.enemies[0];
            Place(enemy.pointA+new Vector2(-2,0));
            Vector2 before=enemy.GetComponent<Rigidbody2D>().position;
            for(int i=0;i<10;i++) yield return new WaitForFixedUpdate();
            Assert.IsTrue(enemy.IsChasing);
            Assert.Less(enemy.GetComponent<Rigidbody2D>().position.x,before.x);
            Place(game.player.spawn);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.IsFalse(enemy.IsChasing);
            Assert.IsTrue(enemy.territory.Contains(enemy.GetComponent<Rigidbody2D>().position));
            game.StartRun(); yield return null;
            Assert.Less(Vector2.Distance(enemy.GetComponent<Rigidbody2D>().position,enemy.pointA),.1f);
        }
        [UnityTest] public IEnumerator EnemyCannotSeeOrPatrolThroughWall()
        {
            var enemy=game.Level.enemies[0];
            enemy.territory=new Rect(-14,-5,28,10);
            enemy.pointA=new Vector2(-7.2f,0); enemy.pointB=new Vector2(-4,0);
            enemy.patrolSpeed=5; enemy.ResetEnemy(); Place(new Vector2(-4,0));
            Physics2D.SyncTransforms();
            Assert.IsTrue(Physics2D.Linecast(new Vector2(-7.2f,0),new Vector2(-4,0),enemy.walls), "Expected a wall between enemy and player.");
            for(int i=0;i<40;i++)
            {
                Assert.IsTrue(game.IsPlaying, "Enemy crossed the wall and hit the player.");
                yield return new WaitForFixedUpdate();
                Assert.LessOrEqual(enemy.GetComponent<Rigidbody2D>().position.x,-6.84f,"Enemy crossed wall during patrol.");
            }
            Assert.IsFalse(enemy.IsChasing);
            Assert.LessOrEqual(enemy.GetComponent<Rigidbody2D>().position.x,-6.84f);
            Assert.Greater(enemy.GetComponent<Rigidbody2D>().position.x,-7.2f);
        }
        [UnityTest] public IEnumerator ProgressionAndRetryKeepCorrectLevelAndResetLife()
        {
            Assert.IsFalse(game.ChallengeUnlocked);
            game.Session.CollectLife(); game.ReachFinish(); game.PrimaryAction(); yield return null;
            Assert.AreEqual(1,game.LevelIndex); Assert.IsTrue(game.ChallengeUnlocked);
            Assert.AreEqual(1,game.Session.Lives); Assert.AreEqual(32,game.Session.Duration);
            Assert.IsFalse(game.levels[0].gameObject.activeSelf); Assert.IsTrue(game.levels[1].gameObject.activeSelf);
            game.TakeDamage(); Assert.AreEqual(RunState.Lost,game.Session.State);
            game.PrimaryAction(); yield return null;
            Assert.AreEqual(1,game.LevelIndex); Assert.AreEqual(1,game.Session.Lives);
            Assert.IsFalse(game.Level.pickups[0].Collected);
            game.ReachFinish(); Assert.IsFalse(game.HasNextLevel);
            Assert.AreEqual("DESAFIO\nSUPERADO!",game.hud.title.text);
            game.ReturnToMenu(); Assert.IsTrue(game.hud.challenge.gameObject.activeSelf);
            game.StartChallenge(); Assert.AreEqual(1,game.LevelIndex);
        }
        [UnityTest] public IEnumerator PauseFreezesEnemyTrapAndClock()
        {
            yield return new WaitForFixedUpdate();
            game.TogglePause();
            var enemy=game.Level.enemies[0]; var trap=game.Level.traps[0];
            Vector2 position=enemy.GetComponent<Rigidbody2D>().position;
            float remaining=game.Session.Remaining; TrapPhase phase=trap.Phase;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.AreEqual(position,enemy.GetComponent<Rigidbody2D>().position);
            Assert.AreEqual(phase,trap.Phase); Assert.AreEqual(remaining,game.Session.Remaining);
        }
    }
}
