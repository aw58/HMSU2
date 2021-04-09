/*
  Nano33BLESensorExample_AllSensors-SerialPlotter.ino
  Copyright (c) 2020 Dale Giancono. All rights reserved..

  This program is an example program showing some of the cababilities of the
  Nano33BLESensor Library. In this case it outputs sensor data and from all
  of the Arduino Nano 33 BLE Sense's on board sensors via serial in a format
  that can be displayed on the Arduino IDE serial plotter.

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

/*****************************************************************************/
/*INCLUDES                                                                   */
/*****************************************************************************/
#include "Arduino.h"
#include "Nano33BLEAccelerometer.h"
#include "Nano33BLEGyroscope.h"
#include "Nano33BLEMagnetic.h"


/*****************************************************************************/
/*MACROS                                                                     */
/*****************************************************************************/

/*****************************************************************************/
/*GLOBAL Data                                                                */
/*****************************************************************************/
/*
   Objects which we will store data in each time we read
   the each sensor.
*/
Nano33BLEMagneticData magneticData;
Nano33BLEGyroscopeData gyroscopeData;
Nano33BLEAccelerometerData accelerometerData;
float gyroX;
float gyroY;
float gyroZ;
float magX;
float magY;
float magZ;
float  gyroXbias;
float  gyroYbias;
float  gyroZbias;
float  magXbias;
float  magYbias;
float  magZbias;
float  accelSensitivity = 0.00061; //[g/LSB](for +-2g sensing setting)
float  magSensitivity = 0.00014; //[mgauss/LSB]
float  gyroSensitivity = .00875; //[mdps/LSB]
/*****************************************************************************/
/*SETUP (Initialisation)                                                          */
/*****************************************************************************/
void setup()
{
  /* Serial setup for UART debugging */
  Serial.begin(115200);
  /*
     Initialises the all the sensor, and starts the periodic reading
     of the sensor using a Mbed OS thread. The data is placed in a
     circular buffer and can be read whenever.
  */
  Magnetic.begin();
  Gyroscope.begin();
  Accelerometer.begin();

  delay(500);
  Serial.println("Setting up");
  for (int i = 0; i < 100; i++)
  {
    Magnetic.pop(magneticData);
    Gyroscope.pop(gyroscopeData);
    
    gyroX = gyroscopeData.x;
    //Serial.print("biasing x: " +String(gyroscopeData.x));
    gyroY = gyroscopeData.y;
    gyroZ = gyroscopeData.z;
    magX = magneticData.x;
    magY = magneticData.y;
    magZ = magneticData.z;

    gyroXbias += gyroX ;
    gyroYbias += gyroY ;
    gyroZbias += gyroZ ;
    magXbias += magX ;
    magYbias += magY ;
    magZbias += magZ ;

    delay (10);
  }

  gyroXbias = gyroXbias / 100 ;
  gyroYbias = gyroYbias / 100 ;
  gyroZbias = gyroZbias / 100 ;
  magXbias = magXbias / 100 ;
  magYbias = magYbias / 100 ;
  magZbias = magZbias / 100 ;
}

/*****************************************************************************/
/*LOOP (runtime super loop)                                                          */
/*****************************************************************************/
void loop()
{
  /*
     This gets all the data from each sensor. Note that each sensor gets data
     at different frequencies. Seeing as this super loop runs every 50mS, not
     all the sensors will have new data. If a sensor does not have new data,
     the old data will just be printed out again. This is a little sloppy, but
     allows the data to be printed in a coherrent way inside serial plotter.
  */
  Gyroscope.pop(gyroscopeData);
  Magnetic.pop(magneticData);
  Accelerometer.pop(accelerometerData);
  //Serial.print(String(accelerometerData.x) + "," +Serial.print(String(accelerometerData.y)) + "," + Serial.print(String(accelerometerData.z)));
  Serial.print(String(gyroscopeData.x - gyroXbias)+ "," + String(gyroscopeData.y - gyroYbias)+ "," +String(gyroscopeData.z - gyroZbias));
  //Serial.print(String(magneticData.x - magXbias)+ "," + String(magneticData.y - magYbias)+ "," +String(magneticData.z - magZbias));
  Serial.println();
}
