using HarmonyLib;
using Il2Cpp;
using ModComponent.Utils;
using System;
using UnityEngine;

namespace CanneryManufacturing
{

	internal static class Utils
	{
        public static string? NormalizeName(string name)
        {
            if (name == null)
            {
                return null;
            }
            else
            {
                return name.Replace("(Clone)", "").Trim();
            }
        }

        public static GearItem? LoadGearItemPrefab(string name)
        {
            GameObject? gameObject = Resources.Load<GameObject>(name);

            return gameObject?.GetComponent<GearItem>();
        }

        public static T? GetOrCreateComponent<T>(this GameObject? gameObject) where T : Component
        {
            if (gameObject == null)
            {
                return default;
            }

            T? result = GetComponentSafe<T>(gameObject);

            if (result == null)
            {
                result = gameObject.AddComponent<T>();
            }

            return result;
        }

        public static T? GetComponentSafe<T>(this GameObject? gameObject) where T : Component
        {
            return gameObject == null ? default : gameObject.GetComponent<T>();
        }
    }
}
