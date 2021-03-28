using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    InputController _inputController;


    void Start()
    {
        // This will do the network stuff
        //_inputController = new InputController();
        _inputController.Begin("192.168.1.231", 80);
    }

    void Update()
    {
        while(!_inputController.StateClient) 
        {
            Debug.Log("Not Connected"); 
        }


        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(_inputController.CurrentValue);
        }
            

     
    }


}

