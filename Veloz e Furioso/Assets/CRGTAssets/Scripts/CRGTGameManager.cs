using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

public class CRGTGameManager : MonoBehaviour {

	public static CRGTGameManager instance = null;

    float startGameSpeed = 1.0f;
    public float gameSpeed = 1.0f;


	int highGameScore = 0;
    int lastHighGameScore = 0;
	int lastGameScore = 0;
	int bonusGameCount = 0;

    public float spawnTime = 2.0f;
    public float spawnSpeed = 2.0f;

    internal bool isGameOver = true;
	internal bool isGamePaused = false; 

	int gameScore = 0;

    [Header("Spawn Time Management")]
    public float startSpawnSpeed = 2.0f;
    public float spawnStep = 0.05f;
    public float minSpawSpeed = 0.48f;
   

	[Header("Spawn Objects")]
	public RectTransform spawnLine;
    public float[] spawnObjectsXPos = new float[4] {-2.25f, -0.75f, 0.75f, 2.25f};
    public GameObject[] spawnGameObjects;
    private GameObject spawnObject;

    [Header("Sounds")]
    public AudioClip buttonClick;

    [Header("Texts")]
    public Text gameScoreText;
	public Text gameBestScoreText;
	public Text gameLastScoreText;
	public Text bonusGameCountText;
	public Text gameOverScoreText;
    public Text gameOverNewText;
	public Text gameOverHighScoreText;
	public Text gameOverBonusCountText;


    [Header("Menus")]
    public GameObject menuCanvas;
	public GameObject gameCanvas;
	public GameObject pauseCanvas;
	public GameObject gameOverCanvas;

    [Header("Quit")]
	public string gameOverURL = "http://u3d.as/JVh";



    void Awake() {
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);
	}

	void Start () {
		Time.timeScale = startGameSpeed;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        LoadGameData();
        ShowGameMenu();

        GoogleAdsManager.Instance.InterstialDoneCallback = InterstialDone;

    }

    void Update() {
        if ((!isGameOver) && (!isGamePaused))
        { 
			spawnTime -= Time.deltaTime;
			if (spawnTime <= 0.0f) {
				SpawnNewObject ();
                if (spawnSpeed > minSpawSpeed)
                    spawnSpeed -= spawnStep;
                else
                    spawnSpeed = minSpawSpeed;

				spawnTime = spawnSpeed;
				gameScore += (int)(1 * gameSpeed);
			}
		}
        UpdateGameData();
	}

	void SpawnNewObject()
	{
		float spawnObjectXPos = spawnObjectsXPos [Random.Range (0, spawnObjectsXPos.Length)];
		Vector3 spawnObjectPos = new Vector3 (spawnObjectXPos, spawnLine.position.y, 0);
		spawnObject = spawnGameObjects [Random.Range (0, spawnGameObjects.Length)];
		GameObject newEnemy = (GameObject)(Instantiate (spawnObject, spawnObjectPos, Quaternion.identity));

        newEnemy.transform.SetParent(spawnLine);
        newEnemy.transform.SetAsFirstSibling();
	}

    void CleanUpScene()
    {
        for (int i = 0; i < spawnLine.childCount; i++)
            Destroy(spawnLine.GetChild(i).gameObject);
    }

    void LoadGameData()
    {
#if UNITY_5_3_OR_NEWER
        // DELETE ALL GAME DATA !!!!! PlayerPrefs.DeleteAll();
        highGameScore = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "HIGH_GAMESCORE", 0);
        lastGameScore = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "LAST_GAMESCORE", 0);
        bonusGameCount = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "BONUS_GAMECOUNT", 0);
        lastHighGameScore = highGameScore;
#else
        // DELETE ALL GAME DATA !!!!! PlayerPrefs.DeleteAll();
        highGameScore = PlayerPrefs.GetInt(Application.loadedLevelName + "HIGH_GAMESCORE", 0);
        lastGameScore = PlayerPrefs.GetInt(Application.loadedLevelName + "LAST_GAMESCORE", 0);
        bonusGameCount = PlayerPrefs.GetInt(Application.loadedLevelName + "BONUS_GAMESCORE", 0);
        lastHighGameScore = highGameScore;
#endif
    }

    void UpdateGameData()
	{
        if (gameScore > highGameScore)
            highGameScore = gameScore;	

        if (!isGameOver)
        {
            gameScoreText.text = gameScore.ToString();
            gameBestScoreText.text = "Melhor: " + highGameScore.ToString();
            gameLastScoreText.text = "Útima: " + lastGameScore.ToString();
        }
        else
        {
            gameOverScoreText.text = "Pontuação: " + lastGameScore.ToString();
            gameOverHighScoreText.text = "Melhor Pontuação: " + highGameScore.ToString();
        }
	}

	void SaveGameData()
	{
#if UNITY_5_3_OR_NEWER
        PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "HIGH_GAMESCORE", highGameScore);
		PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "LAST_GAMESCORE", lastGameScore);
		PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "BONUS_GAMECOUNT", bonusGameCount);
