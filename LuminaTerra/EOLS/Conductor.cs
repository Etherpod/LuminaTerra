using System.Collections;
using System.Linq;
using DitzyExtensions.Collection;
using NewHorizons.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LuminaTerra;

public class Conductor : MonoBehaviour
{
    [SerializeField] private EndOfLoopController endOfLoopController = null;
    [SerializeField] private OWAudioSource musicSource = null;
    [SerializeField] private OWAudioSource[] planetAmbience = null;
    [SerializeField] private Transform mainRitualTable = null;

    private bool startedSequence = false;
    // private float debugTimeOffset = 0f;
    private readonly float loopDuration = 1000f;

    public bool InEndSequence => startedSequence;

    private void Awake()
    {
        GlobalMessenger.AddListener("GamePaused", OnGamePaused);
        GlobalMessenger.AddListener("GameUnpaused", OnGameUnpaused);
    }

    private void OnDestroy()
    {
        GlobalMessenger.RemoveListener("GamePaused", OnGamePaused);
        GlobalMessenger.RemoveListener("GameUnpaused", OnGameUnpaused);
    }

    private void Update()
    {
#if DEBUG
        if (Keyboard.current.slashKey.wasPressedThisFrame && !startedSequence)
        {
            startedSequence = true;
            endOfLoopController.StartEOLS();
        }
        if (Keyboard.current.rightShiftKey.wasPressedThisFrame && !startedSequence)
        {
            var table = GameObject.Find("/ShimmeringHeart_Body/Sector/LivingPlanet/RitualChamber/RitualTable");
            var crystals = FindObjectsOfType<CrystalItem>().Where(c => c.gameObject.activeInHierarchy).AsList();
            var lamp = FindObjectOfType<LAMP>();
            var slot1 = table.FindChild("Slot1Detector");
            var slot2 = table.FindChild("Slot2Detector");
            var slot3 = table.FindChild("Slot3Detector");
            var lampSlot = table.FindChild("ItemDetector");
            lamp.transform.parent = lampSlot.transform;
            lamp.transform.localPosition = Vector3.zero;
            lamp.transform.localRotation = Quaternion.identity;
            crystals.ForEach(crystal =>
            {
                crystal.transform.parent = crystal.name switch
                {
                    "RedCrystal_Item" => slot1.transform,
                    "BlueCrystal_Item" => slot2.transform,
                    "PurpleCrystal_Item" => slot3.transform,
                    _ => crystal.transform.parent
                };

                crystal.transform.localPosition = Vector3.zero;
                crystal.transform.localRotation = Quaternion.identity;
                crystal.SetCharged(true, 0);
            });
            var p = Locator.GetPlayerBody();
            p.SetPosition(table.transform.position);
            p.SetRotation(table.transform.rotation);
            p.SetVelocity(table.GetAttachedOWRigidbody().GetVelocity());
        }
#endif
        if (!startedSequence && Time.timeSinceLevelLoad > loopDuration - 32f)
        {
            StartCoroutine(StartEOLS());
            startedSequence = true;
        }
    }

    private IEnumerator StartEOLS()
    {
        FindObjectOfType<HeartSlidesInterface>().DisableProjector();
        Locator.GetAudioMixer().MixEndTimes(5f);
        musicSource.FadeIn(0.5f);
        
        yield return new WaitUntil(() => musicSource.time >= 29f);

        FindObjectOfType<PlayerCameraEffectController>().CloseEyes(3f);
        FindObjectOfType<LAMP>()?.ForceReleaseLights();

        yield return new WaitUntil(() => musicSource.time >= 32f);

        endOfLoopController.StartEOLS();
    }

    public OWAudioSource[] GetPlanetAmbience()
    {
        return planetAmbience;
    }

    public Transform GetMainRitualTable()
    {
        return mainRitualTable;
    }

    public void AssignEOLController(EndOfLoopController controller)
    {
        endOfLoopController = controller;
    }

    public EndOfLoopController GetEOLController()
    {
        return endOfLoopController;
    }

    private void OnGamePaused()
    {
        if (startedSequence && musicSource.isPlaying)
        {
            musicSource.FadeOut(2f);
        }
    }

    private void OnGameUnpaused()
    {
        if (startedSequence && !EndOfLoopController.EnteredSequence)
        {
            musicSource.time = 32f - (loopDuration - Time.timeSinceLevelLoad);
            musicSource.FadeIn(2f);
        }
    }
}