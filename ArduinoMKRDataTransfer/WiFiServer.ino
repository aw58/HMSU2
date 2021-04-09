#include <SPI.h>
#include <WiFiNINA.h>
#include <MKRIMU.h>

char ssid[] = "SlamDunksOnly";          //  your network SSID (name)
char pass[] = "PFCPelkington";   // your network password
int status = WL_IDLE_STATUS;

IPAddress ip(192,168,1,231);
IPAddress gateway(192,168,1,1);
IPAddress subnet(255,255,255,0);

WiFiServer server(80); //sets up port: 5001 as TCP socket port
WiFiClient client; //estables a client connection



void setup() {

  //Initialize serial and wait for port to open:
  Serial.begin(9600);
  while (!Serial);// wait for serial port to connect. Needed for native USB port only
  
  if(!IMU.begin())
  {
    Serial.println("Failed to initalize IMU!");

    while(1);
  }
  else
  {
    Serial.println("IMU initialized!");
  }

  Serial.print("Accelerometer sample rate = ");
  Serial.print(IMU.accelerationSampleRate());
  Serial.println(" Hz");
  Serial.println();
  Serial.println("Acceleration in G's");
  Serial.println("X\tY\tZ");

  Serial.println("Attempting to connect to WPA network...");
  WiFi.begin(ssid, pass);
  WiFi.config(ip, gateway, subnet);
  
  if ( WiFi.status() != WL_CONNECTED) {
    Serial.println("Couldn't get a wifi connection");
    while(true);
  }
  else {
    Serial.print("Connected to wifi. My address:");
    IPAddress myAddress = WiFi.localIP();
    printWiFiStatus();
    server.begin();
  }
  
}

void loop() {
  client = server.available();

  float x, y, z;

  if(client)
  {
    Serial.println("Client Connected");
    while(client.connected())
    {
      //Serial.println(1);
      //client.print(1);
      //client.print('\r');

      if(IMU.accelerationAvailable())
      {
        IMU.readAcceleration(x,y,z);

        Serial.print(x);
        Serial.print('\t');
        Serial.print(y);
        Serial.print('\t');
        Serial.println(z);

        client.print(x);
        client.print('\t');
        client.print(y);
        client.print('\t');
        client.print(z);   
        client.print('\r');    
      }

      delay(1);
    }
  }
}

void printWiFiStatus() {
  // print the SSID of the network you're attached to:
  Serial.print("SSID: ");
  Serial.println(WiFi.SSID());

  // print your board's IP address:
  IPAddress ip = WiFi.localIP();
  Serial.print("IP Address: ");
  Serial.println(ip);

  // print the received signal strength:
  long rssi = WiFi.RSSI();
  Serial.print("signal strength (RSSI):");
  Serial.print(rssi);
  Serial.println(" dBm");
}
