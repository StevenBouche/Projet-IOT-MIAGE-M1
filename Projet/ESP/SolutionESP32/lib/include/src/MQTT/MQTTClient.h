#ifndef MQTTClient_h
#define MQTTClient_h

#include <Arduino.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include "LinkedList.h"
#include "MQTTAction.h"
#include "MQTTConfig.h"
#include "../Tasks/TaskRobot.h"

typedef void (*OnMessageMQTT)(char*, uint8_t*, unsigned int);

class MQTTClient : public TaskRobot {

    private:
        PubSubClient * clientMQTT;
        int waitingTimeMQTTLoop = 15;
        int waitingTimeMQTT = 2000;
        MQTTConfig *config;
        OnMessageMQTT callbackMessage;
        LinkedList<MQTTAction> *actions;

        void subscriptTopics();
  
    protected:

        void execute();
        void task();
        void stop();

    public: 

        MQTTClient(PubSubClient * clientMQTT, MQTTConfig *config) : TaskRobot("MQTT", 16384, 0)
        {
            this->config = config;
            this->clientMQTT = clientMQTT;

        }

        void setCallback(OnMessageMQTT callback){
            this->callbackMessage = callback;
        }

        void setActions(LinkedList<MQTTAction> *actions){
            this->actions = actions;
        }

        bool isConnected(){
            return this->clientMQTT->connected();
        }

        void sendData(String topic, String payload);
};

/**
 * Subscribe all topics referenced in MQTTAction.
 */
void MQTTClient::subscriptTopics(){
    for(int i = 0; i < this->actions->size(); i++){
      MQTTAction action = this->actions->get(i);
      this->clientMQTT->subscribe(action.getTopic().c_str());
    }
}

/**
 * Excute task og MQTT Client.
 * 
 * While task running, if MQTT client is not connected try to connect to broker, and if is connected, subscribe to topics.
 * Finally if is already connected trigger loop of client 
 * 
 */
void MQTTClient::task(){
    
    while(this->running){

      if(!this->isConnected()) { 

          //trying to connect MQTT broker
          Serial.println("Attempting MQTT connection...");
          bool connected = this->clientMQTT->connect(
            this->config->getID().c_str(), 
            this->config->getUsername().c_str(), 
            this->config->getPasswd().c_str()
          );

          //if connected subscribe
          if (connected){
            Serial.println("MQTT connected");
            this->subscriptTopics();
          }
          else {
            Serial.print("failed, rc=");
            Serial.print(this->clientMQTT->state());
            Serial.println("Failed connection MQTT.");
          }
        //delay connection
        vTaskDelay(this->waitingTimeMQTT); 
      } 
      
      this->clientMQTT->loop();
      vTaskDelay(this->waitingTimeMQTTLoop); 
  }
  Serial.println("Stop task MQTT.");
  //vTaskDelete(NULL);
}

/**
 * Send data to MQTT broker.
 * 
 * @param topic topic receive data
 * @param payload data to send
 * 
 */
void MQTTClient::sendData(String topic, String payload){

  if(!clientMQTT->connected())
    return;

  clientMQTT->publish(
    topic.c_str(), 
    payload.c_str()
  );

  clientMQTT->loop();
}

/**
 * Stop MQTT Client task.
 */
void MQTTClient::stop(){
  this->clientMQTT->disconnect();
}

/**
 * Start MQTT Client task.
 * 
 * @param callback Callback when client receibed data from broker.
 */
void MQTTClient::execute(){

  clientMQTT->setKeepAlive(15);
  clientMQTT->setServer(this->config->getHost(), this->config->getPort());
  clientMQTT->setCallback(callbackMessage);
  clientMQTT->setBufferSize(512);

}

#endif



