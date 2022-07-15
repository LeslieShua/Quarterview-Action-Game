using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public RectTransform uiGroup;
    public Animator anim;

    public GameObject[] itemObj;
    public int[] itemPrice;
    public Transform[] itemPos;
    public string[] talkDate; //Price 유효성 검사변수
    public Text talkText; //Text UI 가져오기
    

    Player enterPlayer;

    public void Enter(Player player)
    {
        enterPlayer = player;
        uiGroup.anchoredPosition = Vector3.zero;
    }

    public void Exit()
    {
        anim.SetTrigger("doHello");
        uiGroup.anchoredPosition = Vector3.down * 1000;
    }
    //퇴장 시, 애니메이션 실행하면서 UI 위치 이동

    public void Buy(int index)
    {
        int price = itemPrice[index];
        if(price > enterPlayer.coin) { //플레이어가 돈이 부족할때
            StopCoroutine(Talk());
            StartCoroutine(Talk());
            return;
        }
        //플레이어가 돈이 충분할때
        enterPlayer.coin -= price;
        Vector3 ranVec = Vector3.right * Random.Range(-3, 3)
                        + Vector3.forward * Random.Range(-3, 3);
        Instantiate(
            itemObj[index], //복사를 하고 싶어하는 Object
         itemPos[index].position + ranVec,
          itemPos[index].rotation
          );
    }

    // Instantiate(missilePrefab, transform.position, transform.rotation);
    //구입 성공시, Instantiate()로 아잉템 생성
    //금액 부족시 return으로 구입로직 건너뛰기


    IEnumerator Talk()
    {
        talkText.text = talkDate[1];  //금액 부족 대사
        yield return new WaitForSeconds(2f); 

        talkText.text = talkDate[0];
    }
}

//UI, 애니메이터, 플레이어 담을 변수

//아이템 Prefabs, 가격, 위치 배열 변수

//금액 부족 유효성 검사 변수
