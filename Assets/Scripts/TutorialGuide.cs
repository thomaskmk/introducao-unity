using System;
using UnityEngine;
using UnityEngine.UI;

namespace ObstacleCourse
{
    // Presentation only: proximity is read from the player position, without adding triggers.
    public sealed class TutorialGuide : MonoBehaviour
    {
        [Serializable] public sealed class Tip
        {
            public Rect area;
            public Transform target;
            public Vector2 cardPosition;
            [TextArea] public string message;
        }

        public Tip[] tips;
        public RectTransform layout;
        public RectTransform card;
        public RectTransform pointer;
        public RectTransform marker;
        public Text message;
        public int CurrentTip { get; private set; } = -1;
        private Camera sceneCamera;

        public void Refresh(GameController game)
        {
            CurrentTip = -1;
            if (game.IsPlaying && game.Level.tutorial)
                for (int i = 0; i < tips.Length; i++)
                    if (tips[i].area.Contains(game.player.Position)) { CurrentTip = i; break; }

            bool visible = CurrentTip >= 0;
            card.gameObject.SetActive(visible);
            pointer.gameObject.SetActive(visible);
            marker.gameObject.SetActive(visible);
            if (!visible) return;

            if (sceneCamera == null) sceneCamera = Camera.main;
            var tip = tips[CurrentTip];
            message.text = tip.message;
            Vector2 target = Project(tip.target.position);
            Vector2 center = Project(tip.cardPosition);
            Vector2 half = card.sizeDelta / 2;
            Vector2 player = Project(game.player.Position);
            // Keep the explanation off the player, including when backtracking.
            if (Mathf.Abs(player.x - center.x) < half.x + 24 && Mathf.Abs(player.y - center.y) < half.y + 24)
                center.y = target.y + (player.y > target.y ? -1 : 1) * (half.y + 44);
            center.x = Mathf.Clamp(center.x, half.x + 44, 1556 - half.x);
            center.y = Mathf.Clamp(center.y, half.y + 150, 765 - half.y);
            card.anchoredPosition = center;
            marker.anchoredPosition = target;

            // End the pointer at the edge of the card instead of crossing its text.
            Vector2 delta = target - center;
            float edge = Mathf.Max(Mathf.Abs(delta.x) / half.x, Mathf.Abs(delta.y) / half.y);
            bool outside = edge > 1;
            pointer.gameObject.SetActive(outside);
            marker.gameObject.SetActive(outside);
            if (!outside) return;
            Vector2 start = center + delta / edge;
            Vector2 line = target - start;
            pointer.anchoredPosition = (start + target) / 2;
            pointer.sizeDelta = new Vector2(line.magnitude, 2);
            pointer.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(line.y, line.x) * Mathf.Rad2Deg);
        }

        private Vector2 Project(Vector2 world)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(layout, sceneCamera.WorldToScreenPoint(world), null, out var local);
            return local + layout.rect.size / 2;
        }
    }
}
