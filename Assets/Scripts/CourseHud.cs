using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ObstacleCourse
{
    public sealed class CourseHud : MonoBehaviour
    {
        public Text timer;
        public Text lives;
        public Text best;
        public Text status;
        public Image timerFill;
        public GameObject overlay;
        public Text title;
        public Text subtitle;
        public Text description;
        public Text primaryLabel;
        public Button primary;
        public Button secondary;
        public Text result;
        public Color dangerColor = new Color(1, 0.39f, 0.33f);
        public Color normalColor = new Color(0.39f, 0.94f, 0.82f);

        public void Refresh(GameController game)
        {
            var run = game.Session;
            timer.text = Mathf.CeilToInt(run.Remaining).ToString("00") + "s";
            timer.color = run.Remaining <= 10 ? dangerColor : Color.white;
            timerFill.fillAmount = run.Remaining / run.Duration;
            timerFill.color = run.Remaining <= 10 ? dangerColor : normalColor;
            lives.text = run.Lives + " / " + run.MaximumLives;
            best.text = game.BestTime > 0 ? "MELHOR  " + game.BestTime.ToString("F1") + "s" : "MELHOR  —";
            status.text = run.State == RunState.Playing ? (run.Remaining <= 10 ? "ÚLTIMOS SEGUNDOS!" : "ALCANCE A CHEGADA") : "PERCURSO 01";
        }
        public void ShowState(GameController game)
        {
            var run = game.Session;
            bool show = run.State != RunState.Playing;
            overlay.SetActive(show);
            Refresh(game);
            if (!show) { if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null); return; }
            secondary.gameObject.SetActive(run.State != RunState.Menu);
            result.text = "";
            if (run.State == RunState.Menu)
            {
                subtitle.text = "PROTÓTIPO  /  PERCURSO 01";
                title.text = "CONTRA O\nRELÓGIO";
                description.text = $"Atravesse o percurso e alcance a área verde.\nDesvie dos perigos laranja. Você tem {run.Duration:0} segundos e {run.MaximumLives} vidas.";
                primaryLabel.text = "COMEÇAR";
            }
            else if (run.State == RunState.Paused)
            {
                subtitle.text = "RESPIRE. O TEMPO ESTÁ PARADO.";
                title.text = "PAUSA";
                description.text = "Continue quando estiver pronto.\nVocê também pode reiniciar com R.";
                primaryLabel.text = "CONTINUAR";
            }
            else
            {
                bool won = run.State == RunState.Won;
                subtitle.text = won ? "CHEGADA ALCANÇADA" : "TENTE UMA NOVA ROTA";
                title.text = won ? "PERCURSO\nCONCLUÍDO" : "FIM DE\nPARTIDA";
                description.text = won ? "Você chegou a tempo.\nConsegue fazer um percurso ainda mais rápido?" : run.EndReason + "\nObserve o ritmo dos perigos e tente de novo.";
                result.text = $"TEMPO  {run.Elapsed:F1}s     VIDAS  {run.Lives}/{run.MaximumLives}     PONTOS  {run.Score}";
                primaryLabel.text = "JOGAR NOVAMENTE";
            }
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(primary.gameObject);
        }
    }
}
