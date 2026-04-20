using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.TopDownEngineSupport
{

    /// <summary>
    /// Add this script to the Dialogue Manager if you allow the player to open
    /// the Inventory Engine UI during conversations. It re-enables UI input after
    /// Inventory Engine disables it when closing the UI.
    /// </summary>
    public class AllowUINavigationDuringConversations : MonoBehaviour
    {
        void Start()
        {
            DialogueManager.instance.conversationStarted += (actor) => { enabled = true; };
            DialogueManager.instance.conversationEnded += (actor) => { enabled = false; };
            enabled = false;
        }

        void Update()
        {
            UnityEngine.EventSystems.EventSystem.current.sendNavigationEvents = true;
        }
    }
}