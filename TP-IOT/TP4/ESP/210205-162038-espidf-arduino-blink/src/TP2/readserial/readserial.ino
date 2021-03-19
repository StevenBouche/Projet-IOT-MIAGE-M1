#define TRUE 1
#define FALSE 0
char receivedChar;
String receivedStr; // souvent evite .. perf !?
int sensorValue;
int jour = FALSE;
float seuil = 0.0;

void setup() {
  Serial.begin(9600);
  Serial.println("<Serial is ready>");
}

void loop() {
  // L'ESP ecrit
  sensorValue = analogRead(A5);   // read analog input : light
  Serial.print(sensorValue, DEC); // Prints the value to the serial port
  Serial.print("\n");             // as human-readable ASCII text
 
  // L'ESP lit
  while(Serial.available() > 0) {
    //receivedChar = Serial.read();              // Just one byte
    receivedStr =  Serial.readStringUntil('\n'); // A string
    Serial.print("\nI received : "); // say what you got:
    //Serial.println(receivedChar);
    //if (receivedChar == '1') jour = TRUE ; else jour = FALSE;
    Serial.println(receivedStr);
    seuil = receivedStr.toFloat();   
    Serial.println(seuil);
  }
  delay(1000);
}
