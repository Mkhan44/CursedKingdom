using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PurrNet;

public class CubeSpawner : NetworkBehaviour
{
    public GameObject cubePrefab;

    public void Start()
    {
        Invoke("Started", 0.1f);
        
    }

    public void Started()
    {
        Debug.Log($"The one who spawned the cube is network manager: {networkManager.isHost}");
        Debug.Log($"The player count connected to the server is: {networkManager.players.Count}");
        Debug.Log($"The player who spawned the cube's ID is: {networkManager.localPlayer.id}");
        Instantiate(cubePrefab, transform.position + transform.forward, transform.rotation);
    }
    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);

        enabled = isOwner;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(networkManager == null)
            {
                Debug.Log($"The one who spawned the cube is network manager: {NetworkManager.main.isHost}");
                Debug.Log($"The player count connected to the server is: {NetworkManager.main.players.Count}");
                Debug.Log($"The player who spawned the cube's ID is: {NetworkManager.main.localPlayer.id}");
                Instantiate(cubePrefab, transform.position + transform.forward, transform.rotation);
            }
            else
            {
                Debug.Log($"The one who spawned the cube is network manager: {networkManager.isHost}");
                Debug.Log($"The player count connected to the server is: {networkManager.players.Count}");
                Debug.Log($"The player who spawned the cube's ID is: {networkManager.localPlayer.id}");
                Instantiate(cubePrefab, transform.position + transform.forward, transform.rotation);
            }
            
        }
            
    }
}
