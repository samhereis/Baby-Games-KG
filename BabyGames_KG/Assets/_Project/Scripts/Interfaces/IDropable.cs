using System.Threading.Tasks;
using UnityEngine;

namespace Interfaces
{
    public interface IDropable
    {
        public enum Mode { Single, Multiple }
        public Mode mode { get; }

        public Sprite icon { get; }
        public Vector3 nearestPosition { get; }

        public void Prepare();
        public void Move(Vector3 uiPosition);
        public Task Drop();
        public Task Deactivate();
    }
}