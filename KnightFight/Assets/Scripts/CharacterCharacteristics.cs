using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCharacteristics : MonoBehaviour
{
    public delegate void OnValueChanged();

    float _health;
    public float health { get { return _health; } private set { _health = value; healthSlider.value = value; } }

    float _maxHealth;
    float maxHealth { get { return _maxHealth; } set { _maxHealth = value; healthSlider.maxValue = value; if (_health > maxHealth) _health = maxHealth; } }

    [SerializeField] float MaxHealth;
    [SerializeField] float healthRegenerationSpeed;
    [SerializeField] float healthRegenerationDecay;

    public event OnValueChanged OnZeroHealth;

    Coroutine healthRegenerationCoroutine;

    [SerializeField] Slider healthSlider;
    [SerializeField] GameObject healthFill;



    float _stamina;
    public float stamina { get { return _stamina; } private set { _stamina = value; if (staminaSlider != null) staminaSlider.value = value; } }

    float _maxStamina;
    float maxStamina { get { return _maxStamina; } set { _maxStamina = value; if(staminaSlider!=null) staminaSlider.maxValue = value; if (_stamina > maxStamina) _stamina = maxStamina; } }

    [SerializeField] float MaxStamina;
    [SerializeField] float staminaRegenerationSpeed;
    [SerializeField] float staminaRegenerationDecay;

    public event OnValueChanged OnZeroStamina;
    public event OnValueChanged OnStaminaRecovery;

    Coroutine staminaRegenerationCoroutine;

    [SerializeField] Slider staminaSlider;
    [SerializeField] GameObject staminaFill;

    private void Awake()
    {
        maxHealth = MaxHealth;
        health = maxHealth;

        maxStamina = MaxStamina;
        stamina = maxStamina;
    }

    public void RemoveHealth(float amount)
    {
        if (amount < 0)
        {
            Debug.LogError("RemoveHealth(float amount): amount can't be < 0");
            return;
        }
        if (health - amount <= 0)
        {
            //Dead
            Die();
        }
        else
        {
            health -= amount;
        }

        if (healthRegenerationCoroutine != null)
        {
            StopCoroutine(healthRegenerationCoroutine);
        }

        healthRegenerationCoroutine = StartCoroutine(HealthRegeneration());
    }

    private void Die()
    {
        StopAllCoroutines();
        maxHealth = 0;
        maxStamina = 0;
        healthFill.SetActive(false);
        if(staminaFill!= null)
            staminaFill.SetActive(false);
        OnZeroHealth?.Invoke();
        this.enabled = false;
    }

    IEnumerator HealthRegeneration()
    {
        yield return new WaitForSeconds(healthRegenerationDecay);

        while (health < maxHealth)
        {
            if (health + healthRegenerationSpeed > maxHealth)
            {
                health = maxHealth;
            }
            else
            {
                health += healthRegenerationSpeed;
            }

            yield return null;
        }
    }


    public bool RemoveStamina(float amount)
    {
        if (amount < 0)
        {
            Debug.LogError("RemoveStamina(float amount): amount can't be < 0");
            return false;
        }

        if (stamina == 0)
        {
            return false;
        }

        if (stamina - amount <= 0)
        {
            //Tired
            stamina = 0;
            OnZeroStamina?.Invoke();
        }
        else
        {
            stamina -= amount;
        }

        if (staminaRegenerationCoroutine != null)
        {
            StopCoroutine(staminaRegenerationCoroutine);
        }

        staminaRegenerationCoroutine = StartCoroutine(StaminaRegeneration());

        return true;
    }
    IEnumerator StaminaRegeneration()
    {
        yield return new WaitForSeconds(staminaRegenerationDecay);
        if (stamina == 0)
        {
            OnStaminaRecovery?.Invoke();
        }

        while (stamina < maxStamina)
        {
            if (stamina + staminaRegenerationSpeed > maxStamina)
            {
                stamina = maxStamina;
            }
            else
            {
                stamina += staminaRegenerationSpeed;
            }

            yield return null;
        }
    }

    public void ChangeMaxHealth(float delta)
    {
        maxHealth -= delta;
    }

    public void ChangeMaxStamina(float delta)
    {
        maxStamina -= delta;
    }

    private void OnDisable()
    {
        Die();
    }
}
