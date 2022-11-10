using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuMgr : MonoBehaviour
{

    public TMPro.TMP_InputField hostIPInput;
    public TMPro.TMP_InputField hostPortInput;
    public TMPro.TMP_InputField clientIPInput;
    public TMPro.TMP_InputField clientPortInput;

    public UnityTransport transport;

    public GameObject menuCanvas;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnHostButtonClick()
    {
        try
        {
            transport.ConnectionData.Address = hostIPInput.text;
            transport.ConnectionData.Port = ushort.Parse(hostPortInput.text);
            NetworkManager.Singleton.StartHost();
            menuCanvas.SetActive(false);
        }
        catch 
        {
            Debug.Log(0);
        }
    }

    public void OnClientButtonClick()
    {
        try
        {
            transport.ConnectionData.Address = clientIPInput.text;
            transport.ConnectionData.Port = ushort.Parse(clientPortInput.text);
            NetworkManager.Singleton.StartClient();
            menuCanvas.SetActive(false);
        }
        catch
        {
            Debug.Log(0);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
