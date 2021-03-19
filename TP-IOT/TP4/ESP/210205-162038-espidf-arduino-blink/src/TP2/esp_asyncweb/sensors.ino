/*--------------------------------*/
void setup_led(int LEDpin, int mode, int status){
  pinMode(LEDpin, mode);
  digitalWrite(LEDpin, status);// Set outputs to LOW
}

/*--------------------------------*/
String get_temperature(DallasTemperature tempSensor) {
  /* Renvoie la valeur du capteur de temperature dans une String
   * Attention !!
   *    J'ai enleve le delay mais convertir prend du temps ! 
   *    moins que les requetes Http.
   */
  float t;
  String s;
  tempSensor.requestTemperaturesByIndex(0);
  t = tempSensor.getTempCByIndex(0);
  s = String(t);
  Serial.println("Temperature : "+s+" C"); // for debug
  return s;
}
/*--------------------------------*/
String get_light(int LightPin) {
  /* Renvoie la valeur du capteur de lumiere dans une String
   */
  int sensorValue;
  sensorValue = analogRead(LightPin);
  String s = String(sensorValue);
  Serial.println("Light : "+s+" Lum"); // for debug
  return s;
}
