using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon }
    public Type type;
    public int value;
    Rigidbody rigid;  //물리 충돌을 담당하는 콜라이더와 충돌하는 문제때문에 생성
    SphereCollider sphereCollider;

    void Awake() {
        //***GetComponent() 함수는 첫번째 컴포넌트를 가져오기때문에 적용하려는 함수가 첫번째에 있어야함
        rigid = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }
    
    void Update()
    {
        transform.Rotate(Vector3.up * 30 * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision) {
        //함수에서 변수를 호출하여 물리효과 변경
        if(collision.gameObject.tag == "Floor") {
            rigid.isKinematic = true;
            sphereCollider.enabled = false;
        }
        
    }
}
//enum: 열거형 타입 (타입 이름 지정 필요)
// transform.Rotate(Vector3.up * 5, Space.World); 아이템의 x y z축을 global의 축과 동일하게 하는 방법

//OnCollisionEnter(Collision collision) : 