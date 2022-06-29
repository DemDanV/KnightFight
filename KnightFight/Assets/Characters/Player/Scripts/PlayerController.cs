using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float playerRunSpeedMultiplier = 1.5f;
    [SerializeField]
    private float playerBlockSpeedMultiplier = 0.75f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField]
    float staminaRunConsumption = 1f;
    [SerializeField]
    float staminaAttackConsumption = 5f;

    [SerializeField] float animatormationPlayTransition = 0.15f;

    PlayerInput playerInput;

    InputAction moveAction;
    InputAction jumpAction;
    InputAction runAction;
    InputAction crouchAction;
    InputAction attackAction;
    InputAction blockAction;


    Transform cameraTransform;
    [SerializeField] float rotationSpeed = 1f;

    [SerializeField] SwordController swordController;
    [SerializeField] float weaponDamage;



    Animator animator;
    int jumpAnimationPatameterID;
    int moveXAnimationPatameterID;
    int moveZAnimationPatameterID;

    int upperAttackAnimationPatameterID;
    int flyingAnimationID;

    int regularAttackAnimationID;
    int blockActionAnimationID;
    int blockAnimationParameterID;

    int parryingAnimationID;

    int hitAnimationID;

    int dieAnimationID;


    Vector2 currentAnimationBlendVector;
    Vector2 animationVelocity;
    [SerializeField] float AnimationSmootTime = 0.1f;

    CharacterCharacteristics characterCharacteristics;

    [SerializeField] List<Transform> posForEnemyToCheckSee;


    public delegate void OnDead();

    public OnDead OnPlayerDead;

    public List<Transform> PosForEnemyToCheckSee { get { return posForEnemyToCheckSee; } }

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Movement"];
        jumpAction = playerInput.actions["Jump"];
        runAction = playerInput.actions["Run"];
        crouchAction = playerInput.actions["Crouch"];
        attackAction = playerInput.actions["Attack"];
        blockAction = playerInput.actions["Block"];


        animator = GetComponentInChildren<Animator>();
        jumpAnimationPatameterID = Animator.StringToHash("Jump");
        moveXAnimationPatameterID = Animator.StringToHash("MoveX");
        moveZAnimationPatameterID = Animator.StringToHash("MoveZ");
        blockAnimationParameterID = Animator.StringToHash("Block");


        regularAttackAnimationID = Animator.StringToHash("Regular Attack");
        //upperAttackAnimationPatameterID = Animator.StringToHash("Push");
        flyingAnimationID = Animator.StringToHash("Flying");
        blockActionAnimationID = Animator.StringToHash("Block Action");
        parryingAnimationID = Animator.StringToHash("Parrying");
        hitAnimationID = Animator.StringToHash("Impact");
        dieAnimationID = Animator.StringToHash("Die");


        characterCharacteristics = GetComponent<CharacterCharacteristics>();
        //characterCharacteristics.OnZeroHealth += Die;
        characterCharacteristics.OnZeroStamina += SlowDown;
        characterCharacteristics.OnStaminaRecovery += SpeedUp;
    }

    private void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();

        cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        attackAction.performed += x => Attack();
        blockAction.performed += x => Block();
        blockAction.canceled += x => Unblock();
    }

    private void OnDisable()
    {
        attackAction.performed -= x => Attack();
        blockAction.performed -= x => Block();
        blockAction.canceled -= x => Unblock();
    }

    bool attacking = false;
    private void Attack()
    {
        if(attacking == false)
        {
            if(characterCharacteristics.RemoveStamina(staminaAttackConsumption))
            {
                attacking = true;
                blockSet = false;
                swordController.Attack(weaponDamage);

                animator.CrossFade(regularAttackAnimationID, animatormationPlayTransition);

                StartCoroutine(AttackCooldown());
            }
        }
    }

    IEnumerator AttackCooldown()
    {
        float timeLeft = 2f;
        while(timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        swordController.StopAttack();
        attacking = false;
    }

    [SerializeField] Vector3 parryingTransform;
    [SerializeField] Vector3 parryingHalfSize;


    [SerializeField] LayerMask EnemyLayer;

    Vector3 ParryingTriggerPos;
    bool blockSet = false;
    private void Block()
    {
        if (blockSet)
            return;

        ParryingTriggerPos = transform.position + new Vector3(parryingTransform.x * transform.forward.x, parryingTransform.y * transform.forward.y, parryingTransform.z * transform.forward.z);
        Collider[] colliders = Physics.OverlapBox(ParryingTriggerPos, parryingHalfSize, Quaternion.identity, EnemyLayer);
        if (colliders.Length != 0)
        {
            Debug.Log("Parrying");
            animator.CrossFade(parryingAnimationID, animatormationPlayTransition);
        }
        else
        {
            Debug.Log("Block");
            blockSet = true;
            animator.SetBool(blockAnimationParameterID, true);
            animator.CrossFade(blockActionAnimationID, animatormationPlayTransition);

        }
    }

    private void Unblock()
    {
        blockSet = false;
        animator.SetBool(blockAnimationParameterID, false);
    }

    public void GetHit(float damage)
    {
        if(blockSet == true)
        {
            characterCharacteristics.RemoveStamina(damage);
            if(characterCharacteristics.stamina <= 0)
            {
                animator.CrossFade(hitAnimationID, animatormationPlayTransition);
                StartCoroutine(BlockInput(1));
            }
        }
        else
        {
            characterCharacteristics.RemoveHealth(damage);
            if (characterCharacteristics.health <= 0)
            {
                Die();
            }
            else
            {
                animator.CrossFade(hitAnimationID, animatormationPlayTransition);
                StartCoroutine(BlockInput(1));
            }
        }
    }

    private void Die()
    {
        OnPlayerDead?.Invoke();
        StopAllCoroutines();
        characterCharacteristics.enabled = false;
        animator.CrossFade(dieAnimationID, animatormationPlayTransition);
        playerInput.DeactivateInput();
    }

    bool tired = false;
    private void SlowDown()
    {
        tired = true;
        animator.speed *= 0.5f;
    }

    private void SpeedUp()
    {
        tired = false;
        animator.speed *= 2;
    }
    IEnumerator BlockInput(float seconds)
    {
        playerInput.DeactivateInput();
        yield return new WaitForSeconds(seconds);
        playerInput.ActivateInput();
    }

    private void Update()
    {
        if (groundedPlayer != controller.isGrounded)
        {
            groundedPlayer = controller.isGrounded;

            if (controller.isGrounded == true)
            {
                animator.SetBool(flyingAnimationID, false);
            }
        }

        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        float movementSpeed = playerSpeed;

        if (blockSet)
        {
            movementSpeed *= playerBlockSpeedMultiplier;
        }

        if(tired)
        {
            movementSpeed *= 0.5f;
        }


        Vector2 dir = moveAction.ReadValue<Vector2>();

        if(dir!= Vector2.zero)
            if (runAction.phase == InputActionPhase.Performed)
            {
                if (characterCharacteristics.RemoveStamina(staminaRunConsumption))
                {
                    Debug.Log("Run");
                    movementSpeed *= playerRunSpeedMultiplier;
                }
            }

        Vector3 moveDir = new Vector3(dir.x, 0, dir.y);

        currentAnimationBlendVector = Vector2.SmoothDamp(currentAnimationBlendVector, new Vector2(moveDir.x, moveDir.z), ref animationVelocity, AnimationSmootTime);
        animator.SetFloat(moveXAnimationPatameterID, currentAnimationBlendVector.x);
        animator.SetFloat(moveZAnimationPatameterID, currentAnimationBlendVector.y);

        moveDir = moveDir.x * cameraTransform.right.normalized + moveDir.z * cameraTransform.forward.normalized;
        moveDir.y = 0;
        controller.Move(moveDir * Time.deltaTime * movementSpeed);

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        if (playerInput.inputIsActive)
        {
            Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(ParryingTriggerPos, parryingHalfSize * 2);
    }
}
