using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnHandler : MonoBehaviour
{
    private GameObject player;

    void Start()
    {
        this.player = GameObject.FindGameObjectWithTag("Player");

        this.Respawn();
    }

    private void Respawn()
    {
        this.player.transform.position = this.transform.position;
        this.player.GetComponent<CharacterController>().ResetHealth();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
