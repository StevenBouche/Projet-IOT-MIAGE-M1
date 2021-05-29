#ifndef SensorHandler_H
#define SensorHandler_H

#include "Arduino.h"
#include "OneWire.h"
#include "DallasTemperature.h"
#include "Models/SensorsState.h"
#include <driver/adc.h>
#include <driver/gpio.h>

#define BLINK_GPIO_LED_GREEN (gpio_num_t)19
#define BLINK_GPIO_LED_RED (gpio_num_t)21
#define BLINK_ADC_PHOTO (gpio_num_t)33

class SensorHandler{

    private:
        SensorsState *state;
        uint8_t pinTemperature, pinObsL, pinObsR, pingU, pongU;
        adc1_channel_t pinLigth;
        OneWire *oneWire;
        DallasTemperature *tempSensor;

        void readValueLigth();
        void readValueTemperature();
        void readObstacle();
        void readUltrasonic();

    public:
        SensorHandler(SensorsState *state, uint8_t pinTemperature, adc1_channel_t pinLigth, uint8_t pinObsL, uint8_t pinObsR, uint8_t pingU, uint8_t pongU){
            this->state = state;
            this->pinTemperature = pinTemperature;
            this->pinLigth = pinLigth;
            this->pinObsL = pinObsL;
            this->pinObsR = pinObsR;
        }
        
        void initSensors();
        void readValues();

};

void SensorHandler::readValueLigth(){
    //Serial.println(adc1_get_raw(this->pinLigth));
    this->state->setLight(adc1_get_raw(this->pinLigth)); 
}

void SensorHandler::readValueTemperature(){
    this->tempSensor->requestTemperaturesByIndex(0);
    this->state->setTemperature(tempSensor->getTempCByIndex(0)); 
}

void SensorHandler::readObstacle(){
    state->setObstacleL(digitalRead(this->pinObsL) == HIGH);
    state->setObstacleR(digitalRead(this->pinObsR) == HIGH);
}

void SensorHandler::readUltrasonic(){

    long Abstand;
    long Dauer;

    // Distance measurement will be started with a 10us long trigger signal
    digitalWrite(this->pingU, HIGH);
    delayMicroseconds(10); 
    digitalWrite(this->pingU, LOW);

    // Now it will be waited at the echo input till the signal was activated
    // and after that the time will be measured how long it is active 
    Dauer  = pulseIn(this->pongU, HIGH);

    // Now the distance will be calculated with the recorded time
    state->setUltrason(Dauer/58.2);
}   

void SensorHandler::readValues(){
    //Serial.println("read sensor");
    this->readValueTemperature();
    this->readValueLigth();
}

void SensorHandler::initSensors(){

    //init Ligth
    adc1_config_width(ADC_WIDTH_BIT_12); //value on 12 bits
    adc1_config_channel_atten(this->pinLigth, ADC_ATTEN_DB_0);

    //adc1_config_width(ADC_WIDTH_BIT_12);
    //adc1_config_channel_atten(ADC1_CHANNEL_5,ADC_ATTEN_DB_0);

    //init Temperature
    this->oneWire = new OneWire(23); 
    this->tempSensor = new DallasTemperature(this->oneWire); 

    //Obstacle
    //pinMode(this->pinObsL, INPUT);
    //pinMode(this->pinObsR, INPUT);

    //Ultrasonic
    //pinMode(pingU, OUTPUT);
    //pinMode(pongU, INPUT);
}

#endif