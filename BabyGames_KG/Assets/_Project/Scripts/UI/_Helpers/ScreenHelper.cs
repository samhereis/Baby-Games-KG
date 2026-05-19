using Interfaces;
using Loggers;
using Services;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Helpers
{
    public static class ScreenHelper
    {
        public static ScreenOrientation currentOrientation { get; private set; } = ScreenOrientation.LandscapeLeft;
        private static ISceneLoader _sceneLoader => DiService.Get<ISceneLoader>();

        public static async Task SetAppOrientationAsync(ScreenOrientation screenOrientation)
        {
            try
            {
                await _sceneLoader.ShowLoadingLayerAsync();
                await _sceneLoader.LoadSceneAsyncWithTransition("_Empty_Scene");
            }
            catch
            {
                CustomLogger.instance?.LogException(new Exception("Error openning empty scene"));
            }
            finally
            {
                SetAppOrientation(screenOrientation);
            }
        }

        public static void SetEnabled(this Behaviour component, bool enabled)
        {
            if (component != null) { component.enabled = enabled; }
        }

        public static void SetAppOrientation(ScreenOrientation screenOrientation)
        {
            currentOrientation = screenOrientation;

            Camera camera = Camera.main;

            try
            {
                camera?.SetEnabled(false);

                if (currentOrientation == ScreenOrientation.LandscapeLeft || screenOrientation == ScreenOrientation.LandscapeRight)
                {
                    Screen.autorotateToLandscapeLeft = true;
                    Screen.autorotateToLandscapeRight = true;
                    Screen.autorotateToPortrait = false;
                    Screen.autorotateToPortraitUpsideDown = false;
                }
                else if (currentOrientation == ScreenOrientation.Portrait)
                {
                    Screen.autorotateToLandscapeLeft = false;
                    Screen.autorotateToLandscapeRight = false;
                    Screen.autorotateToPortrait = true;
                    Screen.autorotateToPortraitUpsideDown = true;
                }

                Screen.orientation = currentOrientation;
            }
            catch (Exception ex)
            {
                CustomLogger.instance?.LogException(ex, "Someting went wrong while changing screen orientation: ");
            }
            finally
            {
                camera?.SetEnabled(true);
            }
        }
    }
}