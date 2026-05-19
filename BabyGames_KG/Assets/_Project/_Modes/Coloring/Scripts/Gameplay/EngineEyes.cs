using Helpers;
using Loggers;
using Services;
using Sirenix.OdinInspector;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Slot = Spine.Slot;

namespace Modes.Coloring
{
    public class EngineEyes : MonoBehaviour
    {
        public GameObject ParentEye => _parent;

        [SerializeField, ReadOnly] private GameObject _parent;
        [SerializeField] private List<EyeData> _listEyesData = new();

        private Gameplay_GameState_Coloring_Model _model;

        public EyesSettings eyesPositionClampData = new();

        private bool _isPlayingAnimation => _model.currentSpine.Get<SkeletonAnimation>().AnimationName != "idle";

        public void Initialize(Gameplay_GameState_Coloring_Model model)
        {
            DiService.Inject(this);

            _model = model;

            if (_model.currentSpine.overrideEyesData) { eyesPositionClampData = _model.currentSpine.eyeSettings; }
            else { eyesPositionClampData = _model.gameSettings.eyeSettings; }

            AutoSet(_model.currentSpine);

            StartCoroutine(Blink());

        }

        private void Update()
        {
            if (Pointer.current.press.isPressed)
            {
                FollowEye(Pointer.current.position.ReadValue());
            }

            foreach (var item in _listEyesData)
            {
                if (item.apple == null) { continue; }
                if (item.outline == null) { continue; }
                if (item.back == null) { continue; }
                if (item.blink == null) { continue; }

                if (_isPlayingAnimation)
                {
                    item.apple.transform.localPosition = Vector3.zero;

                    SetOpenEyes(true);

                    continue;
                }
                else
                {
                    item.apple.transform.localPosition = Vector3.Lerp(item.apple.transform.localPosition, item.targetPosition, _model.gameSettings.eyeSettings.eyeMovementSpeed * Time.deltaTime);
                }
            }
        }

        public void FollowEye(Vector3 mousePosition)
        {
            foreach (var item in _listEyesData)
            {
                if (item.apple == null) { continue; }
                if (item.outline == null) { continue; }
                if (item.back == null) { continue; }
                if (item.blink == null) { continue; }

                Vector3 targetPosition = Camera.main.ScreenToWorldPoint(mousePosition) - item.outline.transform.position;

                targetPosition.z = 0;
                Vector3 directionFromCenter = targetPosition - item.center;

                float distance = directionFromCenter.magnitude;

                if (distance > eyesPositionClampData.eyeMaxRadius)
                {
                    directionFromCenter.Normalize();
                    targetPosition = item.center + directionFromCenter * eyesPositionClampData.eyeMaxRadius;
                }
                else
                {
                    targetPosition = item.center + directionFromCenter;
                }

                targetPosition += item.offset;
                item.targetPosition = targetPosition;
            }
        }

