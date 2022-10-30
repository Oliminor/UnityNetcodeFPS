using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{

    [SerializeField] List<GameObject> _AllRespawnPoints;

    // Start is called before the first frame update
    void Start()
    {
        
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
        return _AllRespawnPoints[Random.Range(0, _AllRespawnPoints.Count-1)];
    }
}
