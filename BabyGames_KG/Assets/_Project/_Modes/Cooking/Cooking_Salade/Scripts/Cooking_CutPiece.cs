using System.Collections.Generic;
using UnityEngine;
using Observables;
using System.Linq;
using DG.Tweening;
using System;
using _Project.Scripts.GameFeel;
using Sirenix.OdinInspector;
using Sounds;
using UnityEngine.InputSystem;
using CustomAttributes;
using Helpers;
using Loggers;
using Modes.Coloring;
using Zenject;



#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class Cooking_CutPiece_MoveData
{
    public Transform pieceToMove;
    public float moveAmount = 0.25f;
}

namespace CoockingSalade
{
    [ExecuteInEditMode]
    public class Cooking_CutPiece : MonoBehaviour
    {
        public ObservableValue<bool> isCompleted = new("isCompleted");

        public GameObject dashPrefab;
        public Transform startPoint;
        [Fg_Se] public int dashCount = 10;
        [Fg_Se] public float spacing = 0.1f;
        [Fg_Se] public float width = 1;
        [Fg_Se] public bool debug;

        [Space]
        public List<Cooking_CutPiece_MoveData> cutPiece_MoveDatas = new();

        [SerializeField] private List<GameObject> _dashes = new List<GameObject>();
        [SerializeField] private List<Vector3> _previewPositions = new List<Vector3>();
        public float lastInputY;

        public Sound cutSound { get; set; }

        private Transform _indicator;

        [SerializeField, Fg_De] private static AudioSource _audioSource_;
        private AudioSource _audioSource
        {
            get
            {
                if (_audioSource_ == null)
                {
                    var go = new GameObject("AudioSource");
                    _audioSource_ = go.AddComponent<AudioSource>();
                }

                return _audioSource_;
            }
        }

        private void Awake()
        {
            foreach (var item in _dashes) { Destroy(item.gameObject); }
            _dashes.Clear();
        }

        private void OnDisable()
        {
            _audioSource?.DOFade(0, 0);
        }

        private void OnDrawGizmos()
        {
            if (debug == false) { return; }

            if (_previewPositions.Count != dashCount) { ComputePreviewPositions(); }

            Gizmos.color = Color.cyan;
            Vector3 cubeSize = new Vector3(0.1f, 1, 0.1f);
            foreach (var pos in _previewPositions) { Gizmos.DrawWireCube(pos, cubeSize); }
        }

        public async void Initialize(Transform indicator)
        {
            _indicator = indicator;
            indicator.DOMoveX(transform.position.x, 0.25f);
            SpawnDashes();

            var nextDash = _dashes.First();
            var srNext = nextDash.GetComponent<SpriteRenderer>();
            float topEdgeY = nextDash.transform.position.y + srNext.bounds.extents.y;
            await _indicator?.DOMoveY(topEdgeY, 0.25f).AsyncWaitForCompletion();
            _indicator?.DOScale(Vector3.one, 0.25f);

            try
            {
                _audioSource.enabled = false;
                _audioSource.clip = await cutSound.GetSound();
                _audioSource.volume = 0;
                _audioSource.loop = true;
                await AsyncHelper.NextFrame();
                _audioSource.enabled = true;
            } catch (Exception ex) { CustomLogger.instance?.LogException(ex); }
        }

        private void ComputePreviewPositions()
        {
            _previewPositions.Clear();
            if (dashPrefab == null || startPoint == null || dashCount <= 0) { return; }

#if UNITY_EDITOR
            var temp = PrefabUtility.InstantiatePrefab(dashPrefab) as GameObject;
#else
            var temp = Instantiate(dashPrefab);
#endif
            if (temp == null) { return; }

            var sr = temp.GetComponent<SpriteRenderer>();
            float dashHeight = sr.bounds.size.y;

#if UNITY_EDITOR
            DestroyImmediate(temp.gameObject);
#else
            Destroy(temp.gameObject);
#endif

            Vector3 cursor = startPoint.position;
            for (int i = 0; i < dashCount; i++)
            {
                _previewPositions.Add(cursor);
                cursor -= Vector3.up * (dashHeight + spacing);
            }
        }

        private void SpawnDashes()
        {
            if (dashPrefab == null || startPoint == null || dashCount <= 0) { return; }

            var sample = Instantiate(dashPrefab, transform);
            var srSample = sample.GetComponent<SpriteRenderer>();
            float dashHeight = srSample.bounds.size.y;
            Destroy(sample.gameObject);

            Vector3 cursor = startPoint.position;
            for (int i = 0; i < dashCount; i++)
            {
                var dash = Instantiate(dashPrefab, transform);
                dash.transform.position = cursor;
                _dashes.Add(dash);
                cursor -= Vector3.up * (dashHeight + spacing);
            }
        }

        private void Update()
        {
            if (_dashes.Count < 1) { return; }
            if (isCompleted.value) { return; }

            bool touching = false;
            float inputY = 0f;

            var pointer = Pointer.current;
            if (pointer == null) { return; }

            if (pointer.press.isPressed)
            {
                Vector3 wp = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());
                inputY = wp.y + Coocking_Controller.instance.fingetYOffset;

                Vector3 myPosition = transform.position;
                myPosition.y = 0;
                myPosition.z = 0;

                wp.y = 0;
                wp.z = 0;

                if (Vector3.Distance(wp, myPosition) <= width) { touching = true; }

                _audioSource?.DOFade(1, 0);
            }
            else { _audioSource?.DOFade(0, 0); }

            if (!touching) { return; }

            var nextDash = _dashes.First();
            var srNext = nextDash.GetComponent<SpriteRenderer>();
            float topEdgeY = nextDash.transform.position.y + srNext.bounds.extents.y;

            if (lastInputY > topEdgeY && inputY < topEdgeY)
            {
                nextDash.gameObject.SetActive(false);
                _dashes.Remove(nextDash);
                Destroy(nextDash.gameObject);

                if (_dashes.Count < 1) { Complete(); }
            }

            lastInputY = inputY;
            _indicator?.DOMoveY(topEdgeY, 0.25f);
        }

        [Button]
        private async void Complete()
        {
            Move_FX.MakeDoneParticle(_indicator.transform.position, 3);
            foreach (var item in cutPiece_MoveDatas)
            {
                if (item == null) { continue; }
                item.pieceToMove.DOLocalMoveX(item.pieceToMove.localPosition.x + item.moveAmount, 0.5f);
            }
            await AsyncHelper.DelayFloat(1f);

            isCompleted.ChangeValue(true);
            _indicator?.DOScale(Vector3.zero, 0.25f);
            _audioSource?.DOFade(0, 0);
        }
    }
}