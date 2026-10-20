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
        private readonly string[] keys = { "ObstacleCourse.ChallengeUnlocked.v2", "ObstacleCourse.BestTime.v6.0", "ObstacleCourse.BestTime.v6.1" };
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
        [UnityTest] public IEnumerator CharactersKeepTheirArtAcrossChasePauseRestartAndLevelChange()
        {
            var prisoner=game.player.GetComponent<PrisonCharacterVisual>();
            var guard=game.Level.enemies[0].GetComponent<PrisonCharacterVisual>();
            Assert.NotNull(prisoner); Assert.NotNull(guard);
            Place(guard.guard.pointA+Vector2.down*2);
            yield return new WaitForSeconds(.25f);
            Assert.IsTrue(guard.guard.IsChasing);
            Assert.AreEqual("!",guard.guard.label.text);
            Assert.AreNotEqual(guard.frames[0],guard.character.sprite,"O guarda precisa caminhar durante a perseguição.");
            Assert.IsFalse(guard.prototype.enabled);
            game.TogglePause();
            var frame=guard.character.sprite;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.AreEqual(frame,guard.character.sprite,"A animação deve pausar junto com o jogo.");
            game.StartRun(); yield return null; yield return null;
            Assert.AreEqual(guard.frames[0],guard.character.sprite);
            Assert.AreEqual("?",guard.guard.label.text);
            game.ReachFinish(); game.PrimaryAction(); yield return null; yield return null;
            Assert.AreEqual(1,game.LevelIndex);
            Assert.IsTrue(prisoner.character.enabled);
            Assert.IsFalse(prisoner.prototype.enabled,"O desafio não deve voltar ao quadrado provisório.");
            foreach(var enemy in game.Level.enemies)
            {
                var visual=enemy.GetComponent<PrisonCharacterVisual>();
                Assert.NotNull(visual);
                Assert.IsTrue(visual.character.enabled);
                Assert.IsFalse(visual.prototype.enabled);
                Assert.AreEqual(16,visual.frames.Length);
                foreach(var sprite in visual.frames) Assert.NotNull(sprite);
            }
        }
        [UnityTest] public IEnumerator TutorialTipsFollowProximityAndHideOutsideTheTutorial()
        {
            // Isolate presentation from collisions while visiting every teaching area.
            foreach(var collider in game.Level.GetComponentsInChildren<Collider2D>())
                if(collider.isTrigger) collider.enabled=false;
            game.player.enabled=false;
            var guide=game.hud.guide;
            var positions=new[] { new Vector2(-10,-3),new Vector2(-9,3),new Vector2(-1,4),
                new Vector2(-1,1.3f),new Vector2(-1,-1),new Vector2(10,-1) };
            for(int i=0;i<positions.Length;i++)
            {
                Place(positions[i]); yield return null; yield return null;
                Assert.AreEqual(i,guide.CurrentTip);
                Assert.IsTrue(guide.card.gameObject.activeSelf);
                Assert.IsTrue(guide.pointer.gameObject.activeSelf,"A dica precisa apontar para o perigo.");
                var target=Camera.main.WorldToScreenPoint(guide.tips[i].target.position);
                var marker=RectTransformUtility.WorldToScreenPoint(null,guide.marker.position);
                Assert.Less(Vector2.Distance(target,marker),2,"A indicação deve acompanhar o obstáculo.");
                Assert.IsFalse(RectTransformUtility.RectangleContainsScreenPoint(guide.card,
                    Camera.main.WorldToScreenPoint(game.player.Position)),"A dica não deve cobrir o personagem.");
            }
            Place(new Vector2(-1,-4)); yield return null; yield return null;
            Assert.AreEqual(-1,guide.CurrentTip,"A segunda parede móvel não deve exibir dica.");
            Assert.IsFalse(guide.card.gameObject.activeSelf);
            Place(new Vector2(-1,2.1f)); yield return null; yield return null;
            Assert.AreEqual(3,guide.CurrentTip,"O aviso de choque deve aparecer antes de alcançar a grade.");
            Assert.IsFalse(RectTransformUtility.RectangleContainsScreenPoint(guide.card,
                Camera.main.WorldToScreenPoint(game.player.Position)));
            game.TogglePause(); yield return null;
            Assert.IsFalse(guide.card.gameObject.activeSelf);
            game.StartRun(); yield return null;
            Assert.AreEqual(0,guide.CurrentTip,"Reiniciar deve trazer a dica inicial.");
            Place(new Vector2(-10,1)); yield return null;
            Assert.AreEqual(-1,guide.CurrentTip,"Fora das áreas de aproximação, não deve haver caixa.");
            Assert.IsFalse(guide.pointer.gameObject.activeSelf);
            game.ReachFinish(); game.PrimaryAction(); yield return null;
            Assert.IsFalse(guide.card.gameObject.activeSelf,"O desafio não tem dicas.");
            game.ReturnToMenu(); yield return null;
            Assert.IsFalse(guide.card.gameObject.activeSelf);
        }
        [UnityTest] public IEnumerator SecurityFloorShowsDistinctSignalsAndFreezesOnPause()
        {
            var trap=game.Level.traps[0];
            var art=trap.GetComponent<PrisonTrapVisual>();
            Assert.NotNull(art);
            yield return null; yield return null;
            Assert.AreEqual(TrapPhase.Safe,trap.Phase);
            Assert.Greater(art.surface.color.b,art.surface.color.r);
            Assert.IsTrue(art.safeMarks.activeSelf);
            Assert.IsFalse(art.warningMarks.activeSelf);
            Assert.IsFalse(art.electricity.activeSelf);
            StringAssert.Contains("SEM CORRENTE",trap.label.text);
            game.Session.Tick(trap.safeSeconds-game.Session.Elapsed);
            yield return null; yield return null;
            Assert.AreEqual(TrapPhase.Warning,trap.Phase);
            Assert.Greater(art.surface.color.r,.95f);
            Assert.Greater(art.surface.color.g,.7f);
            Assert.Less(art.surface.color.b,.3f);
            Assert.IsFalse(art.safeMarks.activeSelf);
            Assert.IsTrue(art.warningMarks.activeSelf);
            Assert.IsFalse(art.electricity.activeSelf);
            StringAssert.Contains("LIGANDO",trap.label.text);
            game.Session.Tick(trap.safeSeconds+trap.warningSeconds-game.Session.Elapsed);
            yield return null; yield return null;
            Assert.AreEqual(TrapPhase.Active,trap.Phase);
            Assert.Greater(art.surface.color.r,.95f);
            Assert.Less(art.surface.color.g,.3f);
            Assert.IsFalse(art.safeMarks.activeSelf);
            Assert.IsFalse(art.warningMarks.activeSelf);
            Assert.IsTrue(art.electricity.activeSelf);
            StringAssert.Contains("ELETRIFICADO",trap.label.text);
            game.TogglePause();
            yield return null; yield return null; // Let LateUpdate display the final paused clock.
            Color paused=art.surface.color;
            Color pausedArc=art.arcs[0].color;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.AreEqual(paused,art.surface.color);
            Assert.AreEqual(pausedArc,art.arcs[0].color);
            game.StartRun(); yield return null; yield return null;
            Assert.AreEqual(TrapPhase.Safe,trap.Phase);
            Assert.Greater(art.surface.color.b,art.surface.color.r);
            Assert.IsTrue(art.safeMarks.activeSelf);
            Assert.IsFalse(art.electricity.activeSelf);
            Assert.IsFalse(art.prototype.enabled);
        }
        [UnityTest] public IEnumerator TrapWarnsBeforeDamageAndHurtsPlayerAlreadyInside()
        {
            var trap=game.Level.traps[0];
            // Test the floor cycle independently of the pendulum above it.
            foreach(var collider in game.Level.GetComponentsInChildren<Collider2D>())
                if(collider.isTrigger && collider.gameObject!=trap.gameObject) collider.enabled=false;
            Assert.AreEqual(TrapPhase.Safe,trap.PhaseAt(0));
            Assert.AreEqual(TrapPhase.Warning,trap.PhaseAt(trap.safeSeconds));
            Assert.AreEqual(TrapPhase.Active,trap.PhaseAt(trap.safeSeconds+trap.warningSeconds));
            Assert.AreEqual(TrapPhase.Safe,trap.PhaseAt(trap.safeSeconds+trap.warningSeconds+trap.activeSeconds));
            Place(trap.transform.position); yield return new WaitForSecondsRealtime(.06f);
            Assert.AreEqual(RunState.Playing,game.Session.State);
            game.Session.Tick(trap.safeSeconds); yield return new WaitForSecondsRealtime(.06f);
            Assert.AreEqual(RunState.Playing,game.Session.State);
            game.Session.Tick(trap.warningSeconds); yield return new WaitForSecondsRealtime(.06f);
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
        [UnityTest] public IEnumerator ElectricFloorHitsSleepingPlayerWhenCurrentSwitchesOn()
        {
            var trap=game.Level.traps[0];
            foreach(var collider in game.Level.GetComponentsInChildren<Collider2D>())
                if(collider.isTrigger && collider.gameObject!=trap.gameObject) collider.enabled=false;
            game.player.enabled=false;
            Place(trap.transform.position);
            var body=game.player.GetComponent<Rigidbody2D>();
            body.linearVelocity=Vector2.zero;
            yield return new WaitForSecondsRealtime(.1f);
            body.Sleep();
            Assert.IsTrue(body.IsSleeping());
            Assert.IsTrue(game.IsPlaying);
            game.Session.Tick(trap.safeSeconds+trap.warningSeconds+.01f-game.Session.Elapsed);
            yield return new WaitForSecondsRealtime(.1f);
            Assert.AreEqual(RunState.Lost,game.Session.State,"Standing still must not bypass the active floor.");
            Assert.AreEqual(DefeatCause.Contact,game.Session.Defeat);
            Assert.AreEqual(0,game.EarnedStars);
        }
        [UnityTest] public IEnumerator EnemyDetectsPlayerAndStopsPursuingOutsideTerritory()
        {
            var enemy=game.Level.enemies[0];
            Place(enemy.pointA+new Vector2(0,-2));
            Vector2 before=enemy.GetComponent<Rigidbody2D>().position;
            for(int i=0;i<10;i++) yield return new WaitForFixedUpdate();
            Assert.IsTrue(enemy.IsChasing);
            Assert.Less(enemy.GetComponent<Rigidbody2D>().position.y,before.y);
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
            enemy.pointA=new Vector2(-7.2f,-1.8f); enemy.pointB=new Vector2(-4,-1.8f);
            enemy.patrolSpeed=5; enemy.ResetEnemy(); Place(new Vector2(-4,-1.8f));
            Physics2D.SyncTransforms();
            Assert.IsTrue(Physics2D.Linecast(enemy.pointA,enemy.pointB,enemy.walls), "Expected a wall between enemy and player.");
            for(int i=0;i<40;i++)
            {
                Assert.IsTrue(game.IsPlaying, "Enemy crossed the wall and hit the player.");
                yield return new WaitForFixedUpdate();
                Assert.LessOrEqual(enemy.GetComponent<Collider2D>().bounds.max.x,-6.5f,"Enemy crossed wall during patrol.");
            }
            Assert.IsFalse(enemy.IsChasing);
            Assert.That(enemy.GetComponent<Collider2D>().bounds.max.x,Is.InRange(-6.54f,-6.5f));
            Assert.Greater(enemy.GetComponent<Rigidbody2D>().position.x,-7.2f);
        }
        [UnityTest] public IEnumerator ProgressionAndRetryKeepCorrectLevelAndRestartAttempt()
        {
            Assert.IsFalse(game.ChallengeUnlocked);
            game.ReachFinish(); game.PrimaryAction(); yield return null;
            Assert.AreEqual(1,game.LevelIndex); Assert.IsTrue(game.ChallengeUnlocked);
            Assert.AreEqual(RunState.Playing,game.Session.State); Assert.AreEqual(38,game.Session.Duration);
            Assert.IsFalse(game.levels[0].gameObject.activeSelf); Assert.IsTrue(game.levels[1].gameObject.activeSelf);
            game.TakeDamage(); Assert.AreEqual(RunState.Lost,game.Session.State);
            game.PrimaryAction(); yield return null;
            Assert.AreEqual(1,game.LevelIndex); Assert.AreEqual(RunState.Playing,game.Session.State);
            game.ReachFinish(); Assert.IsFalse(game.HasNextLevel);
            Assert.AreEqual("VOCÊ\nESCAPOU!",game.hud.title.text);
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
        [UnityTest] public IEnumerator RotatingBarHitsStationaryPlayerWithItsMovingCollider()
        {
            game.ReachFinish(); game.PrimaryAction(); yield return null;
            var rotor=game.Level.rotors[0];
            Place((Vector2)rotor.transform.position+Vector2.up*1.5f);
            float deadline=Time.realtimeSinceStartup+3;
            while(game.IsPlaying && Time.realtimeSinceStartup<deadline) yield return null;
            Assert.AreEqual(RunState.Lost,game.Session.State,"A barra deve atingir o jogador durante o giro.");
        }
        [UnityTest] public IEnumerator RotorsPauseResetAndPendulumReverses()
        {
            game.ReachFinish(); game.PrimaryAction(); yield return null;
            var rotor=game.Level.rotors[0]; var pendulum=game.Level.rotors[1];
            var rb=rotor.GetComponent<Rigidbody2D>(); var pb=pendulum.GetComponent<Rigidbody2D>();
            float initial=rb.rotation;
            yield return new WaitForSeconds(.15f);
            Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(initial,rb.rotation)),5);
            game.TogglePause();
            float stopped=rb.rotation; float stoppedPendulum=pb.rotation;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.AreEqual(stopped,rb.rotation); Assert.AreEqual(stoppedPendulum,pb.rotation);
            game.TogglePause();
            // Posicione o relógio em lados opostos da reversão e observe a física real.
            game.Session.Tick(pendulum.period/4-game.Session.Elapsed);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            float peak=pb.rotation;
            game.Session.Tick(pendulum.period/4);
            yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
            Assert.Greater(peak-pb.rotation,30);
            Assert.That(pb.rotation,Is.InRange(pendulum.initialAngle-pendulum.amplitude-1,pendulum.initialAngle+pendulum.amplitude+1));
            game.StartRun();
            Assert.Less(Mathf.Abs(Mathf.DeltaAngle(rotor.initialAngle,rb.rotation)),.01f);
            Assert.Less(Mathf.Abs(Mathf.DeltaAngle(pendulum.initialAngle,pb.rotation)),.01f);
        }
        [UnityTest] public IEnumerator TutorialGrillesBlockTheFourLateralBypasses()
        {
            // Isolate solid collisions: the bars must stop bypasses even with hazards disabled.
            foreach(var collider in game.Level.GetComponentsInChildren<Collider2D>())
                if(collider.isTrigger) collider.enabled=false;
            foreach(var enemy in game.Level.enemies) enemy.enabled=false;
            game.player.enabled=false;
            var body=game.player.GetComponent<Rigidbody2D>();
            foreach(var crossing in new[] {
                new Vector2(-14,-.5f),new Vector2(-8,-.5f),
                new Vector2(-4.5f,2.8f),new Vector2(1.5f,2.8f) })
            {
                game.StartRun(); Place(crossing+Vector2.down*1.5f);
                for(int i=0;i<30;i++)
                {
                    body.linearVelocity=Vector2.up*5;
                    yield return new WaitForFixedUpdate();
                }
                Assert.That(game.player.GetComponent<Collider2D>().bounds.max.y,
                    Is.InRange(crossing.y-.2f,crossing.y-.16f),
                    "A grade deve bloquear o desvio lateral em "+crossing);
                body.linearVelocity=Vector2.zero;
            }
        }
        [UnityTest] public IEnumerator TutorialRotorsHaveSafeAndUnsafeCrossingWindows()
        {
            foreach(var collider in game.Level.GetComponentsInChildren<Collider2D>())
                if(collider.isTrigger) collider.enabled=false;
            foreach(var enemy in game.Level.enemies) enemy.enabled=false;
            game.player.enabled=false;
            var body=game.player.GetComponent<Rigidbody2D>();
            foreach(var rotor in game.Level.rotors)
            {
                var hazard=rotor.GetComponentInChildren<BoxCollider2D>();
                hazard.enabled=true;
                Vector2 center=rotor.transform.position;
                Vector2 direction=rotor.oscillates ? Vector2.down : Vector2.up;
                float cycle=rotor.oscillates ? rotor.period : 360/rotor.degreesPerSecond;
                int safe=0,unsafeAttempts=0;
                for(int phase=0;phase<12;phase++)
                {
                    game.StartRun(); game.Session.Tick(cycle*phase/12);
                    rotor.GetComponent<Rigidbody2D>().rotation=rotor.AngleAt(game.Session.Elapsed);
                    // Follow the arm's movement on the right; the pendulum reverses on either side.
                    Place(center+Vector2.right*(rotor.oscillates ? -1 : 1.05f)-direction*2.15f);
                    float deadline=Time.realtimeSinceStartup+2;
                    while(game.IsPlaying && Vector2.Dot(body.position-center,direction)<2.15f &&
                        Time.realtimeSinceStartup<deadline)
                    {
                        body.linearVelocity=direction*game.player.speed;
                        yield return null;
                    }
                    body.linearVelocity=Vector2.zero;
                    if(game.Session.State==RunState.Lost) unsafeAttempts++;
                    else
                    {
                        Assert.GreaterOrEqual(Vector2.Dot(body.position-center,direction),2.15f,
                            "A travessia ficou presa na grade de "+rotor.name);
                        safe++;
                    }
                }
                hazard.enabled=false;
                Assert.Greater(safe,0,rotor.name+" precisa permitir uma travessia no momento certo.");
                Assert.Greater(unsafeAttempts,0,rotor.name+" precisa oferecer risco na mesma passagem.");
            }
        }
        [UnityTest] public IEnumerator TutorialGuardBlocksEveryLineThroughTheNarrowExit()
        {
            var enemy=game.Level.enemies[0];
            enemy.enabled=false;
            game.player.enabled=false;
            var body=game.player.GetComponent<Rigidbody2D>();
            // Mesmo com o guarda parado, não há espaço para passar ao lado dele.
            // Include both wall edges, not only the middle of the gap.
            foreach(float x in new[] {10.44f,10.6f,10.8f,11f,11.16f})
            {
                game.StartRun(); Place(new Vector2(x,1.2f)); body.linearVelocity=Vector2.up*5;
                float deadline=Time.realtimeSinceStartup+2;
                while(game.IsPlaying && Time.realtimeSinceStartup<deadline) yield return null;
                Assert.AreEqual(RunState.Lost,game.Session.State);
                Assert.Less(body.position.y,2.4f);
            }
        }
        [UnityTest] public IEnumerator TutorialCanBeFinishedByLuringGuardAndCirclingAround()
        {
            game.player.enabled=false;
            Place(new Vector2(10.8f,-.9f));
            yield return new WaitForSeconds(.35f);
            Assert.IsTrue(game.Level.enemies[0].IsChasing);
            foreach(var point in new[] {
                new Vector2(8.5f,-.9f),new Vector2(8.5f,-2.3f),new Vector2(13,-2.3f),
                new Vector2(13,1.3f),new Vector2(10.8f,1.3f),new Vector2(10.8f,3.6f),new Vector2(13,4.6f) })
                yield return WalkTo(point);
            Assert.AreEqual(RunState.Won,game.Session.State);
        }
        private IEnumerator WalkTo(Vector2 target)
        {
            var body=game.player.GetComponent<Rigidbody2D>();
            float deadline=Time.realtimeSinceStartup+4;
            while(game.IsPlaying && Vector2.Distance(body.position,target)>.12f && Time.realtimeSinceStartup<deadline)
            {
                body.linearVelocity=(target-body.position).normalized*game.player.speed*game.player.SpeedMultiplier;
                yield return null;
            }
            body.linearVelocity=Vector2.zero;
            Assert.AreNotEqual(RunState.Lost,game.Session.State,"Impacto no trajeto até "+target);
            if(game.IsPlaying) Assert.Less(Vector2.Distance(body.position,target),.2f,"Caminho bloqueado até "+target);
        }
        [UnityTest] public IEnumerator ChallengeEnemiesDetectPlayerAlongCorridorEdges()
        {
            game.ReachFinish(); game.PrimaryAction(); yield return null;
            var edges=new[] { new Vector2(-3.7f,-2.4f),new Vector2(8.3f,1.2f),new Vector2(9.7f,1.8f) };
            for(int i=0;i<edges.Length;i++)
            {
                game.StartRun(); Place(edges[i]);
                yield return new WaitForFixedUpdate(); yield return new WaitForFixedUpdate();
                Assert.IsTrue(game.Level.enemies[i].IsChasing,"Inimigo ignora jogador junto à parede do setor "+i);
            }
        }
        [UnityTest] public IEnumerator ChallengeUsesThreeEnemiesAndConnectedMandatorySectors()
        {
            game.ReachFinish(); game.PrimaryAction(); yield return null;
            Assert.AreEqual(3,game.Level.enemies.Length);
            // Verifica a conectividade sem depender da fase dos perigos móveis.
            foreach(var hazard in game.Level.GetComponentsInChildren<Collider2D>())
                if(hazard.isTrigger && !hazard.TryGetComponent<FinishZone>(out _)) hazard.enabled=false;
            foreach(var enemy in game.Level.enemies) enemy.enabled=false;
            game.player.enabled=false;
            int walls=1<<8;
            foreach(float x in new[] {-9f,-3f,3f,9f})
                Assert.IsTrue(Physics2D.Linecast(new Vector2(x-1,0),new Vector2(x+1,0),walls),"Atalho indesejado no meio da divisória.");
            foreach(var point in new[] {
                new Vector2(-13,5.45f),new Vector2(-6,5.45f),new Vector2(-6,-4.8f),
                new Vector2(0,-4.8f),new Vector2(0,4.8f),new Vector2(6,4.8f),
                new Vector2(6,-4.8f),new Vector2(12,-4.8f),new Vector2(12,4.6f) })
                yield return WalkTo(point);
            Assert.AreEqual(RunState.Won,game.Session.State);
        }
    }
}
