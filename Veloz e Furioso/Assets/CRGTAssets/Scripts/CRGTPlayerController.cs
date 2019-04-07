using UnityEngine;
using System.Collections;

public enum ControlType
{
    ACCELEROMETER,
    TOUCH
}

public class CRGTPlayerController : MonoBehaviour {

    public ControlType playerControlType;
    public static bool isAlive;
    public static float playerSpeed = 1.0f;
    public float playerSideSpeed = 6.0f;
    public bool movePlayerRot = true;
    public float playerRot = 5.0f;
    public float playerRotSpeed = 4.0f;

    public float[] spawnPlayerXPos = new float[4] { -2.25f, -0.75f, 0.75f, 2.25f };
    public float playerMinX = -2.8f;
    public float playerMaxX = 2.8f;

    public GameObject crashParticles;
    public GameObject turboEffect;
    public ParticleSystem turboFlareL;
    public ParticleSystem turboFlareR;
    public ParticleSystem turboCoreL;
    public ParticleSystem turboCoreR;
    public float rotationMultiplier = 2.1f;

    public AudioClip carCrash;

    private float playerPosY;
    private float playerVelX;


    private Rigidbody2D rigBody2D;

    void Start()
    {
        isAlive = true;
        rigBody2D = GetComponent<Rigidbody2D>();

        playerPosY = rigBody2D.position.y;
        playerVelX = 0;
        rigBody2D.position = new Vector3(spawnPlayerXPos[Random.Range(0, spawnPlayerXPos.Length)], playerPosY, 0f);

    }

    void FixedUpdate()
    {
#if UNITY_ANDROID || UNITY_IOS
#if UNITY_EDITOR
        playerVelX = Input.GetAxis("Horizontal");
#else
		if (playerControlType == ControlType.ACCELEROMETER)
        {
            playerVelX = Input.acceleration.x;
        }
        else
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    if (touch.position.x < Screen.width / 2f)
                    {
                        playerVelX = -1;
                    }
                    else
                    {
                        playerVelX = 1;
                    }
                }
            }
            else
                playerVelX = 0;
        }
        
#endif
#else
		playerVelX = Input.GetAxis("Horizontal");
#endif

        rigBody2D.velocity = new Vector3(playerVelX, 0, 0) * playerSideSpeed;
        rigBody2D.position = new Vector3(Mathf.Clamp(rigBody2D.position.x, playerMinX, playerMaxX), playerPosY, 0f);
        if (movePlayerRot)
        {
            float num = 0.0f;
            if (rigBody2D.position.x < playerMaxX && rigBody2D.position.x > playerMinX)
            {
                num = rigBody2D.velocity.x;
            }
            rigBody2D.rotation = Mathf.Lerp(rigBody2D.rotation, num * -playerRot, Time.deltaTime * playerRotSpeed);
        }

    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Car")
        {
            if (carCrash)
                CRGTSoundManager.instance.PlaySound(carCrash);
            CRGTGameManager.instance.GameOver();
        }
    }

}