#else
        PlayerPrefs.SetInt(Application.loadedLevelName + "HIGH_GAMESCORE", highGameScore);
        PlayerPrefs.SetInt(Application.loadedLevelName + "LAST_GAMESCORE", lastGameScore);
        PlayerPrefs.SetInt(Application.loadedLevelName + "BONUS_GAMECOUNT", bonusGameCount);
#endif
    }



    #region --------------- MENUS AND GAME CONTROL --------------- 
    public void ShowGameMenu()
    {
        gameOverCanvas.SetActive(false);
        pauseCanvas.SetActive(false);
        gameCanvas.SetActive(false);
        menuCanvas.SetActive(true);

        DisplayBanner(true);
    }

    public void ShowGamePlayMenu()
    {
        gameCanvas.SetActive(true);
        gameOverCanvas.SetActive(false);
        pauseCanvas.SetActive(false);
        menuCanvas.SetActive(false);

        DisplayBanner(false);

    }

    public void ShowPauseMenu()
    {
        gameCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        pauseCanvas.SetActive(true);
        menuCanvas.SetActive(false);

        DisplayBanner(true);
    }

    public void ShowGameOverMenu()
    {
        gameOverCanvas.SetActive(true);
        pauseCanvas.SetActive(false);
        gameCanvas.SetActive(false);
        menuCanvas.SetActive(false);

        DisplayBanner(true);
    }

    public void GameMenu()
    {
        Time.timeScale = startGameSpeed;
        ButtonSound();
        ShowGameMenu();
    }

    public void GameStart()
	{
        ButtonSound();
        if (isGamePaused)
            GameResume();

        isGameOver = false;
        gameScore = 0;

        spawnSpeed = startSpawnSpeed;
        spawnTime = spawnSpeed;

        RequestInterstial();
        DisplayInterstial();


        CleanUpScene();
        UpdateGameData();

        ShowGamePlayMenu();
      
        Time.timeScale = startGameSpeed;
    }

    public void GameRestart()
	{
        ButtonSound();
        GameStart();
	}

	public void GamePause()
	{
        ButtonSound();
        isGamePaused = true;
		Time.timeScale = 0;

        ShowPauseMenu();
	}

	public void GameResume()
	{
        ButtonSound();
        isGamePaused = false;
		Time.timeScale = startGameSpeed;

        ShowGamePlayMenu();
	}

    public void GameStop()
    {
        ButtonSound();
        GameOver();     
    }

	public void GameOver()
	{
		isGameOver = true;

        CleanUpScene();

        DisplayInterstial();

        lastGameScore = gameScore;
        UpdateGameData();
        SaveGameData();
        if (gameScore > lastHighGameScore)
        {
            gameOverNewText.text = "NOVA";
            lastHighGameScore = gameScore;
        }
        else
            gameOverNewText.text = "";
        gameScore = 0;
        ShowGameOverMenu();
       
	}

    public void GameQuit()
    {
        ButtonSound();
        SaveGameData();
        DisplayInterstial();
        GoogleAdsManager.Instance.displayInterstial = false;
    }

    public void GameQuitNow()
    {
        ButtonSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
            Application.OpenURL(gameOverURL);
#else
            Application.Quit();
#endif   
    }

    public void UpdateScore(int scoreValue)
    {
        gameScore += scoreValue;
        UpdateGameData();
    }

    void ButtonSound()
    {
        if (buttonClick != null)
            CRGTSoundManager.instance.PlaySound(buttonClick);
    }

    #endregion

    #region --------------- ADVERTISING ---------------

    public void InterstialDone()
    {
        GameQuitNow();
    }

    private void RequestInterstial()
    {
        GoogleAdsManager.Instance.RequestInterstitial();
    }

    private void DisplayInterstial()
    {
        if (GoogleAdsManager.Instance.displayInterstial)
            GoogleAdsManager.Instance.ShowInterstitial();
        else
            InterstialDone();
    }

    private void DisplayBanner(bool show)
    {
        if (GoogleAdsManager.Instance.displayBanner)
        {
            if (show)
                GoogleAdsManager.Instance.ShowBanner();
            else
                GoogleAdsManager.Instance.HideBanner();
        }
    }

    #endregion
}
