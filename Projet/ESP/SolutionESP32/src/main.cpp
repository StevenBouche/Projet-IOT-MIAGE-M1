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

#include "MQTTAsync/MQTTAsyncClient.h"

#pragma region PinDefinition
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
#pragma endregion PinDefinition

#pragma region JsonFieldDefinition
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
#pragma endregion JsonFieldDefinition

#pragma region IdDefinition
const String idEquipment = "Esp32Robot";
#pragma endregion IdDefinition

#pragma region OtherDefinition
const int LoopDelay = 5000;
uint32_t stateBlink = 0;
int delayTaskBlink = 1000;
#pragma endregion OtherDefinition

#pragma region QueueMqtt
typedef struct DataMQTT {
  char * topic;
  byte * message; 
  unsigned int length;
} DataMQTT_t;

QueueHandle_t queue;
#pragma endregion QueueMqtt

#pragma region WifiVariables
WiFiUDP ntpUDP;
NTPClient timeClient(ntpUDP);
IdentifiantWifi ids[] = {
  IdentifiantWifi("Bbox-4DD70ADE", "551F54E2D72A27CA1EA44567F149E1"),
  IdentifiantWifi("iPhone de Steven", "iobwgn7obkf91")
};
WifiESP wifi;
#pragma endregion WifiVariables

#pragma region MqttVariables

const char* HostMqtt = "62.35.150.64";
const char* DataTopicName = "IOT/Data";
const String ControlerTopicNameSub = "IOT/Controler/" + idEquipment;

#if ASYNC_TCP_SSL_ENABLED
bool mqttSecure = true;
const uint16_t PortMqtt = 8884;
#else
bool mqttSecure = false;
const uint16_t PortMqtt = 1883;
#endif

LinkedList<MQTTAction> actions;
MQTTConfig mqttConfig(HostMqtt, PortMqtt, "", "", "", mqttSecure);
MQTTAsyncClient mqttAsyncClient(&mqttConfig);
#pragma endregion MqttVariables

#pragma region AuthVariables
HTTPClient http; 
const String AuthJson = "{ \"IdEquipment\": \"" + idEquipment +"\", \"Password\": \"25Zjqgr8AQxxZsyz\", \"TypeEquipment\": \"Robot\", \"Role\": \"Station\" }";
const String AuthUrl = "http://62.35.150.64:8000/api/AuthEquipment/auth";
String token = "";
long tokenExpiration = 0;
#pragma endregion AuthVariables

#pragma region BluetoothVariables
BTServer btServer;
#pragma endregion BluetoothVariables

#pragma region StateVariables
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
#pragma endregion StateVariables

#pragma region Auth
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
#pragma endregion Auth

#pragma region Mqtt
/**
 * Callback when received data from MQTT broker from topic controler.
 * 
 * @param message message received
 * @param length size of message
 */
void receivedActionControler(const byte* message, unsigned int length){

  StaticJsonDocument<256> doc;
  DeserializationError error = deserializeJson(doc, message, length);

  if(error != DeserializationError::Ok){
    Serial.println("Error deserialisation"); Serial.println(error.c_str());
    return;
  }
  else if(doc.containsKey(MotorLJson) && doc.containsKey(MotorRJson)){
    state.motorsState->setMotorL(doc[MotorLJson]);
    state.motorsState->setMotorR(doc[MotorRJson]);
    //state.motorsState->print();
    motor.rotateMotor(MOTOR_L, state.motorsState->getMotorL());
    motor.rotateMotor(MOTOR_R, state.motorsState->getMotorR());
  } else {
    Serial.println("Error json parser");
  }
}

void mqtt_pubcallback_async(char* topic, char* payload, AsyncMqttClientMessageProperties properties, size_t len, size_t index, size_t total){
  size_t lenTopic = strlen(topic)+1;
  DataMQTT_t * data = reinterpret_cast<DataMQTT_t*>(pvPortMalloc(sizeof(DataMQTT_t)));
  data->length = len;
  data->topic = (char*)pvPortMalloc(lenTopic*sizeof(char));
  data->message = (byte*)pvPortMalloc(len*sizeof(byte));
  memcpy(data->message,payload,len);
  strcpy(data->topic,topic);
  xQueueSend(queue, (void*) &data, portMAX_DELAY);
}

void onConnectMqtt(){

}

void onDisconnectMqtt(){
  motor.breakMotors();
  tryRefreshAuthorizationForMqtt();
}

/**
 * Init MQTT Client. 
 * Create actions for subscribe topics and execute a callback associate with topic.
 */
void initMQTT(){

  Serial.println("Init MQTT connection.");

  MQTTAction actionToto = MQTTAction(ControlerTopicNameSub, receivedActionControler);
  actions.add(actionToto);
  mqttAsyncClient.setConnectCallback(onConnectMqtt);
  mqttAsyncClient.setDisconnectCallback(onDisconnectMqtt);
  mqttAsyncClient.setMessageCallback(mqtt_pubcallback_async);
  mqttAsyncClient.addTopic(ControlerTopicNameSub.c_str());

  Serial.println("End init MQTT connection.");
}
#pragma endregion Mqtt

#pragma region WifiConnection
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
#pragma endregion WifiConnection

#pragma region Bluetooth

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
#pragma endregion Bluetooth

#pragma region SendingData
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
#pragma endregion SendingData

#pragma region MainTasks
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

void taskHandlePubCallback(void * args){
  DataMQTT_t *data;
  while(true){
    BaseType_t t = xQueueReceive(queue, &data, portMAX_DELAY);
    if(t == pdPASS)
    {
     for(int i = 0; i < actions.size(); i++){
        if(actions.get(i).getTopic() == data->topic){
          actions.get(i).getHandler()((const byte*) data->message,data->length);
          break;
        }
      }
    }
    vPortFree(data); 
  }
  vTaskDelete(NULL);
}
#pragma endregion MainTasks

#pragma region Main
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
#pragma endregion Main