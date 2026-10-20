using UnityEngine;
using UnityEngine.UI;

namespace ObstacleCourse
{
    // A transparent opening is only visual: the full-screen graphic still blocks clicks behind it.
    public sealed class SpotlightGraphic : MaskableGraphic
    {
        public Rect Opening { get; private set; }
        public bool Circular { get; private set; }
        public Color outlineColor = new Color(.945f, .804f, .47f);

        public void SetOpening(Rect opening, bool circular)
        {
            if (Opening == opening && Circular == circular) return;
            Opening = opening; Circular = circular; SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var bounds = rectTransform.rect;
            var h = Opening;
            Quad(mesh, new Rect(bounds.xMin,bounds.yMin,bounds.width,h.yMin-bounds.yMin),color);
            Quad(mesh, new Rect(bounds.xMin,h.yMax,bounds.width,bounds.yMax-h.yMax),color);
            Quad(mesh, new Rect(bounds.xMin,h.yMin,h.xMin-bounds.xMin,h.height),color);
            Quad(mesh, new Rect(h.xMax,h.yMin,bounds.xMax-h.xMax,h.height),color);
            if (Circular)
            {
                // Fill the four corners around the ellipse; the 64 segments include every box corner.
                for(int i=0;i<64;i++)
                {
                    Vector2 a = Direction(i), b = Direction(i+1);
                    Vector2 innerA = h.center + Vector2.Scale(a,h.size/2);
                    Vector2 innerB = h.center + Vector2.Scale(b,h.size/2);
                    Vector2 outerA = h.center + Vector2.Scale(a / Mathf.Max(Mathf.Abs(a.x),Mathf.Abs(a.y)),h.size/2);
                    Vector2 outerB = h.center + Vector2.Scale(b / Mathf.Max(Mathf.Abs(b.x),Mathf.Abs(b.y)),h.size/2);
                    Face(mesh,innerA,outerA,outerB,innerB,color);
                    Face(mesh,innerA,innerA-a*2,innerB-b*2,innerB,outlineColor);
                }
            }
            else
            {
                Quad(mesh,new Rect(h.xMin,h.yMin,h.width,2),outlineColor);
                Quad(mesh,new Rect(h.xMin,h.yMax-2,h.width,2),outlineColor);
                Quad(mesh,new Rect(h.xMin,h.yMin,2,h.height),outlineColor);
                Quad(mesh,new Rect(h.xMax-2,h.yMin,2,h.height),outlineColor);
            }
        }

        private static Vector2 Direction(int index)
        {
            float angle=index*Mathf.PI/32;
            return new Vector2(Mathf.Cos(angle),Mathf.Sin(angle));
        }
        private static void Quad(VertexHelper mesh,Rect r,Color fill)
        {
            if(r.width<=0 || r.height<=0) return;
            Face(mesh,new Vector2(r.xMin,r.yMin),new Vector2(r.xMin,r.yMax),
                new Vector2(r.xMax,r.yMax),new Vector2(r.xMax,r.yMin),fill);
        }
        private static void Face(VertexHelper mesh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color fill)
        {
            int n=mesh.currentVertCount;
            mesh.AddVert(a,fill,Vector2.zero); mesh.AddVert(b,fill,Vector2.zero);
            mesh.AddVert(c,fill,Vector2.zero); mesh.AddVert(d,fill,Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2); mesh.AddTriangle(n,n+2,n+3);
        }
    }
}
