#ifndef MQTTAction_h
#define MQTTAction_h

#include <Arduino.h>

typedef void (*handleReceiveMQTT)(const byte* message, unsigned int length);

class MQTTAction{

    private:
        String topic;
        handleReceiveMQTT handler;
    
    public:
        MQTTAction(){

        }

        MQTTAction(String topic, handleReceiveMQTT handler){
            this->topic = topic;
            this->handler = handler;
        }

        String getTopic() {
            return this->topic;
        }

        handleReceiveMQTT getHandler(){
            return this->handler;
        }
};

#endif