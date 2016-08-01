﻿using UnityEngine;
using System.Collections;
using System;

public class OB_Laser : MonoBehaviour, IGun
{
    public float maxDelay;
    const float laserLifetime = 3.0f;
    private float playerHeight;

    bool isLight = true;
    bool fired = false;

    public GameObject lightBolt;
    public GameObject darkBolt;
    private GameObject player;
    private Transform laserTran;

    void IGun.fire()
    {
        fired = true;
        GameObject tempBulletHandler;
        if (isLight)
        {
            tempBulletHandler = Instantiate(lightBolt, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
            Destroy(tempBulletHandler, laserLifetime);
        }
        else if(!isLight)
        {
            tempBulletHandler = Instantiate(darkBolt, gameObject.transform.position, gameObject.transform.rotation) as GameObject;
            Destroy(tempBulletHandler, laserLifetime);
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