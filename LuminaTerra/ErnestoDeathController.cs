using UnityEngine;

namespace LuminaTerra;

public class ErnestoDeathController : MonoBehaviour
{
    [SerializeField] GameObject _activateParent = null;

    private CharacterDialogueTree _dialogue;

    private void Awake()
    {
        _dialogue = GetComponent<CharacterDialogueTree>();
    }

    private void Start()
    {
        _dialogue.OnEndConversation += OnEndConversation;

        if (PlayerData.GetPersistentCondition("LT_ERNESTO_DEATH"))
        {
            _activateParent.SetActive(false);
        }
    }

    private void OnEndConversation()
    {
        if (PlayerData.GetPersistentCondition("LT_ERNESTO_DEATH"))
        {
            _activateParent.SetActive(false);
        }
        else
        {
            _dialogue._interactVolume.DisableInteraction();
        }
    }

    private void OnDestroy()
    {
        _dialogue.OnEndConversation -= OnEndConversation;
    }
}
