using System.Collections.Generic;

[System.Serializable]
public class UserDataResponse
{
    public bool status;
    public string message;
    public Data data;
}

[System.Serializable]
public class Data
{
    public Player player;
    public Game game;
}

[System.Serializable]
public class Player
{
    public string player_id;
    public string agent_id;
    public string player_name;
    public string player_balance;
    public string player_currency;
    public string player_language;
    public string player_session;
    public string player_last_active;
}

[System.Serializable]
public class LimitBet
{
    public int minimal;
    public int maximal;
    public int minimal_50;
    public int maximal_50;
    public int multiplication;
    public int multiplication_50;
}

[System.Serializable]
public class PrizeDetail
{
    public float purchase_bonus;
    public float purchase_mega;
    public NormalRound normal_round;
    public BonusRound bonus_round;
    public BonusRound mega_bonus_round;
}


[System.Serializable]
public class NormalRound
{
    public float win;
    public float lose;
}

[System.Serializable]
public class BonusRound
{
    public BonusRoundWin win;
    public float lose;
}

[System.Serializable]
public class BonusRoundWin
{
    public BonusRoundWinModel model_1;
    public BonusRoundWinModel model_2;
    public BonusRoundWinModel model_3;
}

[System.Serializable]
public class BonusRoundWinModel
{
    public float[] prize;
    public float[] probability;
}


[System.Serializable]
public class Game
{
    public string game_code;
    public int energy_bar;
    public int orb_state;
    public int total_energy_orb;
    public string current_round;
    public string lobby_url;
    public LimitBet limit_bet;
    public PrizeDetail prize_detail;
    public int[] chip_base;
    public string[] running_text;
    public Sounds sounds;
}

[System.Serializable]
public class Sounds
{
    public bool effect;
    public bool music;
}