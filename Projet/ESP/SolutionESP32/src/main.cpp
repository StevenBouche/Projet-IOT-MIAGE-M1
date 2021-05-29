#include <Arduino.h>
#include "ArduinoJson.h"
#include "Wifi/IdentifiantWifi.h"
#include "Wifi/WifiESP.h"

#include <WiFiUdp.h>
#include <NTPClient.h>
#include <HTTPClient.h>
#include "Bluetooth/BTServer.h"
#include "Models/StateApp.h"

#include "Handler/MotorHandler.h"
#include "Handler/SensorHandler.h"

//region PIN
#define PIN_MOTOR_L_1_1 12 //D12 OUTPUT
#define PIN_MOTOR_L_1_2 14 //D14 OUTPUT
#define PIN_MOTOR_L_PWM_1 13 //D13 OUTPUT
#define PIN_MOTOR_R_2_1 26 //D26 OUTPUT
#define PIN_MOTOR_R_2_2 25 //D25 OUTPUT
#define PIN_MOTOR_R_PWM_2 27 //D27 OUTPUT
#define MOTOR_L (motor_num_t) 1
#define MOTOR_R (motor_num_t) 2
#define PIN_SENSOR_LIGTH (adc1_channel_t) ADC1_CHANNEL_5 //D34 INPUT
#define PIN_SENSOR_TEMPERATURE (uint8_t) 35 //D35 INPUT
#define OBSTACLE_L 19
#define OBSTACLE_R 21
#define ULTRASONIC_PING 23
#define ULTRASONIC_PONG 22
#define BLINK_LED_GREEN (gpio_num_t) 19
//endregion PIN


//region Const
//  Json
const char* MotorLJson = "MotorL";
const char* MotorRJson = "MotorR";
const char* LatitudeJson = "Latitude";
const char* LongitudeJson = "Longitude";
const char* IdEspJson = "IdESP";
const char* TimestampJson = "Timestamp";
const char* LigthJson= "Ligth";
const char* TemperatureJson = "Temperature";
const char* AccessTokenJson = "accessToken";
const char* ExpirationTokenJson = "expireAt";
const String idEquipment = "Esp32Robot";
//  Mqtt
const uint16_t PortMqtt = 1883;
const char* HostMqtt = "62.35.150.64";
const char* DataTopicName = "IOT/Data";
const String ControlerTopicNameSub = "IOT/Controler/" + idEquipment;

//  Authentification
const String AuthJson = "{ \"IdEquipment\": \"" + idEquipment +"\", \"Password\": \"25Zjqgr8AQxxZsyz\", \"TypeEquipment\": \"Robot\", \"Role\": \"Station\" }";
const String AuthUrl = "http://62.35.150.64:8000/api/AuthEquipment/auth";
String token = "";
long tokenExpiration = 0;
// Other
const int LoopDelay = 5000;
uint32_t stateBlink = 0;
int delayTaskBlink = 1000;
//endregion Const

typedef struct DataMQTT {
  char * topic;
  byte * message; 
  unsigned int length;
} DataMQTT_t;

QueueHandle_t queue;

//region variable_connection
//  WiFi
IdentifiantWifi ids[] = {
  IdentifiantWifi("Bbox-4DD70ADE", "551F54E2D72A27CA1EA44567F149E1"),
  IdentifiantWifi("iPhone de Steven", "iobwgn7obkf91")
};
WifiESP wifi;

//  Ntp
WiFiUDP ntpUDP;
NTPClient timeClient(ntpUDP); //time ntp
//  Http
HTTPClient http; 
//  Bluetooth
BTServer btServer;
//endregion variable_connection

//region variable_state_handler
StateApp state;
MotorHandler motor(
  PIN_MOTOR_L_1_1, 
  PIN_MOTOR_L_1_2, 
  PIN_MOTOR_L_PWM_1, 
  PIN_MOTOR_R_2_1 , 
  PIN_MOTOR_R_2_2, 
  PIN_MOTOR_R_PWM_2);
 SensorHandler sensors(state.sensorsState, 
  PIN_SENSOR_TEMPERATURE, 
  PIN_SENSOR_LIGTH, 
  OBSTACLE_L, 
  OBSTACLE_R, 
  ULTRASONIC_PING, 
  ULTRASONIC_PONG);
  //endregion variable_state_handler



void setup() {

}

void loop() {
 
  delay(1000);
}