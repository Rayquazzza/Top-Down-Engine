using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace PixelCrushers.TopDownEngineSupport
{

    /// <summary>
    /// This subclass of MMAdditiveSceneLoadingManager integrates with the 
    /// Pixel Crushers Save System.
    /// </summary>
    public class PixelCrushersMMAdditiveSceneLoadingManager : MMAdditiveSceneLoadingManager
    {

        protected virtual void Start()
        {
            OnExitFade.AddListener(ApplyDataToAdditivelyLoadedScene);
        }

        /// <summary>
        /// After additively loading scene, apply data to its savers.
        /// </summary>
        protected virtual void ApplyDataToAdditivelyLoadedScene()
        {
            var scene = SceneManager.GetSceneByName(_sceneToLoadName);
            if (!scene.IsValid()) return;
            var rootGOs = scene.GetRootGameObjects();
            for (int i = 0; i < rootGOs.Length; i++)
            {
                PixelCrushers.SaveSystem.RecursivelyApplySavers(rootGOs[i].transform);
            }
        }

        /// <summary>
        /// Record savers in outgoing scenes before base method unloads them.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator UnloadOriginScenes()
        {
            foreach (Scene scene in _initialScenes)
            {
                var rootGOs = scene.GetRootGameObjects();
                for (int i = 0; i < rootGOs.Length; i++)
                {
                    var rootGO = rootGOs[i].transform;
                    PixelCrushers.SaveSystem.RecursivelyRecordSavers(rootGO, scene.buildIndex);
                    PixelCrushers.SaveSystem.RecursivelyInformBeforeSceneChange(rootGO);
                }
            }
            return base.UnloadOriginScenes();
        }

    }

}
