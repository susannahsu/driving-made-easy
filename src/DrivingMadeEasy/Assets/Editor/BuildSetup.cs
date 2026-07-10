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
            EnsureBaseMaterials();

            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.drivingmadeeasy.app");
            // Lock to ONE landscape orientation. Allowing both lets iOS flip the app between
            // LandscapeLeft/Right as you tilt, which inverts the accelerometer axis mid-drive
            // and breaks tilt steering unless the user manually locks rotation. Fixing a
            // single orientation removes that requirement.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.useAnimatedAutorotation = false;

            Debug.Log("Prepared iOS build: Standard shader force-included, landscape orientation, " +
                      "bundle id com.drivingmadeeasy.app. Next: File ▸ Build Settings ▸ iOS ▸ Build.");
        }

        // Create Standard-shader materials under Resources/ so the shader AND the exact
        // variants we use (opaque + emissive) are force-included in the build — the reliable
        // fix for "everything is pink" on device. M0Bootstrap clones these at runtime.
        private static void EnsureBaseMaterials()
        {
            const string dir = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(dir))
                AssetDatabase.CreateFolder("Assets", "Resources");

            CreateStandardMaterial(dir + "/dme_std.mat", emissive: false);
            CreateStandardMaterial(dir + "/dme_std_emissive.mat", emissive: true);
            AssetDatabase.SaveAssets();
        }

        private static void CreateStandardMaterial(string path, bool emissive)
        {
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;
            var shader = Shader.Find("Standard");
            if (shader == null) { Debug.LogWarning("Standard shader not found — can't create " + path); return; }

            var m = new Material(shader);
            if (emissive)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", Color.white);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            AssetDatabase.CreateAsset(m, path);
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

            // Force a single landscape orientation — the definitive way, independent of
            // Unity PlayerSettings. This stops the app launching portrait (cropping the
            // landscape UI) and stops iOS flipping between landscapes as you tilt (which
            // would invert the accelerometer and break steering).
            plist.root.values.Remove("UISupportedInterfaceOrientations");
            plist.root.values.Remove("UISupportedInterfaceOrientations~ipad");
            plist.root.SetBoolean("UIRequiresFullScreen", true);
            // Allow both landscapes at the OS level (excludes portrait); the game pins one
            // at runtime via Screen.orientation so it never flips mid-drive.
            var orient = plist.root.CreateArray("UISupportedInterfaceOrientations");
            orient.AddString("UIInterfaceOrientationLandscapeLeft");
            orient.AddString("UIInterfaceOrientationLandscapeRight");

            plist.WriteToFile(plistPath);
        }
#endif
    }
}
#endif
