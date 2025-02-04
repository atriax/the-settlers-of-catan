﻿using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{

    private Rigidbody rigidBody;

    private bool hasLanded;
    private bool thrown;

    private Vector3 initPosition;

    [SerializeField]
    private Vector3 throwPosition;

    private int diceValue = 0;

    [SerializeField]
    private DiceSide[] diceSides;

    private PhotonView photonView;

    [SerializeField]
    private DiceController diceController;

    private bool justGotOwnership;

    public int getDiceValue()
    {
        return diceValue;
    }

    

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        rigidBody = GetComponent<Rigidbody>();
        initPosition = transform.position;
        rigidBody.useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (!photonView.IsMine) { return; }
        
        // Reset if you're reaching this after being reset.

        if (justGotOwnership)
        {
            justGotOwnership = false;
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }

        if (rigidBody.IsSleeping() && !hasLanded && thrown)
        {
            hasLanded = true;
            rigidBody.useGravity = false;

            // Freeze movement.
            rigidBody.freezeRotation = true;
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;
            SideValueCheck();
            
        }
        else if (rigidBody.IsSleeping() && hasLanded && diceValue == 0)
        {
            RollAgain();
        }
    }


    public void RollDice()
    {

        Reset();
        

        if (!thrown && !hasLanded)
        {
            transform.localPosition += throwPosition;
            thrown = true;
            rigidBody.useGravity = true;
            rigidBody.AddTorque(Random.Range(500, 1000), Random.Range(300, 500), Random.Range(500, 1000));
        }
        //else if (thrown && hasLanded)
        //{
        //    Reset();
        //}
    }

    private void Reset()
    {
        transform.position = initPosition;
        thrown = false;
        hasLanded = false;
        rigidBody.useGravity = false;
        rigidBody.constraints = RigidbodyConstraints.None;
    }

    private void RollAgain()
    {

        Reset();

        transform.localPosition += throwPosition;
        thrown = true;
        rigidBody.useGravity = true;
        rigidBody.AddTorque(Random.Range(0, 500), Random.Range(0, 500), Random.Range(0, 500));
    }

    private void SideValueCheck()
    {
        diceValue = 0;

        foreach (DiceSide diceSide in diceSides)
        {
            if (diceSide.OnGround())
            {
                // Inform dice controller.
                
                diceController.DiceFallen(diceValue = diceSide.GetSideValue());
            }
        }

        if (diceValue == 0)
        {
            RollAgain();
        }
    }

    public void SetOwner(Player owner)
    {
        photonView.TransferOwnership(owner);
        justGotOwnership = true;
    }

    
}
