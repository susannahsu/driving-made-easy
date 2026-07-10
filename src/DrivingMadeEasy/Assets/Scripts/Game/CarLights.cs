using UnityEngine;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Toggleable headlights: two forward spotlights the player can switch on/off. Subtle in
    /// daylight, but the control is there (and matters once night/rain lessons arrive).
    /// </summary>
    public class CarLights : MonoBehaviour
    {
        public bool On { get; private set; }

        private Light _left;
        private Light _right;

        private void Awake()
        {
            _left = MakeSpot(new Vector3(-0.6f, 0.15f, 2.0f));
            _right = MakeSpot(new Vector3(0.6f, 0.15f, 2.0f));
            Apply();
        }

        private Light MakeSpot(Vector3 localPos)
        {
            var go = new GameObject("Headlight");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
            var l = go.AddComponent<Light>();
            l.type = LightType.Spot;
            l.range = 45f;
            l.spotAngle = 55f;
            l.intensity = 3f;
            l.color = new Color(1f, 0.97f, 0.85f);
            return l;
        }

        public void Toggle()
        {
            On = !On;
            Apply();
        }

        private void Apply()
        {
            if (_left != null) _left.enabled = On;
            if (_right != null) _right.enabled = On;
        }
    }
}
