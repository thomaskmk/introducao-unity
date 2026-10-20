using UnityEngine;

namespace ObstacleCourse
{
    public sealed class CourseLevel : MonoBehaviour
    {
        public string title;
        public bool tutorial;
        public int layoutVersion = 2;
        [Min(1)] public float timeLimit = 90;
        [Min(1)] public float threeStarSeconds = 25;
        [Min(1)] public float twoStarSeconds = 40;
        public Vector2 spawn = new Vector2(-13, -4.5f);
        public PatrolHazard[] blocks;
        public RotatingHazard[] rotors = System.Array.Empty<RotatingHazard>();
        public ChaserEnemy[] enemies;
        public CycleTrap[] traps;
        public SlowZone[] slowZones;

        public int StarsForTime(float elapsed) => StarRating.ForTime(elapsed, threeStarSeconds, twoStarSeconds);

        public void ResetLevel()
        {
            foreach (var block in blocks) block.ResetHazard();
            foreach (var rotor in rotors) rotor.ResetHazard();
            foreach (var enemy in enemies) enemy.ResetEnemy();
            foreach (var trap in traps) trap.Refresh();
        }
    }
}
