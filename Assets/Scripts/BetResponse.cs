[System.Serializable]
public class BetResponse
{
    public bool status;
    public string message;
    public BetData data;
    public string type;

}

[System.Serializable]
public class ErrorBetResponse
{
    public bool status;
    public string message;
    public string type;

}

[System.Serializable]
public class BetData
{
    public string round_id;
    public int energy_bar;
    public int orb_state;
    public int total_energy_orb;
    public string type;
    public string result;
    public float prize;
    //public long current_balance;
    public string total_win;
    public string current_balance;
    public int try_count;
}

[System.Serializable]
public class EncryptedResponse
{
    public string encrypted_data;
}