using System.Collections;
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

    /*private void Awake()
    {
        trigger.OnEntry += TriggerEntered;
    }

    private void OnDestroy()
    {
        trigger.OnEntry -= TriggerEntered;
    }*/

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
        if (Keyboard.current.slashKey.wasPressedThisFrame && !startedSequence)
        {
            LuminaTerra.Instance.ModHelper.Console.WriteLine("start");
            StartCoroutine(StartEOLS());
            startedSequence = true;
        }
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

    /*private void TriggerEntered(GameObject hitObj)
    {
        if (!hitObj.CompareTag("PlayerDetector")) return;

        endOfLoopController.StartEOLS();
    }*/
}