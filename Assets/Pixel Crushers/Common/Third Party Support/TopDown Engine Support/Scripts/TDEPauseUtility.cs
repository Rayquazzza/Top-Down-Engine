using MoreMountains.InventoryEngine;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PixelCrushers.TopDownEngineSupport
{

    /// <summary>
    /// Pause/disable input utility methods for TDE.
    /// </summary>
    public static class TDEPauseUtility
    {

        private static int pauseDepth = 0;
        private static bool prevSendNavEvents = false;
        private static Dictionary<InventorySoundPlayer, bool> inventorySoundPlayers = new Dictionary<InventorySoundPlayer, bool>();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void InitStaticVariables()
        {
            pauseDepth = 0;
            prevSendNavEvents = false;
            inventorySoundPlayers = new Dictionary<InventorySoundPlayer, bool>();
        }
#endif

        /// <summary>
        /// Pauses the game or disables player input.
        /// </summary>
        /// <param name="pause">If true, pauses the game using TDE's pause method. Takes precedence over disableInput.</param>
        /// <param name="disableInput">If true, stop the player and disables player input.</param>
        /// <param name="floatAnimatorParametersToStop">Optional player animator parameters to set to zero.</param>
        /// <param name="boolAnimatorParametersToStop">Optional player animator parameters to set to false.</param>
        public static void Pause(bool pause, bool disableInput,
            string[] floatAnimatorParametersToStop,
            string[] boolAnimatorParametersToStop)
        {
            // In case we get multiple requests to pause before unpause, only unpause after last call to Unpause:
            if (pauseDepth == 0)
            {
                if (pause)
                {
                    GameManager.Instance.Pause(PauseMethods.NoPauseMenu, false);
                }
                if (disableInput)
                {
                    prevSendNavEvents = EventSystem.current.sendNavigationEvents;
                }
            }
            pauseDepth++;
            if (disableInput && !pause)
            {
                SetTopDownInput(false);
                SetPlayerControl(false, floatAnimatorParametersToStop, boolAnimatorParametersToStop);
            }
            EventSystem.current.sendNavigationEvents = true;
            EventSystem.current.SetSelectedGameObject(null);
        }

        public static void Unpause(bool pause, bool disableInput,
            string[] floatAnimatorParametersToStop,
            string[] boolAnimatorParametersToStop)
        {
            pauseDepth--;
            if (pauseDepth == 0)
            {
                GameManager.Instance.StartCoroutine(UnpauseAtEndOfFrame(pause, disableInput, floatAnimatorParametersToStop, boolAnimatorParametersToStop));
            }
        }

        private static IEnumerator UnpauseAtEndOfFrame(bool pause, bool disableInput,
            string[] floatAnimatorParametersToStop,
            string[] boolAnimatorParametersToStop)
        {
            yield return new WaitForEndOfFrame();
            if (pause)
            {
                GameManager.Instance.UnPause(PauseMethods.NoPauseMenu);
            }
            if (disableInput)
            {
                SetTopDownInput(true);
                EventSystem.current.sendNavigationEvents = prevSendNavEvents;
            }
            SetPlayerControl(true, floatAnimatorParametersToStop, boolAnimatorParametersToStop);
        }

        private static void SetTopDownInput(bool value)
        {
            SetAllInputManagers(value);
        }

        private static void SetAllInputManagers(bool value)
        {
            // Enable/disable the TDE input managers:
            foreach (var inputManager in GameObjectUtility.FindObjectsByType<InputManager>())
            {
                inputManager.InputDetectionActive = value;
            }
            // Enable/disable the Inventory Engine input managers:
            foreach (var inputManager in GameObjectUtility.FindObjectsByType<InventoryInputManager>())
            {
                inputManager.enabled = value;
            }
            // Disable inventory sounds (temp. fix while getting fix from Renaud):
            if (value == false)
            {
                inventorySoundPlayers.Clear();
                foreach (var inventorySoundPlayer in GameObjectUtility.FindObjectsByType<InventorySoundPlayer>())
                {
                    inventorySoundPlayers[inventorySoundPlayer] = inventorySoundPlayer.enabled;
                    inventorySoundPlayer.enabled = false;
                }
            }
            else
            {
                foreach (var kvp in inventorySoundPlayers)
                {
                    if (kvp.Key == null) continue;
                    kvp.Key.enabled = kvp.Value;
                }
            }
        }

        private static void SetPlayerControl(bool value, string[] floatAnimatorParametersToStop, string[] boolAnimatorParametersToStop)
        {
            // Freeze or unfreeze characters, including stopping movements, shooting, and walk particles.
            if (value)
            {
                LevelManager.Instance.UnFreezeCharacters();
            }
            else
            {
                LevelManager.Instance.FreezeCharacters();
                GameManager.Instance.StartCoroutine(StopAnimators(floatAnimatorParametersToStop, boolAnimatorParametersToStop));
            }
            foreach (Character player in LevelManager.Instance.Players)
            {
                player.LinkedInputManager.RunButton.TriggerButtonUp();
                var characterRun = player.GetComponent<CharacterRun>();
                if (characterRun != null) characterRun.RunStop();
                player.GetComponent<CharacterMovement>().ResetSpeed();
                player.MovementState.ChangeState(CharacterStates.MovementStates.Idle);

                var characterMovement = player.GetComponent<CharacterMovement>();
                if (characterMovement != null)
                {
                    characterMovement.PermitAbility(value);
                    characterMovement.MovementForbidden = !value;
                    if (value == false)
                    {
                        characterMovement.PlayAbilityStopSfx();
                        characterMovement.PlayAbilityStopFeedbacks();
                        characterMovement.StopAbilityUsedSfx();
                    }
                }
                foreach (CharacterHandleWeapon characterHandleWeapon in player.GetComponents<CharacterHandleWeapon>())
                {
                    characterHandleWeapon.ShootStop();
                }
            }
        }

        private static IEnumerator StopAnimators
            (string[] floatAnimatorParametersToStop,
            string[] boolAnimatorParametersToStop)
        {
            yield return null;
            foreach (Character player in LevelManager.Instance.Players)
            {
                var animator = player.GetComponent<Character>()._animator;
                foreach (var floatParameter in floatAnimatorParametersToStop)
                {
                    animator.SetFloat(floatParameter, 0);
                }
                foreach (var boolParameter in boolAnimatorParametersToStop)
                {
                    animator.SetBool(boolParameter, false);
                }
                foreach (var ps in player.GetComponent<CharacterMovement>().WalkParticles)
                {
                    ps.Stop();
                }
            }
        }
    }
}
