#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;

namespace DrivingMadeEasy.EditorTools
{
    /// <summary>
    /// One-click prep for an iPhone build. Handles the things that usually break a first
    /// Unity iOS build: the procedural materials use the built-in Standard shader (which can
    /// get stripped from a player build → pink everything), landscape orientation, a bundle
    /// id, and the motion-usage permission that tilt steering needs.
    ///
    /// Run:  Tools ▸ Driving Made Easy ▸ Prepare iOS Build   (once, before building).
    /// </summary>
    public static class BuildSetup
    {
        [MenuItem("Tools/Driving Made Easy/Prepare iOS Build")]
        public static void Prepare()
        {
            EnsureAlwaysIncludedShaders(new[] { "Standard", "Sprites/Default" });

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.drivingmadeeasy.app");
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            Debug.Log("Prepared iOS build: Standard shader force-included, landscape orientation, " +
                      "bundle id com.drivingmadeeasy.app. Next: File ▸ Build Settings ▸ iOS ▸ Build.");
        }

        private static void EnsureAlwaysIncludedShaders(string[] names)
        {
            var so = new SerializedObject(GraphicsSettings.GetGraphicsSettings());
            var arr = so.FindProperty("m_AlwaysIncludedShaders");

            var have = new HashSet<string>();
            for (int i = 0; i < arr.arraySize; i++)
            {
                var s = arr.GetArrayElementAtIndex(i).objectReferenceValue as Shader;
                if (s != null) have.Add(s.name);
            }

            foreach (var n in names)
            {
                if (have.Contains(n)) continue;
                var shader = Shader.Find(n);
                if (shader == null) continue;
                int idx = arr.arraySize;
                arr.InsertArrayElementAtIndex(idx);
                arr.GetArrayElementAtIndex(idx).objectReferenceValue = shader;
            }

            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

#if UNITY_IOS
        // Add the motion-usage permission so Input.gyro (tilt steering) doesn't crash on device.
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;
            string plistPath = path + "/Info.plist";
            var plist = new UnityEditor.iOS.Xcode.PlistDocument();
            plist.ReadFromFile(plistPath);
            plist.root.SetString("NSMotionUsageDescription",
                "Driving Made Easy uses the device's motion sensor so you can tilt the phone to steer.");
            plist.WriteToFile(plistPath);
        }
#endif
    }
}
#endif
