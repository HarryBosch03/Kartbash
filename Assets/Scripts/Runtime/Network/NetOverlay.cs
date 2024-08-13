using System;
using FishNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetOverlay : MonoBehaviour
{
    public string address = "127.0.0.1";
    
    private void Update()
    {
        var serverManager = InstanceFinder.ServerManager;
        var clientManager = InstanceFinder.ClientManager;
        var kb = Keyboard.current;
        
        if (!serverManager.Started && !clientManager.Started && (kb.spaceKey.wasPressedThisFrame || kb.hKey.wasPressedThisFrame))
        {
            serverManager.StartConnection();
            clientManager.StartConnection("127.0.0.1");
        }
        
        if (!clientManager.Started && kb.cKey.wasPressedThisFrame)
        {
            clientManager.StartConnection(address);
        }
    }

    private void OnGUI()
    {
        using (new GUILayout.AreaScope(new Rect(10, 10, 150, Screen.height - 20)))
        {
            var serverManager = InstanceFinder.ServerManager;
            var clientManager = InstanceFinder.ClientManager;
            
            GUILayout.Button($"Server {(serverManager.Started ? "Enabled" : "Disabled")}");
            GUILayout.Button($"Client {(clientManager.Started ? "Enabled" : "Disabled")}");
        }
    }
}
