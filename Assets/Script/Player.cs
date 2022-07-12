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
    public GameObject[] grenades; //(필살기)공전하는 물체 컨트롤위한 배열 변수
    public int hasGrenades; //Current 수류탄 
    public GameObject grenadeObj; //수류탄 프리펩을 저장할 변수
    public Camera followCamera; //플레이어 turn Camera


    public int ammo;
    public int coin;
    public int health;

    
    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenades; //수류탄 최대값

    float hAxis;
    float vAxis;

    bool wDown; //left Shift 속도 0.3f 일반걷기 
    bool jDown; //점프 기능
    bool fDown; //스윙
    bool gDown; //grenade
    bool rDown; //재장전
    bool iDown; //무기 입수 (e)
    bool sDown1; //무기 교체
    bool sDown2;
    bool sDown3;

    bool isJump; //무한 점프를 막기위한 제약 조건
    bool isDodge;
    bool isSwap; //무기 교체 시간차를 위한 플래그 로직
    bool isReload; //애니메터 트리거 호출과 플래그변수 변화 작성
    bool isRfireReady = true; //장전중에 공격불가
    bool isFireReady = true; //쿨타임 완료
    bool isSide; //벽 충돌 유무
    bool isDamage; //무적타임을 위한 변수

    Vector3 sideVec; //벽 충돌 방향 저장
    Vector3 moveVec;
    Vector3 dodgeVec; //회피 도중 방향전환 금지

    Rigidbody rigid;
    Animator anim;
    MeshRenderer[] meshs;

    GameObject nearObject;
    Weapon equipWeapon; //이미 장착한 무기를 저장하는 변수
    int equipWeaponIndex = -1;
    float fireDelay; //쿨타임

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>(); //[] 배열이라 모든 자식 컴포넌트 가져오기
    }

    // Update is called once per frame
    void Update()
  //normalized: 방향 값이 1로 보정된 벡터
  //대각선 거리가 1 : 1 : √2로 좀더 멀기때문에 더 빠르게 가지않게 보정해줘야함 
  //transform 이동은 꼭 Time.deltaTime 까지 곱해주어야함.
  //SetBool 함수("파라메타 기능", true일때)
  //LookAt(): 지정된 벡터를  향해서 회전시켜주는 함수
  //AddForce() 함수로 물리적인 힘을 가하기
  //ScreenPointToRay(): 스크린에서 월드로 Ray를 쏘는 함수
  //out : return처럼 반환값을 주어진 변수에 저장하는 키워드
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Grenade();
        Attack();
        Reload();
        Dodge();
        Swap();
        Interation();
    }

  void StopToWall()
  {
      Debug.DrawRay(transform.position, transform.forward * 5, Color.green); //유효 범위 인식용
      isSide = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
      //Wall이랑 충돌하게 되면 bool값이 true로 바뀜
  }
  void freezeRotation() 
  {
      rigid.angularVelocity = Vector3.zero;
      //외부 충돌에 의해 리지드바디의 회전 속력 발생 방지
  }
    void FixedUpdate() {
      freezeRotation();
      StopToWall();
    }


  void GetInput()
    {
      hAxis = Input.GetAxisRaw("Horizontal");
      vAxis = Input.GetAxisRaw("Vertical");
      wDown = Input.GetButton("Walk");
      //Shift를 누르는 중일때만 작동이 되도록함
      jDown = Input.GetButtonDown("Jump");
      fDown = Input.GetButton("Fire1");
      gDown = Input.GetButtonDown("Fire2");
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

      if(isSwap || fDown) //무기스왑 || 공격 중에는 이동불가   !isFireReady
        moveVec = Vector3.zero;


      if(!isSide)
        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

      anim.SetBool("isRun", moveVec != Vector3.zero);
      anim.SetBool("isWalk", wDown);
    }

    void Turn()
    {
      //#1.키보드에 의한 회전
      transform.LookAt(transform.position + moveVec);

    //#2.마우스에 의한 회전
    if (fDown && equipWeaponIndex > 0 )
    {
      Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
      RaycastHit rayHit; //정보를 저장할 변수 추가
      if (Physics.Raycast(ray, out rayHit, 100))
      {
        Vector3 nextVec = rayHit.point - transform.position;
        //Ray닿았던 지점(RaycastHit의 마우스 클릭위치) - 플레이어 움직이는 위치
        nextVec.y = 0;
        transform.LookAt(transform.position + nextVec); //돌아보게 하기
      }
    }
  }

   void Jump()
  {   //Vector3.zero = 속도가 없을때만 점프
    if (jDown && moveVec == Vector3.zero && !isJump && !isSwap)
    {
      rigid.AddForce(Vector3.up * 30, ForceMode.Impulse);
      //15대신 public float JumpPower 변수 만들어서 조종가능
      anim.SetBool("isJump", true);
      anim.SetTrigger("doJump");
      isJump = true;
    }
  }
   void Grenade()
  {
    if (hasGrenades == 0)
      return;

    if (gDown && !isReload && !isSwap)
    {
      Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
      RaycastHit rayHit;
      if (Physics.Raycast(ray, out rayHit, 100))
      {
        Vector3 nextVec = rayHit.point - transform.position;
        nextVec.y = 20;

        GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
        //Instantiate() 함수로 수류탄 생성
        Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
        rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
        rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

        hasGrenades--;
        grenades[hasGrenades].SetActive(false);

      }
    }
  }
    void Attack()
    {
      if(equipWeapon == null)
        return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady && !isDodge && !isSwap && isRfireReady) {
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

      if(rDown && !isJump && !isDodge && !isSwap && isFireReady && !isReload && equipWeapon.curAmmo < equipWeapon.maxAmmo) {

          isRfireReady = false;
          anim.SetTrigger("doReload");
          isReload = true;
          speed = 5; 
          Invoke("ReloadOut", 2.5f);

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
      speed = 15;
      isRfireReady = true;

      /* 7발중 3발을 쏴서 4발이 남았을때  reAmmo -= equipWeapon.curAmmo(현재 4발 남음)
      권총일때 reAmmo는 
      */

      // equipWeapon.curAmmo = reAmmo;
      // ammo -= reAmmo;  이 두개 로직만 쓰면 빠져나가는 총알 값이 30, 7로 정적으로 빠짐
    }

    void Dodge()
    {  //액션 도중에 다른 액션이 실행되지 않도록 조건 추가
      //moveVec != Vector3.zero 속도가 있을땐 Dodge 회피기능 발생
        if(jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isSwap && !isReload) {
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
    if (other.tag == "Item") {
      Item item = other.GetComponent<Item>();
      switch (item.type)
      {
        case Item.Type.Ammo:
          ammo += item.value;
          if (ammo > maxAmmo) //수치가 Max값을 넘지 못하도록
            ammo = maxAmmo;
          break;
        case Item.Type.Coin:
          coin += item.value;
          if (coin > maxCoin)
            coin = maxCoin;
          break;
        case Item.Type.Heart:
          health += item.value;
          if (health > maxHealth)
            health = maxHealth;
          break;
        case Item.Type.Grenade:
          if(hasGrenades == maxHasGrenades)
            return;
          grenades[hasGrenades].SetActive(true); //수류탄 개수대로 공전체가 활성화 되도록 구현
          hasGrenades += item.value;
          if (hasGrenades > maxHasGrenades) 
            hasGrenades = maxHasGrenades;
          break;
      }
      Destroy(other.gameObject);
    }
    else if (other.tag == "EnemyBullet") {
      if (!isDamage) { // 무적이 아닐때만 데미지감소
          Bullet enemyBullet = other.GetComponent<Bullet>();
          health -= enemyBullet.damage;

          bool isBossAtk = other.name == "Boss Melee Area";
          StartCoroutine(OnDamage(isBossAtk)); //플레이어가 맞으면 isBossAtk true가된걸 넘김
      }
      //무적과 관계없이 투사체는 Destroy 되도록
      if(other.GetComponent<Rigidbody>() != null) //rigidbody가 있는거에 붙이칠때 
        Destroy(other.gameObject);

    }
  }

  IEnumerator OnDamage(bool isBossAtk)
  {
    isDamage = true;
    foreach(MeshRenderer mesh in meshs) {
      mesh.material.color = Color.yellow;
    }

    if (isBossAtk)
      rigid.AddForce(transform.forward * 45, ForceMode.Impulse); //넉백

    yield return new WaitForSeconds(1f); //1초가 지난뒤

    isDamage = false;  //무적 풀리고
    foreach(MeshRenderer mesh in meshs) {
      mesh.material.color = Color.white;
    }

    if (isBossAtk)
        rigid.velocity = Vector3.zero;
  }

    void OnTriggerStay(Collider other) {
      if (other.tag == "Weapon") {
        nearObject = other.gameObject;

        //Debug.Log(nearObject.name);
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



 /*충돌 함수 - OnTrigger: 두 오브젝트가 물리 연산을 하지 않고 충돌을 하려고 할 때 사용한다. 
  두 오브젝트 중 하나는 Collider의 Is Trigger가 true 상태여야 한다.
  */

  /* 두 오브젝트가 물리 법칙에 영향을 받을 때 사용한다. 두 오브젝트가 부딪혔을 때 충돌을 감지하며,
   적어도 하나의 오브젝트가 Rigidbody의 Body Type이 Dynamic 으로 설정되어야 한다.*/