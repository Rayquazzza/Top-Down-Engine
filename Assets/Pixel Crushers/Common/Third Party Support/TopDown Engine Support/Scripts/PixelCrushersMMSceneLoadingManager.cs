using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace PixelCrushers.TopDownEngineSupport
{

    /// <summary>
    /// This subclass of MMSceneLoadingManager tells the Pixel Crushers save system
    /// to save the outgoing scene's state before changing scenes, and then applies
    /// saved state to the newly-loaded scene.
    /// </summary>
    public class PixelCrushersMMSceneLoadingManager : MMSceneLoadingManager
    {

        protected override IEnumerator LoadAsynchronously()
        {

            // we setup our various visual elements
            LoadingSetup();

            // we fade from black
            MMFadeOutEvent.Trigger(StartFadeDuration, _tween);
            yield return new WaitForSeconds(StartFadeDuration);

            // we start loading the scene
            _asyncOperation = SceneManager.LoadSceneAsync(_sceneToLoad, LoadSceneMode.Single);
            _asyncOperation.allowSceneActivation = false;

            // while the scene loads, we assign its progress to a target that we'll use to fill the progress bar smoothly
            while (_asyncOperation.isDone) // Changed to isDone
            {
                _fillTarget = _asyncOperation.progress;
                yield return null;
            }
            // when the load is close to the end (it'll never reach it), we set it to 100%
            _fillTarget = 1f;

            // we wait for the bar to be visually filled to continue
            while (_progressBarImage.fillAmount != _fillTarget)
            {
                yield return null;
            }

            // the load is now complete, we replace the bar with the complete animation
            LoadingComplete();
            yield return new WaitForSeconds(LoadCompleteDelay);

            // we fade to black
            MMFadeInEvent.Trigger(ExitFadeDuration, _tween);
            yield return new WaitForSeconds(ExitFadeDuration);

            // [PixelCrushers] Wait for scene to completely load:
            PixelCrushers.SaveSystem.ApplySavedGameData();

            // we switch to the new scene
            _asyncOperation.allowSceneActivation = true;
            LoadingSceneEvent.Trigger(_sceneToLoad, LoadingStatus.LoadTransitionComplete);

        }
    }
}