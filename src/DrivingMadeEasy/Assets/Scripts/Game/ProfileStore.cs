using System.IO;
using UnityEngine;
using DrivingMadeEasy.Coaching;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Local-first persistence for the player's coaching progress (SPEC §8.1): saves the
    /// PlayerProfile to a JSON file under Application.persistentDataPath so the Coach
    /// remembers what you've already learned between drives and app launches.
    /// </summary>
    public static class ProfileStore
    {
        private static string FilePath =>
            Path.Combine(Application.persistentDataPath, "coach_profile.json");

        public static PlayerProfile Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    var dto = JsonUtility.FromJson<PlayerProfile.ProfileDto>(json);
                    if (dto != null) return PlayerProfile.FromDto(dto);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Coach profile load failed: {e.Message}");
            }
            return new PlayerProfile();
        }

        public static void Save(PlayerProfile profile)
        {
            try
            {
                File.WriteAllText(FilePath, JsonUtility.ToJson(profile.ToDto()));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Coach profile save failed: {e.Message}");
            }
        }

        public static void Clear()
        {
            try
            {
                if (File.Exists(FilePath)) File.Delete(FilePath);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Coach profile clear failed: {e.Message}");
            }
        }
    }
}
