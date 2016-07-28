﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OctoBossGun : LivingEntity
{
    public enum EnemyState { SCAN, TRACK_CLOSE, TRACK_FAR, SPIN_ATTACK };
    EnemyState enemyState;

    public float rotationRange = 45;
    public float rotationSpeed = 60;
    public float gunRotation;
    public float detectionAngle = 7.5f;
    public float aimRange = 60;
    public float fireRateNormal = 1.2f;
    public float fireRateFast = .12f;
    public float closeTrackDistance = 10;
    public int spinRateMultiplier;

    // Maximum prediction time the gun will predict in the future
    public float maxPrediction = .5f;

    public GameObject OB_Bolt;
    public GameObject bulletSpawn;
    public Slider healthSlider;
    public OctoBossController obControl;

    float hoverHeight;
    public float shotTimer;
    float shotLifetime = 3.0f;
    float playerDistance, playerAngle;
    Vector3 playerDirection;
    bool scanRight = true;
    bool shootGun = false;
    bool firstShot = true;
    bool initROF = true;
    bool ROFSet = false;
    bool spinFireRateSet = false;

    ShiftLaser shiftLaser;
    WeaponSystems weaponSystems;
    SteeringBasics steeringBasics;

    protected override void Start()
    {
        base.Start();

        weaponSystems = GetComponent<WeaponSystems>();
        steeringBasics = GetComponent<SteeringBasics>();
        hoverHeight = HeightManager.Instance.setHeight;

        SetHeight();
        enemyState = EnemyState.SCAN;
        startingHealth = 100;
        shotTimer = 0;
        healthSlider.maxValue = startingHealth;
    }
	
	void Update ()
    {
        if (obControl.spinMode)
        {
            enemyState = EnemyState.SPIN_ATTACK;
        }

        if (shootGun) ShootGun();
        if (currentHealth <= 0) gunCount();

        StateResolution();
        ControlUI();
    }

    void SetHeight()
    {
        transform.position = new Vector3(transform.position.x, hoverHeight, transform.position.z);
    }

    void StateResolution()
    {
        switch (enemyState)
        {
            case EnemyState.SCAN:
                GunScan();
                break;
            case EnemyState.TRACK_CLOSE:
                TrackClose();
                break;
            case EnemyState.TRACK_FAR:
                TrackFar();
                break;
            case EnemyState.SPIN_ATTACK:
                SpinAttack();
                break;
        }
    }

    void GunScan()
    {
        // Turn off primary weapon fire
        shootGun = false;

        // Move gun back and forth between scan boundaries
        gunRotation = transform.localEulerAngles.y;

        if (scanRight)
        {
            gunRotation += rotationSpeed * Time.deltaTime;

            if (gunRotation < 180 && gunRotation >= rotationRange)
                scanRight = false;
        }
        else
        {
            gunRotation -= rotationSpeed * Time.deltaTime;

            if (gunRotation > 180 && gunRotation <= 360 - rotationRange)
                scanRight = true;
        }

        transform.localEulerAngles = new Vector3(0, gunRotation, 0);

        // If the gun sees the player, switch to track mode
        playerDirection = obControl.playerTarget.transform.position - transform.position;
        playerDistance = Vector3.Distance(obControl.playerTarget.transform.position, transform.position);
        playerAngle = Vector3.Angle(playerDirection, transform.forward);

        if(playerAngle < detectionAngle && isLight == obControl.player.isLight)
        {
            if (playerDistance < closeTrackDistance)
                enemyState = EnemyState.TRACK_CLOSE;
            else
                enemyState = EnemyState.TRACK_FAR;
        }
    }

    void TrackClose()
    {
        playerDirection = obControl.playerTarget.transform.position - transform.position;
        playerDistance = Vector3.Distance(obControl.playerTarget.transform.position, transform.position);
        playerAngle = Vector3.Angle(playerDirection, transform.forward);

        // Check if the gun still sees the player.  If not, change to SCAN
        if (playerAngle > detectionAngle)
            enemyState = EnemyState.SCAN;

        // Check is player is still within close tracking distance.  If not, change to TRACK_FAR
        if (playerDistance > closeTrackDistance)
            enemyState = EnemyState.TRACK_FAR;

        // Rotate gun to track player position
        steeringBasics.LookAtDirection(obControl.playerTarget.transform.position);
        CheckRotationBounds();

        // Check if player location is in front of gun and shoot
        if(steeringBasics.IsInFront(obControl.playerTarget.transform.position))
        {
            shootGun = true;
        }
    }

    void TrackFar()
    {
        playerDirection = obControl.playerTarget.transform.position - transform.position;
        playerDistance = Vector3.Distance(obControl.playerTarget.transform.position, transform.position);
        playerAngle = Vector3.Angle(playerDirection, transform.forward);

        // Check if the gun still sees the player.  If not, change to SCAN
        if (playerAngle > detectionAngle)
            enemyState = EnemyState.SCAN;

        // Check is player is still within close tracking distance.  If not, change to TRACK_FAR
        if (playerDistance < closeTrackDistance)
            enemyState = EnemyState.TRACK_CLOSE;

        // Rotate gun to track player position + velocity
        // Get the targets's speed
        float speed = obControl.playerRB.velocity.magnitude;

        // Calculate the prediction time
        float prediction;
        if (speed <= playerDistance / maxPrediction)
        {
            prediction = maxPrediction;
        }
        else
        {
            prediction = playerDistance / speed;
            //Place the predicted position a little before the target reaches the character
            prediction *= 0.9f;
        }

        // Set where the AI thinks the target will be and look at it
        Vector3 explicitTarget = obControl.playerRB.position + obControl.playerRB.velocity * prediction;
        steeringBasics.LookAtDirection(explicitTarget);
        CheckRotationBounds();

        // Check if player position + velocity is in front of gun
        if (steeringBasics.IsInFront(explicitTarget))
        {
            shootGun = true;
        }
    }

    void CheckRotationBounds()
    {
        gunRotation = transform.localEulerAngles.y;

        if (gunRotation < 180 && gunRotation > aimRange)
        {
            transform.localEulerAngles = new Vector3(0, aimRange, 0);
        }
        else if (gunRotation > 180 && gunRotation <= 360 - aimRange)
        {
            transform.localEulerAngles = new Vector3(0, 360 - aimRange, 0);
        }
    }

    void ShootGun()
    {
        shotTimer -= Time.deltaTime;

        if (shotTimer <= 0)
        {
            Instantiate(OB_Bolt, bulletSpawn.transform.position, bulletSpawn.transform.rotation);

            if (obControl.spinMode)
            {
                shotTimer = fireRateFast;
            }
            else
            {
                shotTimer = fireRateNormal;
            }
        }

        // Fire primary weapon
        //weaponSystems.setState(WeaponSystems.WEAPON.PRIMARY);

    }

    void SpinAttack()
    {
        // Return gun to rotation 0 and shoot gun
        transform.localEulerAngles = Vector3.zero;

        // Set initial spin mode rate of fire and shoot gun
        if (initROF && !ROFSet)
        {
            shotTimer = fireRateFast;
            initROF = false;
            ROFSet = true;
        }

        shootGun = true;

        // If spin duration is reached return enemy state to SCAN
        if (!obControl.spinMode)
        {
            enemyState = EnemyState.SCAN;
            shotTimer = 0;
            initROF = true;
            ROFSet = false;
        }
    }

    void gunCount()
    {
        obControl.gunCount--;
    }

    void ControlUI()
    {
        healthSlider.value = currentHealth;
    }

}
