using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type { Ammo, Coin, Grenade, Heart, Weapon }
    public Type type;
    public int value;
    
    private void Update() {
        transform.Rotate(Vector3.up * 20 * Time.deltaTime);
    }
}
//enum: 열거형 타입 (타입 이름 지정 필요)