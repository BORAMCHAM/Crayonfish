using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Content;
using UnityEngine;

public enum HindranceType { Blowfish, Seahorse, Octopus, Fisher }

[System.Serializable]
public class HSpawnInfo
{
    public HindranceType type;
    public float spawnChance;
}

[System.Serializable]
public class HSpawnLevelData
{
    public int level;
    public float minInterval;
    public float maxInterval;
    public List<HSpawnInfo> spawnChances;
}

/// <summary>
/// [방해 요소 등장, 등장 주기 및 확률 등을 관리 담당하는 매니저 스크립트]
/// - 카메라 시야를 기준으로 동적 스폰 위치 계산하여 방해 요소 생성
/// </summary>
public class HindranceManager : MonoBehaviour
{
    [Header("Hindrance Prefabs")] 
    [SerializeField] [Tooltip("방해 요소 복어 프리팹")] private GameObject blowfishPrefab;
    [SerializeField] [Tooltip("방해 요소 해마 프리팹")] private GameObject seahorsePrefab;
    [SerializeField] [Tooltip("방해 요소 문어 프리팹")] private GameObject octopusPrefab;
    [SerializeField] [Tooltip("방해 요소 낚시꾼 프리팹")] private GameObject fisherPrefab;

    [Header("Spawn Setting")] 
    [SerializeField] [Tooltip("플레이어 레벨별 방해 요소 등장 정보")] private List<HSpawnLevelData> levelDataList;

    private float spawnTimer = 0f;
    private float currentSpawnInterval = 5f;
    private Camera mainCamera;
    
    public static HindranceManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        mainCamera = Camera.main;
        // 동시에 다른 방해 요소들의 스폰 타이머 시작
        SetNextSpawnInterval();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;
        
        // 타이머가 설정된 주기에 도달하면 방해 요소 스폰
        if (spawnTimer >= currentSpawnInterval)
        {
            spawnTimer = 0f; // 타이머 리셋
            SetNextSpawnInterval();
            TrySpawnRandomHindrance();
        }
    }

    private void SetNextSpawnInterval()
    {
        int level = GameManager.Instance.player.Level;
        var data = GetLevelData(level);
        if (data != null)
        {
            currentSpawnInterval = Random.Range(data.minInterval, data.maxInterval);
        }
        else
        {
            Debug.LogWarning($"[HindrancesManager] Level {level} 데이터가 없습니다.");
            currentSpawnInterval = 5f;
        }
    }
    
    /// <summary>
    /// 현재 플레이어 레벨 기준으로 등장 확률 적용하여 스폰 시도
    /// </summary>
    private void TrySpawnRandomHindrance()
    {
        int level = GameManager.Instance.player.Level;
        var data = GetLevelData(level);
        if (data == null) return;

        HindranceType? selectedType = 
            GetRandomHindranceTypeByProbability(data.spawnChances);

        if (selectedType.HasValue)
        {
            SpawnHindrance(selectedType.Value);
        }
    }

    private void SpawnHindrance(HindranceType type)
    {
        GameObject prefab = GetPrefabByType(type);
        if (prefab == null)
        {
            Debug.LogWarning($"[HindrancesManager] 프리팹이 설정되지 않은 타입 : {type}");
            return;
        }
        
        // 타입에 따라 카메라 바깥쪽 적절한 위치 계산
        Vector2 spawnPosition = GetSpawnPositionByType(type);
        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }

    private Vector2 GetSpawnPositionByType(HindranceType type)
    {
        Vector3 viewportPos = Vector3.zero;
        float padding = 0.05f; // 화면 가장자리에서 얼마나 안쪽으로 들어올지 (5%)

        switch (type)
        {
            case HindranceType.Octopus:
                viewportPos = new Vector2(Random.Range(0f, 1f), padding);
                break;
            case HindranceType.Seahorse:
                if (Random.value < 0.5f)
                    viewportPos = new Vector2(padding, Random.Range(0f, 1f));
                else
                    viewportPos = new Vector2(1 - padding, Random.Range(0f, 1f));
                break;
            case HindranceType.Fisher:
                viewportPos = new Vector2(Random.Range(0f, 1f), 1 - padding);
                break;
            case HindranceType.Blowfish:
                viewportPos = new Vector2(Random.Range(padding, 1 - padding), Random.Range(padding, 1 - padding));
                break;
        }
        
        viewportPos.z = 0 - mainCamera.transform.position.z;
        return mainCamera.ViewportToWorldPoint(viewportPos);
    }
    
    /// <summary>
    /// 등장 확률(가중치)에 따라 타입 무작위 선택
    /// </summary>
    private HindranceType? GetRandomHindranceTypeByProbability(List<HSpawnInfo> chances)
    {
        float total = chances.Sum(c => c.spawnChance);
        if (total <= 0f) return null;
        
        float r = Random.Range(0f, total);
        float cumulative = 0f;

        foreach (var info in chances)
        {
            cumulative += info.spawnChance;
            if (r <= cumulative) return info.type;
        }

        return null;
    }
    
    /// <summary>
    /// 타입에 따라 등록된 프리팹 반환
    /// </summary>
    private GameObject GetPrefabByType(HindranceType type)
    {
        return type switch
        {
            HindranceType.Octopus => octopusPrefab,
            HindranceType.Blowfish => blowfishPrefab,
            // HindranceType.Crab => crabPrefab,
            HindranceType.Seahorse => seahorsePrefab,
            HindranceType.Fisher => fisherPrefab,
            _ => null,
        };
    }

    /// <summary>
    /// 플레이어 레벨에 맞는 설정 데이터 반환
    /// </summary>
    private HSpawnLevelData GetLevelData(int level)
    {
        return levelDataList.FirstOrDefault(d => d.level == level);
    }
}
