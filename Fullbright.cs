using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Terraria;

namespace FullbrightMod
{
    // PATCH 1: Intercept standard XY tile lighting checks
    [HarmonyPatch(typeof(Lighting), nameof(Lighting.GetColor), new Type[] { typeof(int), typeof(int) })]
    public static class Patch_Lighting_GetColor_XY
    {
        static void Postfix(ref Color __result)
        {
            LightingHelper.ApplyBrightnessFloor(ref __result);
        }
    }

    // PATCH 2: Intercept the overload used by legacy lighting modes and special entities
    [HarmonyPatch(typeof(Lighting), nameof(Lighting.GetColor), new Type[] { typeof(int), typeof(int), typeof(Color) })]
    public static class Patch_Lighting_GetColor_XYOld
    {
        static void Postfix(ref Color __result)
        {
            LightingHelper.ApplyBrightnessFloor(ref __result);
        }
    }

    // PATCH 3: Intercept clamped color checks used by shaders, waterfalls, and UI
    [HarmonyPatch(typeof(Lighting), nameof(Lighting.GetColorClamped), new Type[] { typeof(int), typeof(int), typeof(Color) })]
    public static class Patch_Lighting_GetColorClamped
    {
        static void Postfix(ref Color __result)
        {
            LightingHelper.ApplyBrightnessFloor(ref __result);
        }
    }

    // PATCH 4: Intercept float brightness checks used by LiquidRenderer to prevent liquid culling!
    [HarmonyPatch(typeof(Lighting), nameof(Lighting.Brightness), new Type[] { typeof(int), typeof(int) })]
    public static class Patch_Lighting_Brightness
    {
        static void Postfix(ref float __result)
        {
            if (Mod.Instance?.Config == null || !Mod.Instance.Config.Enabled)
                return;

            float minFloat = Math.Max(0f, Math.Min(1f, Mod.Instance.Config.MinimumBrightness));
            __result = Math.Max(__result, minFloat);
        }
    }

    // PATCH 5: Intercept item rendering to force 100% brightness on items
    [HarmonyPatch(typeof(Item), nameof(Item.GetAlpha), new Type[] { typeof(Color) })]
    public static class Patch_Item_GetAlpha
    {
        static void Prefix(ref Color newColor)
        {
            if (Mod.Instance?.Config != null && Mod.Instance.Config.BrightItems)
            {
                newColor = LightingHelper.GetXnaColor(Mod.Instance.Config.ItemColor);
            }
        }
    }

    // PATCH 6: Intercept NPC rendering to force 100% brightness on hostile mobs
    [HarmonyPatch(typeof(NPC), nameof(NPC.GetAlpha), new Type[] { typeof(Color) })]
    public static class Patch_NPC_GetAlpha
    {
        static void Prefix(NPC __instance, ref Color newColor)
        {
            if (Mod.Instance?.Config != null && Mod.Instance.Config.BrightEnemies)
            {
                if (!__instance.friendly && __instance.damage > 0)
                {
                    newColor = LightingHelper.GetXnaColor(Mod.Instance.Config.EnemyColor);
                }
            }
        }
    }

    public static class LightingHelper
    {
        public static void ApplyBrightnessFloor(ref Color color)
        {
            // If the mod isn't loaded yet or is disabled via F6/Hotkey, do nothing
            if (Mod.Instance?.Config == null || !Mod.Instance.Config.Enabled)
                return;

            // Use .NET 4.8 compatible Math.Max/Math.Min
            float rawVal = Mod.Instance.Config.MinimumBrightness;
            float minFloat = Math.Max(0f, Math.Min(1f, rawVal));
            
            byte minByte = (byte)(minFloat * 255f);

            // Elevate each channel to the floor without diminishing existing bright light sources
            color.R = Math.Max(color.R, minByte);
            color.G = Math.Max(color.G, minByte);
            color.B = Math.Max(color.B, minByte);
        }

        // Translates the F6 enum choice into an actual XNA Color
        public static Color GetXnaColor(string colorName)
        {
            // Safety check in case the string is null or empty
            if (string.IsNullOrWhiteSpace(colorName)) return Color.White;

            switch (colorName.Trim().ToLower())
            {
                case "red": return Color.Red;
                case "green": return Color.LimeGreen; 
                case "blue": return Color.DeepSkyBlue; 
                case "yellow": return Color.Yellow;
                case "purple": return Color.Magenta;
                case "cyan": return Color.Cyan;
                case "pink": return Color.HotPink;
                case "orange": return Color.Orange;
                default: return Color.White; // Fallback if a typo is made in the menu
            }
        }
    }
}