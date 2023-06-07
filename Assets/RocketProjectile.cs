using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketProjectile : MonoBehaviour
{

    [SerializeField] float speed = 8.0f;
    public GameObject explosion;

    void Start()
    {
        


    }

    void Update()
    {
        transform.position += transform.forward * Time.deltaTime * speed;   


    }
    private void OnTriggerEnter(Collider other)
    {
    Debug.Log("triggercolkldieridedr");
        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
