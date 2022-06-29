using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(CharacterCharacteristics))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] CharacterCharacteristics characterCharacteristics;

    [SerializeField] Transform leftEye;
    [SerializeField] Transform rightEye;


    [SerializeField] float maxSeeingDistance;
    [SerializeField] float hitDistance;
    [Range(70, 180), SerializeField] float POV = 70;
    [SerializeField] float wanderRadius = 10;
    [SerializeField] float idleMinDuration = 5;
    [SerializeField] float idleMaxDuration = 15;
    [SerializeField] float normalSpeed = 2;
    [SerializeField] float approachingSpeed = 3.5f;

    Coroutine lstWork;

    [SerializeField] Animator animator;
    [SerializeField] float animatormationPlayTransition = 0.15f;
    [SerializeField] float detHitCooldownSeconds;
    int walkAnimationID;
    int runAnimationID;

    int defendAnimationID;
    int attackAnimationID;
    int getHitAnimationID;
    int dieAnimationID;
    int idleAnimationID;


    int walkAnimationParameterID;
    int stopAnimationParameterID;

    [SerializeField] SwordController sword;
    [SerializeField] float swordDamage;




    public void Start()
    {
        walkAnimationID = Animator.StringToHash("Walk");
        runAnimationID = Animator.StringToHash("Run");

        defendAnimationID = Animator.StringToHash("Defend");
        attackAnimationID = Animator.StringToHash("Attack");
        getHitAnimationID = Animator.StringToHash("GetHit");
        dieAnimationID = Animator.StringToHash("Die");
        idleAnimationID = Animator.StringToHash("Idle");

        walkAnimationParameterID = Animator.StringToHash("Walking");
        stopAnimationParameterID = Animator.StringToHash("Stop");


        StartCoroutine(See());

        GameManager.instance.Player.OnPlayerDead += Clear;
    }

    private void Clear()
    {
        canSeePlayer = false;
        canAttackPlayer = false;
    }

    bool canSeePlayer = false;
    bool canAttackPlayer = false;
    IEnumerator See()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            if (GameManager.instance.Player == null)
            {
                yield return new WaitForSeconds(5);
                continue;
            }


            Vector3 playerPos = GameManager.instance.Player.transform.position;
            float distanceToPlayer = (playerPos - transform.position).magnitude;


            //Look
            Quaternion targetRotToBeForwardPlayer = Quaternion.LookRotation(transform.position -
                GameManager.instance.Player.transform.position, GameManager.instance.Player.transform.TransformDirection(Vector3.up));
            float angleBtwPlayerAndNPC = Quaternion.Angle(transform.rotation, targetRotToBeForwardPlayer);

            canSeePlayer = false;

            if (angleBtwPlayerAndNPC > 180 - POV)
            {
                //Trying left Eye
                Vector3 startRayPos = leftEye.position;
                Ray ray;

                RaycastHit hit;

                List<Transform> posToCheck = GameManager.instance.Player.PosForEnemyToCheckSee;

                for (int i = 0; i < 2; i++)
                {
                    foreach (Transform checkPos in posToCheck)
                    {
                        ray = new Ray(startRayPos, checkPos.position - startRayPos);


                        Physics.Raycast(ray, out hit, maxSeeingDistance);
                        if (hit.collider != null)
                        {
                            if (hit.transform.CompareTag("Player"))
                            {
                                Debug.DrawRay(startRayPos, checkPos.position - startRayPos, Color.red, 1);
                                canSeePlayer = true;
                                Think();
                                break;
                            }
                            else
                            {
                                Debug.DrawRay(startRayPos, checkPos.position - startRayPos, Color.blue, 1);
                            }
                        }
                    }

                    if (canSeePlayer)
                        break;

                    //Trying right Eye;
                    startRayPos = rightEye.position;
                }
            }


            //If too close
            if (distanceToPlayer < hitDistance)
            {
                canSeePlayer = true;
                canAttackPlayer = true;
                Think();
            }
            else
            {
                if (canAttackPlayer == true)
                {
                    canAttackPlayer = false;
                    //Think();
                }
            }
            yield return new WaitForFixedUpdate();
        }
    }

    private void Think()
    {
        if (canAttackPlayer)
        {
            Attack();
        }
        else if (canSeePlayer)
        {
            FollowPlayer();
        }
        else
        {
            if (agent.speed != normalSpeed)
            {
                agent.speed = normalSpeed;
            }

            if (Random.value > 0.7f)
            {
                Wander();
            }
            else
            {
                Idle();
            }
        }
    }

    bool attaking = false;
    bool blocking = false;
    private void Attack()
    {
        transform.LookAt(GameManager.instance.Player.transform);
        if (attaking || blocking)
            return;
        else
        {
            animator.CrossFade(idleAnimationID, animatormationPlayTransition);

            if (Random.value > 0.75f)
            {
                attaking = true;
                sword.Attack(swordDamage);
                animator.CrossFade(attackAnimationID, animatormationPlayTransition);

                Debug.Log("Attacking");
                if (lstWork != null)
                {
                    StopCoroutine(lstWork);
                }
                StartCoroutine(AttackCooldown());
            }
            else
            {
                blocking = true;
                animator.CrossFade(defendAnimationID, animatormationPlayTransition);

                Debug.Log("Blocking");
                if (lstWork != null)
                {
                    StopCoroutine(lstWork);
                }
                StartCoroutine(BlockingCooldown());
            }
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(1f);
        sword.StopAttack();
        attaking = false;
    }

    IEnumerator BlockingCooldown()
    {
        yield return new WaitForSeconds(1f);
        blocking = false;
    }

    bool running;
    private void FollowPlayer()
    {
        if (lstWork != null)
        {
            StopCoroutine(lstWork);
        }

        agent.speed = approachingSpeed;
        Debug.Log("Following Player");

        lstWork = StartCoroutine(GoTo(GameManager.instance.Player.transform.position));


        if (running == false)
        {
            running = true;
            animator.CrossFade(runAnimationID, animatormationPlayTransition);
            StartCoroutine(FollowingPlayer());
        }
    }

    IEnumerator FollowingPlayer()
    {
        yield return new WaitForSeconds(0.8f);
        running = false;
    }

    private void Wander()
    {
        Vector3 newPos = RandomPosition(agent, wanderRadius);
        animator.CrossFade(walkAnimationID, animatormationPlayTransition);

        lstWork = StartCoroutine(GoTo(newPos));
    }

    public Vector3 RandomPosition(NavMeshAgent agent, float radius)
    {
        var randDirection = Random.insideUnitSphere * radius;
        randDirection += agent.transform.position;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, radius, -1);
        return navHit.position;
    }

    IEnumerator GoTo(Vector3 pos)
    {
        //animator.SetBool(walkAnimationParameterID, true);

        agent.SetDestination(pos);
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            yield return new WaitForSeconds(0.3f);
        }

        yield return new WaitForFixedUpdate();
        Think();
    }

    private void Idle()
    {

        lstWork = StartCoroutine(Idling());
    }

    IEnumerator Idling()
    {
        animator.CrossFade(idleAnimationID, animatormationPlayTransition);

        yield return new WaitForSeconds(Random.Range(idleMinDuration, idleMaxDuration));
        Think();
    }

    public void GetHit(float amount)
    {
        StopAllCoroutines();

        characterCharacteristics.RemoveHealth(amount);
        if (characterCharacteristics.health <= 0)
        {
            Die();
        }
        else
        {
            agent.ResetPath();
            animator.CrossFade(getHitAnimationID, animatormationPlayTransition);
            StopAllCoroutines();
            StartCoroutine(HitCooldown());
        }
    }

    private void Die()
    {
        agent.isStopped = true;
        Collider[] cols = GetComponents<Collider>();
        foreach (Collider col in cols)
        {
            col.enabled = false;
        }
        animator.CrossFade(dieAnimationID, animatormationPlayTransition);
        StopAllCoroutines();
    }

    IEnumerator HitCooldown()
    {
        attaking = false;
        blocking = false;
        running = false;
        agent.ResetPath();
        yield return new WaitForSeconds(detHitCooldownSeconds);
        StartCoroutine(See());
    }

    public void OnDrawGizmos()
    {
        if (GameManager.instance == null)
            return;

        if (agent.destination == null)
            return;

        if (agent.pathPending == true)
            return;

        if (agent.path.corners.Length == 0)
            return;

        Gizmos.color = Color.blue;

        Vector3 prevCorner = transform.position;
        foreach (Vector3 corner in agent.path.corners)
        {
            Gizmos.DrawLine(prevCorner, corner);
            prevCorner = corner;
        }

        Gizmos.color = Color.cyan;

        Gizmos.DrawSphere(agent.path.corners[agent.path.corners.Length - 1], 0.2f);
    }
}
