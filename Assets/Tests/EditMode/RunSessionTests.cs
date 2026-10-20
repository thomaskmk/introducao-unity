using NUnit.Framework;

namespace ObstacleCourse.Tests
{
    public sealed class RunSessionTests
    {
        [Test] public void MenuDoesNotConsumeTimeOrLives()
        {
            var run = new RunSession(60, 3);
            run.Tick(20); Assert.IsFalse(run.TakeDamage()); run.Win();
            Assert.AreEqual(RunState.Menu, run.State);
            Assert.AreEqual(60, run.Remaining); Assert.AreEqual(3, run.Lives);
        }
        [Test] public void TimeoutEndsTheRunAndCannotBecomeVictory()
        {
            var run = new RunSession(60, 3); run.Begin(); run.Tick(61); run.Win();
            Assert.AreEqual(RunState.Lost, run.State); Assert.AreEqual(0, run.Remaining);
            Assert.AreEqual(60, run.Elapsed); Assert.AreEqual(0, run.Score);
        }
        [Test] public void ThreeHitsEndTheRunAndExtraHitsDoNothing()
        {
            var run = new RunSession(60, 3); run.Begin();
            run.TakeDamage(); run.TakeDamage(); Assert.AreEqual(RunState.Playing, run.State);
            run.TakeDamage(); Assert.IsFalse(run.TakeDamage());
            Assert.AreEqual(0, run.Lives); Assert.AreEqual(RunState.Lost, run.State);
        }
        [Test] public void PauseFreezesTimeAndRejectsDamageAndVictory()
        {
            var run = new RunSession(60, 3); run.Begin(); run.Tick(10); run.Pause();
            run.Tick(30); Assert.IsFalse(run.TakeDamage()); run.Win();
            Assert.AreEqual(RunState.Paused, run.State); Assert.AreEqual(50, run.Remaining);
            run.Resume(); run.Tick(5); Assert.AreEqual(45, run.Remaining);
        }
        [Test] public void VictoryFreezesFinalTimeAndAwardsScore()
        {
            var run = new RunSession(60, 3); run.Begin(); run.Tick(20); run.TakeDamage(); run.Win();
            run.Tick(40); Assert.IsFalse(run.TakeDamage());
            Assert.AreEqual(RunState.Won, run.State); Assert.AreEqual(20, run.Elapsed);
            Assert.AreEqual(1600, run.Score);
        }
        [Test] public void RestartResetsEveryRunValue()
        {
            var run = new RunSession(60, 3); run.Begin(); run.TakeDamage(); run.Tick(60); run.Begin();
            Assert.AreEqual(RunState.Playing, run.State); Assert.AreEqual(3, run.Lives);
            Assert.AreEqual(60, run.Remaining); Assert.AreEqual("", run.EndReason);
        }
        [Test] public void ReturningToMenuStopsTheClock()
        {
            var run = new RunSession(60, 3); run.Begin(); run.Tick(5); run.ReturnToMenu(); run.Tick(10);
            Assert.AreEqual(RunState.Menu, run.State); Assert.AreEqual(55, run.Remaining);
        }
    }
}
