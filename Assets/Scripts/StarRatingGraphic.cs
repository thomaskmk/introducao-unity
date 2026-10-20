using UnityEngine;
using UnityEngine.UI;

namespace ObstacleCourse
{
    // UI geometry keeps the stars crisp at every window size, without relying on font glyphs.
    public sealed class StarRatingGraphic : MaskableGraphic
    {
        public Color earnedColor = new Color(1, .78f, .32f);
        public Color emptyColor = new Color(.24f, .32f, .40f);
        public int Value { get; private set; }

        public void SetRating(int value)
        {
            value = Mathf.Clamp(value, 0, 3);
            if (Value == value) return;
            Value = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var rect = GetPixelAdjustedRect();
            float spacing = rect.width / 3;
            float radius = Mathf.Min(rect.height / 2, spacing * .42f);
            for (int star = 0; star < 3; star++)
            {
                Vector2 center = new Vector2(rect.xMin + spacing * (star + .5f), rect.center.y);
                Color fill = star < Value ? earnedColor : emptyColor;
                int first = mesh.currentVertCount;
                mesh.AddVert(center, fill, Vector2.zero);
                for (int point = 0; point < 10; point++)
                {
                    float angle = (90 - point * 36) * Mathf.Deg2Rad;
                    float r = point % 2 == 0 ? radius : radius * .45f;
                    mesh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r, fill, Vector2.zero);
                }
                for (int point = 0; point < 10; point++)
                    mesh.AddTriangle(first, first + 1 + point, first + 1 + (point + 1) % 10);
            }
        }
    }
}
