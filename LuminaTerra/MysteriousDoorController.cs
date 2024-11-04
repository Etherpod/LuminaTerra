using LuminaTerra.Util;
using System;
using System.Collections;
using UnityEngine;


namespace LuminaTerra;

public class MysteriousDoorController : MonoBehaviour
{
    private static readonly int StarGlow = Shader.PropertyToID("_StarGlow");
    
    [SerializeField] private InteractReceiver _interactReceiver = null;
    [SerializeField] private OWAudioSource _loopingAudio = null;
    [SerializeField] private OWAudioSource _oneShotAudio = null;
    [SerializeField] private Transform _lookTransform = null;
    [SerializeField] private GameObject _nextRoomParent = null;
    [SerializeField] private ParticleSystem _stars = null;
    [SerializeField] private MeshRenderer _doorRenderer = null;

    private Animator _animator;
    private PlayerLockOnTargeting _lockOnTargeting;
    private Fader _fader = new();

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _lockOnTargeting = Locator.GetPlayerTransform().GetComponent<PlayerLockOnTargeting>();
        _interactReceiver.OnPressInteract += OnPressInteract;
        _interactReceiver.ChangePrompt("Open Door");
        _nextRoomParent.SetActive(false);
        enabled = false;
    }

    private void Update()
    {
        _doorRenderer.material.SetFloat(StarGlow, _fader.Value);

        if (!_fader.IsFading)
        {
            enabled = false;
        }
    }

    private void OnPressInteract()
    {
        _animator.SetTrigger("Open");
        _interactReceiver.DisableInteraction();

        OWInput.ChangeInputMode(InputMode.None);
        Locator.GetPauseCommandListener().AddPauseCommandLock();
        _lockOnTargeting.LockOn(_lookTransform, 1f, false, 1f);
    }

    public void OnDoorOpen()
    {
        //_loopingAudio.FadeIn(2f, true, true);
        _nextRoomParent.SetActive(true);
        _oneShotAudio.pitch = 0.8f;
        _oneShotAudio.PlayOneShot(AudioType.Door_OpenStart);
        _fader.StartFade(_doorRenderer.material.GetFloat("_StarGlow"), 0f, 3f);
        enabled = true;
    }

    public void OnDoorComplete()
    {
        _oneShotAudio.PlayOneShot(AudioType.Door_OpenStop);
        StartCoroutine(SuckDelay());
    }

    private IEnumerator SuckDelay()
    {
        yield return new WaitForSeconds(2f);
        _stars.Play();
        _loopingAudio.FadeIn(1f, true, true);
        yield return new WaitForSeconds(2f);
        Locator.GetPlayerBody().AddImpulse(Locator.GetPlayerTransform().forward * 0.05f);
        OWInput.ChangeInputMode(InputMode.Character);
        Locator.GetPauseCommandListener().RemovePauseCommandLock();
        _lockOnTargeting.BreakLock();
        yield return new WaitForSeconds(1f);
        _animator.SetTrigger("Close");
    }

    private void OnDoorCloseStart()
    {
        _oneShotAudio.PlayOneShot(AudioType.Door_CloseStart);
        _loopingAudio.FadeOut(1f);
    }

    private void OnDoorClose()
    {
        _oneShotAudio.PlayOneShot(AudioType.Door_CloseStop);
    }

    private void OnDestroy()
    {
        _interactReceiver.OnPressInteract -= OnPressInteract;
    }
}
