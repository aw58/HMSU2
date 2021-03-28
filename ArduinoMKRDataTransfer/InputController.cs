using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class InputController : MonoBehaviour
{
    public float CurrentValue = 2;

    public bool StateClient;

    public void Begin(string ipAddress, int port)
    {
        // Give the network stuff its own special thread
        var thread = new Thread(() =>
        {
            Debug.Log("Hello");
            // This class makes it super easy to do network stuff
            var client = new TcpClient();

            // Change this to your devices real address
            client.Connect(ipAddress, port);
            var stream = new StreamReader(client.GetStream());

            // We'll read values and buffer them up in here
            var buffer = new List<byte>();

            StateClient = client.Connected;


            while (StateClient)
            {
                
                // Read the next byte
                var read = stream.Read();

                // We split readings with a carriage return, so check for it 
                if (read == 13)
                {
                    Debug.Log("Hello");
                    // Once we have a reading, convert our buffer to a string, since the values are coming as strings
                    var str = Encoding.ASCII.GetString(buffer.ToArray());

                    var val = int.Parse(str);

                    CurrentValue = val;

                    // Clear the buffer ready for another reading
                    buffer.Clear();
                }
                else
                {
                    // If this wasn't the end of a reading, then just add this new byte to our buffer
                    buffer.Add((byte)read);
                }
                    
            }
        });

        thread.Start();
    }
}
