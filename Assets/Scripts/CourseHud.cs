using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ObstacleCourse
{
    public sealed class CourseHud : MonoBehaviour
    {
        public Text timer;
        public Text best;
        public Text status;
        public TutorialGuide guide;
        public Button challenge;
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
            best.text = game.BestTime > 0 ? "MELHOR  " + game.BestTime.ToString("F1") + "s" : "MELHOR  —";
            status.text = $"0{game.LevelIndex + 1} / {game.Level.title.ToUpper()}" + (run.Remaining <= 10 ? "  •  CORRA!" : "");
            if (guide != null) guide.Refresh(game);
        }
        public void ShowState(GameController game)
        {
            var run = game.Session;
            bool show = run.State != RunState.Playing;
            overlay.SetActive(show);
            Refresh(game);
            if (!show) { if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null); return; }
            secondary.gameObject.SetActive(run.State != RunState.Menu);
            challenge.gameObject.SetActive(run.State == RunState.Menu && game.ChallengeUnlocked);
            result.text = "";
            if (run.State == RunState.Menu)
            {
                subtitle.text = "DUAS FASES  /  FUGA CONTRA O TEMPO";
                title.text = "CONTRA O\nRELÓGIO";
                description.text = "Aprenda no tutorial e enfrente o desafio. Alcance a área verde.\nEvite os perigos: um impacto encerra a tentativa.";
                primaryLabel.text = "INICIAR TUTORIAL";
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
                subtitle.text = won ? "CHEGADA ALCANÇADA" : "OBSERVE O MOMENTO DE PASSAR";
                title.text = won ? (game.HasNextLevel ? "TUTORIAL\nCONCLUÍDO" : "DESAFIO\nSUPERADO!") : "FIM DE\nPARTIDA";
                description.text = won ? (game.HasNextLevel ? "Você aprendeu os perigos. Agora combine o que aprendeu.\nA próxima fase tem seu próprio relógio." : "Você concluiu as duas fases!\nMelhore sua execução para baixar o tempo.") : run.EndReason + "\nPressione R ou repita esta fase.";
                result.text = $"TEMPO  {run.Elapsed:F1}s     PONTOS  {run.Score}";
                primaryLabel.text = won && game.HasNextLevel ? "PRÓXIMA FASE" : "REPETIR FASE";
            }
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(primary.gameObject);
        }
    }
}
