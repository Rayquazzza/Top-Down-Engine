using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

public class SmoothAnimController : MonoBehaviour
{
    public float AccelerationSmooth = 0.05f; // Très rapide pour le démarrage
    public float DecelerationSmooth = 0.2f;  // Plus lent pour l'arrêt fluid

    protected CharacterMovement _movement;
    protected Animator _animator;
    protected Character _character;

    private float _currentVisualSpeed;
    private float _velocityRef;

    void Start()
    {
        _character = GetComponent<Character>();

        _movement = _character.FindAbility<CharacterMovement>();
        _animator = _character._animator;
    }

    void Update()
    {
        if (_movement == null || _animator == null) return;

        float targetSpeed = 0f;

        if (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Normal)
        {
            if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Walking
                || _character.MovementState.CurrentState == CharacterStates.MovementStates.Running)
            {
                targetSpeed = _movement.MovementSpeed;
            }
        }

        float currentSmooth = (targetSpeed > _currentVisualSpeed) ? AccelerationSmooth : DecelerationSmooth;

        _currentVisualSpeed = Mathf.SmoothDamp(_currentVisualSpeed, targetSpeed, ref _velocityRef, currentSmooth);

        _animator.SetFloat("VisualSpeed", _currentVisualSpeed);
    }
}