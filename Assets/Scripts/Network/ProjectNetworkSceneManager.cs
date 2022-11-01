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
    }
    private void CheckLoadStatus(SceneEventProgressStatus loadStatus, bool isLoading = true)
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
        switch (sceneEvent.SceneEventType) //put things in this switch case that you want to happen at specific points (for example, set the player positions on the LoadEventComplete case)
        {
            case SceneEventType.Load:
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
            case SceneEventType.Unload:
                if (IsServer)
                {

                }
                else
                {

                }
                break;
            case SceneEventType.LoadEventCompleted:
                foreach (var clientID in sceneEvent.ClientsThatCompleted)
                {
                    if (IsServer)
                    {
                        loadedScene = sceneEvent.Scene;
                    }
                    else
                    {

                    }
                }
                break;
            case SceneEventType.UnloadEventCompleted:
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
            case SceneEventType.LoadComplete:
                if (IsServer)
                {
                    if (sceneEvent.ClientId == NetworkManager.LocalClientId)
                    {

                    }
                    else
                    {

                    }
                }
                else
                {

                }
                break;
            case SceneEventType.UnloadComplete:
                break;
        }
    }
    public bool SceneLoadedSuccessfully
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
    //SERVER VALIDATION THINGZZZZZZZZZZZZZZZZZZZ
    private bool ServerValidation(int sceneIndex, string sceneName, LoadSceneMode loadSceneMode)//KEEP AN EYE ON ALL OF THIS
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
    public override void OnNetworkSpawn() //MIGHT CAUSE PROBLEMS WHO KNOWS I DONT
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
