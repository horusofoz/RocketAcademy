﻿using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{  
    Rigidbody rigidBody;
    AudioSource myAudioSource;

    [SerializeField] float rcsThrust = 200f;
    [SerializeField] float mainThrust = 1000f;
    [SerializeField] float levelLoadDelay = 2f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip levelComplete;
    [SerializeField] AudioClip deathExplosion;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem deathParticles;

    int WinSceneNumber;

    enum State { Alive, Dying, NextLevel };
    State state = State.Alive;

    bool enableCollisions = true;

    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody>();
        myAudioSource = GetComponent<AudioSource>();
        WinSceneNumber = SceneManager.sceneCountInBuildSettings - 1;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (state == State.Alive)
        {
            RespondToThrustInput();
            RespondToRotateInput();
        }

        if(Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
	}

    private void RespondToDebugKeys()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            LoadNextScene();
        }
        else if(Input.GetKeyDown(KeyCode.C))
        {
            // Toggle collission
            enableCollisions = !enableCollisions;
            Debug.Log("Collisions toggled.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive || !enableCollisions) { return; } // Ignore collision

        switch (collision.gameObject.tag)
        {
            case "Friendly":
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    private void StartSuccessSequence()
    {
        state = State.NextLevel;
        myAudioSource.Stop();
        myAudioSource.PlayOneShot(levelComplete);
        successParticles.Play();
        Invoke("LoadNextScene", levelLoadDelay); // Paramterise time
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        myAudioSource.Stop();
        myAudioSource.PlayOneShot(deathExplosion);
        deathParticles.Play();
        Invoke("LoadSameScene", levelLoadDelay); // Paramterise time
    }

    private void LoadNextScene()
    {
        if (SceneManager.GetActiveScene().buildIndex < WinSceneNumber) // If not win scene
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            SceneManager.LoadScene(0); // Start over
        }
    }

    private void LoadSameScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow)) // Can thrust while rotating
        {
            ApplyThrust();
        }
        else
        {
            StopApplyingThrust();
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * (mainThrust * Time.deltaTime));
        if (!myAudioSource.isPlaying)
        {
            myAudioSource.PlayOneShot(mainEngine);

        }
        if (!mainEngineParticles.isPlaying)
        {
            mainEngineParticles.Play();
        }
    }

    private void StopApplyingThrust()
    {
        myAudioSource.Stop();
        mainEngineParticles.Stop();
    }

    private void RespondToRotateInput()
    {

        rigidBody.angularVelocity = Vector3.zero; // Remove rotation due to physics
        
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }
    }

    
}
