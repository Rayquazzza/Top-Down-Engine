using UnityEngine;
using PixelCrushers.TopDownEngineSupport;

namespace PixelCrushers.DialogueSystem.TopDownEngineSupport
{

    /// <summary>
    /// Pauses TopDown and/or disables player input during conversations.
    /// If you add it to the Dialogue Manager, it will affect all conversations.
    /// If you add it to a player, it will only affect conversations that the
    /// player is involved in.
    /// 
    /// You can also add a copy of this component to a quest log window. 
    /// Configure OnOpen() to call Pause and OnClose() to call Unpause.
    /// Untick the quest log window's Pause While Open and Unlock Cursor While
    /// Open checkboxes since this script will handle it.
    /// 
    /// You can add an MMCursorVisible component to the dialogue UI's 
    /// Dialogue Panel or quest log window's Main Panel to show the cursor
    /// while open.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Dialogue System/Third Party/TopDown Engine/Pause TopDown During Conversations")]
    public class PauseTopDownDuringConversations : MonoBehaviour
    {
        [Tooltip("Tell Topdown Engine to pause during conversations.")]
        public bool pauseDuringConversations = true;

        [Tooltip("Disable TopDown player input during conversations.")]
        public bool disableInputDuringConversations = true;

        public string[] floatAnimatorParametersToStop = new string[] { "Speed" };
        public string[] boolAnimatorParametersToStop = new string[] { "Walking", "Running", "Jumping" };

        protected virtual void OnConversationStart(Transform actor)
        {
            Pause();
        }

        private void OnConversationEnd(Transform actor)
        {
            Unpause();
        }

        public virtual void Pause()
        {
            TDEPauseUtility.Pause(pauseDuringConversations, disableInputDuringConversations,
                floatAnimatorParametersToStop, boolAnimatorParametersToStop);
        }

        public virtual void Unpause()
        {
            TDEPauseUtility.Unpause(pauseDuringConversations, disableInputDuringConversations,
                floatAnimatorParametersToStop, boolAnimatorParametersToStop);
        }

    }
}
