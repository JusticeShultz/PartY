using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NOTE: This class is entirely example code. It is expected of you to repurpose this system as well as the player controller for your own projects.
/// </summary>
public class CharacterRepresentation : MonoBehaviour
{
    public KinematicCharacterController.Examples.ExampleCharacterController playerController;
    public KinematicCharacterController.KinematicCharacterMotor playerMotor;
    public GameObject usernameField;

    public void Cleanup()
    {
        Destroy(playerController);
        Destroy(playerMotor);
        Destroy(this);
    }

    public void Reset()
    {
        playerController = GetComponent<KinematicCharacterController.Examples.ExampleCharacterController>();
        playerMotor = GetComponent<KinematicCharacterController.KinematicCharacterMotor>();
    }
}