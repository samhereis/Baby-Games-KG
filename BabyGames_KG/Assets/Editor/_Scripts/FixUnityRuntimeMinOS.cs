#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace EditorScripts
{
    public class FixUnityRuntimeMinOS
    {
        [PostProcessBuild(999)]
        public static void OnPostProcessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;

            {
                string frameworkPlistPath = Path.Combine(path,
                    "Frameworks/UnityRuntime.framework/Info.plist");

                if (File.Exists(frameworkPlistPath))
                {
                    var plist = new PlistDocument();
                    plist.ReadFromFile(frameworkPlistPath);

                    string minVersion = PlayerSettings.iOS.targetOSVersionString;

                    plist.root.SetString("MinimumOSVersion", minVersion);
                    plist.WriteToFile(frameworkPlistPath);
                }
            }

            {
                string projectPath = PBXProject.GetPBXProjectPath(path);
                PBXProject project = new PBXProject();
                project.ReadFromFile(projectPath);

                string frameworkGuid = project.GetUnityFrameworkTargetGuid();
                project.SetBuildProperty(frameworkGuid,
                    "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");

                string mainGuid = project.GetUnityMainTargetGuid();
                project.SetBuildProperty(mainGuid,
                    "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");

                project.WriteToFile(projectPath);
            }
        }
    }
}
#endif