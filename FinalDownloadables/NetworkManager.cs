using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Net;

public class NetworkManager : MonoBehaviour
{
    InputController _inputController;

    [SerializeField]
    public static string IP = "192.168.1.239";

    //if the variable IP does not work there has been issues with that in the past, just replace it directly with the string
    private System.Net.IPAddress Address = System.Net.IPAddress.Parse(IP);

    [SerializeField]
    public int port = 80;


    void Start()
    {
        // This will do the network stuff
        _inputController = new InputController();
        _inputController.Begin(Address, port);
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(_inputController.CurrentValue);
        }
            

     
    }


}

