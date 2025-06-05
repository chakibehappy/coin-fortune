using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    public string domain = "https://ffn22dev.saibara619.xyz/";
    public string dataUserURL = "api/auth/data";
    public string historyTransactionURL = "game/history";
    public string sendBetDataURL = "game/send_bet";
    public string encryptDataURL = "game/enc-result";
    public string settingPropertiesURL = "game/settings/properties";
    public string triggerLoginURL = "https://apin22dev.saibara619.xyz/api/operator/auth-game/?agent=meja-hoki&game=FF000001&token=abcd12345";
    public string encryptionKey = "bce13cb9b55baa2409fa33259ad589319caa27eeb3fff94bf2c111aed1fc81e6";

    public string jsonFileName = "config.json";

    [Serializable]
    public class ConfigData
    {
        public string api_url;
    }

    public IEnumerator GetAPIFromConfig(Action nextAction = null)
    {
        string url = Application.absoluteURL;
        if (url.EndsWith("index.html"))
        {
            url = url.Substring(0, url.Length - "index.html".Length);
        }
        url += jsonFileName;

        using UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

    
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error loading JSON: " + request.error);
            nextAction?.Invoke();
        }
        else
        {
            string jsonData = request.downloadHandler.text;
            if (jsonData != string.Empty)
            {
                ConfigData config = JsonUtility.FromJson<ConfigData>(jsonData);
                if (config.api_url != "")
                {
                    domain = config.api_url;
                }
            }
            nextAction?.Invoke();
        }
    }

    public string GetDataUserAPI() { 
        return domain + dataUserURL; 
    }

    public string GetHistoryTransactionAPI()
    {
        return domain + historyTransactionURL;
    }

    public string GetSendBetDataAPI()
    {
        return domain + sendBetDataURL;
    }

    public string GetEncryptDataAPI()
    {
        return domain + encryptDataURL;
    }

    public string GetSettingPropertiesAPI()
    {
        return domain + settingPropertiesURL;
    }

    public string TriggerLoginAPI()
    {
        return triggerLoginURL;
    }
}

[System.Serializable]
public class DataToSend
{
    public string data;
}

[System.Serializable]
public class BetDataToSend
{
    public int total_amount;
    public ButtonBet button_bet;
}

[System.Serializable]
public class ButtonBet
{
    public string button;
    public int amount;
    public string type;
}