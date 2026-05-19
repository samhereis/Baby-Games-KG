using Identifiers;
using Observables;
using PaintCore;
using PaintIn3D;
using Services;
using Spine;
using UnityEngine;
using Zenject;

namespace Modes.Coloring
{
    public class Paintable_Identifier_Basic : IdentifierBase
    {
        public static bool isEverPainted;

        [field: SerializeField] public CwPaintableMesh _paintable;
        [field: SerializeField] public CwPaintableTexture paintableTexture;

        [Space]
        [field: SerializeField] protected MeshCollider _meshCollider;

        [Inject] protected ObservableValue<Paintable_Identifier_Basic> _tapObject;
        [Inject] protected GameController _gameController;

        public virtual void Init(Spine_Identifier spine_Identifier, AtlasRegion atlasRegion)
        {
            _paintable = Get<CwPaintableMesh>();
            paintableTexture = Get<CwPaintableTexture>();
            _meshCollider = Get<MeshCollider>();
            DiService.Inject(this);
        }

        private void OnEnable()
        {
            if (_tapObject == null)
            {
                _tapObject = DiService.Get<ObservableValue<Paintable_Identifier_Basic>>();
            }

            _tapObject.AddListener(ProcessTapObjectChanged);
        }

        private void OnDisable()
        {
            _tapObject.RemoveListener(ProcessTapObjectChanged);
        }

        public virtual void OnPress()
        {
            _meshCollider.enabled = true;

            foreach (var item in _gameController.tools)
            {
                item.SetPaintable(this);
            }

            isEverPainted = true;
        }

        public virtual void DisablePaint()
        {
            _meshCollider.enabled = false;
        }

        public virtual void OnRelease()
        {
            _meshCollider.enabled = true;
        }

        protected virtual void ProcessTapObjectChanged(Paintable_Identifier_Basic paintable)
        {
            if (paintable == null)
            {
                OnRelease();
            }
            else
            {
                if (paintable == this)
                {
                    OnPress();
                }
                else
                {
                    DisablePaint();
                }
            }
        }
    }
}