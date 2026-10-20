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
        public ExtraLife[] pickups;

        public void ResetLevel()
        {
            foreach (var block in blocks) block.ResetHazard();
            foreach (var rotor in rotors) rotor.ResetHazard();
            foreach (var enemy in enemies) enemy.ResetEnemy();
            foreach (var trap in traps) trap.Refresh();
            foreach (var pickup in pickups) pickup.ResetPickup();
        }

        public string Hint(Vector2 position)
        {
            if (!tutorial)
            {
                if (position.x > 8) return "Última passagem: espere o bloco vertical abrir espaço e avance até a chegada.";
                if (position.y > 2.7f) return "Rota superior: menos risco, mais tempo no piso lento. Desça ao final do corredor.";
                if (position.x > -2 && position.x < 2)
                    return "Vida extra no desvio abaixo: atravesse a armadilha apagada. A coleta é opcional.";
                return "Atalho central: a barra gira, o pêndulo vai e volta. Observe a abertura e evite o perseguidor.";
            }
            if (position.x < -3.5f && position.y > 2.5f)
                return "Bloco móvel: por cima é mais seguro; por baixo é mais rápido. Espere a abertura.";
            if (position.x < -6.5f)
                return position.y < 1.5f
                    ? "WASD / setas: mova-se. A cruz rosa dá +1 vida. A coleta é opcional."
                    : "Contorne a parede pelo alto. No bloco móvel, escolha passar por cima ou por baixo.";
            if (position.x < 3.5f)
                return position.y > -1
                    ? "Armadilha: APAGADA → AVISO amarelo → ATIVA vermelha. Espere para atravessar."
                    : "Piso azul reduz sua velocidade. Contorne ou atravesse; depois passe por baixo.";
            return "O inimigo rosa persegue quando você se aproxima. Atraia-o e desvie até a chegada.";
        }
    }
}
