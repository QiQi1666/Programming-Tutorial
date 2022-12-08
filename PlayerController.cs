using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Insert Character Controller")]
    private CharacterController controller;
    
    [SerializeField]
    [Tooltip("Insert Main Camera")]
    private Camera mainCamera;
    
    [SerializeField]
    [Tooltip("Insert Animator Controller")]
    private Animator playerAnimator;
    
    [SerializeField]
    [Tooltip("Insert Pokeball Prefab")]
    private GameObject pokeBallPF;
    
    [SerializeField]
    [Tooltip("Insert Pokeball Bone Transform")]
    private Transform pokeBallBone;
    
    [SerializeField] private AudioClip[] grassSounds;
    [SerializeField] private AudioClip[] sandSounds;
    
    private AudioSource playerAS1;

    private Vector3 velocity;
    private bool grounded;
    private float groundCastDist = 0.05f;
    private float gravity = -9.8f;

    public float speed = 2f;
    public float runSpeed = 5f;
    public float jumpHeight = 20f;
    
    private bool throwing = false;
    public float throwStrength = 4f;
    private GameObject instantiatedPokeball;
    
    // Start is called before the first frame update
    void Start()
    {
        playerAS1 = GetComponents<AudioSource>()[0];
        playerAS1.spatialBlend = 1f;
        playerAS1.maxDistance = 5f;
    }

    // Update is called once per frame
    void Update()
    {
        // Grab transforms
        Transform playerTransform = transform;
        Transform cameraTransform = mainCamera.transform;
        
        // Grounded
        grounded = Physics.Raycast(playerTransform.position, Vector3.down, groundCastDist);
        
        // DEBUG - Visualize raycast
        if (grounded)
        {
            Debug.DrawRay(playerTransform.position, Vector3.down, Color.blue);
        }
        else
        {
            Debug.DrawRay(playerTransform.position, Vector3.down, Color.red);
        }

        // Ground movement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 movement = (playerTransform.right * x) + (playerTransform.forward * z);

        // Throw
        if (Input.GetButtonDown("Fire1") && grounded)
        {
            throwing = true;
            SpawnPokeballToBone();
            playerAnimator.SetBool("IsThrowing", true);
        }
        
        //Apply movement
        if (!throwing)
        {
            // Regular movement and running
            if (Input.GetKey(KeyCode.LeftShift))
            {
                controller.Move(movement * runSpeed * Time.deltaTime);
                playerAnimator.SetBool("IsRunning", true);
                playerAS1.volume = 0.75f;
            }
            else
            {
                controller.Move(movement * speed * Time.deltaTime);
                playerAnimator.SetBool("IsRunning", false);
                playerAS1.volume = 0.5f;
            }
            
            // Gravity and jumping
            velocity.y += gravity * Time.deltaTime;
            if (Input.GetButtonDown("Jump") && grounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight);
            }
            controller.Move(velocity * Time.deltaTime);
            playerAnimator.SetBool("IsJumping", !grounded);
        }
        
        if (movement.magnitude > 0)
        {
            playerAnimator.SetBool("IsWalking", true);
        }
        else
        {
            playerAnimator.SetBool("IsWalking", false);
        }

        // Rotate alongside the camera
        playerTransform.rotation = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up);
    }

    public void ThrowEnded()
    {
        throwing = false;
        playerAnimator.SetBool("IsThrowing", false);
    }

    public void SpawnPokeballToBone()
    {
        if (instantiatedPokeball == null)
        {
            instantiatedPokeball = Instantiate(pokeBallPF, pokeBallBone, false);
        }
    }

    public void ReleasePokeball()
    {
        if (instantiatedPokeball != null)
        {
            instantiatedPokeball.transform.parent = null;
            instantiatedPokeball.GetComponent<SphereCollider>().enabled = true;
            instantiatedPokeball.GetComponent<Rigidbody>().useGravity = true;
            Transform cameraTransform = mainCamera.transform;
            Vector3 throwAdjustment = new Vector3(0f, 0.5f, 0f);
            Vector3 throwVector = (cameraTransform.forward + throwAdjustment) * throwStrength;
            instantiatedPokeball.GetComponent<Rigidbody>().AddForce(throwVector, ForceMode.Impulse);
            instantiatedPokeball = null;
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    
    public float[] GetTextureMix(Vector3 pokemonPosition, Terrain terrain)
    {
        Vector3 terrainPosition = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;
        
        // Position of player in relation to terrain alphamap
        int mapPositionX = Mathf.RoundToInt((pokemonPosition.x - terrainPosition.x) / terrainData.size.x * terrainData.alphamapWidth);
        int mapPositionZ = Mathf.RoundToInt((pokemonPosition.z - terrainPosition.z) / terrainData.size.z * terrainData.alphamapHeight);

        // 3D: 1st x, 2nd z, 3rd percentage of the terrain layers (grass/sand) used
        float[,,] splatMapData = terrainData.GetAlphamaps(mapPositionX, mapPositionZ, 1, 1);
        
        // Extract all the values into that cell mix converting 3D to 1D
        float[] cellMix = new float[splatMapData.GetUpperBound(2) + 1];
        for (int i = 0; i < cellMix.Length; i++)
        {
            cellMix[i] = splatMapData[0, 0, i];
        }
        return cellMix;
    }

    public string FootStepLayerName(Vector3 pokemonPosition, Terrain terrain)
    {
        float[] cellMix = GetTextureMix(pokemonPosition, terrain);
        float strongestTexture = 0;
        int maxIndex = 0;
        for (int i = 0; i < cellMix.Length; i++)
        {
            if (cellMix[i] > strongestTexture)
            {
                strongestTexture = cellMix[i];
                maxIndex = i;
            } 
        }
        return terrain.terrainData.terrainLayers[maxIndex].name;
    }

    public void footStep()
    {
        playerAS1.clip = grassSounds[Random.Range(0, grassSounds.Length)];
        if (FootStepLayerName(transform.position, Terrain.activeTerrain) == "TL_Sand")
        {
            playerAS1.clip = sandSounds[Random.Range(0, sandSounds.Length)];
        }
        playerAS1.Play();
    }
}
