using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//던졌을때 폭파되는 script + 수류탄 피격
public class Grenade : MonoBehaviour
{
    //자식 오브젝트와 리지드바디(자기자신을 넣어줌)를 담을 변수추가
    public GameObject meshObj;   //(Trail Renderer)
    public GameObject effectObj; //Explosion
    public Rigidbody rigid;
    
    void Start()
    {
        StartCoroutine(Explosion());
    }

    //시간차 폭발을 위한 코루틴함수
    IEnumerator Explosion()
    {
        yield return new WaitForSeconds(3f);
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
        meshObj.SetActive(false);
        effectObj.SetActive(true);

    RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, //수류탄 시작위치
                                                    15, //반지름 
                                                    Vector3.up, 0f, //쏘는 방향, 길이
                                                    LayerMask.GetMask("Enemy"));

    foreach(RaycastHit hitObj in rayHits) {
        hitObj.transform.GetComponent<Enemy>().HitByGrenade(transform.position); //수류탄 시작위치
    }
    //foreach 문으로 수류탄 범위 적들의 피격함수를 호출
  }
}

// SphereCastAll: 구체 모양의 레이캐스팅 (모든 오브젝트)
