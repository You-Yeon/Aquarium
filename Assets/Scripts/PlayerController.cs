﻿using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

// 플레이어 캐릭터를 사용자 입력에 따라 움직이는 스크립트
public class PlayerController : MonoBehaviour {

    private float moveSpeed; // 앞뒤 움직임의 속도
    private float mouseSpeed; // 마우스 속도

    private float fireRate; // 발사 딜레이
    private float fireTimer; // 타이머

    private float accuracy; // 산탄 거리

    public CinemachineVirtualCamera vcam; // 추적 카메라

    public GameObject bulletPrefab; // 총알 Prefab
    public GameObject shootPoint; // 발사 지점

    private PlayerInput playerInput; // 플레이어 입력을 알려주는 컴포넌트
    private Rigidbody playerRigidbody; // 플레이어 캐릭터의 리지드바디
    private Animator playerAnimator; // 플레이어 캐릭터의 애니메이터

    private int bulletsPerMag; // 탄창 속 총알 수
    private int bulletsTotal; // 총 총알 수
    private int currentBullets; // 현재 탄창의 총알 수

    public Transform RayPoint; // 레이캐스트 시작 지점
    public float range; // 레이캐스트 범위

    public AudioSource audioSource_walk; // 걷는 소리
    public AudioSource audioSource_fire; // 발사 소리


    private void Start() {
        // 사용할 컴포넌트들의 참조를 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();

        bulletsPerMag = 25; // 한 탄창의 수 
        bulletsTotal = 125; // 전체 - 한 탄창
        currentBullets = bulletsPerMag; // 현재 총알 수

        moveSpeed = 5f; // 앞뒤 움직임의 속도
        mouseSpeed = 2.0f; // 마우스 속도

        fireRate = 0.3f; // 발사 딜레이
        fireTimer = 0f; // 타이머

        accuracy = 0f; // 초기 값은 0

        vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y = 1.9f; // 초기 화면 y값

        UI_Controller.ui_instance.bulletsText.text = currentBullets + " / " + bulletsTotal; // UI 총알 개수 반영
    }

    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨
    private void FixedUpdate() {

        // 움직임 실행
        Move();

        // 발사
        Fire();

        // 마우스 회전
        Mouse();

        // 입력 값에 따라 애니메이터의 Move 파라미터 값 변경
        playerAnimator.SetFloat("Vertical", playerInput.move);
        playerAnimator.SetFloat("Horizontal", playerInput.rotate); 

        if (fireTimer < fireRate)
        {
            fireTimer += Time.deltaTime;
        }
    }

    // 입력값에 따라 캐릭터를 앞뒤로 움직임
    private void Move()
    {

        // 이동 방향 벡터 계산
        Vector3 moveDir = (playerInput.move * transform.forward) + (playerInput.rotate * transform.right);

        // 상대적으로 이동할 거리 계산
        Vector3 moveDistance = moveDir.normalized * moveSpeed * Time.deltaTime;

        // 리지드바디를 이용해 게임 오브젝트 위치 변경
        playerRigidbody.MovePosition(playerRigidbody.position + moveDistance);

        if (!audioSource_walk.isPlaying) // 걷는 사운드가 나오지 않으면
        {
            if (playerInput.move != 0 || playerInput.rotate != 0 ) // 걷고 있다면
            {
                audioSource_walk.Play(); // 사운드 플레이
            }
        }

        if (playerInput.move == 0 && playerInput.rotate == 0) // 움직이지않으면
        {
            audioSource_walk.Stop(); // 사운드 스탑
        }

    }

    private void Fire()
    {
        if (fireTimer < fireRate) //마지막 발사 시간 간격이 fireRate보다 작으면 return
        {
            return;
        }

        if (currentBullets == 0) // 총알이 없으면 return
        {
            return;
        }

        if (playerInput.fire) // 발사 버튼.
        {
            if (playerInput.move == 0 && playerInput.rotate == 0) // 산탄 효과 비활성
            {
                accuracy = 0f;
            }
            else // 산탄 효과 활성
            {
                accuracy = 0.02f;
            }


            playerAnimator.CrossFadeInFixedTime("Fire", 0.01f); // 발사 애니메이션

            RaycastHit hit;
            Debug.DrawRay(RayPoint.position, RayPoint.transform.forward * range+ Random.onUnitSphere * accuracy , Color.blue, 0.3f); // 레이케스트 발사
            Physics.Raycast(RayPoint.position, RayPoint.transform.forward + Random.onUnitSphere * accuracy, out hit, range); // 레이케스트 발사

            GameObject bullet = Instantiate(bulletPrefab, shootPoint.transform.position, shootPoint.transform.rotation); // 총알 생성
            bullet.transform.LookAt(hit.point);

            fireTimer = 0.0f; // 시간 리셋

            audioSource_fire.Play(); // 발사 소리

            currentBullets--; // 총알 초기화
            UI_Controller.ui_instance.bulletsText.text = currentBullets + " / " + bulletsTotal; // UI 총알 개수 반영

            Debug.Log("ray : " + hit.point);
        }
    }

    private void Mouse()
    {
        playerRigidbody.rotation = playerRigidbody.rotation * Quaternion.Euler(Vector3.up * mouseSpeed * playerInput.mouseX); // X

        if ((vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y -= playerInput.mouseY / 10 )<= 3f && (vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y -= playerInput.mouseY / 10 )>= 1f)
        {
            vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y -= playerInput.mouseY / 10; // Y 
        }
        else if (vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y > 3f)
        {
            vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y = 3f;
        }
        else if (vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y < 1f)
        {
            vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y = 1f;
        }

        Debug.Log(" cam : " + vcam.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset.y);
        Debug.Log("mouse y : " + playerInput.mouseY);
    }
}