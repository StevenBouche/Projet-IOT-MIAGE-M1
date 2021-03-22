/* Blink Example
   This example code is in the Public Domain (or CC0 licensed, at your option.)
   Unless required by applicable law or agreed to in writing, this
   software is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
   CONDITIONS OF ANY KIND, either express or implied.
*/

#include <stdio.h>
#include <freertos/FreeRTOS.h>
#include <freertos/task.h>
#include <driver/gpio.h>
#include "sdkconfig.h"
#include <Arduino.h>
#include <pthread.h>

#include <driver/adc.h>
#include "driver/gpio.h"
#include <iostream>   
#include <cstdlib> 

#include "OneWire.h"
#include "DallasTemperature.h"

#include <SPI.h>
#include <WiFi.h>
#include <AsyncTCP.h>
#include "ESPAsyncWebServer.h"
#include <PubSubClient.h>

#include "MQTT/MQTT.h"
#include "Wifi/WifiESP.h"
#include "ServerHTTP/ServerHTTP.h"

/*
    Port
*/
#define BLINK_GPIO_LED_GREEN (gpio_num_t)19
#define BLINK_GPIO_LED_RED (gpio_num_t)21
#define BLINK_ADC_PHOTO (gpio_num_t)33

/*
    Seuils
*/
#define SEUIL_DAY_NIGHT 300;
static int SHJ = 27;
static int SHN = 22;
static int SBJ = 19;
static int SBN = 17;
static double loop_mili_sec = 100;

class StateApp{

    public : bool *led_green;
    public : bool *led_red;
    public : int *photo;
    public : int *temp;

    StateApp(){
        this->led_green = (bool*) malloc(sizeof(bool));
        this->led_red = (bool*) malloc(sizeof(bool));
        this->photo = (int*) malloc(sizeof(int));
        this->temp = (int*) malloc(sizeof(int));
        *this->led_green = false;
        *this->led_red = false;
        *this->photo = 0;
        *this->temp = 0;
    }

    public: void print(){
        printf("MODEL \n");
        printf("VALUE TEMP : %d\n", *this->temp);
        printf("VALUE PHOTO : %d\n", *this->photo);
        printf("STATE GREEN LED : %d\n", *this->led_green);
        printf("STATE RED LED : %d\n", *this->led_red);
    };

};

/*
    Models and Thread Proccess
*/
StateApp element;
pthread_t thread_photo;
pthread_t thread_temp;

//WIFI
WiFiClient clientWifi;
char* ssid = "iPhone de Steven";
char* password = "iobwgn7obkf91";

//Server Web
AsyncWebServer server(80);

// MQTT
//char* mqtt_server = "ec2-18-223-15-182.us-east-2.compute.amazonaws.com";
char* mqtt_server = "172.20.10.3";
String whoami; 
PubSubClient clientMQTT(clientWifi);

/*
  TASKS
*/

void *executePhoto(void *pvParameter){

    adc1_config_width(ADC_WIDTH_BIT_12);
    adc1_config_channel_atten(ADC1_CHANNEL_5,ADC_ATTEN_DB_0);
   
    while(1) {
        *element.photo = adc1_get_raw(ADC1_CHANNEL_5); 
        vTaskDelay(loop_mili_sec/portTICK_PERIOD_MS);
    }

}

void *executeTemp(void *pvParameter){

    OneWire oneWire(23); 
    DallasTemperature tempSensor(&oneWire); 

    while(1) {
        tempSensor.requestTemperaturesByIndex(0);
        *element.temp = tempSensor.getTempCByIndex(0); 
        vTaskDelay(loop_mili_sec/portTICK_PERIOD_MS);
    }

}

void executeTask(){

    //configuration off output LED
    gpio_pad_select_gpio(BLINK_GPIO_LED_GREEN);
    gpio_set_direction(BLINK_GPIO_LED_GREEN, GPIO_MODE_OUTPUT);

    gpio_pad_select_gpio(BLINK_GPIO_LED_RED);
    gpio_set_direction(BLINK_GPIO_LED_RED, GPIO_MODE_OUTPUT);
  
    bool isNigth ;
    int valueB;
    int valueH;

    while(1){

        isNigth = *element.photo <= SEUIL_DAY_NIGHT;
        valueB = isNigth ? SBN : SBJ;
        valueH = isNigth ? SHN : SHJ;
        *element.led_green = *element.temp >= valueH;
        *element.led_red = *element.temp <= valueB;
        
        gpio_set_level(BLINK_GPIO_LED_GREEN,  *element.led_green ? 1 : 0);
        gpio_set_level(BLINK_GPIO_LED_RED,  *element.led_red ? 1 : 0);
   
        vTaskDelay(loop_mili_sec/portTICK_PERIOD_MS);

    }

    printf ("Exit main loop !\n");

}

void blink_task(void *pvParameter)
{
    printf ("Creation du thread Photo !\n");
    int ret2 = pthread_create (&thread_photo, NULL, executePhoto, NULL);
    printf ("Creation du thread Temp !\n");
    int ret3 = pthread_create (&thread_temp, NULL, executeTemp, NULL);
    
    executeTask();

    printf ("Waiting !\n");
    pthread_join(thread_photo, NULL);
    pthread_join(thread_temp, NULL);

}

/*
  ESP LOOP
*/
void loop() {
    Serial.print(*element.temp);
    element.print();
    print_network_status();
    task_mqtt();
    delay(10000);
}

/*
  ESP SETUP
*/
void setup() {
    
  initArduino();
    
  Serial.begin(115200);

  connect_wifi(ssid, password);
  print_network_status();

  init_HTTP_server(&server, element.temp, element.photo);

  char* idMQTT = getAddrMac();

  init_MQTT(&clientMQTT, mqtt_server, idMQTT, element.temp, element.photo);

  xTaskCreate(&blink_task, "blink_task", configMINIMAL_STACK_SIZE, NULL, 5, NULL);

}