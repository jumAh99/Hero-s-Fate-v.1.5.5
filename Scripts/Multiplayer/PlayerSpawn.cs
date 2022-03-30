using System.Transactions;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

public enum BattleStateMultiplayer
{
    //these are the different game state that can be possible in our game
    START, MASTER_PLAYER_TURN, LOCAL_PLAYER_TURN, ENEMY_TURN, WON, LOST
}
public class PlayerSpawn : MonoBehaviourPunCallbacks
{
    public GameObject[] playerPefabs;
    public GameObject[] enemyPrefabs;
    public Transform[] playerSpawnLocations;
    public Transform enemySpawnLocation;
    public BattleHUB[] playerHub;
    public BattleHUB enemyHub;
    public TextMeshProUGUI dialogueText;
    public BattleStateMultiplayer state;
    GameObject localClientPrefab;
    GameObject foreingClientPrefab;
    GameObject enemyPrefab;
    Unit[] unitList;
    Unit enemyUnit;
    Player foreignPlayer;
    Player localPlayer;
    PhotonView view;
    bool isAttacked = false;
    int currentPlayer = 0;
    int currentEenemy = 0;
    int counter = 0;
    PlayFabLobbyManager playFabManager;
    //win/lose management 
    public TextMeshProUGUI winLoseText;
    public TextMeshProUGUI bossRemainingText;
    public TextMeshProUGUI waitMasterText;
    public GameObject endPanel;
    public GameObject generalPanel;
    public GameObject buttonTrigger;
    public GameObject buttonExit;
    public GameObject inGamePanel;
    void Start()
    {
        view = GetComponent<PhotonView>();

        foreach (Player p in PhotonNetwork.PlayerListOthers)
        {
            foreignPlayer = p;
        }
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (foreignPlayer != p)
            {
                localPlayer = p;
            }
        }
        GameObject masterPrefab = playerPefabs[(int)localPlayer.CustomProperties["playerAvatar"]];
        GameObject foreingPrefab = playerPefabs[(int)foreignPlayer.CustomProperties["playerAvatar"]];

