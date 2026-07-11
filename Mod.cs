using HarmonyLib;
using Terraria;
using TerrariaModder.Core;
using TerrariaModder.Core.Config;
using TerrariaModder.Core.Logging;

namespace FullbrightMod
{
    // Generates the F6 menu UI automatically
    public class FullbrightConfig : ModConfig
    {
        // FIX: Added the required abstract Version property
        public override int Version => 1;

        [Client] 
        public bool Enabled { get; set; } = true;

        [Client] 
        public float MinimumBrightness { get; set; } = 0.4f; // 0.0f is pitch black, 1.0f is max brightness

        [Client]
        public bool BrightItems { get; set; } = false;
    }

    public class Mod : IMod, IModLifecycle
    {
        public string Id => "fullbright-mod";
        public string Name => "Fullbright Mod";
        public string Version => "1.0.0";

        // Static instance so our Harmony patches in Fullbright.cs can read the config
        public static Mod Instance { get; private set; }
        public FullbrightConfig Config { get; private set; }

        private ILogger _log;
        private Harmony _harmony;

        public void Initialize(ModContext context)
        {
            Instance = this;
            _log = context.Logger;
            Config = context.GetConfig<FullbrightConfig>();

            // Register an in-game hotkey to toggle Fullbright on/off
            context.RegisterKeybind("toggle-fullbright", "Toggle Fullbright",
                "Enables or disables the minimum brightness floor", "B", ToggleFullbright);

            // Initialize and apply all Harmony patches in this assembly
            _harmony = new Harmony(Id);
            _harmony.PatchAll();

            _log.Info($"Fullbright Mod initialized! Floor set to {Config.MinimumBrightness * 100}%");
        }

        // FIX: Implemented required IModLifecycle methods (left empty since we don't need them)
        public void OnContentReady(ModContext context)
        {
        }

        public void OnWorldLoad()
        {
        }

        public void OnWorldUnload()
        {
        }

        public void OnConfigChanged()
        {
            _log.Info($"Config updated: Enabled={Config.Enabled}, MinimumBrightness={Config.MinimumBrightness}");
        }

        public void Unload()
        {
            // Clean up patches when the mod unloads
            _harmony?.UnpatchAll(Id);
            Instance = null;
            _log.Info("Fullbright Mod unloaded");
        }

        private void ToggleFullbright()
        {
            Config.Enabled = !Config.Enabled;
            
            string status = Config.Enabled ? $"ON ({Config.MinimumBrightness * 100}%)" : "OFF";
            Main.NewText($"Fullbright: {status}", 255, 255, 100);
            
            _log.Info($"Toggled Fullbright to {Config.Enabled}");
        }
    }
}