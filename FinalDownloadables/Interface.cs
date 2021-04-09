//C#
using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System;
using System.Linq;
using System.IO; //for writing to a file
using System.Text.RegularExpressions; //data parsing with Regular Expressions


public class Interface : MonoBehaviour
{
    public int waitToInitializeTable = 300; //number of frames before initializing orientation and position
    public int waitToInitializeBody = 1000; //number of frames before initializing orientation and position
    public Transform target; //Fill this with the object this script is attached to in the Unity editor
   
    private SerialPort sp; //for connecting to the serial port and reading data
    private string[] data; //where the incoming data is stored for each frame update

    //positioning initialization
    private Vector3 prevVel = new Vector3(0f, 0f, 0f); //the velocity of the previous frame
    private Vector3 velBias; //used to offset calculated velocities by an initial, sitting-still value
    private int initializeTableCount = 0; //counter for the table waiting period
    private int initializeBodyCount = 0; //counter for the body waiting period
    private Vector3 homeOrientation; //the orientation of the target as it is originally in the World space
    private Vector3 initialOrientation; //the orientation of the device after the Body initialization waiting period
    private Boolean initializedTable = false; //used for initialization control
    private Boolean initializedBody = false; //used for initialization control
    private Vector3 initialPos; //used for outputting relative positions to the data file



