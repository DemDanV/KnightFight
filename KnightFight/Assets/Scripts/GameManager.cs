using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] PlayerController player;
    public PlayerController Player { get { return player; } }


    [SerializeField] PlayerInput gameInput;
    InputAction exitAction;
    InputAction restartAction;


    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Debug.LogWarning("Game manager is'nt one");
            return;
        }

        instance = this;

        player.OnPlayerDead += RemovePlayer;

        exitAction = gameInput.actions["Exit"];
        restartAction = gameInput.actions["RestartScene"];

        Debug.Log(exitAction);
        Debug.Log(restartAction);


        exitAction.performed += x => Exit();
        restartAction.performed += x => Restart();
    }

    private void Exit()
    {
        Application.Quit();
    }

    private void Restart()
    {
        SceneManager.LoadScene("Main");
    }


    private void RemovePlayer()
    {
        player = null;
    }


    [SerializeField] EnemyController ec;

    private void Update()
    {
        //if(attackAction.triggered)
        //{
        //    ec.GetHit(10);
        //}
    }
}
