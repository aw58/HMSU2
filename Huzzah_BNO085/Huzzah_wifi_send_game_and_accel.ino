/*  Connects to the home WiFi network
 *  Asks some network parameters
 *  Starts WiFi server with fixed IP and listens
 *  When requested from client, sends data to the client from BNO08X IMU
 *  Communicates: wifi_client_receive_imu_data_template.ino
 */

 // To use this on your network, add needed information on lines with ^^^^
 
#include <SPI.h>
#include <ESP8266WiFi.h>

// set up BNO08X
#include <Adafruit_BNO08x.h>
// For SPI mode, we need a CS pin
#define BNO08X_CS 10
#define BNO08X_INT 9
// For SPI mode, we also need a RESET 
//#define BNO08X_RESET 5
// but not for I2C or UART
#define BNO08X_RESET -1
Adafruit_BNO08x  bno08x(BNO08X_RESET);
sh2_SensorValue_t sensorValue;

// set up WiFi
byte ledPin = 2;
char ssid[] = "Bill Wi the Science Fi";               // ^^^^ SSID of your home WiFi
char pass[] = "dinosaurs";               // ^^^^ password of your home WiFi
WiFiServer server(80);                    
IPAddress ip(10,0,0,142);            // ^^^^ IP address of the server -- note that this should be out of the range of my server's DCHP address pool
IPAddress gateway(10,0,0,1);           // ^^^^ gateway of your network (IP saddress of the router)
IPAddress subnet(255,255,255,0);          // ^^^^ subnet mask of your network

void setup() {
  Serial.begin(115200);                   // only for debug
  WiFi.config(ip, gateway, subnet);       // forces the use of the fixed IP address
  WiFi.begin(ssid, pass);                 // connects to the WiFi router
  while (WiFi.status() != WL_CONNECTED) {
    Serial.print(".");
    delay(500);
  }
  server.begin();                         // starts the server
  Serial.println("Connected to wifi");
  Serial.print("Status: "); Serial.println(WiFi.status());  // some parameters from the network
  Serial.print("IP: ");     Serial.println(WiFi.localIP());
  Serial.print("Subnet: "); Serial.println(WiFi.subnetMask());
  Serial.print("Gateway: "); Serial.println(WiFi.gatewayIP());
  Serial.print("SSID: "); Serial.println(WiFi.SSID());
  Serial.print("Signal: "); Serial.println(WiFi.RSSI());
  Serial.print("Networks: "); Serial.println(WiFi.scanNetworks());
  pinMode(ledPin, OUTPUT);

  // Try to initialize BNO08X
  if (!bno08x.begin_I2C()) {
  //if (!bno08x.begin_UART(&Serial1)) {  // Requires a device with > 300 byte UART buffer!
  //if (!bno08x.begin_SPI(BNO08X_CS, BNO08X_INT)) {
    Serial.println("Failed to find BNO08x chip");
    while (1) { delay(10); }
  }
  Serial.println("BNO08x Found!");

  for (int n = 0; n < bno08x.prodIds.numEntries; n++) {
    Serial.print("Part ");
    Serial.print(bno08x.prodIds.entry[n].swPartNumber);
    Serial.print(": Version :");
    Serial.print(bno08x.prodIds.entry[n].swVersionMajor);
    Serial.print(".");
    Serial.print(bno08x.prodIds.entry[n].swVersionMinor);
    Serial.print(".");
    Serial.print(bno08x.prodIds.entry[n].swVersionPatch);
    Serial.print(" Build ");
    Serial.println(bno08x.prodIds.entry[n].swBuildNumber);
  }
  setReports();
  Serial.println("Reading events");
  delay(100);
}

void loop () {
 
  // ****************** collect data
   delay(10);
   if (bno08x.wasReset()) {
    Serial.print("sensor was reset ");
    setReports();
   }
   if (! bno08x.getSensorEvent(&sensorValue)) {
    return;
   }

    /*
    switch (sensorValue.sensorId) {
    
        case SH2_GAME_ROTATION_VECTOR:
          //Serial.print("Game Rotation Vector - r: ");
          Serial.print(sensorValue.un.gameRotationVector.real);
          Serial.print(",");
          //Serial.print(" i: ");
          Serial.print(sensorValue.un.gameRotationVector.i);
          Serial.print(",");
          //Serial.print(" j: ");
          Serial.print(sensorValue.un.gameRotationVector.j);
          Serial.print(",");
          //Serial.print(" k: ");
          Serial.print(sensorValue.un.gameRotationVector.k);
          Serial.print(",");
          break;
    
        case SH2_LINEAR_ACCELERATION:
          // Serial.print("Linear Acceleration x: ");
          Serial.print(sensorValue.un.linearAcceleration.x);
          Serial.print(",");
          //Serial.print(" y: ");
          Serial.print(sensorValue.un.linearAcceleration.y);
          Serial.print(",");
          //Serial.print(" z: ");
          Serial.println(sensorValue.un.linearAcceleration.z);
          break;
      }
    */
  
  // ******************
  
  WiFiClient client = server.available();
  if (client) {
    if (client.connected()) {
      digitalWrite(ledPin, LOW);  // to show the communication only (inverted logic)
      //Serial.println(".");
      String request = client.readStringUntil('\r');    // receives the message from the client
      //Serial.print("From client: "); Serial.println(request);
      client.flush();
      //client.print("Game Rotation Vector - r: ");
      client.print(sensorValue.un.gameRotationVector.real);
      client.print(",");
      //client.print(" i: ");
      client.print(sensorValue.un.gameRotationVector.i);
      client.print(",");
      //client.print(" j: ");
      client.print(sensorValue.un.gameRotationVector.j);
      client.print(",");
      //client.print(" k: ");
      client.print(sensorValue.un.gameRotationVector.k);
      client.print(",");

      // client.print("Linear Acceleration x: ");
      client.print(sensorValue.un.linearAcceleration.x);
      client.print(",");
      //client.print(" y: ");
      client.print(sensorValue.un.linearAcceleration.y);
      client.print(",");
      //client.print(" z: ");
      client.println(sensorValue.un.linearAcceleration.z);
      //client.println('\r'); // sends the answer to the client
      digitalWrite(ledPin, HIGH);
    }
    client.stop();                // terminates the connection with the client
  }
}

// Here is where you define the sensor outputs you want to receive
void setReports(void) {
  Serial.println("Setting desired reports");
  if (! bno08x.enableReport(SH2_GAME_ROTATION_VECTOR)) {
    Serial.println("Could not enable game vector");
  }
  if (!bno08x.enableReport(SH2_LINEAR_ACCELERATION)) {
    Serial.println("Could not enable linear acceleration");
  }
}
