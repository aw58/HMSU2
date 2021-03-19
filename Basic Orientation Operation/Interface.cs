//C#
using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Linq;
using static Data.Namespace.DataMap; //Team-created class. See Scripts Folder

public class Interface : MonoBehaviour
{
    SerialPort sp;
    public Transform target; //The item we want to affect with our accelerometer

    // offset for rotating into world-space in-game
    Vector3 imuInitialOrientation = new Vector3();
    bool imuInitialized = false;
    Vector3 homeOrientation = new Vector3(-1, 0, 0); //set semi-arbitrarily as the desired start orientation of the cursor in the world space

    void Start()
    {
        Debug.Log("Inside");
        sp = new SerialPort("COM10", 115200, Parity.None, 8, StopBits.One); //Replace "COMx" with whatever port your Arduino is on.
        sp.DtrEnable = true; //set this to true. False makes it timeout. false = Prevent the Arduino from rebooting once we connect to it. 
                              //A 10 uF cap across RST and GND will prevent this. Remove cap when programming.
        sp.ReadTimeout = 200; //Shortest possible read time out.
        sp.WriteTimeout = 200; //Shortest possible write time out.
        sp.Open();
        if (!sp.IsOpen)
        {
            Debug.LogError("Serial port: " + sp.PortName + " is unavailable");
            sp.Close(); //You can't program the Arduino while the serial port is open, so let's close it.
        }

    }


    void Update()
    {
        if (sp.IsOpen)
        {

            string inData = sp.ReadLine();
            
            //Debug.Log("Arduino==>" + inData);
            
            //must have these lines for when the arduino sends faster than the readTimeout
            sp.BaseStream.Flush();
            sp.DiscardInBuffer();
            
            if(inData != ""){ //throw out empty data
                string[] data = inData.Split(',');

                /*
                foreach (var item in data)
                {
                    Debug.Log(item);
                }
                */

                //create Vector3's to hold accelerometer, Gryoscope, and magnetometer data. First, convert the strings to floats
                //for Accel, map the values to full-rotation values (360 degrees) using the DataMap class written by the team. This file can be found in the scripts folder
                Debug.Log("0: " + data[0] + " 1: " + data[1] + " 2: " + data[2]);

                //Vector3 accel = new Vector3(float.Parse(data[0]).Map(-60f, 60f, -360f, 360f), float.Parse(data[1]).Map(-60f, 60f, -360f, 360f), float.Parse(data[2]).Map(-60f, 60f, -360f, 360f));

                //Vector3 gyro = new Vector3(float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5]));

                Vector3 gyro = new Vector3(float.Parse(data[0]).Map(-600f, 600f, -360f, 360f),  float.Parse(data[2]).Map(-300f, 300f, -360f, 360f),  float.Parse(data[1]).Map(-300f, 300f, -360f, 360f));

                /*
               Debug.Log(accel.ToString());
               Debug.Log(gyro.ToString());
               Debug.Log(magnet.ToString());
               */

                //as of this version, gyro is used for rudimentary work with directioning

                if (!imuInitialized)
                {
                    // This is the first IMU reading; just store it as
                    // the initial IMU rotation.
                    imuInitialOrientation = gyro;
                    imuInitialized = true;
                }
                else
                {
                    //rotates the target object 
                    target.transform.rotation = Quaternion.Lerp(target.transform.rotation, Quaternion.FromToRotation((gyro - imuInitialOrientation), homeOrientation), Time.deltaTime * 3f); //the 3f is a smoothing factor
                    //subtracts initial to get the data into the initial reference frame, then rotates to get it into world space based on the home orientation
                    //spherically interpolates (thus integrating the gyro values) between the current object position and the new euler angles, then gives that to the object as its new position.
                }    
                


            }

        }
        else //serial port is not open
        {
            Debug.LogError("Serial port: " + sp.PortName + " is unavailable");
        }
    }
}