using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    public  Transform viewPoint;
    public  float     mouseSensitivity = 1f;    // 마우스 감도
    private float     verticalRotStore;         // 화면상하 증감값 저장용도
    private Vector2   mouseInput;               // 마우스 이동값

    public bool invertLook;                     // 상하반전              

    public  float   moveSpeed = 5f, runSpeed = 8f;
    private float   activeMoveSpeed;
    private Vector3 moveDir, movement;

    public CharacterController charCon;

    private Camera cam;

    public float jumpForce = 12f, gravityMod = 2.5f;

    public  Transform groundCheckPoint;         // 발위치(0.95)
    private bool      isGrounded;           
    public  LayerMask groundLayers;             // 본인 클라이언트 Raycast가 히트할 레이어(그라운드, Player) -> 본인클라 레이어 Player(Mine)제외

    public  GameObject bulletImpact;
    private float shotCounter;
    public  float muzzleDisplayTime;
    private float muzzleCounter;

    public  float maxHeat = 10f, coolRate = 4f, overheatCoolRate = 5f;
    private float heatCounter;
    private bool  overHeated;

    public Gun[] allGuns;
    private int selectedGun;

    public GameObject playerHitImpact;

    public int maxHealth = 100;
    private int currentHealth;

    public Animator anim;
    public GameObject playerModel;
    public Transform modelGunPoint, gunHolder;

    public Material[] allSkins;

    public float adsSpeed = 5f;
    public Transform adsOutPoint, adsInPoint;

    public AudioSource footstepSlow, footstepFast;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        cam = Camera.main;

        //UIController.instance.weaponTempSlider.maxValue = maxHeat;

        photonView.RPC("SetGun", RpcTarget.All, selectedGun); // 자신의 클라 및 다른 클라들의 모든 플레이어 총 설정

        currentHealth = maxHealth;

        //그리고 플레이어가 호출될 때 마다, 체력이 다시 채워지는 오류를 방지하기 위해, 자기 자신의 UIController.instance만 초기화 해주도록, 넣어둔다.
        if(photonView.IsMine)
        {
            playerModel.SetActive(false);
        
            // UIController.instance.healthSlider.maxValue = maxHealth;
            // UIController.instance.healthSlider.value = currentHealth;
        } 
        else
        {
            gunHolder.parent = modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }

        playerModel.GetComponent<Renderer>().material = allSkins[photonView.Owner.ActorNumber % allSkins.Length];
    }

    void Update()
    {
        // PhotonView = 타입변수 // photonView = 해당 오브젝트에 첨부된 컴포넌트 제어
        // 자기 자신이 마스터인 오브젝트만 제어
        if (photonView.IsMine)
        {
            // 마우스 이동시 인풋값 +-됨. 민감도를 곱한 값 VECTOR2 받고,
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
            // 이동한 Vector.X값 만큼, (스크립트가 들어가 있는 부모 Plyaer)플레이어의 Y회전값을 바꿔서, 좌우화면 이동
            // 좌우(y값에 마우스인풋X를 더해줌)
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
            // 이동한 Vector.Y값 만큼, (자식인) viewPoint의 X회전값을 바꿔서, 상하화면 이동
            // 상하(x값에 마우스인풋Y를 빼줌)  + 상하 회전값 제한
            // viewPoint.transform.rotation.eulerAngles.x - mouseInput.y은 오브젝트에선 60까지 가다가 이상하게 -60으로 가지만,
            // Debug(viewPoint.transform.rotation.eulerAngles.x - mouseInput.y)로 보면, 오일러 360까지 간다.
            // 그래서, 인풋값을 += 받아서, 값을 더하고, 직접 각도 제한을 주고, 상하값을 조정해준다.
            verticalRotStore += mouseInput.y;
            verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

            if (invertLook)
            {
                viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }
            else
            {
                viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
            }

            moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

            if (Input.GetKey(KeyCode.LeftShift))
            {
                activeMoveSpeed = runSpeed;

                if(!footstepFast.isPlaying && moveDir != Vector3.zero)
                {
                    footstepFast.Play();
                    footstepSlow.Stop();
                }
            }
            else
            {
                activeMoveSpeed = moveSpeed;

                if (!footstepSlow.isPlaying && moveDir != Vector3.zero)
                {
                    footstepFast.Stop();
                    footstepSlow.Play();
                }
            }

            if(moveDir == Vector3.zero || !isGrounded)
            {
                footstepSlow.Stop();
                footstepFast.Stop();
            }
            
            // 중력(Y)
            // ★ normalized전 movement.y값 저장(방향 forward right에 입력키 moveDir이 곱해지는, movement.x 와 movement.z와 다르게
            // movement.y는 키 입력이 아닌, 자체적으로 -값을 가지고 있음. 하지만 normalized하면 값이 0이 됨. 하이어라키 디버그로 확인 가능)
            // 물체가 회전되도, transform.forward와 transform.right는 바뀌지 않음
            float yVel = movement.y;
            //          앞뒤(Z) * +-1                       좌우(X) * +-1                // 대각선 속도조절
            movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized * activeMoveSpeed;
            movement.y = yVel;  // ★ normalized안 된  movement. 불러오기

            if (charCon.isGrounded)
            {
                movement.y = 0f;
            }

            isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                movement.y = jumpForce;
            }

            movement.y += Physics.gravity.y * Time.deltaTime * gravityMod;

            charCon.Move(movement * Time.deltaTime);

            if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
            {
                muzzleCounter -= Time.deltaTime;
            
                if (muzzleCounter <= 0)
                {
                    allGuns[selectedGun].muzzleFlash.SetActive(false);
                }
            }
            
            if (!overHeated)
            {
            
                if (Input.GetMouseButtonDown(0))
                {
                    Shoot();
                }
            
                if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
                {
                    shotCounter -= Time.deltaTime;
            
                    if (shotCounter <= 0)
                    {
                        Shoot();
                    }
                }
            
                heatCounter -= coolRate * Time.deltaTime;
            }
            else
            {
                heatCounter -= overheatCoolRate * Time.deltaTime;
                if (heatCounter <= 0)
                {
                    overHeated = false;
            
                    UIController.instance.overheatedMessage.gameObject.SetActive(false);
                }
            }
            
            if (heatCounter < 0)
            {
                heatCounter = 0f;
            }
            //UIController.instance.weaponTempSlider.value = heatCounter;
            
            
            if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
            {
                selectedGun++;
            
                if (selectedGun >= allGuns.Length)
                {
                    selectedGun = 0;
                }
                SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            
            }
            else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
            {
                selectedGun--;
            
                if (selectedGun < 0)
                {
                    selectedGun = allGuns.Length - 1;
                }
                SwitchGun();
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            }

            for (int i = 0; i < allGuns.Length; i++)
            {
                if (Input.GetKeyDown((i + 1).ToString()))
                {
                    selectedGun = i;
                    //SwitchGun();
                    photonView.RPC("SetGun", RpcTarget.All, selectedGun);
                }
            }

            anim.SetBool("grounded", isGrounded);
            anim.SetFloat("speed", moveDir.magnitude);

            if(Input.GetMouseButton(1))
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, allGuns[selectedGun].adsZoom, adsSpeed * Time.deltaTime);
                gunHolder.position = Vector3.Lerp(gunHolder.position, adsInPoint.position, adsSpeed * Time.deltaTime);
            } else
            {
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, adsSpeed * Time.deltaTime);
                gunHolder.position = Vector3.Lerp(gunHolder.position, adsOutPoint.position, adsSpeed * Time.deltaTime);
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                if (Input.GetMouseButtonDown(0) && !UIController.instance.optionsScreen.activeInHierarchy)
                {
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            if (MatchManager.instance.state == MatchManager.GameState.Playing)
            {
                cam.transform.position = viewPoint.position;
                cam.transform.rotation = viewPoint.rotation;
            } 
            else
            {
                cam.transform.position = MatchManager.instance.mapCamPoint.position;
                cam.transform.rotation = MatchManager.instance.mapCamPoint.rotation;
            }
        }
    }
    
    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));
        ray.origin = cam.transform.position;
    
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            //Debug.Log("We hit " + hit.collider.gameObject.name);
    
            if (hit.collider.gameObject.tag == "Player")
            {
                Debug.Log("Hit " + hit.collider.gameObject.GetPhotonView().Owner.NickName);
    
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);
    
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, allGuns[selectedGun].shotDamage, PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
    
                GameObject bulletImpactObject = Instantiate(bulletImpact, hit.point + (hit.normal * .002f), Quaternion.LookRotation(hit.normal, Vector3.up));
    
                Destroy(bulletImpactObject, 10f);
            }
        }
    
        shotCounter = allGuns[selectedGun].timeBetweenShots;
    
    
        heatCounter += allGuns[selectedGun].heatPerShot;
        if(heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
    
            overHeated = true;
    
            UIController.instance.overheatedMessage.gameObject.SetActive(true);
        }
    
        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    
        allGuns[selectedGun].shotSound.Stop();
        allGuns[selectedGun].shotSound.Play();
    }

    // ☆ 다른 클라이언트의 Shoot에 의해서, 다른 클라이언트의 자신의 클론 player에 의해서 모든 자신의 player에게 전달되기 때문에, if (photonView.IsMine) 필요 ---> 비교 SetGun와 DealDamage
    // ☆ currentHealth가 플레이어 캐릭터는 Mine의 체력만 감소
    [PunRPC]
    public void DealDamage(string damager, int damageAmount, int actor)
    {
        TakeDamage(damager, damageAmount, actor);
    }
    
    public void TakeDamage(string damager, int damageAmount, int actor)
    {
        if (photonView.IsMine)
        {
            currentHealth -= damageAmount;
    
            if (currentHealth <= 0)
            {
                currentHealth = 0;
    
                PlayerSpawner.instance.Die(damager);
    
                MatchManager.instance.UpdateStatsSend(actor, 0, 1);
            }
            //UIController.instance.healthSlider.value = currentHealth;
        }
    }
    
    void SwitchGun()
    {
        foreach(Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
    
        allGuns[selectedGun].gameObject.SetActive(true);
    
        allGuns[selectedGun].muzzleFlash.SetActive(false);
    }
    
    [PunRPC]
    public void SetGun(int gunToSwitchTo)
    {
        if(gunToSwitchTo < allGuns.Length)
        {
            selectedGun = gunToSwitchTo;
            SwitchGun();
        }
    }
}
