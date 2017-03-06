/*
  Button

 Turns on and off a light emitting diode(LED) connected to digital
 pin 13, when pressing a pushbutton attached to pin 2.


 The circuit:
 * LED attached from pin 13 to ground
 * pushbutton attached to pin 2 from +5V
 * 10K resistor attached to pin 2 from ground

 * Note: on most Arduinos there is already an LED on the board
 attached to pin 13.


 created 2005
 by DojoDave <http://www.0j0.org>
 modified 30 Aug 2011
 by Tom Igoe

 This example code is in the public domain.

 http://www.arduino.cc/en/Tutorial/Button
 */

// constants won't change. They're used here to
// set pin numbers:
const int buttonPins[] = {3,5,6,9};     // the number of the pushbutton pin
const int n=4;
//const int ledPin=buttonPins[3];
// variables will change:
int buttonStates[] = {0,0,0,0};         // variable for reading the pushbutton status

void setup() {
  Serial.begin(115200);
}
//void Show()
//{
//  for(int i=0;i<n;i++)
//  {
//    if(buttonStates[i])
//    {
//      digitalWrite(ledPin,HIGH);
//      return;
//    }
//  }
//  digitalWrite(ledPin,LOW);
//}
void loop() {
//  return;
  // read the state of the pushbutton value:
  for(int i=0;i<n;i++)
  {
    pinMode(buttonPins[i], INPUT);
    buttonStates[i]=digitalRead(buttonPins[i]);
    pinMode(buttonPins[i], OUTPUT);
    digitalWrite(buttonPins[i],/*i==3?(HIGH*2+LOW)/3.0:*/LOW);
  }
  for(int i=0;i<n;i++)Serial.print(buttonStates[i]);
  Serial.println("");
//  Show();
  delay(100);
}
