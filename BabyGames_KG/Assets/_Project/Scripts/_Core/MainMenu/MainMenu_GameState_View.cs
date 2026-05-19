using Coloring.ForParents;
using DataClasses;
using Saratan.Coloring.Gallery;
using SO;
using System;
using System.Threading.Tasks;
using UI.Menus;
using static GameState.MainMenu_GameState;

namespace GameState
{
    public class MainMenu_GameState_View : IDisposable
    {
        private MainMenu_GameState_Model _model;

        public ActivityCategories_Menu categoriesMenu;

        public Activities_Menu activitiesMenu_Standart;
        public Activities_Menu_Colorings activities_Menu_Colorings;
        public Activities_Menu_Video activities_Menu_Video;

        public SettingsMenu settingsMenu;

        public MainMenu_GameState_View(MainMenu_GameState_Model model)
        {
            _model = model;
        }

        public async void Initialize(OpenSettings openSettings = OpenSettings.None)
        {
            categoriesMenu = await _model.listOfAllMenus.categoriesMenu.InstantiateAsync();

            activitiesMenu_Standart = await _model.listOfAllMenus.activitiesMenu.InstantiateAsync();
            activities_Menu_Colorings = await _model.listOfAllMenus.activitiesMenu_Coloring.InstantiateAsync();
            activities_Menu_Video = await _model.listOfAllMenus.activitiesMenu_Video.InstantiateAsync();

            settingsMenu = await _model.listOfAllMenus.settingsMenu.InstantiateAsync();

            settingsMenu.Construct(_model);

            await categoriesMenu.Initialize(_model);

            _model.onActivityCategoryOpenRequested += OpenActivitiesMenu;

            activitiesMenu_Standart.backButton.onClick.AddListener(OpenCategoriesMenu);
            activities_Menu_Colorings.backButton.onClick.AddListener(OpenCategoriesMenu);
            activities_Menu_Video.backButton.onClick.AddListener(OpenCategoriesMenu);

            settingsMenu.backButton.onClick.AddListener(OpenCategoriesMenu);

            categoriesMenu.forParentsButton.onClick.AddListener(OpenForParents);

            switch (_model.openSettings)
            {
                case OpenSettings.None:
                    {
                        OpenCategoriesMenu();
                        break;
                    }
                case OpenSettings.OpenActivitiesMenu:
                    {
                        OpenActivitiesMenu(MainMenu_GameState_Model.selectedActivityCategory);
                        break;
                    }
            }
        }

        public void Dispose()
        {
            _model.onActivityCategoryOpenRequested -= OpenActivitiesMenu;

            activitiesMenu_Standart.backButton.onClick.RemoveListener(OpenCategoriesMenu);
            activities_Menu_Colorings.backButton.onClick.RemoveListener(OpenCategoriesMenu);
            activities_Menu_Video.backButton.onClick.RemoveListener(OpenCategoriesMenu);

            settingsMenu.backButton.onClick.RemoveListener(OpenCategoriesMenu);

            categoriesMenu.forParentsButton.onClick.RemoveListener(OpenForParents);
        }

        public async void OpenParentalGate(Action callbackOnSuccess, Action callbackOnFailure)
        {
            var parentalGateMenu = await _model.listOfAllMenus.parentalGateMenu.InstantiateAsync();

            parentalGateMenu.ShowPopup(callbackOnSuccess, callbackOnFailure);
        }

        private void OpenCategoriesMenu()
        {
            categoriesMenu.Enable();
        }

        private async void OpenActivitiesMenu(ActivityCategory_SO category)
        {
            if (category.activities.TrueForAll(x => x.type == ActivityType.Coloring))
            {
                MainMenu_GameState_Model.selectedActivityCategory = category;
                await activities_Menu_Colorings.Initialize(_model, category);
            }
            else if (category.activities[0].type == ActivityType.Video)
            {
                MainMenu_GameState_Model.selectedActivityCategory = category;
                await activities_Menu_Video.Initialize(_model, category);
            }
            else
            {
                await activitiesMenu_Standart.Initialize(_model, category);
            }
        }

        private void OpenForParents()
        {
            OpenParentalGate(() =>
            {
                settingsMenu?.Enable();
            }, null);
        }
    }
}