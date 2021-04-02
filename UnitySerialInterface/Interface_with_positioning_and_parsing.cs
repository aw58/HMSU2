//C#
using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Linq;
using static Data.Namespace.DataMap; //Team-created class: See Unity Scripts Folder- A mapping function for use with raw Euler Angles
using System.IO; //for writing to a file

public class Interface : MonoBehaviour
{
    SerialPort sp;
    public Transform target; //Fill this with the object this script is attached to
    private int setInitialOrientationCount = 0; //counter for the initial waiting period
    private Vector3 homeOrientation; //the orientation of the target as it is originally in the game space
    private Quaternion initialOrientation; //the orientation of the device after the initialization waiting period
    private Boolean initialized = false;

    private string[] data; //where the incoming data is stored for each frame update

    //positioning initialization
    private Vector3 prevVel = new Vector3(0f, 0f, 0f); //the velocity of the previous frame
    //initial position is set by where the object is originally placed in the game-space



    void Start()
    {
        sp = new SerialPort("COM11", 115200, Parity.None, 8, StopBits.One); //Replace "COMx" with whatever port your Arduino is on. See this via the Arduino IDE while the device is plugged in.
        sp.DtrEnable = true; //set this to true. False makes it timeout. false = Prevent the Arduino from rebooting once we connect to it. 
                             //A 10 uF cap across RST and GND will prevent this. Remove cap when programming.
        sp.ReadTimeout = 50; //Shortest possible read time out. If timeout Errors are given by Unity, either the device is not turned on properly, or this value can be increased
        sp.WriteTimeout =50; //Shortest possible write time out. Not used in this code since no messages are currently sent to the device
        sp.Open(); //open Serial port communication
        if (!sp.IsOpen)
        {
            Debug.LogError("Serial port: " + sp.PortName + " is unavailable");
            sp.Close(); //You can't program the Arduino while the serial port is open, so let's close it.
        }

        homeOrientation = target.transform.rotation.eulerAngles; //the in-game starting orientaiton, as defined by where the object is originally rotated relative to the game-space

        // Set a variable to the Documents path.
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        // Either create (if it doesn't exist) or clear the contents of the file "outputData.txt" in the computer's Documents folder
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "outputData.txt"), false))
        {
            //Output any start-of-document text here
        }


    }

    void Update()
    {
        if (sp.IsOpen)
        {
            //read from the COM Port
            string inData = sp.ReadLine();

            //Debug.Log("Arduino==>" + inData);

            //must have these lines for when the arduino sends faster than the readTimeout
            sp.BaseStream.Flush();
            sp.DiscardInBuffer();

            if (inData != "")
            { //throw out empty data

                //DATA FORMAT:
                //"RVw,RVx,RVy,RVz,Ax,Ay,Az\n" no spaces, newline character at the end


                //parse the data into an array of strings
                data = inData.Split(',');

                //Debug.Log(data.Length);


                /*
                Debug.Log("Data: ");
                foreach (var item in data)
                {
                    Debug.Log(item);
                }
                Debug.Log("End Data");
                */

                //initialize starting orientation after a waiting period
                /*
                if (setInitialOrientationCount > 1000 && !initialized) //wait for 1000 frames to pass- about 1 sec
                {
                    initialOrientation = new Quaternion(float.Parse(data[3]), -float.Parse(data[0]), -float.Parse(data[1]), -float.Parse(data[2]));
                    
                    initialized = true;
                }
                else
                {
                    setInitialOrientationCount++;
                }
                */
                //Debug.Log("0: " + data[0] + " 1: " + data[1] + " 2: " + data[2] + "3: " + data[3] + data[1] + " 4: " + data[4] + "5: " + data[5] + " 6: " + data[6]);

                //create quaternions from incoming orientation data. Constructor: Quaternion, x, y, z, w)
                //parse out erroneous data (experimentally found)
                if (data.Length < 7 || data[0] == "" || data[3] == "" || (!inData.StartsWith("0") && !inData.StartsWith("1")))
                {
                    //Debug.Log("Incorrect data was sent.");
                }
                else //data is correct and complete, continue to pass that on to the target
                {

                    //write to a file
                    // Set a variable to the Documents path.
                    string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    //Append text to an existing file named "outputData.txt".
                    using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "outputData.txt"), true))
                    {
                        outputFile.WriteLine(DateTime.Now.ToString("h:mm:ss") + "- Qw: " + data[0] + " Qx: " + data[1] + " Qy: " + data[2] + " Qz: " + data[3] + "Ax: " + data[4] + " Ay: " + data[5] + " Az: " + data[6]);
                    }

                    //transform orientation!
                    Quaternion newQuat = new Quaternion(float.Parse(data[0]), float.Parse(data[3]), -float.Parse(data[2]), -float.Parse(data[1])); //out-of-orderness and negatives are to align the rotation movement with the cursor object
                    target.transform.rotation = Quaternion.Normalize(newQuat); //normalize, then update orientation
                    target.transform.Rotate(homeOrientation.x, homeOrientation.y, -homeOrientation.z, Space.Self);

                    //transform positioning
                    var deltaPos = new Vector3(prevVel.x * Time.deltaTime + ((1 / 2) * float.Parse(data[4]) * Time.deltaTime * Time.deltaTime), prevVel.y * Time.deltaTime + ((1 / 2) * float.Parse(data[5]) * Time.deltaTime * Time.deltaTime), prevVel.z * Time.deltaTime + ((1 / 2) * float.Parse(data[6]) * Time.deltaTime * Time.deltaTime));
                    //Debug.Log("deltaPos: " + deltaPos +", prevVelX: " + prevVel.x);
                    target.transform.position = target.transform.position + deltaPos;

                    //Debug.Log("Position: " + target.transform.position);

                    //multiply accel by delta time to get velocity, then store that for the next frame to use
                    prevVel.x = float.Parse(data[4]) * Time.deltaTime *100;
                    prevVel.y = float.Parse(data[5]) * Time.deltaTime * 100;
                    prevVel.z = float.Parse(data[6]) * Time.deltaTime * 100;
                }
            }
        }
        else //serial port is not open
        {
            //Debug.LogError("Serial port: " + sp.PortName + " is unavailable. Please check system power and expected COM Port.");
        }
    }
}