using UnityEngine;

namespace ObstacleCourse
{
    public sealed class CourseLevel : MonoBehaviour
    {
        public string title;
        public bool tutorial;
        [Min(1)] public float timeLimit = 90;
        public Vector2 spawn = new Vector2(-13, -4.5f);
        public PatrolHazard[] blocks;
        public ChaserEnemy[] enemies;
        public CycleTrap[] traps;
        public SlowZone[] slowZones;
        public ExtraLife[] pickups;

        public void ResetLevel()
        {
            foreach (var block in blocks) block.ResetHazard();
            foreach (var enemy in enemies) enemy.ResetEnemy();
            foreach (var trap in traps) trap.Refresh();
            foreach (var pickup in pickups) pickup.ResetPickup();
        }

        public string Hint(Vector2 position)
        {
            if (!tutorial)
                return "O atalho central poupa tempo. A vida extra permite absorver um impacto.";
            if (position.x < -6.5f)
                return position.y < 1.5f
                    ? "WASD / setas: mova-se. A cruz rosa dá +1 vida. A coleta é opcional."
                    : "Blocos laranja causam dano. Observe o movimento e passe por cima da parede.";
            if (position.x < 3.5f)
                return position.y > -1
                    ? "Armadilha: APAGADA → AVISO amarelo → ATIVA vermelha. Espere para atravessar."
                    : "Piso azul reduz sua velocidade. Contorne ou atravesse; depois passe por baixo.";
            return "O inimigo rosa persegue quando você se aproxima. Atraia-o e desvie até a chegada.";
        }
    }
}
