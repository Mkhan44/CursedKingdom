//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PurrNet;
using PurrNet.Modules;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagementNetworking : NetworkBehaviour
{
    public static SceneManagementNetworking Instance { get; private set;}
    [PurrScene] public string sceneToChange;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ChangeScene()
    {
        PurrSceneSettings settings = new()
        {
            isPublic = false,
            mode = LoadSceneMode.Single
        };
        networkManager.sceneModule.LoadSceneAsync(sceneToChange, settings);
    }

    [ServerRpc(requireOwnership: false)]
    public void RequestSceneChange(RPCInfo info = default)
    {
        var scene = SceneManager.GetSceneByName(sceneToChange);
        if(!scene.isLoaded)
        {
            return;
        }

        if(networkManager.sceneModule.TryGetSceneID(scene, out SceneID sceneId))
        {
            networkManager.scenePlayersModule.AddPlayerToScene(info.sender, sceneId);
            //
        }
    }

    public void RequestSceneChangeLocal()
    {
        RequestSceneChange();
    }
}
