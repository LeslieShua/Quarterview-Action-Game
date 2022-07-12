using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform target;
    public float orbitSpped;
    Vector3 offSet;

    void Start()
    {
        offSet = transform.position - target.position;
    }

    
    void Update()
    //RotateAround(): 타겟 주위를 회전하는 함수  목표가 움직이면 일그러짐
    /*Transform.RotateAround(Vector3 point(기준점),
                            Vector3 axis(움직이는 방향),
                            float angle(속도))*/
    {
        transform.position = target.position + offSet;
        transform.RotateAround(target.position,
                                Vector3.up,
                                orbitSpped * Time.deltaTime);

       //RotateAround() 후의 위치를 가지고 목표와의 거리를 유지
       offSet = transform.position - target.position;
    }
}
