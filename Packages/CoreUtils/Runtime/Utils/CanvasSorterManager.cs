using System.Collections.Generic;
using UnityEngine;

namespace CoreUtils {
    public class CanvasSorterManager : Singleton<CanvasSorterManager> {
        private readonly List<CanvasSorter> m_Sorters = new List<CanvasSorter>();

        private Transform m_Target;
        private Transform Target => UnityUtils.GetOrSet(ref m_Target, () => UnityUtils.CameraTransform);

        private void Update() {
            m_Sorters.Sort((s1, s2) => DistanceTo(s2).CompareTo(DistanceTo(s1)));
            m_Sorters.ForEach((s, i) => s.Order = i);
        }

        private float DistanceTo(Component sorter) {
            return sorter.transform.position.SqrDistanceTo(Target.position);
        }

        public static void Add(CanvasSorter sorter) {
            Instance.m_Sorters.Add(sorter);
        }

        public static void Remove(CanvasSorter sorter) {
            Instance.m_Sorters.Remove(sorter);
        }
    }
}