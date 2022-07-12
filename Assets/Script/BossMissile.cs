using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossMissile : Bullet  //Bullet 상속받음 변수와 함수 그대로 유지 + 로직 추가 가능
{
    public Transform target;
    NavMeshAgent nav;
    void Awake()
    {
        nav = GetComponent<NavMeshAgent>();    
    }
    void Update()
    {
        nav.SetDestination(target.position);
        //SetDestination(): 도착할 목표 위치 추적 함수
    }
}
