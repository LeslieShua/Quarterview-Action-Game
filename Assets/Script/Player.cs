using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{ 
    //bool 변수는 실행조건으로 활용
    //물리 효과를 위해 Rigidbody 변수 선언 후, 초기화 GetComponent<Rigidbody>()
    public float speed;
    public GameObject[] weapons; //무기관련 배열 함수
    public bool[] hasWeapons;

    public int ammo;
    public int coin;
    public int health;

    float hAxis;
    float vAxis;

    bool wDown; //left Shift 속도 0.3f 일반걷기 
    bool jDown; //점프 기능
    bool fDown; //스윙
    bool rDown; //재장전
    bool iDown; //무기 입수 (e)
    bool sDown1; //무기 교체
    bool sDown2;
    bool sDown3;

    bool isJump; //무한 점프를 막기위한 제약 조건
    bool isDodge;
    bool isSwap; //무기 교체 시간차를 위한 플래그 로직
    bool isReload; //애니메터 트리거 호출과 플래그변수 변화 작성
    bool isFireReady = true; //쿨타임 완료
    bool isSide; //벽 충돌 유무

    Vector3 sideVec; //벽 충돌 방향 저장
    Vector3 moveVec;
    Vector3 dodgeVec; //회피 도중 방향전환 금지

    Rigidbody rigid;
    Animator anim;

    GameObject nearObject;
    Weapon equipWeapon; //이미 장착한 무기를 저장하는 변수
    int equipWeaponIndex = -1;
    float fireDelay; //쿨타임

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
  //normalized: 방향 값이 1로 보정된 벡터
  //대각선 거리가 1 : 1 : √2로 좀더 멀기때문에 더 빠르게 가지않게 보정해줘야함 
  //transform 이동은 꼭 Time.deltaTime 까지 곱해주어야함.
  //SetBool 함수("파라메타 기능", true일때)
  //LookAt(): 지정된 벡터를  향해서 회전시켜주는 함수
  //AddForce() 함수로 물리적인 힘을 가하기
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interation();
    }

  void GetInput()
    {
      hAxis = Input.GetAxisRaw("Horizontal");
      vAxis = Input.GetAxisRaw("Vertical");
      wDown = Input.GetButton("Walk");
      //Shift를 누르는 중일때만 작동이 되도록함
      jDown = Input.GetButtonDown("Jump");
      fDown = Input.GetButton("Fire1");
      rDown = Input.GetButtonDown("Reload");
      iDown = Input.GetButtonDown("Interation");
      sDown1 = Input.GetButtonDown("Swap1");
      sDown2 = Input.GetButtonDown("Swap2");
      sDown3 = Input.GetButtonDown("Swap3");

    }

    void Move()
    {
      moveVec = new Vector3(hAxis, 0, vAxis).normalized;

      if(isDodge)
        moveVec = dodgeVec; //회피 중에는 움직임 벡터 -> 회피방향 벡터로 바뀌도록

      if(isSwap || !isFireReady) //무기스왑 || 공격 중에는 이동불가 
        moveVec = Vector3.zero;

      transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

      anim.SetBool("isRun", moveVec != Vector3.zero);
      anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
      transform.LookAt(transform.position + moveVec);
      //Player가 나아가는 방향으로 바라본다
    }

    void Jump()
    {   //Vector3.zero = 속도가 없을때만 점프
        if(jDown && moveVec == Vector3.zero && !isJump && !isSwap) {
            rigid.AddForce(Vector3.up * 30, ForceMode.Impulse);
            //15대신 public float JumpPower 변수 만들어서 조종가능
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Attack()
    {
      if(equipWeapon == null)
        return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap) {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
            //공격딜레이를 0으로 돌려서 다음 공격까지 기다리도록 작성
        }
    }

    void Reload() 
    {
      if(equipWeapon == null)  //무기가 있는지 확인
        return;

      if(equipWeapon.type == Weapon.Type.Melee) //무기 타입이 맞는지 확인
        return;

      if(ammo == 0) //총알이 있는지 확인
        return;

      if(rDown && !isJump && !isDodge && !isSwap && isFireReady && !isReload) {
          anim.SetTrigger("doReload");
          isReload = true;

          Invoke("ReloadOut", 3f);
      }
    }

    void ReloadOut()
    {
      /*
      reAmmo = 권총기준 maxAmmo(7) 보다 작으면(true) 장전이 가능함 false면 총알은 Max다.
      */  
      int reAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo; 
      reAmmo -= equipWeapon.curAmmo; //장전 총알 7 -= 현재남은 총알 4  = 3(쏜 총알)
      equipWeapon.curAmmo += reAmmo; //현재남은 총알 4 += 장전 총알 3  = 7
      ammo -= reAmmo; //플레이어가 가진 기본ammo 100 -= 쏜 총알 3   = 97

      isReload = false;

      /* 7발중 3발을 쏴서 4발이 남았을때  reAmmo -= equipWeapon.curAmmo(현재 4발 남음)
      권총일때 reAmmo는 
      */

      // equipWeapon.curAmmo = reAmmo;
      // ammo -= reAmmo;  이 두개 로직만 쓰면 빠져나가는 총알 값이 30, 7로 정적으로 빠짐
    }

        void Dodge()
    {  //액션 도중에 다른 액션이 실행되지 않도록 조건 추가
      //moveVec != Vector3.zero 속도가 있을땐 Dodge 회피기능 발생
        if(jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap) {
            dodgeVec = moveVec;
            speed *= 2;
            anim.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.5f);
            //함수로 시간차 함수 호출
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }

  void Swap() //무기 교체
  {
    //무기 중복 교체, 없는 무기 확인을 위한 조건 추가
    if(sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
      return;
    if(sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
      return;
    if(sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
      return;


      int weaponIndex = -1;
      if (sDown1) weaponIndex = 0;
      if (sDown2) weaponIndex = 1;
      if (sDown3) weaponIndex = 2;

      if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge) {
            if(equipWeapon != null) //빈손일때만
              equipWeapon.gameObject.SetActive(false);
            
            equipWeaponIndex =weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
      isSwap =false;
    }


  void Interation() //상호 작용
  {
    if(iDown && nearObject != null && !isJump &&!isDodge) {
      if(nearObject.tag == "Weapon") {
          Item item = nearObject.GetComponent<Item>();
          int weaponsIndex = item.value; 
          hasWeapons[weaponsIndex] = true;

          Destroy(nearObject);
      }
    }
  }

    void OnCollisionEnter(Collision collision) { //Player 착지 구현
        if(collision.gameObject.tag == "Floor") {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }

    void OnTriggerEnter(Collider other)
     {
      if(other.tag == "Weapon"){
        nearObject = other.gameObject;

        Debug.Log(nearObject.name);
      }
    }
    void OnTriggerExit(Collider other)
     {
       if(other.tag == "Weapon"){
        nearObject = null;
      }
    }
}

/*OnTriggerEnter: 스크립트가 달린 물체(A)가 다른 콜라이더를 가진 태그된 물체(B)와 
"닿았을 때"를 의미
OnTriggerExit: 스크립트가 달린 물체(A)가 다른 콜라이더를 가진 태그된 물체(B)와
"닿았다가 떨어졌을 때"를 의미*/


// OnCollisionEnter: 물리 충돌을 감지하여 충돌 처리하는 클래스
/* #1 충돌 발생조건
 * 충돌이 일어나기 위해서는, 두 오브젝트가 모두 Collider를 갖고 있어야 하며,
 둘 중 하나 이상은 RigidBody 컴퍼넌트를 갖고 있어야 합니다.

 * 두 개의 오브젝트 중 하나의 오브젝트만 움직인다면,
 움직이는 오브젝트가 RigidBody 컴퍼넌트를 가지고 있어야 합니다.*/