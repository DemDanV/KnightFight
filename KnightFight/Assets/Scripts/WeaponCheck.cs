using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponCheck : MonoBehaviour
{
    [SerializeField] Collider collider;
    private void Start()
    {
        collider = GetComponent<Collider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyController>().GetHit(20);
            collider.enabled = false;
        }
    }

    public void ActivateAttackTrigger()
    {
        collider.enabled = true;
    }
}
