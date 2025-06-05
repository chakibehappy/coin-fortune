using System;
using System.Collections.Generic;

[Serializable]
public class HistoryDataResponse
{
    public bool status;
    public string message;
    public List<GameRound> data;
}

[Serializable]
public class GameRound
{
    public int round_id;
    public HistoryResult result;
    public string player_id;
    public string agent_id;
    public string created_date;
    public RoundData data;
}

[Serializable]
public class HistoryResult
{
    public string output;
    public string prize;
    public string status;
    public string type;
}

[Serializable]
public class RoundData
{
    public DetailBet detail_bet;
    public string total_amount;
    public string total_win;
    public string last_balance;
}

[Serializable]
public class DetailBet
{
    public string button;
    public string amount;
    public string type;
}