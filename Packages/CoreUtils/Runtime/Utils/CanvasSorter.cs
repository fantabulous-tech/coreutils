using UnityEngine;

namespace CoreUtils {
    [RequireComponent(typeof(Canvas))]
    public class CanvasSorter : MonoBehaviour {
        private Canvas m_Canvas;

        private void OnEnable() {
            m_Canvas = GetComponent<Canvas>();
            CanvasSorterManager.Add(this);
        }

        private void OnDisable() {
            CanvasSorterManager.Remove(this);
        }

        public int Order {
            get => m_Canvas.sortingOrder;
            set => m_Canvas.sortingOrder = value;
        }
    }
}