using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    bool attacking;
    float damage;
    [SerializeField] string ignoreTag;
    public void Attack(float damage)
    {
        this.damage = damage;
        attacking = true;
    }

    public void StopAttack()
    {
        attacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("P");
        if (attacking == false)
            return;
        Debug.Log("Pl");


        if (other.tag == ignoreTag)
            return;
        Debug.Log("Pla");


        switch (other.tag)
        {
            case "Environment":
                Debug.Log("Hit obstacle");
                break;

            case "NPC":
                Debug.Log("Hit NPC");
                other.GetComponent<EnemyController>().GetHit(damage);
                break;


            case "Player":
                Debug.Log("Hit Player");
                other.GetComponent<PlayerController>().GetHit(damage);
                break;

            default:
                Debug.Log("Hit unknown gmaeObject: " + other.gameObject.ToString());
                break;
        }

        StopAttack();
    }
}
