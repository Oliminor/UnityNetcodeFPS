using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{

    private GameObject _ObjectiveManager;
    // Start is called before the first frame update
    void Start()
    {
        _ObjectiveManager = GameObject.Find("ObjectiveManager");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (other.transform.GetChild(2).GetChild(0).GetComponent<WeaponInventory>().CheckIfPlayerHasThatWeapon(3))
            {
                other.transform.GetChild(2).GetChild(0).GetComponent<WeaponInventory>().DestroyCurrentObject();
                _ObjectiveManager = GameObject.Find("ObjectiveManager");
                other.gameObject.GetComponent<PlayerStats>().AddScoreServerRPC(1);
                _ObjectiveManager.GetComponent<ObjectiveManager>().FlagCapturedServerRPC();
            }
        }
    }
}
