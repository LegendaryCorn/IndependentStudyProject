using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UIMgr : NetworkBehaviour
{
    public TMPro.TMP_Text posText;
    public TMPro.TMP_Text speedText;
    public TMPro.TMP_Text headingText;

    public Slider desSpeedSlider;
    public TMPro.TMP_Text desSpeedText;
    public Slider desHeadingSlider;
    public TMPro.TMP_Text desHeadingText;

    // Start is called before the first frame update
    void Start()
    {
        EventMgr.instance.onPlayerJoin.AddListener(RefreshShipList);
        EventMgr.instance.onPlayerLeave.AddListener(RefreshShipList);

        EventMgr.instance.onDesiredSpeedChanged.AddListener(UpdateDesiredSpeedSlider);
        EventMgr.instance.onDesiredHeadingChanged.AddListener(UpdateDesiredHeadingSlider);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateShipNumbers();
    }

    void RefreshShipList()
    {
        desSpeedSlider.interactable = true;
        desHeadingSlider.interactable = true;
    }

    void UpdateShipNumbers()
    {
        if (GameMgr.instance.shipDict.ContainsKey(GameMgr.instance.userID))
        {
            PlayerShip userShip = GameMgr.instance.shipDict[GameMgr.instance.userID];
            float newHeading = userShip.heading * Mathf.Rad2Deg;

            posText.text = "x: " + userShip.position.x.ToString("F2") + " z: " + userShip.position.z.ToString("F2");
            speedText.text = "<b>Speed:</b> " + userShip.speed.ToString("F2") + " m/s";
            headingText.text = "<b>Heading:</b> " + newHeading.ToString("F1") + "°";
        }
    }

    void UpdateDesiredSpeedSlider()
    {
        PlayerShip userShip = GameMgr.instance.shipDict[GameMgr.instance.userID];
        desSpeedSlider.SetValueWithoutNotify(userShip.desiredSpeed);
        desSpeedText.text = "<b>Desired Speed:</b> " + userShip.desiredSpeed.ToString("F2") + " m/s";
    }

    void UpdateDesiredHeadingSlider()
    {
        PlayerShip userShip = GameMgr.instance.shipDict[GameMgr.instance.userID];
        float newHeading = userShip.desiredHeading * Mathf.Rad2Deg;
        desHeadingSlider.SetValueWithoutNotify(newHeading);
        desHeadingText.text = "<b>Desired Heading:</b> " + newHeading.ToString("F1") + "°";
    }
    
}
