#include <Arduino.h>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"
#include <WiFi.h>
#include "Wifi/IdentifiantWifi.h"
#include "Wifi/WifiESP.h"
#include "Camera/CameraStream.h"
#include <NTPClient.h>
#include <HTTPClient.h>
#include "ArduinoJson.h"
#include <AsyncTCP.h>
#include <esp_wifi.h>

#define BLINK_LED_GREEN (gpio_num_t) 12

const char* ssid = "iPhone de Steven";
const char* password = "iobwgn7obkf91";
const char* ACCESS_TOKEN_JSON_FIELD = "accessToken";
const char* EXPIRATION_JSON_FIELD = "expireAt";

//WiFi
WiFiClient client;
HTTPClient http;

WifiESP wifi;
const IdentifiantWifi ids[] = {
  IdentifiantWifi("Bbox-4DD70ADE", "551F54E2D72A27CA1EA44567F149E1"),
  IdentifiantWifi("iPhone de Steven", "iobwgn7obkf91")
};
const char* SSID = "Bbox-4DD70ADE";
const char* SSID_PASSWORD = "551F54E2D72A27CA1EA44567F149E1";

String token = "";
long tokenExpiration = 0;
const uint16_t portTCP = 11000;
const char* urlTCP = "62.35.150.64";
const String urlAuth = "http://62.35.150.64:8000/api/AuthEquipment/auth";
const String jsonAuth = "{ \"IdEquipment\": \"Esp32Robot\", \"Password\": \"25Zjqgr8AQxxZsyz\", \"TypeEquipment\": \"Robot\", \"Role\": \"Camera\" }";

CameraStream stream;

uint32_t stateBlink = 0;
int delayTaskBlink = 1000;

bool setToken(String message,unsigned long currentTime){

    long diff = 0;
    StaticJsonDocument<512> doc;
    DeserializationError error = deserializeJson(doc, message.c_str(), message.length());

    if(error != DeserializationError::Ok){
        Serial.println("Error deserialisation : " + String(error.c_str()));
        return false;
    }

    else if(doc.containsKey(ACCESS_TOKEN_JSON_FIELD) && doc.containsKey(EXPIRATION_JSON_FIELD)){

        token = String((const char *)doc[ACCESS_TOKEN_JSON_FIELD]);
        tokenExpiration = doc[EXPIRATION_JSON_FIELD];

        diff = tokenExpiration-currentTime;
        Serial.println("Token authentification : " + token);
        Serial.println("Valid time in second : " + String(diff));

    } else {
      Serial.println("Error json parser");
      return false;
    }
    return true;
}

bool authProcess(){

  Serial.println("Start authentification.");

  http.begin(urlAuth);
  http.addHeader("Content-Type", "application/json");

  int httpResponseCode = http.POST(jsonAuth);
  bool result = httpResponseCode>0;

  Serial.print("HTTP Response code: " + String(httpResponseCode));

  if (result) {
    String payload = http.getString();
    result = setToken(payload,0);
  }

  http.end();
  return result;
}

void connectTCP(){

  delayTaskBlink = 250;

  Serial.println("Trying to connect TCP.");

  client.connect(urlTCP, portTCP);
  if(client.connected()){
      int tokenLength = token.length();
      byte *size = (byte*) malloc(4*sizeof(byte)+tokenLength*sizeof(byte));
      memcpy(size, &tokenLength, 4);
      memcpy(size + 4, token.c_str(), tokenLength);
      client.write(size, 4 + tokenLength); 
      free(size);
      stream.executeTask();
  }
  else {
    Serial.println("Connection to host failed.");
    ESP.restart();
  }
}

void stopTaskStream(){
  if(stream.isRunning()){
    Serial.print("Waiting stop task Stream.");
    stream.stopTask();
    
    while(stream.isRunning())
      Serial.print(".");
  }
  Serial.print("/n");
}

void timeout(int maxTime, int delayTime, int* currentTime){
  delay(delayTime);
  *currentTime += delayTime;
  if(*currentTime > maxTime)
    ESP.restart();
}

void connectWifi(){

  delayTaskBlink = 1000;
  
  WiFi.begin(SSID, SSID_PASSWORD);
  int time = 0;

  while (WiFi.status() != WL_CONNECTED) {
    Serial.print(".");
    timeout(20000, delayTaskBlink, &time);
  }

  delayTaskBlink = 500;

  if(WiFi.status() == WL_CONNECTED && !client.connected()){
    if(!authProcess()) ESP.restart();
    else connectTCP();
  }
}

void taskQueue(void * args){

  DataVideo_t *data;

  while(true){

    BaseType_t t = xQueueReceive(stream.queue, &data, portMAX_DELAY);
    if(t == pdPASS)
    {
      if(client.connected()){
        size_t len = data->size;
        byte *message = (byte*) pvPortMalloc(4*sizeof(byte)+len*sizeof(byte));
        memcpy(message, &len, 4);
        memcpy(message + 4, data->buffer, len);
        client.write(message, len + 4);
        free(message);
      } else {
        ESP.restart(); 
      }     
    }
    //Serial.println(t);
    vPortFree(data); 
  }
  vTaskDelete(NULL);
}

void taskBlink(void * args){

  while(true){

    if(WiFi.status() == WL_CONNECTED && client.connected()){
      stateBlink = 1;
      delayTaskBlink = 5000;
    }
    else 
      stateBlink = stateBlink == 0 ? 1 : 0;
  
    gpio_set_level(BLINK_LED_GREEN,  stateBlink);

    vTaskDelay(delayTaskBlink);
  }

  vTaskDelete(NULL);
}

void setup() {
  
  Serial.begin(115200);

  stream.init();
  gpio_pad_select_gpio(BLINK_LED_GREEN);
  gpio_set_direction(BLINK_LED_GREEN, GPIO_MODE_OUTPUT);

  //clientTCP->onConnect(onConnectTCP);
  //clientTCP->onDisconnect(onDisconnectTCP);

  /*if(queue == NULL){
    Serial.println("Error creating the queue");
    ESP.restart();
  }*/

  http.setReuse(true);
  WiFi.mode(WIFI_STA);
  esp_wifi_set_ps(WIFI_PS_NONE);

  xTaskCreatePinnedToCore(taskQueue, "QueueTCP", 16384, NULL, 1, NULL, 1);
  xTaskCreatePinnedToCore(taskBlink, "Blink", 2048, NULL, 2, NULL, 1);

  connectWifi();

}

void loop() {    
  if(!client.connected())
    ESP.restart(); 
  delay(5000);
}