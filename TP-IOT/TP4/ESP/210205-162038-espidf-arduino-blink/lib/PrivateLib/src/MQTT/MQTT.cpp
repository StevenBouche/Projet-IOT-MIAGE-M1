#include "MQTT.h"
#include <PubSubClient.h>
#include <Arduino.h>
#include <iostream>
#include <string>

int* temperature;
int* ligth;
PubSubClient* client;
pthread_t thread_send;

/*============== MQTT CALLBACK ===================*/

void mqtt_pubcallback(char* topic, byte* message, unsigned int length) {
  /* 
   *  Callback if a message is published on this topic.
   */
  
  // Byte list to String ... plus facile a traiter ensuite !
  // Mais sans doute pas optimal en performance => heap ?
  String messageTemp ;
  for(int i = 0 ; i < length ; i++) {
    messageTemp += (char) message[i];
  }
  
  Serial.print("Message : ");
  Serial.println(messageTemp);
  Serial.print("arrived on topic : ");
  Serial.println(topic) ;
 
  // Analyse du message et Action 
  if(String (topic) == "TEST") {
     // Par exemple : Changes the LED output state according to the message   
    Serial.print("Action : Changing output to ");
    if(messageTemp == "on") {
      Serial.println("on");
      //set_pin(ledPin,HIGH);
      //TODO
    } else if (messageTemp == "off") {
      Serial.println("off");
      //set_pin(ledPin,LOW);
      //TODO
    }
  }
}

/*============= MQTT SUBSCRIBE =====================*/

void mqtt_mysubscribe(PubSubClient* clientMQTT, char* topic) {

  /*
   * ESP souscrit a ce topic. Il faut qu'il soit connecte.
   */
  while(!clientMQTT->connected()) { // Loop until we are reconnected
    Serial.print("Attempting MQTT connection...");
    if(clientMQTT->connect("esp32", "try", "try")) { // Attempt to connect 
      Serial.println("connected");
      clientMQTT->subscribe(topic); // and then Subscribe
    } else { // Connection failed
      Serial.print("failed, rc=");
      Serial.print(clientMQTT->state());
      Serial.println("try again in 5 seconds");
      // Wait 5 seconds before retrying
      delay(5*1000);
    }
  }

}

void task_mqtt()
{
    int32_t period = 60 * 5000l; 
    int sizeData = 80;

   

        char data[80];

        String payload = "{\"temperature\": \"";
        payload += *temperature;   
        payload += "\", \"ligth\": " ;
        payload += *ligth; 
        payload += "}";

        // Convert String payload to a char array
        payload.toCharArray(data, (payload.length() + 1)); 

        Serial.println(data);

        if((client->connected())){
            client->publish("IOT/data", data);  // publish it 
        }
      
 
        // Process MQTT ... obligatoire une fois par loop()
        client->loop();
  
     

}

void init_MQTT(PubSubClient* clientMQTT, char* mqtt_server, char* id, int* temp, int* l){

  client = clientMQTT; 
  temperature = temp;
  ligth = l;
  char* Username = "clientProjectIOTMIAGE";
  char* Password = "XAjNyUPfS8FmBQ[s";

  client->setServer(mqtt_server, 1883);

  while (!client->connected()) { // Loop until we're reconnected

    Serial.print("Attempting MQTT connection...");

    if (client->connect(id, Username, Password)) {
      Serial.println("MQTT connected");

      //xTaskCreate(&task_mqtt, "task_mqtt", configMINIMAL_STACK_SIZE, NULL, 5, NULL);

    } else {
      Serial.print("failed, rc=");
      Serial.print(client->state());    
      Serial.println(" try again in 5 seconds");
      delay(5000); // Wait 5 seconds before retrying
    }
  }
  
}

