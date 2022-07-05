using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{ 
    //bool 변수는 실행조건으로 활용
    //물리 효과를 위해 Rigidbody 변수 선언 후, 초기화 GetComponent<Rigidbody>()
    public float speed;
    float hAxis;
    float vAxis;
    bool wDown; //left Shift 속도 0.3f 일반걷기 
    bool jDown; //점프 기능

    bool isJump; //무한 점프를 막기위한 제약 조건
    bool isDodge;
    bool isSide; //벽 충돌 유무
    Vector3 sideVec; //벽 충돌 방향 저장
    
    Rigidbody rigid;
    Vector3 moveVec;
    Vector3 dodgeVec; //회피 도중 방향전환 금지

    Animator anim;
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
        Dodge();
    }
    void GetInput()
    {
      hAxis = Input.GetAxisRaw("Horizontal");
      vAxis = Input.GetAxisRaw("Vertical");
      wDown = Input.GetButton("Walk");
      //Shift를 누르는 중일때만 작동이 되도록함
      jDown = Input.GetButtonDown("Jump");
    }

    void Move()
    {
      moveVec = new Vector3(hAxis, 0, vAxis).normalized;

      if(isDodge)
        moveVec = dodgeVec; //회피 중에는 움직임 벡터 -> 회피방향 벡터로 바뀌도록

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
        if(jDown && moveVec == Vector3.zero && !isJump) {
            rigid.AddForce(Vector3.up * 30, ForceMode.Impulse);
            //15대신 public float JumpPower 변수 만들어서 조종가능
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }

        void Dodge()
    {  //액션 도중에 다른 액션이 실행되지 않도록 조건 추가
      //moveVec != Vector3.zero 속도가 있을땐 Dodge 회피기능 발생
        if(jDown && moveVec != Vector3.zero && !isJump && !isDodge) {
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

    void OnCollisionEnter(Collision collision) { //Player 착지 구현
        if(collision.gameObject.tag == "Floor") {
            anim.SetBool("isJump", false);
            isJump = false;
        }
    }
}

// if (moveVec != Vector3.zero)
// {
//     Vector3 relativePos = (transform.position + moveVec) - transform.position;
//     Quaternion rotation = Quaternion.LookRotation(relativePos, Vector3.up);
//     transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime*10);   
// }


// OnCollisionEnter: 물리 충돌을 감지하여 충돌 처리하는 클래스
/* #1 충돌 발생조건
 * 충돌이 일어나기 위해서는, 두 오브젝트가 모두 Collider를 갖고 있어야 하며,
 둘 중 하나 이상은 RigidBody 컴퍼넌트를 갖고 있어야 합니다.

 * 두 개의 오브젝트 중 하나의 오브젝트만 움직인다면,
 움직이는 오브젝트가 RigidBody 컴퍼넌트를 가지고 있어야 합니다.*/



// 0. 벽에 새로운 태그 Wall 추가

// 1. 전역변수 선언
// bool isSide; //벽 충돌 유무
// Vector3 sideVec; //벽 충돌 방향 저장

// 2. 충돌 이벤트 추가
// //벽 충돌 In 체크
//     void OnCollisionStay(Collision collision)
//     {
//         if(collision.gameObject.tag == "Wall") {
//             isSide = true;
//             sideVec = moveVec;
//         }
//     }

//     //벽 충돌 Out 체크
//     void OnCollisionExit(Collision collision)
//     {
//         if (collision.gameObject.tag == "Wall") {
//             isSide = false;
//             sideVec = Vector3.zero;
//         }
//     }

// 3. 충돌하려는 방향 제거
// void Move()
//     {
//         moveVec = new Vector3(hAxis, 0, vAxis).normalized;

//         if (isDodge)
//             moveVec = dodgeVec;

//         //충돌하는 방향은 무시
//         if (isSide && moveVec == sideVec)
//             moveVec = Vector3.zero;

//         transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

//         anim.SetBool("isRun", moveVec != Vector3.zero);
//         anim.SetBool("isWalk", wDown);
//     }