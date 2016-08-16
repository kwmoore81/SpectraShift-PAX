﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ShiftLaser : MonoBehaviour, IGun
{
    public float maxDelay = 0.2f;
    const float laserLifetime = 6.0f;

    float laserDelay;
    float currentAmmo;
    private float playerHeight;

    bool isLight = true;
    bool fired = false;

    public Canvas PlayerUI;
    public GameObject lightBolt;   
    public GameObject darkBolt;   
    private GameObject player;
    private Transform laserTran;
    private ParticleSystem lightBoltPartical;
    private ParticleSystem darkBoltPartical;
    
    // Use this for initialization
    void Start ()
    {
        laserDelay = maxDelay;
        lightBoltPartical = GetComponent<ParticleSystem>();
        darkBoltPartical = GetComponent<ParticleSystem>();      
    }

    void IGun.fire()
    {
        laserDelay = laserDelay - Time.deltaTime;
        if (laserDelay <= 0)
        {
            laserDelay = maxDelay;
            fired = true;
            GameObject tempBulletHandler;
            if (isLight)
            {
                tempBulletHandler = Instantiate(lightBolt, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
                if (!isLight)
                {
                    lightBoltPartical.Stop();
                }
                Destroy(tempBulletHandler, laserLifetime);              
            }
            else if(!isLight)
            {              
                tempBulletHandler = Instantiate(darkBolt, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
                if (isLight)
                {
                    darkBoltPartical.Stop();
                }
                Destroy(tempBulletHandler, laserLifetime);               
            }
        }
    }

    void IGun.setShift(bool shiftState)
    {
        isLight = shiftState;
    }

    bool IGun.loaded
    {
        get { return fired; }
        set { fired = value; }
    }
}
