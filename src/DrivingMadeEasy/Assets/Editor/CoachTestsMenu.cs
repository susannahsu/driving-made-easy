#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using DrivingMadeEasy.Coaching;

namespace DrivingMadeEasy.EditorTools
{
    /// <summary>
    /// Runs the headless Coach scenarios and prints PASS/FAIL to the Console.
    /// Menu: Tools ▸ Driving Made Easy ▸ Run Coach Tests. (Scripts in an Assets/Editor
    /// folder compile into the editor assembly automatically — no asmdef needed.)
    /// </summary>
    public static class CoachTestsMenu
    {
        [MenuItem("Tools/Driving Made Easy/Run Coach Tests")]
        public static void Run()
        {
            var results = CoachScenarios.RunAll();
            int passed = 0;
            foreach (var r in results)
            {
                if (r.Passed)
                {
                    passed++;
                    Debug.Log($"PASS  {r.Name}");
                }
                else
                {
                    Debug.LogError($"FAIL  {r.Name} — {r.Detail}");
                }
            }
            Debug.Log($"Coach tests: {passed}/{results.Count} passed.");
        }
    }
}
#endif
