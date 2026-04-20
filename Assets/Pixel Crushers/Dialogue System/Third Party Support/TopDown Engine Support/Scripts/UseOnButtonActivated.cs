using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.TopDownEngineSupport
{

    /// <summary>
    /// This is a TopDown ButtonActivated component that invokes OnUse on 
    /// any Dialogue System triggers.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Dialogue System/Third Party/TopDown Engine/Use On Button Activated")]
    public class UseOnButtonActivated : ButtonActivated
    {
        [Tooltip("If Hide Prompt After Use is ticked, allow prompt to appear when re-entering zone.")]
        public bool hidePromptOnlyDuringConversation = true;

        private Character _character;
        private bool _canActivate;

        public override void Initialization()
        {
            _character = null;
            _canActivate = true;
            base.Initialization();
        }

        protected override void TriggerEnter(GameObject collider)
        {
            _character = collider.gameObject.MMGetComponentNoAlloc<Character>();
            base.TriggerEnter(collider);
        }

        protected override void TriggerExit(GameObject collider)
        {
            _character = null;
            base.TriggerExit(collider);
        }

        protected override void ActivateZone()
        {
            if (!_canActivate) return;
            if (DialogueDebug.logInfo) Debug.Log(name + ".ActivateZone: Invoking OnUse on triggers.", this);
            base.ActivateZone();
            var actor = (_character != null) ? _character.transform : GameObject.FindGameObjectWithTag("Player").transform;
            SendMessage("OnUse", actor);
            if (hidePromptOnlyDuringConversation) _promptHiddenForever = false;
        }

        protected virtual void OnConversationEnd(Transform actor)
        {
            // Prevent double-activation if the activation button is the same as the dialogue UI's continue button hotkey.
            StartCoroutine(TemporarilyBlockActivation());
            if (hidePromptOnlyDuringConversation) ShowPrompt();
        }

        protected IEnumerator TemporarilyBlockActivation()
        {
            _canActivate = false;
            yield return null;
            _canActivate = true;
        }
    }
}
