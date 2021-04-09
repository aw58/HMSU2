//C#
using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Linq;
using static Data.Namespace.DataMap; //Team-created class. See Scripts Folder
using System.IO;

public class Interface : MonoBehaviour
{
    SerialPort sp;
    public Transform target; //The item we want to affect with our accelerometer
    public int setInitialOrientationCount = 0;
    public Quaternion homeOrientation;
    public Quaternion initialOrientation;
    public Boolean initialized = false;


    void Start()
    {
        Debug.Log("Inside");
        sp = new SerialPort("COM11", 115200, Parity.None, 8, StopBits.One); //Replace "COMx" with whatever port your Arduino is on.
        sp.DtrEnable = true; //set this to true. False makes it timeout. false = Prevent the Arduino from rebooting once we connect to it. 
                             //A 10 uF cap across RST and GND will prevent this. Remove cap when programming.
        sp.ReadTimeout = 20; //Shortest possible read time out.
        sp.WriteTimeout = 20; //Shortest possible write time out.
        sp.Open();
        if (!sp.IsOpen)
        {
            Debug.LogError("Serial port: " + sp.PortName + " is unavailable");
            sp.Close(); //You can't program the Arduino while the serial port is open, so let's close it.
        }

        homeOrientation = new Quaternion(1, 0, 0, 0);

        // Set a variable to the Documents path.
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        // Append text to an existing file named "WriteLines.txt".
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "outputData.txt"), false))
        {
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
                Debug.Log("Data: ");
                foreach (var item in data)
                {
                    Debug.Log(item);
                }
                Debug.Log("End Data");
                */

                 //Debug.Log("0: " + data[0] + " 1: " + data[1] + " 2: " + data[2] + "3: " + data[3]);

                //initialize the beginning orientation
                /*
                if (setInitialOrientationCount > 500 && !initialized)
                {
                    initialOrientation = new Quaternion(float.Parse(data[3]), -float.Parse(data[0]), -float.Parse(data[1]), -float.Parse(data[2]));
                    initialized = true;
                }
                else
                {
                    setInitialOrientationCount++;
                }
                */

                //create quat's from data. Constructor: Quaternion, x, y, z, w)
                //parse out erroneous i data
                if (data.Length < 7 || data[3] == "nan" ||float.Parse(data[0]) > 2)
                {
                    Debug.Log("Incomplete Data");
                }
                else
                {


                    //write to a file
                    // Set a variable to the Documents path.
                    string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                    // Append text to an existing file named "WriteLines.txt".
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "outputData.txt"), true))
                    {
                        outputFile.WriteLine(DateTime.Now.ToString("h:mm:ss") + ": " + inData);
                    }

                    //transform!
                    Quaternion newQuat = new Quaternion(float.Parse(data[0]), float.Parse(data[3]), -float.Parse(data[2]), -float.Parse(data[1])); //negatives are to align the rotation movement with the plane object
                    target.transform.rotation = Quaternion.Normalize(newQuat); //the #f acts as a smoothing factor
                    // target.transform.rotation = Quaternion.RotateTowards(Quaternion.Normalize(newQuat), Quaternion.Normalize(homeOrientation), 90);
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
