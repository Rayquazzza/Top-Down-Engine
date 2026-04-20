using UnityEngine;
using UnityEngine.Events;
using MoreMountains.TopDownEngine;

namespace PixelCrushers.TopDownEngineSupport
{

    /// <summary>
    /// Provides UnityEvents to call OnDeath and OnRevive.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class OnDeathEvent : MonoBehaviour
    {

        public UnityEvent OnDeath = new UnityEvent();
        public UnityEvent OnRevive = new UnityEvent();

        private void OnEnable()
        {
            GetComponent<Health>().OnDeath += InvokeOnDeathEvent;
            GetComponent<Health>().OnRevive += InvokeOnReviveEvent;
        }

        private void OnDisable()
        {
            GetComponent<Health>().OnDeath -= InvokeOnDeathEvent;
            GetComponent<Health>().OnRevive -= InvokeOnReviveEvent;
        }

        private void InvokeOnDeathEvent()
        {
            OnDeath.Invoke();
        }

        private void InvokeOnReviveEvent()
        {
            OnRevive.Invoke();
        }

    }
}