    void Start()
    {
        //UPDATE THIS LINE FOR EVERY NEW COMPUTER THE DEVICE RUNS ON
        sp = new SerialPort("COM11", 115200, Parity.None, 8, StopBits.One); //Replace "COMx" with whatever port your Arduino is on. See this via the Arduino IDE while the device is plugged in.
        sp.DtrEnable = true; //set this to true. False makes it timeout. false = Prevent the Arduino from rebooting once we connect to it. 
                             //A 10 uF cap across RST and GND will prevent this. Remove cap when programming.
        sp.ReadTimeout = 40; //Shortest possible read time out. If timeout Errors are given by Unity, either the device is not turned on properly, or this value can be increased
        sp.WriteTimeout = 40; //Shortest possible write time out. Not used in this code since no messages are currently sent to the device
        sp.Open(); //open Serial port communication
        if (!sp.IsOpen)
        {
            Debug.LogError("Serial port: " + sp.PortName + " is unavailable");
            sp.Close(); //You can't program the Arduino while the serial port is open, so let's close it.
        }

        homeOrientation = target.transform.rotation.eulerAngles; //the in-game starting orientaiton, as defined by where the object is originally rotated relative to the world-space
        //Debug.Log("homeOrientation:" + homeOrientation);

        // Set a variable to the Documents path. The file will be in the root documents folder under the name held in docPath
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        // Either create (if it doesn't exist) or clear the contents of the file "outputData.txt" in the computer's Documents folder
        using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "outputData.txt"), false))
        {
            //Output any start-of-document text here
            outputFile.WriteLine("Start of Data:");
        }

    }

    void Update() //repeats every frame refresh
    {
        if (sp.IsOpen)
        {
            //read from the COM Port
            string inData = sp.ReadLine();

            //Print raw incoming strings, unfiltered
            //Debug.Log("Arduino==>" + inData);

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

                //initialize velocity bias while resting on a table. This waits for a certain amount of time for the researcher to place the device somewhere it is completely still
                if (initializeTableCount > waitToInitializeTable && !initializedTable)
                {
                    initializedTable = true;
                    velBias = new Vector3(float.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]));
                    Debug.Log("Table-initialized.");
                }
                else
                {
                    initializeTableCount++;
                }

                if (initializeBodyCount > waitToInitializeBody && !initializedBody)
                {
                    initializedBody = true;
                    initialPos = target.transform.position;
                    initialOrientation = target.transform.localEulerAngles; //get current rotation and use that to initialize
                    Debug.Log("Body-initialized.");
                }
                else
                {
                    initializeBodyCount++;
                }

                //Debug.Log("0: " + data[0] + " 1: " + data[1] + " 2: " + data[2] + "3: " + data[3] + data[1] + " 4: " + data[4] + "5: " + data[5] + " 6: " + data[6]);

                //parse out erroneous data (experimentally found)
                if (data.Length < 7 || data[3] == "" || (float.Parse(data[0]) == 0 && (data[0] != "0.00")) || float.Parse(data[0]) > 1)
                {
                    //Debug.Log("Incorrect data was sent.");
                }
                else //data is correct and complete, continue to pass that on to the target
                {
                    if (initializedBody)
                    {
                        /* //for debugging orientation corrections
                       Debug.Log("before rotating: " + target.transform.rotation.eulerAngles);
                       Debug.Log("home: " + homeOrientation + " initial: " + initialOrientation + " difference z: " + (initialOrientation.z - homeOrientation.z));
                       target.transform.Rotate(homeOrientation.z - initialOrientation.z, homeOrientation.x - initialOrientation.x, homeOrientation.y - initialOrientation.y, Space.World); //prient the cursor to the world space after the initialization period
                       Debug.Log("after rotating: " + target.transform.rotation.eulerAngles);
                        */

                        //transform orientation!
                        //create a quaternion from incoming orientation data. Constructor: Quaternion, x, y, z, w)
                        Quaternion newQuat = new Quaternion(float.Parse(data[0]), float.Parse(data[3]), float.Parse(data[1]), -float.Parse(data[2])); //out-of-orderness and negatives are to align the rotation movement with the cursor object. experimentally found
                        target.transform.rotation = Quaternion.Normalize(newQuat); //normalize, then update orientation
                        target.transform.Rotate(homeOrientation.x-initialOrientation.x, 0, homeOrientation.z-initialOrientation.z, Space.Self); //rotate to initialize about homeOrientation

                        //transform positioning!
                        var deltaPos = new Vector3((prevVel.x * Time.deltaTime + ((1 / 2) * float.Parse(data[4]) * Time.deltaTime * Time.deltaTime)) * 2, (prevVel.y * Time.deltaTime + ((1 / 2) * float.Parse(data[5]) * Time.deltaTime * Time.deltaTime)) * 2, (prevVel.z * Time.deltaTime + ((1 / 2) * float.Parse(data[6]) * Time.deltaTime * Time.deltaTime)) * 2);
                        target.transform.position = target.transform.position + deltaPos;

                        //Debug.Log("Position: " + target.transform.position);

                        //multiply accel by delta time to get velocity, then store that for the next frame to use
                        prevVel.x = (float.Parse(data[4]) * Time.deltaTime * 10) - velBias.x; //multiplying by an integer factor is necessary to avoid truncation of such small numbers. This also increases velocity sensitivity.
                        prevVel.y = (float.Parse(data[5]) * Time.deltaTime * 10) - velBias.y;
                        prevVel.z = (float.Parse(data[6]) * Time.deltaTime * 10) - velBias.z;

                        //write to a file
                        // Set a variable to the Documents path.
                        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        //Append text to an existing file named "outputData.txt".
                        using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "outputData.txt"), true))
                        {
                            //outputFile.WriteLine(DateTime.Now.ToString("h:mm:ss") + "- Qw: " + data[0] + " Qx: " + data[1] + " Qy: " + data[2] + " Qz: " + data[3] + "Ax: " + data[4] + " Ay: " + data[5] + " Az: " + data[6]);
                            outputFile.WriteLine(DateTime.Now.ToString("h:mm:ss") + " X_rot: " + target.transform.rotation.eulerAngles.x + " Y_rot: " + target.transform.rotation.eulerAngles.y + " Z_rot: " + target.transform.rotation.eulerAngles.z + " X_pos: " + (target.transform.position.x - initialPos.x) + " Y_pos: " + (target.transform.position.y - initialPos.y) + " Z_pos: " + (target.transform.position.z - initialPos.z));
                        }
                    }
                }
            }

            //these lines discard any data that was passed to the serial port since starting the frame. Removing these lines causes a backup of data in the buffers
            sp.BaseStream.Flush();
            sp.DiscardInBuffer();
        }
        else //serial port is not open
        {
            Debug.LogError("Serial port: " + sp.PortName + " is unavailable. Please check system power and expected COM Port.");
        }
    }
}
