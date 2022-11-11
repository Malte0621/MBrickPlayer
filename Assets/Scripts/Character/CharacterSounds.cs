using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSounds : MonoBehaviour
{
    public AudioClip[] FootstepSounds;
    public AudioClip JumpSound;
    public AudioClip FallSound;

    public bool walking = false;

    public float StepRateMultiplier;
    private float stepRate = 1.0f / 3.0f; // how often a step occurs, in seconds
    //private float fallThreshold = 10.0f; // how far you must have fallen for the fall/land sound to play, in studs

    private float lastStepTime; // time (in seconds) that the last step occured
    
    private void Update () {
        if (walking) {
            if (Time.time > (lastStepTime + stepRate) / StepRateMultiplier) {
                lastStepTime = Time.time;
                
                // play sound
                int index = Random.Range(0, FootstepSounds.Length);
                AudioManager.PlaySound(FootstepSounds[index]);
            }
        } else {
            lastStepTime = 0f;
        }
    }

    public void PlaySound (string name, float volume, Vector3 position) {
        AudioClip sound = name == "jump" ? JumpSound : FallSound;
        AudioManager.PlaySound3D(sound, position, volume);
    }
}
