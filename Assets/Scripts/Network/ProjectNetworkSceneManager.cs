using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class ProjectNetworkSceneManager : NetworkBehaviour
{
    public static ProjectNetworkSceneManager singleton;
    private string sceneName;
    private Scene loadedScene;

    private void Start()
    {
        singleton = this;
        DontDestroyOnLoad(this);
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
    private bool ServerValidation(int sceneIndex, string sceneName, LoadSceneMode loadSceneMode)//KEEP AN EYE ON ALL OF THIS OLLIE EVERYONE ELSE IGNORE
    {
        if (sceneName == "REMEMBER_TO_CHANGE_THIS" || sceneIndex == 3)//CHANGE ALL OF THIS FUNCTION
        {
            return false;
        }
        if (loadSceneMode == LoadSceneMode.Single)
        {
            return false;
        }
        return true;
    }
    public override void OnNetworkSpawn() //MIGHT CAUSE PROBLEMS WHO KNOWS I DONT (also prob not needed)
    {
        if (IsServer && !string.IsNullOrEmpty(sceneName))
        {
            NetworkManager.SceneManager.OnSceneEvent += SceneManager_OnSceneEvent;
            var status = NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            CheckLoadStatus(status);

        }
        base.OnNetworkSpawn();
    }
}
    //END OF SERVER VALIDATION THINGZZZZZZZZZZZZ
