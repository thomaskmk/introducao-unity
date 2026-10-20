using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ObstacleCourse.Tests
{
    public sealed class CharacterCollisionTests
    {
        private GameController game;

        [UnitySetUp] public IEnumerator Load()
        {
            Time.timeScale=1;
            yield return SceneManager.LoadSceneAsync("Percurso");
            yield return null;
            game=Object.FindFirstObjectByType<GameController>();
            game.StartRun();
            game.player.enabled=false;
            foreach(var level in game.levels)
            {
                foreach(var enemy in level.enemies) enemy.enabled=false;
                foreach(var collider in level.GetComponentsInChildren<Collider2D>(true))
                    if(collider.isTrigger && collider.GetComponent<ChaserEnemy>()==null) collider.enabled=false;
            }
        }

        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale=1;
            yield return null;
        }

        [UnityTest] public IEnumerator HitboxesStayInsideEveryAnimationFrameOnBothLevels()
        {
            CheckFrames(game.player.GetComponent<PrisonCharacterVisual>());
            foreach(var level in game.levels)
            {
                bool active=level.gameObject.activeSelf;
                level.gameObject.SetActive(true);
                foreach(var enemy in level.enemies) CheckFrames(enemy.GetComponent<PrisonCharacterVisual>());
                level.gameObject.SetActive(active);
            }
            yield return null;
        }

        private static void CheckFrames(PrisonCharacterVisual art)
        {
            var collider=art.GetComponent<Collider2D>();
            Physics2D.SyncTransforms();
            var expected=collider.bounds;
            var original=art.character.sprite;
            foreach(var frame in art.frames)
            {
                art.character.sprite=frame;
                var visible=art.character.bounds;
                var actual=collider.bounds;
                Assert.That(actual.min.x,Is.GreaterThanOrEqualTo(visible.min.x-.001f),frame.name);
                Assert.That(actual.max.x,Is.LessThanOrEqualTo(visible.max.x+.001f),frame.name);
                Assert.That(actual.min.y,Is.GreaterThanOrEqualTo(visible.min.y-.001f),frame.name);
                Assert.That(actual.max.y,Is.LessThanOrEqualTo(visible.max.y+.001f),frame.name);
                Assert.AreEqual(expected,actual,"Walking must not change the collision area.");
            }
            art.character.sprite=original;
        }

        [UnityTest] public IEnumerator VisibleSideGapIsSafeButBodyContactEndsAttempt()
        {
            var enemy=game.Level.enemies[0];
            var guard=enemy.GetComponent<Rigidbody2D>();
            var player=game.player.GetComponent<Rigidbody2D>();
            guard.position=new Vector2(-11,-3);
            // The old .7/.9 squares overlapped here, despite a visible gap.
            player.position=guard.position+Vector2.right*.6f;
            Physics2D.SyncTransforms();
            yield return new WaitForSecondsRealtime(.1f); // Defeat freezes the physics clock.
            Assert.IsTrue(game.IsPlaying,"Invisible lateral space must not kill the player.");
            player.position=guard.position+Vector2.right*.44f;
            Physics2D.SyncTransforms();
            yield return new WaitForSecondsRealtime(.1f); // Defeat freezes the physics clock.
            Assert.AreEqual(RunState.Lost,game.Session.State,"Real contact must still end the attempt.");
        }

        [UnityTest] public IEnumerator VisibleHeadContactWithGateEndsAttempt()
        {
            var block=game.Level.blocks[0];
            block.enabled=false;
            block.GetComponent<Collider2D>().enabled=true;
            block.GetComponent<Rigidbody2D>().position=new Vector2(-12,-3);
            var player=game.player.GetComponent<Rigidbody2D>();
            player.position=new Vector2(-12,-4.15f);
            Physics2D.SyncTransforms();
            yield return new WaitForSecondsRealtime(.1f); // Defeat freezes the physics clock.
            Assert.IsTrue(game.IsPlaying,"A visible gap above the head must remain safe.");
            // The old feet-centered square missed this head-on collision.
            player.position=new Vector2(-12,-4.05f);
            Physics2D.SyncTransforms();
            yield return new WaitForSecondsRealtime(.1f); // Defeat freezes the physics clock.
            Assert.AreEqual(RunState.Lost,game.Session.State);
        }

        [UnityTest] public IEnumerator GuardStopsAtWallsAboveAndBelowUsingItsActualBody()
        {
            foreach(var collider in game.Level.GetComponentsInChildren<Collider2D>()) collider.enabled=false;
            var enemy=game.Level.enemies[0];
            var hitbox=enemy.GetComponent<Collider2D>(); hitbox.enabled=true;
            enemy.territory=new Rect(-14,-5,28,10); enemy.detectionRadius=.1f; enemy.patrolSpeed=5;
            var wallObject=new GameObject("Test wall"); wallObject.layer=8;
            wallObject.transform.position=new Vector2(-11,-2);
            var wall=wallObject.AddComponent<BoxCollider2D>(); wall.size=new Vector2(4,.2f);
            game.player.GetComponent<Rigidbody2D>().position=new Vector2(12,-4);
            foreach(bool fromBelow in new[] {true,false})
            {
                enemy.pointA=new Vector2(-11,fromBelow ? -4 : 0);
                enemy.pointB=new Vector2(-11,fromBelow ? 0 : -4);
                enemy.ResetEnemy(); enemy.enabled=true; Physics2D.SyncTransforms();
                for(int i=0;i<30;i++) yield return new WaitForFixedUpdate();
                var separation=hitbox.Distance(wall);
                Assert.IsFalse(separation.isOverlapped,"The guard's body penetrated the wall.");
                Assert.That(separation.distance,Is.InRange(0f,.04f),"The guard stopped away from its visible body.");
                Assert.IsTrue(fromBelow ? hitbox.bounds.max.y<wall.bounds.min.y : hitbox.bounds.min.y>wall.bounds.max.y);
                enemy.enabled=false;
            }
            Object.Destroy(wallObject);
        }
    }
}
