using UnityEngine;

namespace ObstacleCourse
{
    public sealed class CourseLevel : MonoBehaviour
    {
        public string title;
        public bool tutorial;
        public int layoutVersion = 2;
        [Min(1)] public float timeLimit = 90;
        public Vector2 spawn = new Vector2(-13, -4.5f);
        public PatrolHazard[] blocks;
        public RotatingHazard[] rotors = System.Array.Empty<RotatingHazard>();
        public ChaserEnemy[] enemies;
        public CycleTrap[] traps;
        public SlowZone[] slowZones;

        public void ResetLevel()
        {
            foreach (var block in blocks) block.ResetHazard();
            foreach (var rotor in rotors) rotor.ResetHazard();
            foreach (var enemy in enemies) enemy.ResetEnemy();
            foreach (var trap in traps) trap.Refresh();
        }

        public string Hint(Vector2 position)
        {
            if (!tutorial) return "Alcance a chegada antes do tempo acabar. Um impacto encerra a tentativa.";
            if (position.x < -6.5f && position.y < 2.5f)
                return "WASD / setas para mover. A barra dá voltas completas: observe o espaço que ela varre.";
            if (position.x < -3.5f && position.y > 2.5f)
                return "Bloco móvel: por cima é mais seguro; por baixo é mais rápido. Espere a abertura.";
            if (position.x < 3.5f)
            {
                if (position.y > 1.4f) return "O pêndulo vai e volta. Espere a barra se afastar para passar.";
                if (position.y > -1) return "Armadilha: APAGADA → AVISO amarelo → ATIVA vermelha. Atravesse apagada.";
                return "O piso azul reduz a velocidade. Depois, contorne a parede por baixo.";
            }
            return "Aproxime-se do vão para atrair o guarda. Recue, contorne-o e atravesse a passagem que ele deixou.";
        }
    }
}
