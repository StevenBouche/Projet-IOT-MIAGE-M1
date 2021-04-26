#include <Arduino.h>
#include "Models/StateApp.h"
#include "Wifi/IdentifiantWifi.h"
#include "Wifi/WifiESP.h"
#include "MQTT/MQTTClient.h"
#include "UDP/UDPClient.h"
#include "Bluetooth/BTServer.h"
#include "Handler/DataHandler.h"


WiFiClient clientWifi;
PubSubClient clientMQTT(clientWifi);
WifiESP wifi;
WiFiUDP udp;

LinkedList<MQTTAction> actions;
MQTTClient mqttClient = MQTTClient(&clientMQTT, "62.35.150.64", 1883, "ESPID-123456789");
UDPClient udpClient = UDPClient(&udp, "localhost", 4000);

BTServer btServer;

StateApp state;

void onDataRead(StateApp *state){

}

DataHandler dataHandler(&state, onDataRead);

void mqtt_pubcallback(char* topic, byte* message, unsigned int length) {
  
  String messageTemp;
  String topicStr(topic);
  for(int i = 0 ; i < length ; i++) 
    messageTemp += (char) message[i];
 
  for(int i = 0; i < actions.size(); i++){
    
    auto action = actions.get(i);

    if(action.getTopic() == topicStr){
      action.getHandler()(&messageTemp);
      break;
    }
  }
}

void onConnectWifi(){
    Serial.println("Connected event trigger.");
    mqttClient.execute(mqtt_pubcallback);
}

void onDisconnectWifi(){
    Serial.println("Disconnected event trigger.");
    mqttClient.stop();
}

void onBluetoothMessage(String message){
    Serial.println("On BLE message");
    Serial.println(message);
    mqttClient.stop();
}

void setup() {

  initArduino();

  Serial.begin(115200);

  //init MQTT
  MQTTAction actionToto = MQTTAction("IOT/toto", [](String* message) { Serial.println(*message); });
  actions.add(actionToto);
  mqttClient.setActions(&actions);

  //init Wifi
  IdentifiantWifi id = IdentifiantWifi("iPhone de Steven", "iobwgn7obkf91");
  wifi.addIdentifiantWifi(id);
  wifi.subEventConnect(onConnectWifi);
  wifi.subEventDisconnect(onDisconnectWifi);
  wifi.execute();

  //init Bluetooth
  btServer.setup(onBluetoothMessage);
  
}

void loop() {
  //mqttClient.sendData("IOT/tata","Hello world!");
  delay(5000);
}