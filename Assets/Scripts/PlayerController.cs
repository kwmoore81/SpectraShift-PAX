﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(MovingEntity))]
public class PlayerController : LivingEntity
{
    MovingEntity movingEntity;
    MeshRenderer mesh;
    WeaponSystems weaponsSystems;
    Afterburners afterburners;
    PauseGame pause;

    public Slider healthSlider;
    public Slider shieldSlider;
    public Slider darkSlider;
    public Slider lightSlider;
    public Text rocketText;
    public float lightEnergy;
    public float lightEnergyMax = 50;
    public float darkEnergy;
    public float darkEnergyMax = 50;


    public bool playerControl = false;

    float deadzone = 0.3f;
    float buttonTimer = 0.5f;
    int buttonCount = 0;

    protected override void Start()
    {
        base.Start();
        movingEntity = GetComponent<MovingEntity>();
        weaponsSystems = GetComponent<WeaponSystems>();
        afterburners = GameObject.FindWithTag("Afterburners").GetComponent<Afterburners>();
        pause = GameObject.FindWithTag("WorldController").GetComponent<PauseGame>();
        healthSlider.maxValue = startingHealth;
        shieldSlider.maxValue = startingShield;
        lightEnergy = 50;
        darkEnergy = 50;      
    }

    void Update ()
    {
        if (playerControl)
        {
            InputControl();
            ControlUI();
            Regen();
        }
        if (!isLight && darkEnergy < darkEnergyMax)
        {
            darkEnergy += 0.01f;
        }

        if (isLight && lightEnergy < lightEnergyMax)
        {
            lightEnergy += 0.01f;
        }    
        ControlUI();

        if (addEnergy == true)
        {
            onKillEnergy();          
        }
    }

    void InputControl()
    {
        /////////////////////////////////////////////////
        // Controller mapping...
        // Fire1 = button A
        // Fire2 = button B
        // Fire3 = button X
        // Fire4 = button Y
        // StrafeLeft / Strafe Right = shoulder buttons
        // Triggers = Left and right trigger axis
        // LS_Horizontal = left stick horiz axis
        /////////////////////////////////////////////////

        // Controller flight controls
        float horiz_cInput = Input.GetAxis("LS_Horizontal");
        float triggers_cInput = Input.GetAxis("Triggers");
        if (-deadzone > horiz_cInput || horiz_cInput > deadzone)    movingEntity.Turn(horiz_cInput);
        if (triggers_cInput != 0)   movingEntity.Thrust(triggers_cInput);

        // Keyboard flight controls
        float horiz_kbInput = Input.GetAxis("Horizontal");
        float vert_kbInput = Input.GetAxis("Vertical");
        if (horiz_kbInput != 0) movingEntity.Turn(horiz_kbInput);
        if (vert_kbInput != 0) movingEntity.Thrust(vert_kbInput);

        // Afterburner control
        if (triggers_cInput > 0 || vert_kbInput > 0)
        {
            afterburners.AfterburnersOn();
        }
        else
        {
            afterburners.AfterburnersOff();
        }

        // Other controls
        if (Input.GetButton("StrafeRight"))
            movingEntity.Strafe(1);
        else if (Input.GetButton("StrafeLeft"))
            movingEntity.Strafe(-1);
        else
            movingEntity.Strafe(0);
        if (Input.GetButtonDown("Home"))
        {
            Debug.Log("Pause Pressed");
            pause.pause();
        }

        //Weapons System, Buttons Should be Left for Laser, Right for Rocket (Mouse Inputs)
        if (Input.GetButton("Fire1"))
        {      
            if (isLight && darkEnergy > 0)
            {
                weaponsSystems.setState(WeaponSystems.WEAPON.PRIMARY);
                Debug.Log("Attempt Firing Laser");
                darkEnergy -= 0.2f;
            }
            else if (!isLight && lightEnergy > 0)
            {
                weaponsSystems.setState(WeaponSystems.WEAPON.PRIMARY);
                Debug.Log("Attempt Firing Laser");
                lightEnergy -= 0.2f;
            }
        }
        else if (Input.GetButton("Fire2"))
        {
            weaponsSystems.setState(WeaponSystems.WEAPON.SECONDARY);
            Debug.Log("Attempt Rocket Firing");
        }
        else weaponsSystems.setState(WeaponSystems.WEAPON.BLANK);

        if (Input.GetButtonDown("Fire4"))
        {
            if (isLight) isLight = false;
            else isLight = true;
            Debug.Log("Fire4 pressed");
        }

        // Strafe and juke controls
        if (Input.GetButtonDown("StrafeLeft"))
        {
            if(buttonTimer > 0 && buttonCount == 1)
            {
                Debug.Log("Left double tap!");
                movingEntity.jukeDirection = -1;
            }
            else
            {
                buttonTimer = 0.5f;
                buttonCount += 1;
            }
        }
        else if (Input.GetButtonDown("StrafeRight"))
        {
            if (buttonTimer > 0 && buttonCount == 1)
            {
                Debug.Log("Right double tap!");
                movingEntity.jukeDirection = 1;
            }
            else
            {
                buttonTimer = 0.5f;
                buttonCount += 1;
            }
        }

        if (buttonTimer > 0)
        {
            buttonTimer -= Time.deltaTime;
        }
        else
        {
            buttonCount = 0;
        }
    }

    void ControlUI()
    {
        healthSlider.value = currentHealth;
        shieldSlider.value = currentShield;
        rocketText.text = weaponsSystems.currentSecondaryAmmo.ToString();
        darkSlider.value = darkEnergy;
        lightSlider.value = lightEnergy;
    }

    public void onKillEnergy()
    {
        if (isLight == true && lightEnergy < lightEnergyMax)
        {
            lightEnergy += 5;
        }
        if (isLight == false && darkEnergy < darkEnergyMax)
        {
            darkEnergy += 5;
        }
        addEnergy = false;
    }
}
