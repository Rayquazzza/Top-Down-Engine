using UnityEngine;
using MoreMountains.TopDownEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Ledge Grab 3D")]
    public class CharacterLedgeGrab3D : CharacterAbility
    {
        [Header("Ledge Detection Settings")]
        public Transform ledgeAnchor; // L'objet vide sur ton perso (mains/poitrine)
        public float ledgeCheckDistance = 0.5f;
        public LayerMask ledgeLayer;
        public Vector2 highCheckOffset = new Vector2(0f, 0.2f); // Hauteur du check de vide

        [Header("Animation Parameters")]
        protected const string _ledgeGrabbingAnimationParameterName = "LedgeGrabbing";
        protected int _ledgeGrabbingAnimationParameter;

        protected bool _isGrabbingLedge = false;

        protected override void Initialization()
        {
            base.Initialization();
            _isGrabbingLedge = false;
        }

        protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter(_ledgeGrabbingAnimationParameterName, AnimatorControllerParameterType.Bool, out _ledgeGrabbingAnimationParameter);
        }

        protected override void HandleInput()
        {
            base.HandleInput();
            // Si on appuie sur Sauter alors qu'on est accroché, on grimpe
            if (_inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonDown && _isGrabbingLedge)
            {
                ClimbLedge();
            }
        }

        public override void ProcessAbility()
        {
            base.ProcessAbility();
            if (!AbilityAuthorized) return;

            // On ne cherche le rebord que si on est en train de tomber ou de sauter
            if (!_isGrabbingLedge && (/*_movement.CurrentState == CharacterStates.MovementStates.Jumping ||*/ _movement.CurrentState == CharacterStates.MovementStates.Falling))
            {
                DetectLedge();
            }

            if (_isGrabbingLedge)
            {
                _controller.SetMovement(Vector3.zero);
                _controller.GravityActive = false; // Sécurité : on force la gravité à off
            }
        }

        protected virtual void DetectLedge()
        {
            if (IsNearLedge())
            {
                StartGrabbing();
            }
        }

        protected virtual void StartGrabbing()
        {
            _isGrabbingLedge = true;
            _controller.GravityActive = false;

            // On replace le perso exactement sur le rebord (Optionnel mais recommandé)
            Vector3 targetPos = GetLedgePosition();
            // Tu peux ajuster ici avec un offset pour que les mains touchent pile le bord
            // this.transform.position = targetPos + (transform.up * -1.5f); 

            _movement.ChangeState(CharacterStates.MovementStates.Idle);

            PlayAbilityStartSfx();
            PlayAbilityStartFeedbacks();
        }

        protected virtual void ClimbLedge()
        {
            _isGrabbingLedge = false;
            _controller.GravityActive = true;

            // Ici tu pourrais ajouter une petite propulsion vers le haut/avant
            // _controller.AddForce(transform.up * 10f + transform.forward * 5f);

            _movement.ChangeState(CharacterStates.MovementStates.Idle);

            PlayAbilityStopSfx();
            PlayAbilityStopFeedbacks();
        }

        public bool IsNearLedge()
        {
            if (ledgeAnchor == null) return false;

            // On récupère la direction vers laquelle le perso fait face
            Vector3 detectionDirection = transform.forward;

            // Rayon bas (Mur)
            RaycastHit wallHit;
            bool hitWall = Physics.Raycast(ledgeAnchor.position, detectionDirection, out wallHit, ledgeCheckDistance, ledgeLayer);
            if (!hitWall) return false;

            // Rayon haut (Vide)
            Vector3 highOrigin = ledgeAnchor.position + Vector3.up * highCheckOffset.y;
            RaycastHit topHit;
            bool hitTop = Physics.Raycast(highOrigin, detectionDirection, out topHit, ledgeCheckDistance, ledgeLayer);

            return !hitTop;
        }

        public Vector3 GetLedgePosition()
        {
            RaycastHit hit;
            if (Physics.Raycast(ledgeAnchor.position, transform.forward, out hit, ledgeCheckDistance, ledgeLayer))
            {
                return new Vector3(hit.point.x, hit.collider.bounds.max.y, hit.point.z);
            }
            return transform.position;
        }

        public override void UpdateAnimator()
        {
            MMAnimatorExtensions.UpdateAnimatorBool(_animator, _ledgeGrabbingAnimationParameter, _isGrabbingLedge, _character._animatorParameters, _character.RunAnimatorSanityChecks);
        }

        private void OnDrawGizmos()
        {
            if (ledgeAnchor == null) return;

            // IMPORTANT : On utilise le forward du transform
            Vector3 faceDir = transform.forward;

            // Debug Direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(ledgeAnchor.position, faceDir * ledgeCheckDistance);

            // Check Mur
            RaycastHit hit;
            bool wall = Physics.Raycast(ledgeAnchor.position, faceDir, out hit, ledgeCheckDistance, ledgeLayer);
            Gizmos.color = wall ? Color.green : Color.red;
            Gizmos.DrawWireSphere(ledgeAnchor.position, 0.05f);

            // Check Vide
            Vector3 highOrigin = ledgeAnchor.position + Vector3.up * highCheckOffset.y;
            bool top = Physics.Raycast(highOrigin, faceDir, out hit, ledgeCheckDistance, ledgeLayer);
            Gizmos.color = top ? Color.red : Color.green;
            Gizmos.DrawRay(highOrigin, faceDir * ledgeCheckDistance);

            // Point final (Ledge)
            if (wall && !top)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(GetLedgePosition(), 0.1f);
            }
        }
    }
}