#include "MQTTClient.h"
#include <iostream>
#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

void MQTTClient::subscriptTopics(){

    if(!this->clientMQTT->connected())
      return;
      
    for(int i = 0; i < this->actions->size(); i++){
      MQTTAction action = this->actions->get(i);
      this->clientMQTT->subscribe(action.getTopic().c_str());
    }
}

void MQTTClient::taskMQTT(){

    this->runningMQTT = true;

    while(this->runningMQTT){

      if(!this->clientMQTT->connected()) { 

          Serial.println("Attempting MQTT connection...");
          bool connected = this->clientMQTT->connect(this->id.c_str(), this->username.c_str(), this->passwd.c_str());

          if (connected){
            Serial.println("MQTT connected");
            this->subscriptTopics();
          }
          else 
            Serial.println("Failed connection MQTT.");
      } 
      else 
        this->clientMQTT->loop();
    
    vTaskDelay(this->waitingTimeMQTT); 
  }

  Serial.println("Stop task MQTT.");
  vTaskDelete(NULL);
}

void MQTTClient::sendData(String topic, String payload){

  if(!clientMQTT->connected())
    return;

  clientMQTT->publish(topic.c_str(), payload.c_str());

  clientMQTT->loop();

}

void startTask(void* _this){
  reinterpret_cast<MQTTClient*>(_this)->taskMQTT();
}

void MQTTClient::stop(){
  this->runningMQTT = false;
  this->clientMQTT->disconnect();
}

void MQTTClient::execute(std::function<void(char*, uint8_t*, unsigned int)> callback){

  if(this->runningMQTT)
    return;

  clientMQTT->setServer(this->host.c_str(), this->port);
  clientMQTT->setCallback(callback);

  xTaskCreate(startTask, "Handle MQTT", 2048, this, 1, &taskHandleMQTT);

}









