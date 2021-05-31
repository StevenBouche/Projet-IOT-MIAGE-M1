#ifndef MQTTAsyncClient_h
#define MQTTAsyncClient_h

#include <Arduino.h>
#include <AsyncMqttClient.h>
#include "../MQTT/MQTTAction.h"
#include "../MQTT/MQTTConfig.h"
#include "../Tasks/TaskRobot.h"
#include <mutex> 
#include <condition_variable> 
#include "LinkedList.h"
#include <functional>

typedef std::function<void()> OnConnectMqtt;
typedef std::function<void()> OnDisconnectMqtt;

class MQTTAsyncClient : public TaskRobot {

    private:
        AsyncMqttClient mqttClient;
        MQTTConfig *config;
        char* username;
        char* password;
        char* idClient;
        LinkedList <const char*> topics;
        OnConnectMqtt callbackConnect;
        OnDisconnectMqtt callbackDisconnect;

        //tasks
        std::mutex m;
        std::condition_variable cv;

        void onMqttConnect(bool sessionPresent);
        void onMqttDisconnect(AsyncMqttClientDisconnectReason reason);

        bool isConnected = false;

        char * cpyChar(String str);

    protected:
    
        void execute();
        void task();
        void stop();

       

    public: 

        MQTTAsyncClient(MQTTConfig *config) : TaskRobot("MQTT", 4096, 0)
        {
            this->config = config;

            auto lambdaDisconnected = [this] (AsyncMqttClientDisconnectReason reason) {
                this->isConnected = false;
                Serial.println("Disconnected to Mqtt broker.");
                if(this->callbackDisconnect)
                    this->callbackDisconnect();
                this->cv.notify_all();
            };

            auto lambdaConnected = [this] (bool sessionPresent) {
                this->isConnected = true;
                Serial.println("Connected to Mqtt broker.");
                for(int i = 0; i < this->topics.size(); i++){
                    mqttClient.subscribe(this->topics.get(i), 0);
                }
                if(this->callbackConnect)
                    this->callbackConnect();
            };

            this->mqttClient.onConnect(lambdaConnected);
            this->mqttClient.onDisconnect(lambdaDisconnected);
        }

        void setConnectCallback(OnConnectMqtt callback){
            this->callbackConnect = callback;
        }

        void setDisconnectCallback(OnDisconnectMqtt callback){
            this->callbackDisconnect = callback;
        }

        void setMessageCallback(AsyncMqttClientInternals::OnMessageUserCallback callback){
            this->mqttClient.onMessage(callback);
        };

        void sendData(const char* topic, String payload);

        void addTopic(const char * topic){
            this->topics.add(topic);
        }

        bool connected(){
            return this->isConnected;
        }
};

void MQTTAsyncClient::onMqttConnect(bool sessionPresent){
    this->isConnected = true;
    Serial.println("Connected to Mqtt broker.");
}

void MQTTAsyncClient::onMqttDisconnect(AsyncMqttClientDisconnectReason reason){
    this->isConnected = false;
    Serial.println("Disconnected to Mqtt broker.");
    this->cv.notify_all();
}

void MQTTAsyncClient::task(){

    while(this->running){
        
        Serial.println("Try to connect to Mqtt broker.");

        mqttClient.connect();

        std::unique_lock<std::mutex> verrou{this->m};
        this->cv.wait(verrou);

        vTaskDelay(2000);
    }
    Serial.println("Stop task mqtt.");
}

char * MQTTAsyncClient::cpyChar(String str){

    int str_len = str.length()+1;
    char* value = (char*) malloc(str_len*sizeof(char));
    str.toCharArray(value, str_len);
    return value;
}

void MQTTAsyncClient::execute(){

    mqttClient.setKeepAlive(30);
    mqttClient.setCleanSession(true);

    if(this->username) free(this->username);
    if(this->password) free(this->password);
    if(this->idClient) free(this->idClient);

    this->username = this->cpyChar(this->config->getUsername());
    this->password = this->cpyChar(this->config->getPasswd());
    this->idClient = this->cpyChar(this->config->getID());

    mqttClient.setCredentials(this->username,this->password); //const char* username, const char* password
    mqttClient.setClientId(this->idClient);
    mqttClient.setServer(this->config->getHost(), this->config->getPort());

}

void MQTTAsyncClient::sendData(const char* topic, String payload){

  mqttClient.publish(topic, 1, false, payload.c_str());

}

void MQTTAsyncClient::stop(){
    mqttClient.disconnect(true);
    this->cv.notify_all();
}

#endif



