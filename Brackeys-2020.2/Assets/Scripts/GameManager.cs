﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private List<EnemyScorePair> _enemyScores;

    private static Dictionary<string, int> enemyScores;
    [HideInInspector]
    public static int score;

    [SerializeField]
    private VolumeProfile normalPostProcess;
    [SerializeField]
    private VolumeProfile warpPostProcess;

    private Volume cameraProfile;

    public static bool isRewinding;

    void Start()
    {
        enemyScores = new Dictionary<string, int>();
        foreach (EnemyScorePair esp in _enemyScores)
        {
            enemyScores.Add(esp.name, esp.value);
        }

        score = 0;

        cameraProfile = Camera.main.transform.GetComponent<Volume>();
    }

    void Update()
    {
        if (Input.GetButton("Rewind"))
        {
            isRewinding = true;
            cameraProfile.profile = warpPostProcess;
        }
        else
        {
            isRewinding = false;
            cameraProfile.profile = normalPostProcess;
        }
    }

    public static void EnemyKill(string tag)
    {
        int scoreGained = GameManager.enemyScores[tag];
        // TODO: update score text UI
        score += scoreGained;
    }
}

[System.Serializable]
public struct EnemyScorePair
{
    public string name;
    public int value;
}
