using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{

    [SerializeField] private List<GameObject> _AllRespawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Awake()
    {
        _AllRespawnPoints = new List<GameObject> { };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddRespawnPoint(GameObject RespawnPoint)
    {
        _AllRespawnPoints.Add(RespawnPoint);
    }

    public GameObject GetRespawnPoint()
    {
        Debug.Log(_AllRespawnPoints.Count);
        return _AllRespawnPoints[Random.Range(0, _AllRespawnPoints.Count)];
    }

    public void RemoveSpawnPoint()
    {
        _AllRespawnPoints.Clear();
    }
}
