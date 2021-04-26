#ifndef MQTTClient_h
#define MQTTClient_h

#include <Arduino.h>
#include <WiFi.h>
#include <PubSubClient.h>
#include <string>
#include <functional>
#include "LinkedList.h"
#include "MQTTAction.h"

class MQTTClient{

    private:
        PubSubClient * clientMQTT;
        TaskHandle_t taskHandleMQTT;
        bool runningMQTT;
        int waitingTimeMQTT = 100;
        void subscriptTopics();

    public: 
        String host;
        uint16_t port;
        String id;
        String username;
        String passwd;
        LinkedList<MQTTAction> *actions;
        void taskMQTT();

        MQTTClient(PubSubClient * clientMQTT, String host, uint16_t port, String id){
            this->host = host;
            this->port = port;
            this->id = id;
            this->username = "";
            this->passwd = "";
            this->clientMQTT = clientMQTT;
            this->runningMQTT = false;
        }

        void setActions(LinkedList<MQTTAction> *actions){
            this->actions = actions;
        }

        bool isConnected(){
            return this->clientMQTT->connected();
        }

        void stop();
        void execute(std::function<void(char*, uint8_t*, unsigned int)> callback);
        void sendData(String topic, String payload);
};

#endif



