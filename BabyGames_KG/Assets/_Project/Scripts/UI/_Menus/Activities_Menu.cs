using System;
using CustomAttributes;
using DataClasses;
using DG.Tweening;
using GameState;
using Helpers;
using Identifiers.UI;
using SO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loggers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menus
{
    public class Activities_Menu : MenuBase
    {
        public static Transform currentBackground;

        [SerializeField] [Re_Fg_Co] public Button backButton;
        [SerializeField] [Re_Fg_Co] public Transform background;
        [SerializeField] [Re_Fg_Co] protected RectTransform _contentHolder;
        [SerializeField] protected List<KeyedObject<string, Sprite>> _backgrounds = new();

    protected List<Activity_UIUnit_Base> _activity_Instances => TryGetAll_List<Activity_UIUnit_Base>();

        protected MainMenu_GameState_Model _model;
        protected ActivityCategory_SO _activityCategory;

        public virtual async Task Initialize(MainMenu_GameState_Model model, ActivityCategory_SO activityCategory)
        {
            Clear();

            _model = model;
            _activityCategory = activityCategory;

            await Spawn();
            Enable();
            await AsyncHelper.NextFrame();

            currentBackground = background;
            await _contentHolder.DOAnchorPosX(_contentHolder.sizeDelta.x / 1.75f, 0.1f).AsyncWaitForCompletion();
        }

        public override void Enable(float? duration = null)
        {
            base.Enable(duration);

            try
            {
                var backgroundKeyedObject = _backgrounds.Find(x => x.key == _activityCategory.activityCategoryName);
                if (backgroundKeyedObject != null)
                {
                    background.GetComponent<Image>().sprite = backgroundKeyedObject.value;
                }
                else if (_backgrounds.Count > 0)
                {
                    background.GetComponent<Image>().sprite = _backgrounds[0].value;
                }
            }
            catch (Exception e)
            {
                CustomLogger.instance?.LogException(e);
            }
        }

        public override void Disable(float? duration = null)
        {
            base.Disable(duration);

            foreach (var item in _activity_Instances)
            {
                item.onActivityChosen -= OnActivityChosen;
            }

            foreach (var item in _activity_Instances)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }
        }

        private async Task Spawn()
        {
            bool moveDown = true;

            foreach (var item in _activity_Instances)
            {
                if (item != null)
                {
                    Destroy(item.gameObject);
                }
            }

            var free = _activityCategory.activities.Where(x => x.isAlwaysUnlocked).ToList();
            var rest = _activityCategory.activities.Where(x => x.isAlwaysUnlocked == false).ToList();

            foreach (var item in free)
            {
                var picture_UIUnit = await _model.activity_UIUnits_Provider.GetActivity_UIUnit(item, _contentHolder);
                Setup(picture_UIUnit, item);

                if (moveDown)
                {
                    picture_UIUnit.MoveHolderDown();
                }

                moveDown = !moveDown;
            }

            foreach (var item in rest)
            {
                var picture_UIUnit = await _model.activity_UIUnits_Provider.GetActivity_UIUnit(item, _contentHolder);
                Setup(picture_UIUnit, item);

                if (moveDown)
                {
                    picture_UIUnit.MoveHolderDown();
                }

                moveDown = !moveDown;
            }
        }

        private void Setup(Activity_UIUnit_Base item, Activity activity)
        {
            item.onActivityChosen -= OnActivityChosen;
            item.onActivityChosen += OnActivityChosen;

            item.Initialize(_model, activity);
        }

        protected virtual void OnActivityChosen(Activity_UIUnit_Base activity_UIUnit_Base)
        {
            foreach (var item in _activity_Instances)
            {
                item.onActivityChosen -= OnActivityChosen;
            }

            _model.onActivityChosen?.Invoke(activity_UIUnit_Base.activityData);
        }

        private void Clear()
        {
            foreach (var item in TryGetAll_List<Activity_UIUnit_Base>())
            {
                Destroy(item.gameObject);
            }
        }
    }
}