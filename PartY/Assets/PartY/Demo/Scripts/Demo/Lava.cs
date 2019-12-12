using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lava : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if(other.gameObject.TryGetComponent(out KinematicCharacterController.Examples.ExampleCharacterController motor))
            {
                motor.Motor.SetPosition(Vector3.up);
            }
        }
    }
}