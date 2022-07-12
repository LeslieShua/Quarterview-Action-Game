using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Boss : Enemy
{
    public GameObject missile;
    public Transform missilePortA;
    public Transform missilePortB;

    Vector3 lookVec; //플레이어 움직임 예측 변수
    Vector3 tauntVec;
    public bool isLook; //플레이어를 바라보는 변수  public으로 선언해서 체크로 true만듬

    void Awake() //Awake() 함수는 자식 스크립트만 단독 실행! 상속이 안됨
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        nav.isStopped = true;
        StartCoroutine(Thinck());
    }

    void Update()
    {
        if (isDead) {
           StopAllCoroutines();
           return;
        }

        if(isLook) {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookVec);
        }
        else
            nav.SetDestination(tauntVec); //점프공격시 목표지점 이동로직
    }

    IEnumerator Thinck()
    {
        yield return new WaitForSeconds(0.1f);

        int ranAction = Random.Range(0, 5);
        switch (ranAction) {
            case 0:
            case 1:
                //미사일 공격 패턴
                StartCoroutine(Taunt());
                //StartCoroutine(MissileShot());
                break;
            case 2:
            case 3:
                //돌 공격 패던
                StartCoroutine(Taunt());
               // StartCoroutine(RockShot());
                break;
            case 4:
                //점프 공격 패턴
                StartCoroutine(Taunt());
                break;
        }
    }

    IEnumerator MissileShot()
    {
        anim.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f); //첫번째 미사일
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BossMissile bossMissileA = instantMissileA.GetComponent<BossMissile>();
        bossMissileA.target = target; //목표물 설정

        yield return new WaitForSeconds(0.3f); //두번째 미사일
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BossMissile bossMissileB = instantMissileB.GetComponent<BossMissile>();
        bossMissileB.target = target; //목표물 설정

        yield return new WaitForSeconds(2f); //액션 하나당 걸리는 시간

        StartCoroutine(Thinck()); //다시 생각하기
    }
    IEnumerator RockShot()
    {
        isLook = false;
        anim.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position, transform.rotation);
        yield return new WaitForSeconds(3f);

        isLook = true;
        StartCoroutine(Thinck());
    }
    IEnumerator Taunt()
    {
        tauntVec = target.position + lookVec;

        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled = false; //콜라이더가 플레이어를 밀지 않도록
        anim.SetTrigger("doTaunt");
        
        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;

        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;


        
        yield return new WaitForSeconds(1f); //위에서 소비하는 초에 따라 감소됨
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;
        StartCoroutine(Thinck());
    }
}

/*Instantiate() 함수로 미사일 생성  (복제체 만들기 || 미리만들어 놓은 옵젝 아님!)
Instantiate()함수를 사용하면 게임을 실행하는 도중에 게임오브젝트를 생성할 수 있다.
Instantiate(GameObject original ,Vector3 position ,Quaternion rotation)
1. GameObject original- 생성하고자 하는 게임오브젝트명.
 현재 씬에 있는 게임오브젝트나 Prefab으로 선언된 객체를 의미함
2. Vector3 position- Vector3으로 생성될 위치를 설정함.
3. Quaternion rotation- 생성될 게임오브젝트의 회전값을 지정한다.
 - 회전을 굳이 줘야할 상황이 아니라면, 그냥 기본값으로 설정하는 것.
 --> Quaternion.identity- 또는 게임오브젝트에서 설정된 회전값. 즉,
  original.transform.rotation으로 작성해도 됨.
*/

//행동 패턴을 만들기 위해 Random.Range() 함수 호출
//새로 인스턴스화 된 오브젝트의 추가적인 조작이 필요할 때 GameObject변수에 담습니다.