using UnityEngine;

namespace Bezier
{
    [DisallowMultipleComponent]
    public class BezierPathCreator : MonoBehaviour
    {
        public BezierPath path;

        public Color anchorCol = Color.red;
        public Color controlCol = Color.white;
        public Color segmentCol = Color.green;
        public Color selectedSegmentCol = Color.yellow;
        public float anchorDiameter = .1f;
        public float controlDiameter = .075f;
        public bool displayControlPoints = true;

        public void CreatePath()
        {
            path = new BezierPath(transform.position);
        }

        private void Reset()
        {
            CreatePath();
        }
    }
}