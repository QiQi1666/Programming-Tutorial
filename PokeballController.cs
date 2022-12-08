using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class PokeballController : MonoBehaviour
{
    public Animator pokeballAnimator;
    public ParticleSystem pokeflashPF;

    private GameObject pokemon;
    private GameObject terrain;
    private int animationStage;
    private bool didOnce;
    private Transform trainer;
    private bool escaped;
    private bool checkForEscape = true;
    private LevelManager levelManager;

    private AudioSource pokeballAS1;
    [SerializeField] private AudioClip clip_Hit;
    [SerializeField] private AudioClip clip_Collision;
    [SerializeField] private AudioClip clip_Wiggle;
    [SerializeField] private AudioClip clip_Success;
    [SerializeField] private AudioClip clip_Escape;
    
    private bool disableCollisionSounds;
    private static readonly int State = Animator.StringToHash("State");

    // Start is called before the first frame update
    void Start()
    {
        pokeballAnimator.speed = 0;
        trainer = GameObject.Find("Shauna").transform.Find("CameraFocus");
        levelManager = GameObject.Find("Level Manager").GetComponent<LevelManager>();
        
        pokeballAS1 = GetComponents<AudioSource>()[0];
        pokeballAS1.volume = 0.25f;
        pokeballAS1.spatialBlend = 1f;
        pokeballAS1.maxDistance = 5f;
    }

    private void FixedUpdate()
    {
        Rigidbody pokeballRigidBody = GetComponent<Rigidbody>();
        
        if (pokemon != null)
        {
            switch (animationStage)
            {
                case 0: // Apply upwards force from the pokemon we just hit
                    pokeballRigidBody.AddForce(Vector3.up * 2, ForceMode.Impulse);
                    animationStage = 1;
                    break;
                case 1: // Check for when the pokeball is coming down again
                    if (pokeballRigidBody.velocity.y < 0)
                    {
                        animationStage = 2;
                    }
                    break;
                case 2: // Hang in thin air, rotate towards pokemon, open the pokeball, spawn a particle on the pokemon, remove the pokemon
                    pokeballRigidBody.isKinematic = true; // Hang in thin air
                    Quaternion rotationTowardsPokemon =
                        Quaternion.LookRotation(pokemon.transform.position - transform.position);
                    transform.rotation = Quaternion.Lerp(transform.rotation, rotationTowardsPokemon, 
                        Time.fixedDeltaTime * 3); // Rotate towards
                    pokeballAnimator.speed = 4; // Speed up when opening (which is handled by animator)
                    if (!didOnce)
                    {
                        Instantiate(pokeflashPF, pokemon.transform.position, quaternion.identity);
                        didOnce = true;
                    }
                    pokemon.SetActive(false);
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
                        && pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Masterball_Open"))
                    {
                        animationStage = 3; 
                    }
                    break;
                case 3: // Close pokeball
                    pokeballAnimator.SetInteger(State, 1);
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
                        && pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Masterball_Close"))
                    {
                        animationStage = 4;
                        terrain = null;
                    }
                    break;
                case 4: // Rotate towards player and drop to the ground
                    transform.LookAt(trainer, Vector3.up);
                    pokeballRigidBody.isKinematic = false;
                    if (terrain != null)
                    {
                        animationStage = 5;
                    }
                    break;
                case 5: //Stop physics, wiggle
                    pokeballRigidBody.isKinematic = true;
                    pokeballAnimator.SetInteger(State, 2);
                    pokeballAnimator.speed = 1.5f;
                    if (checkForEscape)
                    {
                        int r = Random.Range(1, 10);
                        if (r == 1)
                        {
                            escaped = true;
                            pokeballAnimator.speed = 0;
                            didOnce = false;
                            animationStage = 6;
                        }
                        StartCoroutine(WaitForCheck(1));
                        checkForEscape = false;
                    }
                    if (pokeballAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 3.0f
                        && pokeballAnimator.GetCurrentAnimatorStateInfo(0).IsName("AN_Masterball_Wiggle"))
                    {
                        pokeballAnimator.speed = 0;
                        didOnce = false;
                        animationStage = 6;
                    }
                    break;
                case 6: // Escape or not
                    if (escaped)
                    {
                        if (!didOnce)
                        {
                            Instantiate(pokeflashPF, pokemon.transform.position, quaternion.identity);
                            pokeballAS1.clip = clip_Escape;
                            pokeballAS1.Play();
                            didOnce = true;
                        }
                        Destroy(gameObject);
                        pokemon.SetActive(true);
                    }
                    else
                    {
                        if (!didOnce)
                        {
                            pokeballAS1.clip = clip_Success;
                            pokeballAS1.Play();
                            didOnce = true;
                        }
                        levelManager.removePokemon(pokemon, true);
                    }
                    disableCollisionSounds = false;
                    break;
            }
            
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pokemon") && pokemon == null)
        {
            pokemon = collision.gameObject;
            pokeballAS1.clip = clip_Hit;
            pokeballAS1.Play();
            disableCollisionSounds = true;
        }

        if (collision.gameObject.name == "Terrain")
        {
            terrain = collision.gameObject;
            if (!disableCollisionSounds)
            {
                pokeballAS1.clip = clip_Collision;
                pokeballAS1.Play();
            }
        }
    }

    public void wiggleSound()
    {
        pokeballAS1.clip = clip_Wiggle;
        pokeballAS1.Play();
    }

    IEnumerator WaitForCheck(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        checkForEscape = true;
    }
}
