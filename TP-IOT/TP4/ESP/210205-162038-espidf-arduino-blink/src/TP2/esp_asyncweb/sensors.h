#include "OneWire.h"
#include "DallasTemperature.h"

void setup_led(int LEDpin, int mode, int status);

String get_temperature(DallasTemperature tempSensor);
String get_light(int LightPin);
