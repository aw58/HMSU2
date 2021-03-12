using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Linq;

public class Interface : MonoBehaviour
{
    SerialPort sp;

    void Start()
    {
        Debug.Log("Inside Interface");
        sp = new SerialPort("COM4", 115200, Parity.None, 8, StopBits.One); //Replace "COM4" with whatever port your Arduino is on.
        sp.DtrEnable = false; //Prevent the Arduino from rebooting once we connect to it. 
                              //A 10 uF cap across RST and GND will prevent this. Remove cap when programming.
        sp.ReadTimeout = 1; //Shortest possible read time out.
        sp.WriteTimeout = 1; //Shortest possible write time out.
        sp.Open();

    }

    void Update()
    {
        CheckForRecievedData();
       // if (Input.GetKeyDown(KeyCode.Escape) && sp.IsOpen)
        //    sp.Close();
    }


    public string CheckForRecievedData()
    {
        try //Sometimes malformed serial commands come through. We can ignore these with a try/catch.
        {
            string inData = sp.ReadLine();
            int inSize = inData.Count();
            if (inSize > 0)
            {
                Debug.Log("ARDUINO->|| " + inData + " ||MSG SIZE:" + inSize.ToString());
            }
            //Got the data. Flush the in-buffer to speed reads up.
            inSize = 0;
            sp.BaseStream.Flush();
            sp.DiscardInBuffer();
            return inData;
        }
        catch { return string.Empty; }
    }
}