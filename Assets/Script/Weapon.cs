using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range }; //근접 원거리
    public Type type;
    public int damage;
    public float rate; //공격 속도
    public int maxAmmo;  //탄약 Max
    public int curAmmo; //현재 탄약

    public BoxCollider meleeArea;   //공격 범위 
    public TrailRenderer trailEffect; //Effect
    public Transform bulletPos; //총 쏘는위치 저장 변수
    public GameObject bullet; //총알 Material, Damage등 저장 변수 종류 2개있음
    public Transform bulletCasePos; //탄피 나오는 위치 저장 변수 
    public GameObject bulletCase; //탄피 Material 또는 총알 관통 방지 기능
    

    public void Use()
    {
        if(type == Type.Melee) {
            //코루틴 함수 호출하는방법
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if (type == Type.Range && curAmmo > 0) {   //현재 탄환 갯수가 0보다 클때만 Shot
            curAmmo--;
            StartCoroutine("Shot");
        }
    }

    IEnumerator Swing()
    {
        //Trail Renderer와 BoxCollider를 시간차로 활성화 컨트롤
        yield return new WaitForSeconds(0.1f); // 0.1초 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true;


        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;
        
        yield return new WaitForSeconds(0.3f); 
        trailEffect.enabled = false;
    }


  IEnumerator Shot()
  { 
    //#1. 총알 발사    Instantiate() 함수로 총알 인스턴스화 하기    (총알, 위치, 각도)
    GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
    Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();  //속도
    bulletRigid.velocity = bulletPos.forward * 50;

    yield return null;      
    //#2. 탄피 배출
     GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
    Rigidbody caseRigid = instantCase.GetComponent<Rigidbody>();  //속도
    Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);
    caseRigid.AddForce(caseVec, ForceMode.Impulse);
    caseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
  }

    //WaitForSeconds(): 주어진 수치만큼 기다리는 함수
    //yield: 결과를 전달하는 키워드  (없으면 에러남)
    //yield break로 코루틴 탈출 가능
    //IEnumerator: 열거형 함수 클래스
    //코루틴 함수: 메인루틴 + 코루틴 (동시 실행)
    //Use() 메인루틴 -> Swing() 서브루틴 -> Use() 메인루틴
    //Use() 메인루틴  + Swing() 코루틴 (Co-Op)
    //AddTorque: 회전함수

    // AddForce와 Velocity의 차이점
    //AddForce는 같은 힘을 연속해서 가하면 자동차의 가속 페달처럼 점점 가속화합니다.
    //Velocity는 같은 힘을 가해도 동일한 속도로 달릴 수 있도록 물리엔진이 자동으로 계산해줍니다.
}
