using HarmonyLib;
using NewHorizons.Builder.Props.Audio;
using OWML.Common;
using OWML.ModHelper;
using OWML.Utils;
using System.Reflection;
using UnityEngine;

namespace LuminaTerra
{
    public class LuminaTerra : ModBehaviour
    {
        public static LuminaTerra Instance;
        public INewHorizons NewHorizons;
        public ItemType CrystalItemType;
        public bool inEndSequence;

        public delegate void SignalLearnEvent();
        public event SignalLearnEvent OnLearnHeartSignal;

        public bool InCorrectSystem => NewHorizons.GetCurrentStarSystem() == "Hornfel's Discovery";

        public void Awake()
        {
            Instance = this;
            // You won't be able to access OWML's mod helper in Awake.
            // So you probably don't want to do anything here.
            // Use Start() instead.
        }

        public void Start()
        {
            // Starting here, you'll have access to OWML's mod helper.
            ModHelper.Console.WriteLine($"My mod {nameof(LuminaTerra)} is loaded!", MessageType.Success);

            // Get the New Horizons API and load configs
            NewHorizons = ModHelper.Interaction.TryGetModApi<INewHorizons>("xen.NewHorizons");
            NewHorizons.LoadConfigs(this);

            CrystalItemType = EnumUtils.Create<ItemType>("Crystal");

            new Harmony("Etherpod.LuminaTerra").PatchAll(Assembly.GetExecutingAssembly());

            // Example of accessing game code.
            OnCompleteSceneLoad(OWScene.TitleScreen, OWScene.TitleScreen); // We start on title screen
            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        public void OnCompleteSceneLoad(OWScene previousScene, OWScene newScene)
        {
            if (newScene != OWScene.SolarSystem) return;
            ModHelper.Console.WriteLine("Loaded into solar system!", MessageType.Success);
        }

        public void LearnHeartSignal()
        {
            OnLearnHeartSignal?.Invoke();
        }

        public void ShowEndScreen()
        {
            GameOverController goController = FindObjectOfType<GameOverController>();
            goController._deathText.text = "The Crystal Heart rejoices with you as glimmering stars return to the sky, and your old universe fades to dust.\nCould this be just a dream? A facade to escape the true fate of the universe?\nMaybe it doesn't matter. It is beautiful, after all.";
            goController.SetupGameOverScreen(10f);
        }
    }

    [HarmonyPatch]
    public static class LuminaTerraPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.LearnSignal))]
        public static void CheckForHeartSignal(SignalName signalName)
        {
            string customSignalName = SignalBuilder.GetCustomSignalName(signalName);
            if (customSignalName == "Heart of the Planet")
            {
                LuminaTerra.Instance.LearnHeartSignal();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Campfire), nameof(Campfire.CanSleepHereNow))]
        public static void PreventSleepingDuringEOLS(ref bool __result)
        {
            if (LuminaTerra.Instance.InCorrectSystem && Object.FindObjectOfType<Conductor>().InEndSequence)
            {
                __result = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Campfire), nameof(Campfire.ShouldWakeUp))]
        public static void WakeUpBeforeEOLS(ref bool __result)
        {
            if (LuminaTerra.Instance.InCorrectSystem && Object.FindObjectOfType<Conductor>().InEndSequence)
            {
                __result = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Flashlight), nameof(Flashlight.TurnOn))]
        public static bool DisableFlashlightEOLS()
        {
            return !EndOfLoopController.EnteredSequence;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ToolModeSwapper), nameof(ToolModeSwapper.EquipToolMode))]
        public static bool DisableFlashlightEOLS(ToolMode mode)
        {
            if (EndOfLoopController.EnteredSequence &&
                (mode == ToolMode.SignalScope || mode == ToolMode.Probe))
            {
                return false;
            }

            return true;
        }
    }
}
