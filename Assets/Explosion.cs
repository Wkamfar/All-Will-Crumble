using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
   private float DestructionDelay = 3.0f;
    

    void Update()
    {
        DestructionDelay -= Time.deltaTime;
        if (DestructionDelay <= 0) {
            Destroy(gameObject);
        }
    }
}
