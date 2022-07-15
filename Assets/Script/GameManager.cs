using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject menuCam;
    public GameObject gameCam;
    public Player player;
    public Boss boss;
    public GameObject itemShop;
    public GameObject weaponShop;
    public GameObject startZone;
    public int stage;
    public float playTime;
    public bool isBattle;
    public int enemyCntA;
    public int enemyCntB;
    public int enemyCntC;
    public int enemyCntD;

    public Transform[] enemyZones; //몬스터 리스폰에 필요한 변수들 선언
    public GameObject[] enemies; //enemy 프리팹
    public List<int> enemyList;

    public GameObject menuPanel;
    public GameObject gamePanel;
    public GameObject overPanel;

    public Text maxScoreTxt; //좌측 상단
    public Text scoreTxt;

    public Text stageTxt;   //우측 상단
    public Text playTimeTxt;

    public Text playerHealthTxt; //좌측 하단
    public Text playerAmmoTxt;
    public Text playerCoinTxt;

    public Image weapon1Img; //중간 하단
    public Image weapon2Img;
    public Image weapon3Img;
    public Image weaponRImg;

    public Text enemyATxt; //우측 하단
    public Text enemyBTxt;
    public Text enemyCTxt;

    //보스가 나올때만 등장
    public RectTransform bossHealthGroup; // 중간 보스HP bossHealthBar가 자식으로 포함됨
    public RectTransform bossHealthBar; // 보스HP 감소 빨간 Bar
    public Text curScoreText;
    public Text bestText;

    void Awake()
    {
        enemyList = new List<int>(); //각 스테이지 마다 나타날 몬스터 저장
        maxScoreTxt.text = string.Format("{0:n0}", PlayerPrefs.GetInt("MaxScore"));

        //HasKey() 함수로 Key가 있는지 확인 후, 없다면 0으로 저장
        if(PlayerPrefs.HasKey("MaxScore"))
            PlayerPrefs.SetInt("MaxScore", 0);
    }

    public void GameStart()  //Game Start 버튼 실행시
    {
        menuCam.SetActive(false); 
        gameCam.SetActive(true);

        menuPanel.SetActive(false);
        gamePanel.SetActive(true);

        player.gameObject.SetActive(true); //플레이어 생성
    }

    public void GameOver()
    {
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        curScoreText.text = scoreTxt.text;  //GameMager 변수에 Ui가 들어있는데 = player score를 대입받음

        int MaxScore = PlayerPrefs.GetInt("MaxScore");
        if(player.score > MaxScore) {
            bestText.gameObject.SetActive(true);
            PlayerPrefs.GetInt("MaxScore", player.score);
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void StageStart()
    {
        itemShop.SetActive(false);
        weaponShop.SetActive(false);
        startZone.SetActive(false);

        //소환 Zone 시작할때 활성화, 종료시 비활성화
        foreach (Transform zone in enemyZones)
        {
            zone.gameObject.SetActive(true);
        }

        isBattle = true;
        StartCoroutine(InBattle());
    }

    public void StageEnd()
    {
        //플레이어 원위치
        player.transform.position = Vector3.up * 1.0f + Vector3.right * 6.7f; 

        itemShop.SetActive(true);
        weaponShop.SetActive(true);
        startZone.SetActive(true);

        //소환 Zone 종료시 비활성화
        foreach (Transform zone in enemyZones)
        {
            zone.gameObject.SetActive(false);
        }

        isBattle = false;
        stage++;
    }

    IEnumerator InBattle()
    {
        if(stage % 5 == 0) {
            enemyCntD++;
            GameObject instantEnemy = Instantiate(enemies[3],
                                                enemyZones[0].position,
                                                enemyZones[0].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.target = player.transform;
            enemy.manager = this;
            boss = instantEnemy.GetComponent<Boss>();
        } else {
                    //소환 리스트를 for문을 사용하여 데이터 채우기
        for (int index = 0; index < stage; index++)
        {
            int ran = Random.Range(0, 3);
            enemyList.Add(ran);

            //소환 리스트 데이터 만들 때 숫자까지 계산
            switch (ran) {
                case 0:
                    enemyCntA++;
                    break;
                case 1:
                    enemyCntB++;
                    break;
                case 2:
                    enemyCntC++;
                    break;
            }
          }  
        }

        //while문으로 지속적인 몬스터 소환
        //안전하게 while문을 돌리기위해선 꼭 yield return이 필요함
        while (enemyList.Count > 0) {
            int ranZone = Random.Range(0, 4);
            GameObject instantEnemy = Instantiate(enemies[enemyList[0]],
                                                enemyZones[ranZone].position,
                                                enemyZones[ranZone].rotation);
            Enemy enemy = instantEnemy.GetComponent<Enemy>();
            enemy.target = player.transform;
            enemy.manager = this;
            enemyList.RemoveAt(0); //생성 후 사용된 데이터 삭제
            yield return new WaitForSeconds(4f);
        }

        

        //남은 몬스터 숫자를 검사하는 while문 추가
        while (enemyCntA + enemyCntB + enemyCntC + enemyCntD > 0) {
            yield return null;
        }

        yield return new WaitForSeconds(4f);
        boss = null;
        StageEnd();
    }

    void Update() 
    {
        if (isBattle)
            playTime += Time.deltaTime; //초당 프레임수
    }


    void LateUpdate()
    {
        //상단 UI
        scoreTxt.text = string.Format("{0:n0}", player.score);
        stageTxt.text = "STAGE " + stage;
        //초단위 시간을 3600, 60으로 나누어 시분초로 계산
        int hour = (int)(playTime / 3600);  
        int min = (int)((playTime - hour * 3600) / 60); // 시간을먼저 계산하고 남은거에 60으로 나눔
        int second = (int)(playTime %  60);
        playTimeTxt.text = string.Format("{0:00}", hour) +
         ":" + string.Format("{0:00}", min) + ":" + string.Format("{0:00}", second);

        //플레이어 좌측하단 UI
        playerHealthTxt.text = player.health + " / " + player.maxHealth;
        playerCoinTxt.text = string.Format("{0:n0}", player.coin);
        if (player.equipWeapon == null) {
            playerAmmoTxt.text = "- / " + player.ammo;
        } else if (player.equipWeapon.type == Weapon.Type.Melee){
            playerAmmoTxt.text = "- / " + player.ammo;
        } else {
            playerAmmoTxt.text = player.equipWeapon.curAmmo + " / " + player.ammo;
        }

        //무기 UI
        //보유 상황에 따라 알파값만 변경
        weapon1Img.color = new Color(1, 1, 1, player.hasWeapons[0] ? 1 : 0);
        weapon2Img.color = new Color(1, 1, 1, player.hasWeapons[1] ? 1 : 0);
        weapon3Img.color = new Color(1, 1, 1, player.hasWeapons[2] ? 1 : 0);
        weaponRImg.color = new Color(1, 1, 1, player.hasGrenades > 0 ? 1 : 0);

        //몬스터 숫자 UI
        enemyATxt.text = enemyCntA.ToString();
        enemyBTxt.text = enemyCntB.ToString();
        enemyCTxt.text = enemyCntC.ToString();

        //보스 체력 UI
        if(boss != null) {
            bossHealthGroup.anchoredPosition = Vector3.down * 30; //Ui 위치 조정 현위치
            bossHealthBar.localScale = new Vector3((float)boss.curHealth / boss.maxHealth, 1, 1);
        } else {
            bossHealthGroup.anchoredPosition = Vector3.up * 200;
        }
            
        //int 형태끼리 연산하면 결과값도 int이므로 주의
        //보스 체력 이미지의 Scale을 남은 체력 비율에 따라 변경
    }
}
//필요한 모든 변수를 public으로 선언
//public으로 선언한 변수들을 Player에서 선언한 변수를 대입해서 실시간으로 변하는걸 볼 수 있게해줌
//PlayerPrefs에서 저장된 데이터 불러오기
//string.Format() 함수로 문자열 양식 적용
//LateUpdate(): Update()가 끝난 후 호출되는 생명주기


//String.Format으로 숫자에 쉼표 추가
// String.Format("{0:n}", 1234);  // Output: 1,234.00
// String.Format("{0:n0}", 9876); // No digits after the decimal point. Output: 9,876