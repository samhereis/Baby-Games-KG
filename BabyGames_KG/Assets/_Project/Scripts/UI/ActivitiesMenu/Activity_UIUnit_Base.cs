using CustomAttributes;
using DataClasses;
using DataClasses.Consts;
using DG.Tweening;
using GameState;
using Helpers;
using Loggers;
using RTLTMPro;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace Identifiers.UI
{
    public abstract class Activity_UIUnit_Base : IdentifierBase
    {
        protected static List<Activity_UIUnit_Base> _lastClickedToOpenAndReady = new();

        public Action<Activity_UIUnit_Base> onActivityChosen;
        public Action<Activity> onActivityDownloaded;

        [SerializeField][Re_Fg_Co] protected Button _downloadAndOpenButton;
        [SerializeField][Re_Fg_Co] protected Button _downloadButton;
        [SerializeField][Re_Fg_Co] protected Image _iconImage;
        [SerializeField][Re_Fg_Co] protected RectTransform _iconImageHolder;
        [SerializeField][Re_Fg_Co] protected RectTransform _iconRam;

        [field: SerializeField][Re_Fg_Co] public RectTransform holder { get; private set; }
        [SerializeField][Re_Fg_Co] protected TMP_Text _itemNameText;
        [SerializeField][Fg_Co] protected LocalizeSpriteEvent _localizeStringEvent;

        [SerializeField][Fg_Se] protected bool _canMoveHolder = false;
        [SerializeField][Fg_Se] protected float _downAmount = 300f;

        [field: SerializeField][Fg_De] public Activity activityData { get; set; }
        [field: SerializeField][Fg_De] public bool isCategory { get; set; }
        [SerializeField][Fg_De] protected float _currentHoldTime = 0;

        protected MainMenu_GameState_Model _model;

        protected bool _isInitialized = false;

        public virtual async void Initialize(MainMenu_GameState_Model model, Activity activity)
        {
            activityData = activity;
            _model = model;

            _itemNameText.text = activityData.displayName;
            if (string.IsNullOrEmpty(_itemNameText.text))
            {
                _itemNameText.text = activityData.activityName;
            }

            if (_model.activity_Identifier_Provider.IsCashed(activityData))
            {
                if (await _model.activity_Identifier_Provider.HasUpdate(activityData))
                {
                    await _model.contentDeliveryService.ClearCacheAsync(activityData.GetName(), activityData.version, activityData.GetUrl());
                }
            }

            SetIcon();
        }

        protected virtual async void OnEnable()
        {
            if (_localizeStringEvent == null)
            {
                _localizeStringEvent = Get<LocalizeSpriteEvent>();
            }

            transform.localScale = Vector3.zero;
            _localizeStringEvent.transform.localScale = Vector3.zero;
            _itemNameText.transform.localScale = Vector3.zero;
            if (_isInitialized == true)
            {
                await AsyncHelper.DelayFloat(UnityEngine.Random.Range(0.0f, 0.25f));
                transform.DOScale(1, 0.25f).SetEase(Ease.OutBack);
            }

            _downloadButton.onClick.AddListener(OnDownloadRequested);
            _downloadAndOpenButton.onClick.AddListener(OnOpenRequested);
            
            TrySetLocalization();
        }

        protected virtual void OnDisable()
        {
            _downloadButton.onClick.RemoveListener(OnDownloadRequested);
            _downloadAndOpenButton.onClick.RemoveListener(OnOpenRequested);

            transform.DOKill();
        }

        protected virtual void Update()
        {
            if (Pointer.current.press.isPressed)
            {
                _currentHoldTime += Time.deltaTime;
            }
            else
            {
                _currentHoldTime = 0;
            }
        }

        public void MoveHolderDown()
        {
            if (_canMoveHolder == false)
            {
                return;
            }

            holder.anchoredPosition3D = new Vector3(1, _downAmount, 1);
        }

        protected async virtual void SetIcon()
        {
            _iconImage.sprite = await activityData.GetIcon(_model.contentDeliveryService);
            if (_iconImage.sprite == null)
            {
                _iconImage.gameObject.SetActive(false);
            }

            await AsyncHelper.DelayFloat(UnityEngine.Random.Range(0.0f, 0.25f));
            if (transform != null) { transform.DOScale(1, 0.25f).SetEase(Ease.OutBack); }

            _isInitialized = true;
        }

        public virtual void OnOpenRequested()
        {
            onActivityChosen?.Invoke(this);
        }

        public virtual void OnDownloadRequested()
        {
            onActivityChosen?.Invoke(this);
        }

        [Button]
        protected virtual async void TrySetLocalization()
        {
            try
            {
                activityData.loc_tableName = "Categories";
                activityData.loc_tableName_String = "Categories";

                if (string.IsNullOrEmpty(activityData.loc_tableName) == false && string.IsNullOrEmpty(activityData.loc_entryName) == false)
                {
                    var obj = new LocalizedSprite();

                    obj.TableReference = activityData.loc_tableName + "_Sprite";
                    obj.TableEntryReference = activityData.loc_entryName;

                    _localizeStringEvent.AssetReference = obj;
                }

                if (string.IsNullOrEmpty(activityData.loc_tableName_String) == false && string.IsNullOrEmpty(activityData.loc_entryName_String) == false)
                {
                    _itemNameText.text = "";
                    if (_itemNameText.GetComponent<RTLTextMeshPro>() is RTLTextMeshPro rTLTextMesh)
                    {
                        rTLTextMesh.text = "";
                        rTLTextMesh.OriginalText = "";
                    }

                    var obj = new LocalizedString();

                    obj.TableReference = activityData.loc_tableName_String;
                    obj.TableEntryReference = activityData.loc_entryName_String;

                    _itemNameText.GetComponent<LocalizeStringEvent>().StringReference = obj;

                    _itemNameText.GetComponent<LocalizeStringEvent>().SetTable(activityData.loc_tableName_String);
                    _itemNameText.GetComponent<LocalizeStringEvent>().SetEntry(activityData.loc_entryName_String);
                }

                await AsyncHelper.DelayFloat(UnityEngine.Random.Range(0.1f, 1f));
                _itemNameText.transform.DOScale(1, 0.25f).SetEase(Ease.OutBack);
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
            }
        }
    }
}