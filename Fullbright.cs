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
            // Check if the config is loaded and the BrightItems toggle is ON
            if (Mod.Instance?.Config != null && Mod.Instance.Config.BrightItems)
            {
                // Override the incoming ambient light with maximum brightness
                // This ignores the MinimumBrightness tile floor completely!
                newColor = Color.White;
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
    }
}