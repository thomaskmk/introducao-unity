using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ObstacleCourse
{
    [DefaultExecutionOrder(200)]
    public sealed class TutorialIntroduction : MonoBehaviour
    {
        public GameController game;
        public RectTransform layout;
        public SpotlightGraphic spotlight;
        public RectTransform header;
        public Collider2D exit;
        public RectTransform card;
        public RectTransform pointer;
        public Text title;
        public Text message;
        public Text progress;
        public Button next;
        public Text nextLabel;
        public int StepIndex { get; private set; }
        private int lastAdvanceFrame = -1;
        private Camera sceneCamera;
        private readonly Vector3[] corners = new Vector3[4];
        private Rigidbody2D[] frozenBodies;
        private RigidbodyInterpolation2D[] interpolationModes;

        public void Begin()
        {
            StepIndex=0; lastAdvanceFrame=Time.frameCount;
            FreezePresentation();
            Refresh();
        }
        private void FreezePresentation()
        {
            RestoreInterpolation();
            var bodies=new System.Collections.Generic.List<Rigidbody2D>(game.Level.GetComponentsInChildren<Rigidbody2D>());
            bodies.Add(game.player.GetComponent<Rigidbody2D>());
            frozenBodies=bodies.ToArray();
            interpolationModes=new RigidbodyInterpolation2D[frozenBodies.Length];
            for(int i=0;i<frozenBodies.Length;i++)
            {
                var body=frozenBodies[i];
                var position=body.position; float angle=body.rotation;
                interpolationModes[i]=body.interpolation;
                // With time stopped, interpolation otherwise displays the previous attempt's pose.
                body.interpolation=RigidbodyInterpolation2D.None;
                body.transform.SetPositionAndRotation(new Vector3(position.x,position.y,body.transform.position.z),Quaternion.Euler(0,0,angle));
            }
            Physics2D.SyncTransforms();
        }
        private void RestoreInterpolation()
        {
            if(frozenBodies==null) return;
            for(int i=0;i<frozenBodies.Length;i++)
                if(frozenBodies[i]!=null) frozenBodies[i].interpolation=interpolationModes[i];
            frozenBodies=null; interpolationModes=null;
        }
        private void OnDisable() { RestoreInterpolation(); }

        public void FocusNext()
        {
            if(EventSystem.current!=null) EventSystem.current.SetSelectedGameObject(next.gameObject);
        }
        public void Advance()
        {
            if(game.Session.State!=RunState.Introduction || lastAdvanceFrame==Time.frameCount) return;
            lastAdvanceFrame=Time.frameCount;
            if(StepIndex==2) { game.StartRun(); return; }
            StepIndex++;
            Refresh(); FocusNext();
        }
        private void LateUpdate()
        {
            if(game.Session.State!=RunState.Introduction) return;
            Refresh();
            if(EventSystem.current!=null && EventSystem.current.currentSelectedGameObject!=next.gameObject) FocusNext();
        }

        public void Refresh()
        {
            if(sceneCamera==null) sceneCamera=Camera.main;
            var level=game.Level;
            progress.text=$"{StepIndex+1} / 3";
            nextLabel.text=StepIndex==2 ? "COMEÇAR" : "PRÓXIMO";
            Vector2 center, target;
            Rect opening;
            if(StepIndex==0)
            {
                title.text="MOVIMENTE-SE";
                message.text="Use WASD ou as setas para mover o prisioneiro. No controle, use o analógico ou o direcional.";
                target=Project(game.player.GetComponent<Collider2D>().bounds.center);
                opening=new Rect(target-Vector2.one*54,Vector2.one*108);
                center=target+new Vector2(380,95);
            }
            else if(StepIndex==1)
            {
                title.text="ENCONTRE A SAÍDA";
                message.text="Para vencer, alcance a saída verde antes de o tempo acabar.";
                var min=Project(exit.bounds.min); var max=Project(exit.bounds.max);
                opening=Rect.MinMaxRect(min.x-10,min.y-10,max.x+10,max.y+10);
                target=opening.center; center=target+new Vector2(-430,-120);
            }
            else
            {
                title.text="TEMPO E ESTRELAS";
                message.text=$"O relógio mostra o tempo restante. As estrelas diminuem conforme o tempo passa.\nNeste tutorial: 3 até {level.threeStarSeconds:0.#}s; 2 até {level.twoStarSeconds:0.#}s; 1 se fugir antes dos {level.timeLimit:0.#}s.\nMELHOR guarda seu recorde. PAUSA interrompe a partida.";
                header.GetWorldCorners(corners);
                var min=(Vector2)layout.InverseTransformPoint(corners[0])+layout.rect.size/2;
                var max=(Vector2)layout.InverseTransformPoint(corners[2])+layout.rect.size/2;
                opening=Rect.MinMaxRect(min.x-6,min.y-6,max.x+6,max.y+6);
                target=new Vector2(opening.center.x,opening.yMin);
                center=new Vector2(800,595);
            }
            Vector2 half=card.sizeDelta/2;
            center.x=Mathf.Clamp(center.x,half.x+45,1555-half.x);
            center.y=Mathf.Clamp(center.y,half.y+145,755-half.y);
            card.anchoredPosition=center;
            var holeMin=(Vector2)spotlight.rectTransform.InverseTransformPoint(layout.TransformPoint(opening.min-layout.rect.size/2));
            var holeMax=(Vector2)spotlight.rectTransform.InverseTransformPoint(layout.TransformPoint(opening.max-layout.rect.size/2));
            spotlight.SetOpening(Rect.MinMaxRect(holeMin.x,holeMin.y,holeMax.x,holeMax.y),StepIndex==0);
            Vector2 delta=target-center;
            float edge=Mathf.Max(Mathf.Abs(delta.x)/half.x,Mathf.Abs(delta.y)/half.y);
            var start=center+delta/Mathf.Max(edge,1);
            // Stop the pointer at the opening edge, keeping the highlighted object unobstructed.
            if(StepIndex!=2)
            {
                Vector2 fromTarget=start-opening.center;
                float scale=StepIndex==0 ? fromTarget.magnitude/54 : Mathf.Max(Mathf.Abs(fromTarget.x)/(opening.width/2),Mathf.Abs(fromTarget.y)/(opening.height/2));
                target=opening.center+fromTarget/Mathf.Max(scale,1);
            }
            Vector2 line=target-start;
            pointer.anchoredPosition=(start+target)/2;
            pointer.sizeDelta=new Vector2(line.magnitude,2);
            pointer.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(line.y,line.x)*Mathf.Rad2Deg);
        }
        private Vector2 Project(Vector3 world)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(layout,sceneCamera.WorldToScreenPoint(world),null,out var local);
            return local+layout.rect.size/2;
        }
    }
}
