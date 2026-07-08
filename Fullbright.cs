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

    public static class LightingHelper
    {
        public static void ApplyBrightnessFloor(ref Color color)
        {
            // If the mod isn't loaded yet or is disabled via F6/Hotkey, do nothing
            if (Mod.Instance?.Config == null || !Mod.Instance.Config.Enabled)
                return;

            // FIX: Replaced Math.Clamp with .NET 4.8 compatible Math.Max/Math.Min
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