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

    int teamNum = -1;
    public List<Button> hostTeamButtons;
    public List<Button> clientTeamButtons;

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
            float aaa = 1 / (teamNum + 1); // Fix this later
            transport.ConnectionData.Address = hostIPInput.text;
            transport.ConnectionData.Port = ushort.Parse(hostPortInput.text);
            PlayerMgr.instance.teamID = teamNum;
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
            float aaa = 1 / (teamNum + 1); // Fix this later
            transport.ConnectionData.Address = clientIPInput.text;
            transport.ConnectionData.Port = ushort.Parse(clientPortInput.text);
            PlayerMgr.instance.teamID = teamNum;
            NetworkManager.Singleton.StartClient();
            menuCanvas.SetActive(false);
        }
        catch
        {
            Debug.Log(0);
        }
    }

    public void SwapTeam(int team)
    {
        teamNum = team;

        for(int i = 0; i < hostTeamButtons.Count; i++)
        {
            if(team == i)
            {
                hostTeamButtons[i].interactable = false;
                clientTeamButtons[i].interactable = false;
            }
            else
            {
                hostTeamButtons[i].interactable = true;
                clientTeamButtons[i].interactable = true;
            }
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
