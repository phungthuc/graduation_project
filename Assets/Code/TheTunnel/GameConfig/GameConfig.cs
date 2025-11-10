using System.Collections.Generic;
using TheTunnel.Level;
using UnityEngine;

public class GameConfig
{
    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameConfig();
            }
            return _instance;
        }
    }

    public bool IsLoaded { get; set; }

    public List<LevelData> LevelDataList { get; set; } = new();
}