        unitList = new Unit[PhotonNetwork.CurrentRoom.PlayerCount];
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.IsLocal && PhotonNetwork.IsMasterClient)
            {
                localClientPrefab = masterPrefab;
                foreingClientPrefab = foreingPrefab;
                SpawnPlayer(0, localClientPrefab);

            }
            else if (p.IsLocal && !PhotonNetwork.IsMasterClient)
            {
                foreingClientPrefab = masterPrefab;
                localClientPrefab = foreingPrefab;
                SpawnPlayer(1, foreingClientPrefab);
            }
        }

        StartCoroutine(SetupBattle(counter));
        if (SceneManager.GetActiveScene().name == "GameSceneMultiplayer")
        {
            FindObjectOfType<AudioManager>().StopPlaying("MainMenu_Sound");
            FindObjectOfType<AudioManager>().Play("Battle_Sound");
            Debug.LogError("Music stopped.");
        }
    }
    IEnumerator SetupBattle(int enemyIndex)
    {
        unitList[0] = localClientPrefab.GetComponent<Unit>();
        unitList[1] = foreingClientPrefab.GetComponent<Unit>();
        //get prefab unit component
        print(unitList[1].unitName);
        print(unitList[0].unitName);
        //spawn the enemy 
        SpawnEnemy(enemySpawnLocation, counter);
        if (counter > 0)
        {
            //reset all previous stats
            view.RPC("RPC_StatReset", RpcTarget.AllBuffered, false, true);
        }
        else
        {
            view.RPC("RPC_StatReset", RpcTarget.AllBuffered, true, true);
        }
        //introduce the enemy
        dialogueText.text = enemyUnit.unitName + " challenges you.";
        photonView.RPC("RPC_SetInfo", RpcTarget.All);
        //uppdate all hubs
        view.RPC("RPC_UpdateAllHubs", RpcTarget.AllBuffered);
        yield return new WaitForSeconds(2f);
        //MASTER player turn
        state = BattleStateMultiplayer.MASTER_PLAYER_TURN;
        //say whos turn it is
        IsYourTurnMessage();
    }
    // ---------------------------------------- CHARACTER SETUP FUNCTIONS ----------------------------------------------------
    void SpawnPlayer(int indexSpawn, GameObject playerPrefab)
    {
        //spawn player 
        Transform spawnLocation = playerSpawnLocations[indexSpawn];
        PhotonNetwork.Instantiate(playerPrefab.name, spawnLocation.position, Quaternion.identity);

        print("created -->" + playerPrefab.name);
    }
    void SpawnEnemy(Transform location, int index)
    {
        //spawn the enemy
        // PhotonNetwork.Instantiate(enemyPrefabs[index].name, location.position, Quaternion.identity);
        enemyPrefab = Instantiate(enemyPrefabs[index], location);
        enemyUnit = enemyPrefab.GetComponent<Unit>();
    }
    void IsYourTurnMessage()
    {
        if (state == BattleStateMultiplayer.MASTER_PLAYER_TURN)
        {
            if (view.IsMine)
            {
                dialogueText.text = "Your Turn. . ";
            }
            else
            {
                dialogueText.text = foreignPlayer.NickName + "'s turn";
            }
        }
        else if (state == BattleStateMultiplayer.LOCAL_PLAYER_TURN)
        {
            if (!view.IsMine)
            {
                dialogueText.text = "Your Turn. . ";
            }
            else
            {
                dialogueText.text = foreignPlayer.NickName + "'s turn";
            }
        }
    }
    void PlayerTurnSetup(bool turn)
    {
        //GIVE ACCESS TO PLAYER
        if (turn || unitList[1].IsDead())
        {
            state = BattleStateMultiplayer.ENEMY_TURN;
            StartCoroutine(EnemyTurn());
        }
        else if (!unitList[1].IsDead())
        {
            state = BattleStateMultiplayer.LOCAL_PLAYER_TURN;
        }
        else
        {
            state = BattleStateMultiplayer.MASTER_PLAYER_TURN;
        }
    }
    // ---------------------------------------- ENEMY TURN ----------------------------------------------------
    IEnumerator EnemyTurn()
    {
        // view.RPC("RPC_PlayerSelected", RpcTarget.AllBuffered); 
        SwitchPlayer(unitList[currentPlayer]);
        //let the players know that its the enemy turn. 
        dialogueText.text = enemyUnit.unitName + " wants to attack " + unitList[currentPlayer].unitName + "!";
        yield return new WaitForSeconds(2f);
        //check weather the attacked player is dead
        unitList[currentPlayer].TakeDamage(enemyUnit.damage);
        //update all the player stats 
        view.RPC("RPC_UpdateAllHubs", RpcTarget.AllBuffered);
        Debug.LogError("selected character =" + unitList[currentPlayer].unitName + ", value " + currentPlayer + isAttacked.ToString());
        //let the player know how much damage they have taken
        dialogueText.text = "Attack Successful: " + unitList[currentPlayer].unitName + " takes " + enemyUnit.damage;
        yield return new WaitForSeconds(2f);
        if (unitList[0].IsDead() && unitList[1].IsDead())
        {
            //pass to the LOST state
            dialogueText.text = "There are no champions left, you have been defeated!";
            yield return new WaitForSeconds(2f);
            state = BattleStateMultiplayer.LOST;
            OnLose();
        }
        else
        {
            //if the player is dead 
            if (unitList[0].IsDead())
            {
                //pass to the LOST state
                dialogueText.text = unitList[0].unitName + " has fallen!";
                yield return new WaitForSeconds(2f);
                //give the turn to the master client
                state = BattleStateMultiplayer.LOCAL_PLAYER_TURN;
            }
            else if (unitList[1].IsDead())
            {
                //pass to the LOST state
                dialogueText.text = unitList[1].unitName + " has fallen!";
                yield return new WaitForSeconds(2f);
                //give the turn to the master client
                state = BattleStateMultiplayer.MASTER_PLAYER_TURN;
            }
            else
            {
                //give the turn to the master client
                state = BattleStateMultiplayer.MASTER_PLAYER_TURN;
            }
            //tell them that its their turn
            IsYourTurnMessage();
        }
    }
    void SwitchPlayer(Unit currentUnit)
    {
        if (currentPlayer == 0 && !unitList[1].IsDead())
        {
            currentPlayer = 1;
        }
        else if (currentPlayer == 1 && !unitList[0].IsDead())
        {
            currentPlayer = 0;
        }
        else if (currentPlayer == 1 && !unitList[0].IsDead())
        {
            currentPlayer = 0;
        }
        else if (unitList[currentPlayer].IsDead())
        {
            for (int i = 0; i < unitList.Length; i++)
            {
                if (!unitList[i].IsDead())
                {
                    currentPlayer = i;
                }
            }
        }
    }
    // ---------------------------------------- BATTLE SYSTEM ----------------------------------------------------
    IEnumerator OnAttack(Unit currentUnit, bool isMasterTurn)
    {
        if (currentUnit.currentAp >= 100)
        {
            //appply damage from the unit that attacked the boss
            bool isDead = enemyUnit.TakeDamage(currentUnit.damage);
            //reduce AP
            currentUnit.APReduct(100);
            //uppdate all hubs
            view.RPC("RPC_UpdateAllHubs", RpcTarget.AllBuffered);
            //update text box 
            dialogueText.text = "Attack Successful: " + currentUnit.unitName + " deals " + currentUnit.damage + "dp " + "to " + enemyUnit.unitName;
            yield return new WaitForSeconds(1f);
            //if the enemy is dead 
            if (isDead)
            {
                state = BattleStateMultiplayer.WON;
                //update the playfab leaderboard value 
                OnWin();
            }
            else
            {
                //GIVE ACCESS TO PLAYER
                PlayerTurnSetup(isMasterTurn);
            }
            IsYourTurnMessage();
        }
        else
        {
            dialogueText.text = "Player: " + currentUnit.unitName + " does not have enough AP for this action.";
            yield return new WaitForSeconds(2f);

            IsYourTurnMessage();
        }
    }
    IEnumerator OnHeal(Unit currentUnit, bool isMasterTurn)
    {
        if (currentUnit.currentAp >= 150)
        {
            //set regen and ap cost
            currentUnit.Heal(currentUnit.regen);
            currentUnit.APReduct(150);
            //uppdate all hubs
            view.RPC("RPC_UpdateAllHubs", RpcTarget.AllBuffered);
            //inform the user about the change
            dialogueText.text = currentUnit.unitName + " has healed by : " + currentUnit.regen;
            yield return new WaitForSeconds(2f);
            //GIVE ACCESS TO PLAYER
            PlayerTurnSetup(isMasterTurn);
            IsYourTurnMessage();
        }
        else
        {
            dialogueText.text = currentUnit.unitName + " has not enough AP for this action. .";
            yield return new WaitForSeconds(2f);
            //GIVE ACCESS TO PLAYER
            PlayerTurnSetup(isMasterTurn);
            IsYourTurnMessage();
        }
    }
    IEnumerator OnAPRegen(Unit currentUnit, bool isMasterTurn)
    {
        //update text box
        currentUnit.APRegen(100);
        //uppdate all hubs
        view.RPC("RPC_UpdateAllHubs", RpcTarget.AllBuffered);
        //inform the user about the change
        dialogueText.text = currentUnit.unitName + " has replenished by 100ap.";
        yield return new WaitForSeconds(1f);
        //GIVE ACCESS TO PLAYER
        PlayerTurnSetup(isMasterTurn);
        IsYourTurnMessage();
    }
    // ---------------------------------------- BUTTON FUNCTIONS ----------------------------------------------------
    public void OnAttackFunction()
    {
        //when attack button is pressed
        if (state != BattleStateMultiplayer.MASTER_PLAYER_TURN && state != BattleStateMultiplayer.LOCAL_PLAYER_TURN)
        {
            //if the turn is of the enemy then the attack button will not do anything.
            print("not your turn, current state is: " + state.ToString());
            return;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient && state == BattleStateMultiplayer.MASTER_PLAYER_TURN)
            {
                //needs to be an RPC call to being update troughout the network
                view.RPC("RPC_AttackTrigger", RpcTarget.AllBuffered, 0, false);
            }
            else if (state == BattleStateMultiplayer.LOCAL_PLAYER_TURN && !view.IsMine)
            {
                //needs to be an RPC call to being update troughout the network
                view.RPC("RPC_AttackTrigger", RpcTarget.AllBuffered, 1, true);
            }
            else
            {
                dialogueText.text = ("not your turn");
            }
        }
    }
    public void OnHPRegenFunction()
    {
        //when the heal button is pressed
        if (state != BattleStateMultiplayer.MASTER_PLAYER_TURN && state != BattleStateMultiplayer.LOCAL_PLAYER_TURN)
        {
            return;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient && state == BattleStateMultiplayer.MASTER_PLAYER_TURN)
            {
                //needs to be an RPC call to being update troughout the network
                view.RPC("RPC_HPRegenTrigger", RpcTarget.AllBuffered, 0, false);
            }
            else if (state == BattleStateMultiplayer.LOCAL_PLAYER_TURN && !view.IsMine)
            {
                //needs to be an RPC call to being update troughout the network
                view.RPC("RPC_HPRegenTrigger", RpcTarget.AllBuffered, 1, true);
            }
            else
            {
                dialogueText.text = ("not your turn");
            }
        }
    }
    public void OnAPRegenFunction()
    {
        //when the ap regen button is pressed
        if (state != BattleStateMultiplayer.MASTER_PLAYER_TURN && state != BattleStateMultiplayer.LOCAL_PLAYER_TURN)
        {
            return;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient && state == BattleStateMultiplayer.MASTER_PLAYER_TURN)
            {
                //needs to be an RPC call to being update troughout the network
                view.RPC("RPC_APRegenTrigger", RpcTarget.AllBuffered, 0, false);
            }
            else if (state == BattleStateMultiplayer.LOCAL_PLAYER_TURN && !view.IsMine)
            {
                //needs to be an RPC call to being update troughout the network
                view.RPC("RPC_APRegenTrigger", RpcTarget.AllBuffered, 1, true);
            }
            else
            {
                dialogueText.text = ("not your turn");
            }
        }
    }
    // ---------------------------------------- BATTLE SYSTEM RPC CALLS ----------------------------------------------------
    [PunRPC]
    void RPC_UpdateAllHubs()
    {
        //update all player hub
        for (int i = 0; i < unitList.Length; i++)
        {
            playerHub[i].setHP(unitList[i].currentHP);
            playerHub[i].setAP(unitList[i].currentAp);
        }
        //update enemy hub
        enemyHub.setHP(enemyUnit.currentHP);
    }
    [PunRPC]
    void RPC_AttackTrigger(int indexUnit, bool isMasterTurn)
    {
        //START THE DAMAGE CALCOLATION PROCEDURE
        StartCoroutine(OnAttack(unitList[indexUnit], isMasterTurn));
    }
    [PunRPC]
    void RPC_HPRegenTrigger(int indexUnit, bool isMasterTurn)
    {
        //START HEAL CALCOLATION PROCEDURE
        StartCoroutine(OnHeal(unitList[indexUnit], isMasterTurn));
    }
    [PunRPC]
    void RPC_APRegenTrigger(int indexUnit, bool isMasterTurn)
    {
        //START AP REGEN PROCEDURE
        StartCoroutine(OnAPRegen(unitList[indexUnit], isMasterTurn));
    }
    [PunRPC]
    void RPC_SetInfo()
    {
        playerHub[0].setHUB(unitList[0]);
        playerHub[1].setHUB(unitList[1]);
        enemyHub.setHUB(enemyUnit);

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.IsLocal && PhotonNetwork.IsMasterClient)
            {
                unitList[0].unitName = localPlayer.NickName;
                unitList[1].unitName = foreignPlayer.NickName;
            }
            else if (p.IsLocal)
            {
                unitList[0].unitName = foreignPlayer.NickName;
                unitList[1].unitName = localPlayer.NickName;
            }
        }
    }
    [PunRPC]
    void RPC_StatReset(bool player, bool enemy)
    {
        if (player == true)
        {
            foreach (Unit unit in unitList)
            {
                //reset stats for champions 
                unit.ResetStats();
            }
        }
        if (enemy == true)
        {
            //RESET ENEMY STATS 
            enemyUnit.ResetStats();
        }
    }
    // ---------------------------------------- WIN/LOSE FUNCTIONS ----------------------------------------------------
    public void OnWin()
    {
        inGamePanel.SetActive(false);
        winLoseText.text = "VICTORY!";
        endPanel.SetActive(true);
        generalPanel.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            //buttonPanel.SetActive(true);
            waitMasterText.text = "";
            buttonExit.SetActive(false);
            buttonTrigger.SetActive(true);
        }
        //set the boss number 
        bossRemainingText.text = (counter + 1) + "/" + enemyPrefabs.Length;
    }
    public void OnLose()
    {
        //activate the pannels
        inGamePanel.SetActive(false);
        winLoseText.text = "DEFEATED!";
        endPanel.SetActive(true);
        generalPanel.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            //buttonPanel.SetActive(true);
            waitMasterText.text = "";
            buttonExit.SetActive(true);
            buttonTrigger.SetActive(false);
        }
        //set the boss number 
        bossRemainingText.text = (counter + 1) + "/" + enemyPrefabs.Length;
    }
    public void OnTrigger()
    {
        view.RPC("RestartMatch", RpcTarget.AllBuffered);
    }
    [PunRPC]
    void RestartMatch()
    {
        Destroy(enemyPrefab);
        inGamePanel.SetActive(true);
        endPanel.SetActive(false);

        if (counter < 2)
        {
            StartCoroutine(SetupBattle(counter++));
        }
        else
        {
            //activate the pannels
            inGamePanel.SetActive(false);
            //print messages 
            winLoseText.text = "VICTORY!";
            waitMasterText.text = "YOU HAVE DEFEATED ALL THE BOSSES AVAILABLE!";
            endPanel.SetActive(true);
            //activate the pannels
            inGamePanel.SetActive(false);
            generalPanel.SetActive(true);
            //deactivate the continue button 
            buttonTrigger.SetActive(false);
            buttonExit.SetActive(true);
        }
    }
}
