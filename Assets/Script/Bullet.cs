using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour  
{
    public int damage;
    public bool isMelee; //근접공격 범위가 파괴되지 않도록 조건 추가
    public bool isRock;

    void OnCollisionEnter(Collision collision)
     {
        if(!isRock && collision.gameObject.tag == "Floor") 
            Destroy(gameObject, 3);
    }

    void OnTriggerEnter(Collider other) {
        if(!isMelee && other.gameObject.tag == "Wall") {
            Destroy(gameObject); //딜레이 없이 사라짐
        } 
    }
}