        public IEnumerator Blink()
        {
            yield return null;

            while (destroyCancellationToken.IsCancellationRequested == false)
            {
                SetOpenEyes(true);
                yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(_model.gameSettings.eyeSettings.eyesOpenDuration.x, _model.gameSettings.eyeSettings.eyesOpenDuration.y));

                SetOpenEyes(false);
                yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(_model.gameSettings.eyeSettings.eyesClosedDuration.x, _model.gameSettings.eyeSettings.eyesClosedDuration.y));
            }
        }

        public void SetOpenEyes(bool eyeOpen)
        {
            if (_model.isAnimationPlaying.value || _isPlayingAnimation) { eyeOpen = true; }

            foreach (var item in _listEyesData)
            {
                if (item.apple == null) { continue; }
                if (item.outline == null) { continue; }
                if (item.back == null) { continue; }
                if (item.blink == null) { continue; }

                item.blink?.gameObject.SetActive(eyeOpen == false);

                item.outline?.gameObject.SetActive(eyeOpen == true);
                item.back?.gameObject.SetActive(eyeOpen == true);
                item.apple?.gameObject.SetActive(eyeOpen == true);
            }
        }

        [Button]
        public void AutoSet(Spine_Identifier spine_Identifier)
        {
            _listEyesData.Clear();

            try
            {
                Dictionary<string, EyeData> tempDict = new();
                List<SkeletonPartsRenderer> allEyes = spine_Identifier.GetComponentsInChildren<SkeletonPartsRenderer>(true).Where(x => x.name.Contains("eyes_")).ToList();

                foreach (var obj in allEyes)
                {
                    string key = obj.name.Replace("apple", "").Replace("outline", "").Replace("blink", "").Replace("back", "");

                    if (tempDict.ContainsKey(key) == false)
                    {
                        tempDict.Add(key, new(key));
                        _listEyesData.Add(tempDict[key]);
                    }
                }

                foreach (var obj in tempDict)
                {
                    GameObject appleParent = new GameObject("apple");
                    GameObject outlineParent = new GameObject("outline");
                    GameObject backParent = new GameObject("back");
                    GameObject blinkParent = new GameObject("blink");

                    SkeletonPartsRenderer apple = allEyes.Find(x => x.name == obj.Key + "apple");
                    SkeletonPartsRenderer outline = allEyes.Find(x => x.name == obj.Key + "outline");
                    SkeletonPartsRenderer back = allEyes.Find(x => x.name == obj.Key + "back");
                    SkeletonPartsRenderer blink = allEyes.Find(x => x.name == obj.Key + "blink");

                    if (apple == null) { continue; }
                    else
                    {
                        apple.gameObject.layer = Constants.SNAPSHOTTABLE;
                        apple.transform.SetParent(appleParent.transform);
                    }

                    if (outline == null) { continue; }
                    else
                    {
                        outline.gameObject.layer = Constants.SNAPSHOTTABLE;
                        outline.transform.SetParent(outlineParent.transform);
                    }

                    if (back == null) { continue; }
                    else
                    {
                        back.gameObject.layer = Constants.SNAPSHOTTABLE;
                        back.transform.SetParent(backParent.transform);
                    }

                    if (blink != null)
                    {
                        blink.gameObject.layer = Constants.SNAPSHOTTABLE;
                        blink.transform.SetParent(blinkParent.transform);

                        Slot blinkSlot = spine_Identifier.Get<SkeletonAnimation>().separatorSlots.Find(x => x.Data.Name == blink.name);
                        var blinkPosition = SpineHelper.CalculatePosition(spine_Identifier.skeletonAnimation, blinkSlot, blinkSlot.Attachment);
                        blinkParent.transform.position = blinkPosition;
                        blink.transform.localPosition -= blinkPosition;
                    }

                    obj.Value.apple = appleParent;
                    obj.Value.outline = outlineParent;
                    obj.Value.back = backParent;
                    obj.Value.blink = blinkParent;

                    Slot appleSlot = spine_Identifier.Get<SkeletonAnimation>().separatorSlots.Find(x => x.Data.Name == apple.name);
                    Slot outlineSlot = spine_Identifier.Get<SkeletonAnimation>().separatorSlots.Find(x => x.Data.Name == outline.name);
                    Slot backSlot = spine_Identifier.Get<SkeletonAnimation>().separatorSlots.Find(x => x.Data.Name == back.name);

                    var applePosition = SpineHelper.CalculatePosition(spine_Identifier.skeletonAnimation, appleSlot, appleSlot.Attachment);
                    var outlinePosition = SpineHelper.CalculatePosition(spine_Identifier.skeletonAnimation, outlineSlot, outlineSlot.Attachment);
                    var backPosition = SpineHelper.CalculatePosition(spine_Identifier.skeletonAnimation, backSlot, backSlot.Attachment);

                    appleParent.transform.position = applePosition;
                    outlineParent.transform.position = outlinePosition;
                    backParent.transform.position = backPosition;
                    apple.transform.localPosition = Vector3.zero;

                    outline.transform.localPosition -= outlinePosition;
                    back.transform.localPosition -= backPosition;

                    Debug.Log("Difference:" + (applePosition - outlinePosition));
                }

                SetSettings(spine_Identifier.gameObject, _listEyesData);
                FollowEye(Vector3.right * Screen.width);

                _model.currentSpine.Get<_Override_Transform>()?.SetEyes();
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Could not setup eyes");
            }
        }

        public void SetSettings(GameObject parentDecor, List<EyeData> listEyesData)
        {
            if (listEyesData.Count == 0) { return; }

            _listEyesData = listEyesData;

            Vector3 sum = Vector3.zero;
            foreach (var item in _listEyesData)
            {
                if (item.apple == null) { return; }
                if (item.outline == null) { return; }
                if (item.back == null) { return; }
                if (item.blink == null) { return; }

                sum += item.outline.transform.position;
            }

            Vector3 midPoint = sum / _listEyesData.Count;
            midPoint.z = 0;

            _parent = Instantiate(new GameObject("eyes"), parentDecor.transform);

            foreach (var item in _listEyesData)
            {
                Transform itemParent = Instantiate(new GameObject(item.name), item.outline.transform).transform;
                itemParent.localPosition = Vector3.zero;

                itemParent.SetParent(_parent.transform, false);

                item.blink?.transform.SetParent(itemParent);

                item.outline?.transform.SetParent(itemParent);
                item.back?.transform.SetParent(itemParent);
                item.apple?.transform.SetParent(itemParent);

                item.targetPosition = item.apple.transform.localPosition;
            }

            SetOpenEyes(true);
        }
    }

    [Serializable]
    public class EyeData
    {
        public string name = "eye";

        public Vector3 targetPosition;
        public Vector3 center;
        public Vector3 offset;

        public EyeData(string name)
        {
            this.name = name;
        }

        public GameObject apple;
        public GameObject outline;
        public GameObject back;
        public GameObject blink;
    }
}