#ifndef MQTT_h
#define MQTT_h

#endif

#include <PubSubClient.h>
#include <Arduino.h>

void mqtt_pubcallback(char* topic, byte* message, unsigned int length);
void mqtt_mysubscribe(PubSubClient* clientMQTT, char* topic);
void init_MQTT(PubSubClient* clientMQTT, char* mqtt_server, char* id, int* temp, int* l);
void task_mqtt();