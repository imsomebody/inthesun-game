using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class RespawnHandler : MonoBehaviour
{
    private GameObject player;

    void Start()
    {
        this.player = GameObject.FindGameObjectWithTag("Player");

        this.Respawn();
    }

    public void Respawn()
    {
        this.player.transform.position = this.transform.position;
        this.player.GetComponent<CharacterController>().ResetHealth();
        this.player.GetComponent<CharacterController>().BeAfk();

        //foreach(var go in GameObject.FindGameObjectsWithTag("Background"))
        //{
        //    go.transform.position = (transform.position + new Vector3(4f, 0f, 0f));
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
