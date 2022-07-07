using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bulliet : MonoBehaviour
{
    public int damage;

    void OnCollisionEnter(Collision collision)
     {
        if(collision.gameObject.tag == "Floor") {
            Destroy(gameObject, 3); //3초뒤 사라짐
        } else if(collision.gameObject.tag == "Wall") {
            Destroy(gameObject); //딜레이 없이 사라짐
        }
    }
}
