using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class NetworkCharacterMovement : MonoBehaviour
{
    public CharacterAnimator ca;
    public float IdleTime = 0.2f;

    private Vector3 lastCheckedPosition;
    private Vector3 newPosition;
    private float lastPositionTime;

    private Vector3 newRotation;

    private bool inAir;

    public void SetPosition (Vector3 pos) {
        newPosition = pos;

        UpdateAnimationState();
    }

    public void SetRotation (float rotation) {
        newRotation = new Vector3(0, rotation);
    }

    public void UpdateAnimationState() {
        if (lastCheckedPosition == null) lastCheckedPosition = newPosition;
        if (lastCheckedPosition != newPosition) {
            // position changed
            if (lastCheckedPosition.y != newPosition.y) {
                // in air
                if (!inAir) {
                    // start jump
                    ca.SetTrigger("Jump");
                    inAir = true;
                }
                ca.SetBool("Is Grounded", false);
                ca.SetBool("Is Running", false);
            } else {
                // walking
                ca.SetBool("Is Grounded", true);
                ca.SetBool("Is Running", true);
                inAir = false;
            }
        } else {
            // same position as previously
            ca.SetBool("Is Grounded", true);
            ca.SetBool("Is Running", false);
            inAir = false;
        }
        lastCheckedPosition = newPosition;
        lastPositionTime = Time.time;
    }

    private void Update() {
        // interpolate movement and rotation
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * 50);
        transform.rotation = Helper.Damp(transform.rotation,Quaternion.Euler(newRotation), 0.001f, Time.deltaTime);

        // check if position has been the same for an extended period of time
        if (Time.time > lastPositionTime+IdleTime && lastCheckedPosition == newPosition) {
            ca.SetBool("Is Grounded", true);
            ca.SetBool("Is Running", false);
            inAir = false;
        }
    }
}
