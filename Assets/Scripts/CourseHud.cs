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
        public RectTransform card;
        public Image accent;
        public Text help;
        public StarRatingGraphic stars;
        public Text starCaption;
        public Text starTargets;
        public Text ratingGuide;
        public Color dangerColor = new Color(1, 0.39f, 0.33f);
        public Color normalColor = new Color(0.39f, 0.94f, 0.82f);

        public void Refresh(GameController game)
        {
            var run = game.Session;
            timer.text = Mathf.CeilToInt(run.Remaining).ToString("00") + "s";
            timer.color = run.Remaining <= 10 ? dangerColor : Color.white;
            timerFill.fillAmount = run.Remaining / run.Duration;
            timerFill.color = run.Remaining <= 10 ? dangerColor : normalColor;
            best.text = game.BestTime > 0 ? "MELHOR  " + TimeText(game.BestTime) + "s\n" + StarText(game.BestStars) : "MELHOR  —";
            status.text = $"0{game.LevelIndex + 1} / {game.Level.title.ToUpper()}" + (run.Remaining <= 10 ? "  •  CORRA!" : "");
            ratingGuide.text = Targets(game.Level);
            ratingGuide.gameObject.SetActive(run.State == RunState.Playing);
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
            bool won = run.State == RunState.Won;
            bool menu = run.State == RunState.Menu;
            subtitle.gameObject.SetActive(menu);
            description.gameObject.SetActive(run.State != RunState.Paused);
            help.gameObject.SetActive(run.State != RunState.Paused);
            result.gameObject.SetActive(won || run.State == RunState.Lost);
            stars.gameObject.SetActive(won);
            stars.SetRating(game.EarnedStars);
            starCaption.gameObject.SetActive(won);
            starCaption.text = StarText(game.EarnedStars);
            starTargets.gameObject.SetActive(won || menu);
            starTargets.text = Targets(game.Level);
            ApplyLayout(run.State);
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
                title.text = "FUGA EM PAUSA";
                description.text = "";
                primaryLabel.text = "CONTINUAR FUGA";
            }
            else
            {
                title.text = won ? (game.HasNextLevel ? "PRIMEIRO SETOR\nSUPERADO" : "VOCÊ\nESCAPOU!") :
                    (run.Defeat == DefeatCause.Timeout ? "TEMPO\nESGOTADO" : "FUGA\nINTERROMPIDA");
                description.text = won ? (game.HasNextLevel ? "Uma etapa vencida. A fuga continua." :
                    "Você deixou a prisão para trás. Consegue fugir mais rápido?") : run.EndReason;
                result.text = $"TEMPO  {TimeText(run.Elapsed)}s     PONTOS  {run.Score}";
                primaryLabel.text = won ? (game.HasNextLevel ? "CONTINUAR A FUGA" : "JOGAR NOVAMENTE") : "TENTAR NOVAMENTE";
            }
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(primary.gameObject);
        }

        private static string Targets(CourseLevel level) =>
            $"3 estrelas: até {level.threeStarSeconds:0.#}s   •   2: até {level.twoStarSeconds:0.#}s   •   1: complete a fuga";

        private static string StarText(int count) => count + (count == 1 ? " ESTRELA" : " ESTRELAS");

        // Round upward for display: a time just over a star limit must not look equal to that limit.
        private static string TimeText(float seconds) => (System.Math.Ceiling((double)seconds * 100) / 100).ToString("F2");

        private void ApplyLayout(RunState state)
        {
            bool paused = state == RunState.Paused;
            bool won = state == RunState.Won;
            bool lost = state == RunState.Lost;
            float width = paused ? 640 : 880;
            float height = paused ? 300 : lost ? 520 : 690;
            card.sizeDelta = new Vector2(width, height);
            Place(accent.rectTransform, width / 2, height - 3, width, 6);
            accent.color = lost ? dangerColor : won ? stars.earnedColor : normalColor;
            if (paused)
            {
                title.fontSize = 42;
                Place(title.rectTransform, 320, 236, 560, 70);
                Place(primary.GetComponent<RectTransform>(), 320, 156, 410, 60);
                Place(secondary.GetComponent<RectTransform>(), 320, 84, 410, 48);
            }
            else if (lost)
            {
                title.fontSize = 54; help.fontSize = 15;
                Place(title.rectTransform, 440, 407, 800, 130);
                Place(description.rectTransform, 440, 298, 780, 70);
                Place(result.rectTransform, 440, 238, 810, 34);
                Place(primary.GetComponent<RectTransform>(), 440, 165, 410, 62);
                Place(secondary.GetComponent<RectTransform>(), 440, 88, 410, 48);
                Place(help.rectTransform, 440, 32, 800, 44);
            }
            else if (won)
            {
                title.fontSize = 54; help.fontSize = 15; starTargets.fontSize = 16;
                Place(title.rectTransform, 440, 566, 800, 130);
                Place(description.rectTransform, 440, 461, 780, 60);
                Place(stars.rectTransform, 440, 372, 240, 68);
                Place(starCaption.rectTransform, 440, 319, 780, 28);
                Place(result.rectTransform, 440, 279, 810, 34);
                Place(starTargets.rectTransform, 440, 242, 800, 28);
                Place(primary.GetComponent<RectTransform>(), 440, 173, 410, 62);
                Place(secondary.GetComponent<RectTransform>(), 440, 96, 410, 48);
                Place(help.rectTransform, 440, 35, 800, 45);
            }
            else
            {
                title.fontSize = 70; help.fontSize = 17; starTargets.fontSize = 17;
                Place(title.rectTransform, 440, 495, 800, 190);
                Place(description.rectTransform, 440, 342, 780, 80);
                Place(starTargets.rectTransform, 440, 278, 800, 32);
                Place(primary.GetComponent<RectTransform>(), 440, 204, 410, 68);
                Place(secondary.GetComponent<RectTransform>(), 440, 121, 410, 52);
                Place(help.rectTransform, 440, 49, 800, 60);
            }
            // Button labels are centered independently of the changing card/button dimensions.
            FitButtonLabel(primary);
            FitButtonLabel(secondary);
        }

        private static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        private static void FitButtonLabel(Button button)
        {
            var size = button.GetComponent<RectTransform>().sizeDelta;
            var label = button.GetComponentInChildren<Text>().rectTransform;
            Place(label, size.x / 2, size.y / 2, size.x - 16, size.y - 6);
        }
    }
}
