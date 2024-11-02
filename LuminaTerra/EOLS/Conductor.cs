using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LuminaTerra;

public class Conductor : MonoBehaviour
{
    [SerializeField] private EndOfLoopController endOfLoopController = null;
    [SerializeField] private OWAudioSource musicSource = null;
    [SerializeField] private OWTriggerVolume trigger = null;
    [SerializeField] private OWAudioSource[] planetAmbience = null;
    [SerializeField] private Transform mainRitualTable = null;

    private bool startedSequence = false;

    /*private void Awake()
    {
        trigger.OnEntry += TriggerEntered;
    }

    private void OnDestroy()
    {
        trigger.OnEntry -= TriggerEntered;
    }*/

    private void Update()
    {
/*        if (Keyboard.current.slashKey.wasPressedThisFrame && !startedSequence)
        {
            LuminaTerra.Instance.ModHelper.Console.WriteLine("start");
            StartCoroutine(StartEOLS());
            startedSequence = true;
        }*/
        if (!startedSequence && Time.timeSinceLevelLoad > 1000f - 32f)
        {
            StartCoroutine(StartEOLS());
            startedSequence = true;
        }
    }

    private IEnumerator StartEOLS()
    {
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

    private void TriggerEntered(GameObject hitObj)
    {
        if (!hitObj.CompareTag("PlayerDetector")) return;

        endOfLoopController.StartEOLS();
    }
}