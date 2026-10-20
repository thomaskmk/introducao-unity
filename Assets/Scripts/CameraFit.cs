using UnityEngine;

namespace ObstacleCourse
{
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFit : MonoBehaviour
    {
        private Camera view;
        private void Awake() { view = GetComponent<Camera>(); }
        private void LateUpdate() { view.orthographicSize = Mathf.Max(9, 16f / Mathf.Max(view.aspect, 0.1f)); }
    }
}
