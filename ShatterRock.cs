using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShatterRock : MonoBehaviour
{
    public GameObject shatteredRock;
    private GameObject pokeball;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pokeball") && pokeball == null && tag.Equals("Rock"))
        {
            Score.Instance.scoreNumber++;
            pokeball = collision.gameObject;
            GameObject fractured = Instantiate(shatteredRock, transform.position, Quaternion.identity);
            Rigidbody[] fracturedPieces = fractured.GetComponentsInChildren<Rigidbody>();
            foreach (var pieces in fracturedPieces)
            {
                pieces.AddExplosionForce(500f, fractured.transform.position, 10f);
                Destroy(fractured, 5f);
            }
            Destroy(gameObject);
        }
    }
}
