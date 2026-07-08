using UnityEngine;
using DrivingMadeEasy.Coaching;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// The end-of-drive report (SPEC §7.1): press the finish key to see your safety score,
    /// the tickets you collected (with the explanations the Coach deferred during the
    /// drive), and which rules you graduated. "Drive again" starts a fresh score while
    /// keeping everything you've learned.
    /// </summary>
    public class DriveReportHud : MonoBehaviour
    {
        public CoachRuntime coach;
        public KeyCode finishKey = KeyCode.Tab;

        [Tooltip("Minimum safety score to pass the lesson (and no severe/instant-fail).")]
        public int passThreshold = 70;

        private bool _show;
        private bool _lessonResult;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(finishKey)) { _show = !_show; _lessonResult = false; }
        }

        /// Called by the finish line: show the report as a whole-lesson pass/fail verdict.
        public void ShowLessonResult()
        {
            _show = true;
            _lessonResult = true;
        }

        private void OnGUI()
        {
            var hint = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            GUI.Label(new Rect(16, Screen.height - 26, 420, 20),
                "[Tab] end drive & see report", hint);

            if (!_show || coach == null) return;

            DriveReport report = coach.Session.BuildReport();

            const float w = 580f, h = 440f;
            var panel = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 22, panel.y + 18, w - 44, h - 36));

            var title = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
            if (_lessonResult)
            {
                bool passed = !report.Failed && report.FinalScore >= passThreshold;
                title.normal.textColor = passed ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 0.55f, 0.55f);
                GUILayout.Label(passed ? "Lesson 1 — Passed!" : "Lesson 1 — Not passed", title);
            }
            else
            {
                GUILayout.Label(report.Failed ? "Drive ended — not a pass" : "Drive report", title);
            }
            GUILayout.Space(6);

            var big = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
            GUILayout.Label($"Safety score:  {report.FinalScore} / 100", big);
            GUILayout.Label($"Tickets:  {report.Tickets.Count}   (fines ${report.TotalFines})", big);
            GUILayout.Space(8);

            if (report.Tickets.Count > 0)
            {
                GUILayout.Label("What you were cited for:", big);
                foreach (var t in report.Tickets)
                    GUILayout.Label($"   • {t.Title}: -{t.Points} pts — {t.Explanation}");
                GUILayout.Space(8);
            }

            if (report.RulesGraduated.Count > 0)
            {
                GUILayout.Label("Graduated this drive:  " +
                                string.Join(", ", report.RulesGraduated), big);
            }
            else if (report.Tickets.Count == 0)
            {
                GUILayout.Label("Clean drive — nicely done.", big);
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Drive again (keep what you learned)", GUILayout.Height(38)))
            {
                coach.ResetDrive();
                _show = false;
            }
            if (GUILayout.Button("Close", GUILayout.Height(38), GUILayout.Width(120)))
            {
                _show = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}
