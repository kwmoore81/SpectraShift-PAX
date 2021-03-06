﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LivingEntity : MonoBehaviour, IDamageable
{
    PlayerController playercontroller;
    [HideInInspector]
    public GameController gameController;

    public bool isLight;
    public float startingHealth;
    public float startingShield;

    public GameObject Explosion;   
    private GameOver gameover;
    private AmmoDrop ammoDrop;
   
    public event System.Action OnDeath;

    protected float currentHealth;
    protected float currentShield;
    protected bool dead;
    

    public float healthRegenRate, shieldRegenRate;
    public float regenDelay;
    private float regenTimer;
    [HideInInspector]
    public bool regenEnabled = false;
 
    protected virtual void Start()
    {
        currentHealth = startingHealth;
        currentShield = startingShield;
        regenTimer = regenDelay;
        ammoDrop = gameObject.GetComponent<AmmoDrop>();
        gameover = GameObject.FindWithTag("WorldController").GetComponent<GameOver>();
        gameController = GameObject.FindWithTag("WorldController").GetComponent<GameController>();
    }

    public void TakeHit(float damage, RaycastHit hit)
    {
        if (currentShield > 0)
        {
            currentShield -= damage;
            regenEnabled = false;
            if (currentShield < 0)
            {
                currentShield = 0;
            }
        }
        else
        {
            currentHealth -= damage;
            regenEnabled = false;
        }
             
        if (currentHealth <= 0 && !dead)
        {
            HasDied();
        }

        if (regenTimer < regenDelay)
        {
            regenTimer = regenDelay;
        }
    }

    public void HasDied()
    {
        dead = true;
        
        if (OnDeath != null)
        {
            OnDeath();                      
        }

        Instantiate(Explosion, gameObject.transform.position, Quaternion.identity);
       
        if (gameObject.tag == "Player")
        {                              
            gameover.gameOver();                         
            GameObject.Destroy(gameObject);                       
        }
        else
        {
            gameController.addEnergy = true;
            ammoDrop.itemDrop();
            GameObject.Destroy(gameObject);          
        }
        
    }

    public void Regen()
    {
        if (regenEnabled)
        {
            if (currentHealth >= startingHealth)
            {
                if (currentShield < startingShield)
                {
                    currentShield += shieldRegenRate * Time.deltaTime;
                }
                else
                {
                    currentShield = startingShield;
                }
               
            }
            if (currentHealth < startingHealth)
            {
                if (currentHealth < startingHealth)
                {
                    currentHealth += healthRegenRate * Time.deltaTime;
                }
                else
                {
                    currentHealth = startingHealth;
                }
            }
        }
        else
        {
            if (regenTimer > 0)
            {
                regenTimer -= Time.deltaTime;
            }
            else
            {
                regenEnabled = true;
                regenTimer = regenDelay;
            }
        }
    }
}
