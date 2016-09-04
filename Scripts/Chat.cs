using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public enum Candidate { speed, haul, burst};

public class Chat : MonoBehaviour {
    public int leaderBoardSize = 10000;

    public string logPath = "C:/Users/veryc/AppData/Roaming/HexChat/logs/lastsession.log";
    private StreamReader streamReader;
    private AgentSpawner spawner;
    private AgentWorld world;
    private LeaderBoardText leaderBoardText;
    private Dictionary<string, int> playerNameToId;
    private int[] playerScores;
    private string[] playerIdToName;
    public string myName = "<twitchPlaysTheClassics>";
    private int newPlayerIndex = 1;
    public int leadersToShow = 3;

    public void updateLeaderBoard()
    {
        HashSet<string> exlcludingMe = new HashSet<string>();
        exlcludingMe.Add(myName);

        leaderBoardText.setLeaderBoardText(leaderBoardString(leadersToShow, exlcludingMe));
    }
        
    // Use this for initialization
    void Start ()
    {
        spawner = FindObjectOfType<AgentSpawner>();
        world = FindObjectOfType<AgentWorld>();
        leaderBoardText = FindObjectOfType<LeaderBoardText>();
        playerNameToId = new Dictionary<string, int>();
        playerIdToName = new string[leaderBoardSize];
        playerScores = new int[leaderBoardSize];
        File.WriteAllText(logPath, string.Empty);
        FileStream fileStream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        streamReader = new StreamReader(fileStream);
    }

    

    public string leaderBoardString(int leaderCount, HashSet<string> excludedPlayers)
    {
        if (leaderCount <= 0) return "\n";
        int maxScore = int.MinValue;
        string kingPlayer = "";
        for(int i = 1;i<newPlayerIndex;i++)
        {
            int pScore = playerScores[i];
            if (pScore < maxScore) continue;
            string pName = playerIdToName[i];
            if (excludedPlayers.Contains(pName)) continue;        
            

            maxScore = pScore;
            kingPlayer = pName;
        }        
        if (kingPlayer == "") return "";
        string currentKingLeaderString = kingPlayer + ":" + maxScore.ToString() + "\n";
        if (leaderCount == 1) return currentKingLeaderString;
        excludedPlayers.Add(kingPlayer);
        return currentKingLeaderString + leaderBoardString(leaderCount - 1, excludedPlayers);

        
    }

    private void newPlayer(string playerName)
    {
        playerIdToName[newPlayerIndex] = playerName;
        playerNameToId[playerName] = newPlayerIndex;
        playerScores[newPlayerIndex] = 0;
        newPlayerIndex++;
    }

    public void addToPlayerScore(int playerId, int scoreToAdd)
    {
        playerScores[playerId] += scoreToAdd;
    }

    public bool upgradeWithoutSum = true;
    bool parseUpgradePayment(string command)
    {
        
        foreach(Candidate c in System.Enum.GetValues(typeof(Candidate)))
        {
            string s = command;

            if(upgradeWithoutSum && c.ToString() == command) return world.payTowardsupgrade(c, world.getScore());            
            
            if (upgradeWithoutSum) continue;
            s = s.Replace(c.ToString(), "");
            int payment;
            if (int.TryParse(s,out payment))
            {
                return world.payTowardsupgrade(c, payment);                
            }
        }
        

        return false;
    }

    // Update is called once per frame
    void Update () {

        string line = streamReader.ReadLine();
        if (line == null || line.Length == 0) return;
        if (line[0] != '<') return;
        string[] splitLine = line.Split('\t');
        if (splitLine.Length < 2) return;


        string playerName = splitLine[0];
        if (!playerNameToId.ContainsKey(playerName)) newPlayer(playerName);
        line = splitLine[1];
        line = line.ToLower();
        line = line.Trim();

        parseUpgradePayment(line);
        spawner.Spawn(playerNameToId[playerName]);
    }
}
/// <summary>
/// Comparer for comparing two keys, handling equality as beeing greater
/// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class DuplicateKeyComparer<TKey>
                :
             IComparer<TKey> where TKey : IComparable
{
    #region IComparer<TKey> Members

    public int Compare(TKey x, TKey y)
    {
        int result = x.CompareTo(y);

        if (result == 0)
            return 1;   // Handle equality as beeing greater
        else
            return result;
    }

    #endregion
}