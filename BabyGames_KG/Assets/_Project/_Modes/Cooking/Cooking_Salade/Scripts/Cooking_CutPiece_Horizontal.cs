using System;
using DG.Tweening;
using Observables;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Helpers;
using Loggers;
using Sounds;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
#endif

namespace CoockingSalade
{
    [ExecuteInEditMode]
    public class Cooking_CutPiece_Horizontal : MonoBehaviour
    {
        public ObservableValue<bool> isCompleted = new("isCompleted");

        public GameObject dashPrefab;
        public int dashCount = 10;
        public float spacing = 0.1f;
        public float width = 1;
        public float offset = 1;
        public Transform startPoint;
        public bool debug;

        public Sound cutSound { get; set; }

        [Space]
        public List<Cooking_CutPiece_MoveData> cutPiece_MoveDatas = new();

        [SerializeField] private List<GameObject> _dashes = new List<GameObject>();
        [SerializeField] private List<Vector3> _previewPositions = new List<Vector3>();

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

        [Button]
        private void Debug()
        {
            if (debug == false) { return; }

            ComputePreviewPositions();
            SpawnDashes();
        }

        private void OnDrawGizmos()
        {
            if (debug == false) { return; }

            if (_previewPositions.Count != dashCount) { ComputePreviewPositions(); }

            Gizmos.color = Color.cyan;
            Vector3 cubeSize = new Vector3(0.1f, 0.1f, 1);
            foreach (var pos in _previewPositions) { Gizmos.DrawWireCube(pos, cubeSize); }
        }

        public async void Initialize(Transform indicator)
        {
            _indicator = indicator;
            indicator.DOMoveY(transform.position.y, 0.25f);
            SpawnDashes();

            await _indicator?.DOMoveX(_dashes.First().transform.position.x, 0.25f).AsyncWaitForCompletion();
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

            var temp = Instantiate(dashPrefab);
            if (temp == null) { return; }

            var sr = temp.GetComponent<SpriteRenderer>();
            float dashWidth = sr.bounds.size.y;

            Destroy(temp.gameObject);

            Vector3 cursor = startPoint.position;
            for (int i = 0; i < dashCount; i++)
            {
                _previewPositions.Add(cursor);
                cursor += Vector3.right * (dashWidth + spacing);
            }
        }

        private void SpawnDashes()
        {
            if (dashPrefab == null || startPoint == null || dashCount <= 0) return;

            var sample = Instantiate(dashPrefab, transform);
            var srSample = sample.GetComponent<SpriteRenderer>();
            float dashWidth = srSample.bounds.size.y;
            Destroy(sample.gameObject);

            Vector3 cursor = startPoint.position;
            for (int i = 0; i < dashCount; i++)
            {
                var dash = Instantiate(dashPrefab, transform);
                dash.transform.eulerAngles = new Vector3(0, 0, 90);
                dash.transform.position = cursor;
                dash.gameObject.name = i.ToString();
                _dashes.Add(dash);
                cursor += Vector3.right * (dashWidth + spacing);
            }
        }

        private void Update()
        {
            if (_dashes.Count < 1) { return; }
            _indicator?.DOMoveX(_dashes.First().transform.position.x + offset, 0.25f);

            bool touching = false;

            var pointer = Pointer.current;
            if (pointer == null) { return; }

            Vector3 wp = Vector3.zero;

            if (pointer.press.isPressed)
            {
                wp = Camera.main.ScreenToWorldPoint(Pointer.current.position.ReadValue());

                Vector3 myPosition = _dashes.First().transform.position;
                myPosition.y = 0;
                myPosition.z = 0;

                wp.y = 0;
                wp.z = 0;

                if (Vector3.Distance(wp, myPosition) <= width) { touching = true; }

                _audioSource?.DOFade(1, 0);
            }
            else { _audioSource?.DOFade(0, 0); }

            var nextDash = _dashes.First();

            if (touching)
            {
                nextDash.gameObject.SetActive(false);
                _dashes.Remove(nextDash);
                Destroy(nextDash.gameObject);

                if (_dashes.Count < 1) { Complete(); }
            }
        }

        private async void Complete()
        {
            try
            {
                Move_FX.MakeDoneParticle(_indicator.transform.position, 3);

                foreach (var item in cutPiece_MoveDatas)
                {
                    await item.pieceToMove.DOMoveY(item.pieceToMove.position.y + item.moveAmount, 0.5f).AsyncWaitForCompletion();
                }

                isCompleted.ChangeValue(true);
                _indicator?.DOScale(Vector3.zero, 0.25f);
                _audioSource?.DOFade(0, 0);
            } catch (Exception e) { CustomLogger.instance?.LogException(e); }
        }
    }
}