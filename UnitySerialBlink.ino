//Arduino IDE C/C++
unsigned int count = 0;
void setup() 
{
  Serial.begin(115200);
  pinMode(LED_BUILTIN, OUTPUT);
}

void loop() 
{
  if(Serial.available() > 0)
  {
        Serial.print("Serial command #");
        Serial.println(count);
        count++;
        delay(10);

  }
}
