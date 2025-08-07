using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public const int MaxLevel = 9;    // 최대 레벨
        private static readonly int[] ExpTable = new int[]
    {
        1000,  // 1레벨
        2000,  // 2레벨
        4000,  // 3레벨
        7000,  // 4레벨
        10000, // 5레벨
        15000, // 6레벨
        20000, // 7레벨 
        27000, // 8레벨 
        35000  // 9레벨 
    };
    public int Score { get; private set; } // 플레이어의 점수
    private int ScoreForLevel; // 다음 레벨에 필요한 점수
    public int Level { get; private set; } // 플레이어의 레벨
    public float Speed { get; set; } // 플레이어의 이동 속도
    public bool Isinvincibility { get; set; } // 플레이어의 무적 상태

    public int CumScore { get; private set; } // 플레이어 누적 점수
    public event Action OnLevelUp; //레벨업 시 이벤트
    public event Action<int> OnCurrentScore;
    public event Action<int> OnDie; // 죽을 때 점수 전달 이벤트 

    public int CurrentExp => Score;
    public int MaxExp => ScoreForLevel;
    public float ExpProgress => Mathf.Clamp01((float)Score / ScoreForLevel);

    public Player(int score, int level, int speed)
    {
        // 기본값 설정
        Speed = 4f;
        Isinvincibility = false;
        Score = score;
        Level = level;
        ScoreForLevel = CalculateScoreForNextLevel(Level);
    }

    /// <summary>
    ///  점수를 얻는 메서드
    /// <param name="score">획득한 점수</param>
    /// </summary>
    public void PlayerGetScore(int score)
    {
        Score += score;
        CumScore += score;
        //점수 갱신 시 이벤트 전달 
        OnCurrentScore?.Invoke(CumScore);
        // AudioManager.Instance.PlaySfx(0);

        //레벨업 조건 달성 시 레벨업
        // Level < MaxLevel일 때만 레벨업 시도
        while (Level < MaxLevel && Score >= ScoreForLevel)
        {
            PlayerLevelUp();
        }
    }

    /// <summary>
    /// 플레이어 무적 함수
    /// </summary>
    public void PlayerInvincibility()
    {
        Isinvincibility = true;
        // Debug.Log("Player 무적 상태");
    }

    public void PlayerDie()
    {
        // 플레이어가 죽었을 때의 처리
        // AudioManager.Instance.PlaySfx(4);
        // 무적 상태 해제, 점수 초기화 등
        // Debug.Log("Player 죽음");

        // 점수 저장
        PlayerPrefs.SetInt("LastScore", CumScore);

        // 점수 3위까지 저장 후 정렬
        List<int> scores = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            scores.Add(PlayerPrefs.GetInt($"HighScore{i}", 0));
        }
        scores.Add(CumScore);
        scores.Sort((a, b) => b.CompareTo(a)); // 내림차순 정렬
        // 정렬 후 저장 
        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.SetInt($"HighScore{i}", scores[i]);
        }
        PlayerPrefs.Save();


        // Debug.Log($"죽을 때 점수: {CumScore}"); // 죽을 때 점수 로그 출력
        int savedScore = PlayerPrefs.GetInt("LastScore", 0);
        // Debug.Log($"저장된 점수: {savedScore}"); // 저장된 점수 로그 출력
        
        // ------- 토스에 점수 저장
        // TossBridge.SubmitScore(CumScore);
        // 스코어 전달 
        OnDie?.Invoke(CumScore);

        Isinvincibility = false;
        Score = 0;
        CumScore = 0; // 누적 점수 
        Level = 1; // 레벨 초기화
        ScoreForLevel = CalculateScoreForNextLevel(Level);

    }


    /// <summary>
    ///  플레이어 레벨업 메서드
    /// </summary>
    private void PlayerLevelUp()
    {
        if (Level >= MaxLevel)
        {
            // 이미 최대 레벨 도달했으면 더 이상 레벨업하지 않음
            return;
        }

        //현재 점수 초기화
        Score -= ScoreForLevel;
        Level++;
        //레벨업 후 필요 경험치 늘리기
        ScoreForLevel = CalculateScoreForNextLevel(Level);

        // 속도 증가
        Speed += 0.5f;

        // 이벤트 호출
        OnLevelUp?.Invoke();
        // AudioManager.Instance.PlaySfx(3);
        Debug.Log("Player Level Up! 현재 Level: " + Level + ", 현재 Score: " + Score);
    }

    /// <summary>
    /// 다음 레벨에 필요한 점수를 계산하는 메서드
    /// </summary>
    /// <param name="level">현재 레벨</param>
    public int CalculateScoreForNextLevel(int level)
    {        
        if (level >= 1 && level <= MaxLevel)
            return ExpTable[level - 1];
        else
            return int.MaxValue;  // 이미 최대 레벨이거나 범위를 벗어나는 경우
    }

}
