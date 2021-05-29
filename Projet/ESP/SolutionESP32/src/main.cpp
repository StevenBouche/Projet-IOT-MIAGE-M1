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

void printAuth(unsigned long currentTime){
  String s = "Token authentification : ";
  s += String(token) + "\n";
  s += "Valid time in second : ";
  s += String(tokenExpiration-currentTime) + "seconds\n";
  Serial.println(s);
}

bool setToken(String message, unsigned long currentTime){

  StaticJsonDocument<512> doc;
  DeserializationError error = deserializeJson(doc, message.c_str(), message.length());

  if(error != DeserializationError::Ok)
      Serial.println(error.c_str());
  else if(doc.containsKey(AccessTokenJson) && doc.containsKey(ExpirationTokenJson)){
      token = String((const char *)doc[AccessTokenJson]);
      tokenExpiration = doc[ExpirationTokenJson];
      printAuth(currentTime);
      return true;
  } else 
    Serial.println("Error json parser");
  
  return false;
}

bool authProcess(unsigned long currentTime){

  Serial.println("Start authentification.");

  bool result = false;
  String payload = ""; 

  http.begin(AuthUrl);

  http.addHeader("Content-Type", "application/json");
  int httpResponseCode = http.POST(AuthJson);

  Serial.println("HTTP Response code: " + String(httpResponseCode)); 

  if (httpResponseCode > 0) {
    payload = http.getString();
    result = setToken(payload,currentTime);
  }

  http.end();
  return result;
}

void tryRefreshAuthorizationForMqtt(){

  if(mqttAsyncClient.connected())
    return;

  if(!wifi.isConnected())
    return;

  timeClient.update();
  unsigned long currentTime = timeClient.getEpochTime();

  String s = "Verify expiration token authentification : \n\tCurrent : ";
  s += String(currentTime) + "\n";
  s += "\tExpiration : " + String(tokenExpiration) + "\n";
  Serial.println(s);

  if(currentTime<tokenExpiration)
    return;
  
  if(!authProcess(currentTime))
    return;
  
  mqttConfig.setUsername(token);
  mqttConfig.setID(wifi.getAddrMac());
  mqttConfig.printConfig();

}
//endregion auth

//region WiFi

/**
 * Callback WiFi connected. Execute task MQTT Client when connected.
 */
void onConnectWifi(){
  Serial.println("Connected WiFi event trigger.");
  timeClient.begin();
  tryRefreshAuthorizationForMqtt();
  mqttAsyncClient.executeTask();
}

/**
 * Callback WiFi dicconnected. Stop task MQTT Client and stop motors.
 */
void onDisconnectWifi(){
  Serial.println("Disconnected WiFi event trigger.");
  timeClient.end();
  motor.breakMotors();
  mqttAsyncClient.stopTask();
}

/**
 * Init WiFi. Push all indentifiants wifi for multi AP. 
 * Subscribe events WiFi whith differents callbacks and execute task WiFi.
 */
void initWiFi(){

  Serial.println("Init WiFi connection.");

  int sizeIds = *(&ids + 1) - ids;
  for(int i = 0; i < sizeIds; i++){
    wifi.addIdentifiantWifi(ids[i]);
  }
  
  wifi.subEventConnect(onConnectWifi);
  wifi.subEventDisconnect(onDisconnectWifi);

  Serial.println("End init WiFi connection.");

}
//endregion WiFi

//region BLE

/**
 * Callback when received data from Bluetooth client.
 * 
 * @param message message received
 */
void receivedActionBluetooth(String *message){

  StaticJsonDocument<256> doc;
  DeserializationError error = deserializeJson(doc, message->c_str());

  if(error != DeserializationError::Ok)
      return;

  if(doc.containsKey(LatitudeJson) && doc.containsKey(LongitudeJson)){
    state.gpsState->setLongitude(doc[LongitudeJson]);
    state.gpsState->setLatitude(doc[LatitudeJson]);
  }
}

/**
 * Init Bluetooth Server with callback on receive message. 
 */
void initBLE(){
  Serial.println("Init Bluetooth server.");
  btServer.setup(receivedActionBluetooth);
  Serial.println("End init Bluetooth server.");
}
//endregion BLE

//region SendData
void sendDataIOT(){

  //actualize data before send MQTT broker
  sensors.readValues();

  //Serialize data
  String output;
  DynamicJsonDocument doc(512);

  doc[IdEspJson] = idEquipment.c_str();
  doc[LatitudeJson] = state.gpsState->getLatitude();
  doc[LongitudeJson] = state.gpsState->getLongitude();
  doc[LigthJson] = state.sensorsState->getLight();
  doc[TemperatureJson] = state.sensorsState->getTemperature();

  serializeJson(doc, output);

  //Serial.println(output);
  //send data to broker
  mqttAsyncClient.sendData(DataTopicName, output);
}
//end region SendData

void taskActionConnection(void * args){

  while(true){

    if(!wifi.isConnected() && !mqttAsyncClient.connected()){
      delayTaskBlink = 300;
      stateBlink = stateBlink == 0 ? 1 : 0;
    }
    else if(wifi.isConnected() && !mqttAsyncClient.connected()){
      delayTaskBlink = 150;
      stateBlink = stateBlink == 0 ? 1 : 0;
    }
    else if(wifi.isConnected() && mqttAsyncClient.connected()){
      delayTaskBlink = 2000;
      stateBlink = 1;
    }
      
    gpio_set_level(BLINK_LED_GREEN,  stateBlink);
    vTaskDelay(delayTaskBlink);
  }

  vTaskDelete(NULL);
}

void setup() {

  initArduino();
  Serial.begin(115200);

  http.setReuse(true);

  //init queue mqtt
  queue = xQueueCreate(10, sizeof(struct DataMQTT));
  xTaskCreatePinnedToCore(taskHandlePubCallback, "QueueTCP", 16384, NULL, 1, NULL, 0);
  xTaskCreatePinnedToCore(taskActionConnection, "ActionConnection", 2048, NULL, 2, NULL, 0);

  motor.init();
  sensors.initSensors();

  initWiFi();
  initMQTT();

  gpio_pad_select_gpio(BLINK_LED_GREEN);
  gpio_set_direction(BLINK_LED_GREEN, GPIO_MODE_OUTPUT);

  wifi.executeTask();

  while(!wifi.isConnected()){
    delay(1000);
  }
    
  initBLE();

}

void loop() {
  sendDataIOT();
  delay(LoopDelay);
}