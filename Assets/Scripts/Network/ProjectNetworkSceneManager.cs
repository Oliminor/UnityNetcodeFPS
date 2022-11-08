using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using TMPro; 

public class ProjectNetworkSceneManager : NetworkBehaviour
{
    public static ProjectNetworkSceneManager singleton;
    private string sceneName;
    public Scene loadedScene;
    
    public static List<string> sceneNames = new List<string>();
    public NetworkVariable<int> playersConnected = new NetworkVariable<int>();
    private NetworkVariable<int> playersLoadedInScene = new NetworkVariable<int>();
    
    public TextMeshProUGUI playersConnectedText;
    public TextMeshProUGUI playersLoadedText;


    private void Start()
    {
        singleton = this;

        sceneNames = SceneNamesForEasySwapping();
        
        foreach(string scene in sceneNames)
        {
            Debug.Log(scene);
        }
        DontDestroyOnLoad(this);

        NetworkManager.Singleton.OnClientConnectedCallback += (id) => { if (NetworkManager.Singleton.IsServer) playersConnected.Value++; };

        NetworkManager.Singleton.OnClientDisconnectCallback += (id) => { if (NetworkManager.Singleton.IsServer) playersConnected.Value--; };

    }
    private void Update()
    {
        playersConnectedText.text = playersConnected.Value.ToString();
        //playersLoadedText.text = playersLoadedInScene.Value.ToString();
    }


    public void SwitchScenes()
    {
        NetworkManager.SceneManager.LoadScene("Ben", LoadSceneMode.Single);
        if (IsServer)
        {
            playersLoadedInScene.Value=0;
        }
        
    }
    public void ExitGameMode()
    {
        NetworkManager.SceneManager.LoadScene("Ollie", LoadSceneMode.Single);
        if (IsServer)
        {
            playersLoadedInScene.Value = 0;
        }
    }
    private void CheckLoadStatus(SceneEventProgressStatus loadStatus, bool isLoading = true) //currently seems useless, but will see
    {
        string sceneEventAction;
        if (isLoading)
        {
            sceneEventAction = "load";
        }
        else
        {
            sceneEventAction = "unload";
        }
        if (loadStatus != SceneEventProgressStatus.Started)
        {
            Debug.LogWarning($"Failed to {sceneEventAction} {sceneName} with" +
                $" a {nameof(SceneEventProgressStatus)}: {loadStatus}");
        }
    }
    public void SceneManager_OnSceneEvent(SceneEvent sceneEvent)
    {
        switch (sceneEvent.SceneEventType) //put things in this switch case that you want to happen at specific points (for example, set the player positions on the LoadComplete case)
        {
            case SceneEventType.Load: //start loading for everyone
                {
                    var loadAsyncOp = sceneEvent.AsyncOperation;
                    if (IsServer)
                    {
                        Debug.Log("hello");
                    }
                    else
                    {
                        
                    }
                    break;
                }
            case SceneEventType.Unload: //start unloading for everyone
                if (IsServer)
                {

                }
                else
                {

                }
                break;
            case SceneEventType.LoadEventCompleted: //when every client and the server/host has loaded 
                foreach (var clientID in sceneEvent.ClientsThatCompleted)
                {
                    if (IsServer)
                    {
                        loadedScene = sceneEvent.Scene;
                        playersLoadedInScene.Value = sceneEvent.ClientsThatCompleted.Count;
                        
                        
                    }
                    else
                    {

                    }
                    Debug.Log("loadeventcompleted");
                }
                break;
            case SceneEventType.UnloadEventCompleted: //when everyone has unloaded 
                foreach (var clientID in sceneEvent.ClientsThatCompleted)
                {
                    if (IsServer)
                    {
                        
                    }
                    else
                    {

                    }
                    //NetworkManager.LocalClient.PlayerObject.GetComponent<HealthManager>().Respawn(false);
                }
                break;
            case SceneEventType.LoadComplete: //event triggers each client successfully loading
                if (IsServer)
                {
                    if (sceneEvent.ClientId == NetworkManager.LocalClientId)
                    {

                    }
                    else
                    {

                    }
                    Debug.Log("loadcompleted");
                }
                else
                {

                }
                break;
            case SceneEventType.UnloadComplete: //same as above but for unload 
                break;

                //theres also sync events unincluded 
        }
    }
    public bool SceneLoadedSuccessfully //prob not needed 
    {
        get => (loadedScene.IsValid() && loadedScene.isLoaded) ? true : false;
    }
    public void UnloadScene()
    {
        if (!IsServer || !IsSpawned || !loadedScene.IsValid() || !loadedScene.isLoaded)
        {
            return;
        }
        var status = NetworkManager.SceneManager.UnloadScene(loadedScene);
        CheckLoadStatus(status, false);
    }
    //SERVER VALIDATION THINGZZZZZZZZZZZZZZZZZZZ (also prob not needed) 
   
    public override void OnNetworkSpawn() //MIGHT CAUSE PROBLEMS WHO KNOWS I DONT (also prob not needed)
    {
        if (IsServer && !string.IsNullOrEmpty(sceneName))
        {
            NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
            var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            CheckLoadStatus(status);

        }
        base.OnNetworkSpawn();
    }

    //END OF SERVER VALIDATION THINGZZZZZZZZZZZZ

    private List<string> SceneNamesForEasySwapping() //absolutely not my code in the slightest
    {
        var regex = new Regex(@"([^/]*/)*([\w\d\-]*)\.unity");
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var name = regex.Replace(path, "$2");
            sceneNames.Add(name);
        }
        return sceneNames;
    }
}
    
