using System;
using UnityEngine;

namespace LuminaTerra;

public class MysteriousDoorController : MonoBehaviour
{
    [SerializeField] private InteractReceiver _interactReceiver;
    [SerializeField] private OWAudioSource _loopingAudio;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _interactReceiver.OnPressInteract += OnPressInteract;
    }

    private void Start()
    {
        _interactReceiver.ChangePrompt("Open Door");
    }

    private void OnPressInteract()
    {
        _animator.SetTrigger("Open");
        _interactReceiver.DisableInteraction();
    }

    public void OnDoorOpen()
    {
        _loopingAudio.FadeIn(2f, true, true);
    }

    private void OnDestroy()
    {
        _interactReceiver.OnPressInteract -= OnPressInteract;
    }
}
