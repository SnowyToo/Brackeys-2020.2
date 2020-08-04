﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Score
    [SerializeField]
    private List<EnemyScorePair> _enemyScores;
    [SerializeField]
    private List<AttributeScorePair> _attributeScores;

    private static Dictionary<string, int> enemyScores;
    private static Dictionary<Enemy.AttributeType, int> attributeScores;
    [HideInInspector]
    public static int score;

    private Volume cameraProfile;

    // Gameplay
    [HideInInspector]
    public static bool isRewinding;

    [HideInInspector]
    public static GameObject playerObject;
    [HideInInspector]
    public static GameObject photonObject;
    [HideInInspector]
    public static Player player;
    [HideInInspector]
    public static Photon photon;
    [SerializeField]
    private Collider2D mirrorCollider;

    [HideInInspector]
    public static bool gameIsOver;

    //Enemies
    [HideInInspector]
    public static EnemySpawner enemySpawner;

    // Visuals & Audio
    [SerializeField]
    private VolumeProfile normalPostProcess;
    [SerializeField]
    private VolumeProfile warpPostProcess;
    public static CameraShake camShake;
    public static UIManager uiManager;
    public static AudioManager audioManager;

    private AudioSource bgm;
    private AudioLowPassFilter lowPass;

    public static GameManager ins;

    void Awake()
    {
        playerObject = GameObject.FindWithTag("Player");
        photonObject = GameObject.FindWithTag("Photon");
        player = playerObject.GetComponent<Player>();
        
        photon = photonObject.GetComponent<Photon>();
        enemySpawner = GetComponent<EnemySpawner>();
        camShake = Camera.main.GetComponent<CameraShake>();
        uiManager = GetComponent<UIManager>();
        audioManager = GameObject.FindWithTag("AudioManager").GetComponent<AudioManager>();

        bgm = GetComponent<AudioSource>();
        lowPass = GetComponent<AudioLowPassFilter>();
        gameIsOver = false;

        score = 0;
    }

    void Start()
    {
        enemyScores = new Dictionary<string, int>();
        attributeScores = new Dictionary<Enemy.AttributeType, int>();
        foreach (EnemyScorePair esp in _enemyScores)
        {
            enemyScores.Add(esp.name, esp.value);
        }
        foreach(AttributeScorePair asp in _attributeScores)
        {
            attributeScores.Add(asp.type, asp.value);
        }

        score = 0;

        uiManager.SetUpUI(player.maxHealth);

        cameraProfile = Camera.main.transform.GetComponent<Volume>();
    }

    void Update()
    {
        if (!gameIsOver)
        {
            if (Input.GetButton("Rewind") && !photon.captured)
                RewindTime();
            else
                RegularTime();
        }
        else
        {
            RegularTime();
            if (Input.GetKeyDown("m"))
                LoadScene(0);
            else if (Input.GetKeyDown("r"))
                LoadScene(1);
        }
    }

    void RegularTime()
    {
        isRewinding = false;
        cameraProfile.profile = normalPostProcess;
        if(!gameIsOver)
            mirrorCollider.enabled = true;

        audioManager.SetNormalMusic();
    }

    void RewindTime()
    {
        isRewinding = true;
        cameraProfile.profile = warpPostProcess;
        mirrorCollider.enabled = false;

        audioManager.SetTimeWarpMusic();
    }

    public static void EnemyKill(Enemy enemy)
    {
        int scoreGain = enemyScores[enemy.tag];
        foreach(EnemyAttribute e in enemy.attributes)
        {
            scoreGain += attributeScores[e.type] * e.amount;
        }

        score += scoreGain;

        uiManager.UpdateScore(score);
    }

    public static void CameraShake(float dur, float mag)
    {
        camShake.Shake(dur, mag);
    }

    public static void GameOver()
    {
        photon.Die();
        enemySpawner.enabled = false;
        gameIsOver = true;
        uiManager.GameOverUI(score);
    }

    public static Vector2 GetMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public static void SpawnParticles(GameObject particles, GameObject source)
    {
        GameObject part = Instantiate(particles, source.transform.position, Quaternion.identity);
        Destroy(part.gameObject, 4f);
    }

    public static void PlaySound(AudioClip clip, GameObject source, float volume = 0.8f, float pitch = 1f)
    {
        audioManager.PlaySound(clip, source, volume, pitch);
    }

    public static void PlaySound(AudioClip[] clips, GameObject source, float volume = 0.8f, float pitch = 1f)
    {
        PlaySound(clips[Random.Range(0, clips.Length)], source, volume, pitch);
    }

    public static void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public static void LoadScene(string name)
    {
        LoadScene(SceneManager.GetSceneByName(name).buildIndex);
    }
}

[System.Serializable]
public struct EnemyScorePair
{
    public string name;
    public int value;
}

[System.Serializable]
public struct AttributeScorePair
{
    public Enemy.AttributeType type;
    public int value;
}