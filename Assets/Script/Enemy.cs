using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D };
    public Type enemyType;
    public int maxHealth;
    public int curHealth;
    public Transform target;
    public BoxCollider meleeArea; //Eenemy 근접 범위
    public GameObject bullet;

    public bool isChase; //추적을 결정하는 변수
    public bool isAttack;
    public bool isDead; //죽었음을 확인하는 플래그
    

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshs;  //피격 이벤트를 플레이어처럼 모든 메테리얼로 변경
    public NavMeshAgent nav;
    public Animator anim;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>(); //몬스터 크기와 맞는지 확인하기
        //자식메뉴에 MashRenderer가 있을땐 mat = GetComponent<MeshRenderer>().material; 대신
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if(enemyType != Type.D)
        Invoke("ChaseStart", 2); //2초 후에 추적 시작
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }

    void Update()
    {
        //SetDestination(): 도착할 목표 위치 지정 함수
       if(nav.enabled && enemyType != Type.D) { //nav가 활성화 되있을때만
         nav.SetDestination(target.position);
         nav.isStopped = !isChase;
       }
    }

    void FixedUpdate() {
        Targeting();
        freezeVelocity();
    }

    void freezeVelocity()
    {
        if (isChase) {
            rigid.velocity = Vector3.zero;
            //물리력이 NavAgent 이동을 방해하지 않도록 로직 추가

            rigid.angularVelocity = Vector3.zero;
            //외부 충돌에 의해 리지드바디의 회전 속력 발생 방지
        }
    }

    void Targeting()
    {
        if(!isDead && enemyType != Type.D) {
            //ShpereCast()의 반지름, 길이를 조정할 변수 선언
        float targetRadius = 0;
        float targetRang = 0;

        switch (enemyType)
        {
            case Type.A:
                targetRadius = 1.5f;
                targetRang = 3f;
                break;
            case Type.B:
                targetRadius = 1f;
                targetRang = 12f;
                break;
            case Type.C:
                targetRadius = 0.5f;
                targetRang = 25f;
                break;
        }
        RaycastHit[] rayHits =
             Physics.SphereCastAll(transform.position, //시작위치
                                                targetRadius, //반지름 
                                                transform.forward,//나아가는 방향,
                                                targetRang, //거리
                                                LayerMask.GetMask("Player"));

        if(rayHits.Length > 0 && !isAttack) {
            StartCoroutine(Attack());
        }
      }
        
    }

    IEnumerator Attack()
    { //먼저 정지를 한 다음, 애니메이션과 함께 공격범위 활성화
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

    switch (enemyType)
    {
      case Type.A:
        yield return new WaitForSeconds(0.2f);
        meleeArea.enabled = true;

        yield return new WaitForSeconds(1f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(0.6f);
        break;
      case Type.B:
        yield return new WaitForSeconds(0.1f);
        rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
        meleeArea.enabled = true;
        
        yield return new WaitForSeconds(0.5f);
        rigid.velocity = Vector3.zero;
        meleeArea.enabled = false;

        yield return new WaitForSeconds(2f);
        break;
      case Type.C:
            yield return new WaitForSeconds(0.5f);
            GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
            Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
            rigidBullet.velocity = transform.forward * 20;

            yield return new WaitForSeconds(2f);
        break;
    }

    isChase = true;
    isAttack = false;
    anim.SetBool("isAttack", false);
    }

    void OnTriggerEnter(Collider other) 
    {
        if(other.tag == "Melee") {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            //현재 위치에 피격 위치를 빼서 반작용 방향 구하기
            Vector3 reactVec = transform.position - other.transform.position;

            StartCoroutine(OnDamage(reactVec, false));
        } else if(other.tag == "Bullet") {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject); //총알 관통 방지

            StartCoroutine(OnDamage(reactVec, false));
        }
    }

    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }
    IEnumerator OnDamage(Vector3 reactVec, bool isGreade) //수류탄만의 리액션을 위한 bool추가
    {
        foreach(MeshRenderer mesh in meshs)
            mesh.material.color = Color.red; //피격 당할때 색
        yield return new WaitForSeconds(0.1F);


            if (curHealth > 0){
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
            } else{
            //죽음
            foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.gray;

            gameObject.layer = 14;  //레이어 번호를 그대로 가져옴
            isDead = true;
            nav.enabled = false; //죽었을때 nav기능 비활성화
            anim.SetTrigger("doDie"); //죽었을때의 애니메이션
            isChase = false;

            if (isGreade) { //수류탄에 죽었을때
                reactVec = reactVec.normalized; //대각선 이동까지 정규화
                reactVec += Vector3.up * 3;


                rigid.freezeRotation = false; //Rigidbody의 Freeze Rotation무효화
                rigid.AddForce(reactVec * 5, ForceMode.Impulse); // //AddForce() 함수로 넉백 구현하기
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            } else {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }
            if(enemyType != Type.D)
                Destroy(gameObject, 4);
        }
    }
}
//enumerate 영어로 수를 세다. 카운팅 
/*IEnumerator : 열거자를 구현하는데 필요한 인터페이스
 (클래스 내부의 컬렉션에 대해 반복할 수 있도록 도와준다.)
 */

//Material은 Mesh Renderer 컴포넌트에서 접근 가능 GetComponent<MeshRenderer>().material;

//Anim Controller 생성 후 자식 오브젝트에 추가
//
//새로 인스턴스화 된 오브젝트의 추가적인 조작이 필요할 때 GameObject변수에 담습니다.
