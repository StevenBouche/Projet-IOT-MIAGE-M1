#ifndef DataHandler_H
#define DataHandler_H

#include <driver/gpio.h>
#include <driver/adc.h>
#include <cmath> 
#include "Arduino.h" 
#include "OneWire.h"
#include "DallasTemperature.h"
#include "Models/StateApp.h"

//ULTRASON
#define Echo_EingangsPin (gpio_num_t)7 // Echo input-pin
#define Trigger_AusgangsPin (gpio_num_t)8 // Trigger output-pin
int maximumRange = 300; 
int minimumRange = 2; 
//ENDULTRASON

//MOTOR
#define MOTOR_1_PIN_1 (gpio_num_t) 27
#define MOTOR_1_PIN_2 (gpio_num_t) 26
#define MOTOR_2_PIN_1 (gpio_num_t) 24
#define MOTOR_2_PIN_2 (gpio_num_t) 23
#define PWM_MOTOR_1 (gpio_num_t) 14
#define PWM_MOTOR_2 (gpio_num_t) 13

#define MIN_MOTOR_INPUT 0
#define MAX_MOTOR_INPUT 100
#define MIN_DUTY_8_BITS 200
#define MAX_DUTY_8_BITS 255
#define MIN_DUTY_16_BITS 51400
#define MAX_DUTY_16_BITS 65536
    // Setting PWM properties
const int freq = 30000;
const int pwmChannel1 = 0;
const int pwmChannel2 = 1;
const int resolution = 8;
//ENDMOTOR

//TEMP LIGTH
#define TEMPERATURE_PIN 23
#define LIGTH_PIN
//ENDTEMP LITH

class DataHandler{

    private:
        TaskHandle_t taskHandle;
        bool running;
        int waitingTime = 100;
        StateApp *state;
        std::function<void(StateApp *state)> onDataRead;
        OneWire oneWire; 
        DallasTemperature tempSensor; 

        void initMotor();
        void setMotor();
        void initUltrasons();
        void readUltrasons();
        void initLigth();
        void readLigth();
        void initTemperature();
        void readTemperature();
        void setupGPIO(gpio_num_t pin, gpio_mode_t mode);
        void setupPWM(gpio_num_t pin, int channel, int freq, int resolution);
        void setMotorValues(gpio_num_t pin1, gpio_num_t pin2, int8_t channel, double value);
       
    public:

        DataHandler(StateApp *state, std::function<void(StateApp *state)> onRead){
            this->state = state;
            this->onDataRead = onRead;
            oneWire = OneWire(TEMPERATURE_PIN);
            tempSensor = DallasTemperature(&oneWire);
        }

        void setup();
        void execute();
};

void DataHandler::setupGPIO(gpio_num_t pin, gpio_mode_t mode){
    gpio_pad_select_gpio(pin);
    gpio_set_direction(pin, GPIO_MODE_OUTPUT);
}

void DataHandler::setupPWM(gpio_num_t pin, int channel, int freq, int resolution){
    ledcSetup(channel, freq, resolution);
    ledcAttachPin(pin, channel);
}

void DataHandler::setMotorValues(gpio_num_t pin1, gpio_num_t pin2, int8_t channel, double value){

    uint32_t level1 = value > LOW ? LOW : value < LOW ? HIGH : LOW;
    uint32_t level2 = value > LOW ? HIGH : value < LOW ? LOW : LOW;

    gpio_set_level(pin1, level1);
    gpio_set_level(pin2, level2);

    uint32_t valueFinal = round((abs(value) * (MAX_DUTY_8_BITS-MIN_DUTY_8_BITS)) / 100);

    ledcWrite(channel, valueFinal);

}

void DataHandler::initMotor(){

    setupGPIO(MOTOR_1_PIN_1, GPIO_MODE_OUTPUT);
    setupGPIO(MOTOR_1_PIN_2, GPIO_MODE_OUTPUT);
    setupGPIO(MOTOR_2_PIN_1, GPIO_MODE_OUTPUT);
    setupGPIO(MOTOR_2_PIN_2, GPIO_MODE_OUTPUT);
    setupGPIO(PWM_MOTOR_1, GPIO_MODE_OUTPUT);
    setupGPIO(PWM_MOTOR_2, GPIO_MODE_OUTPUT);

    setupPWM(PWM_MOTOR_1, pwmChannel1, freq, resolution);
    setupPWM(PWM_MOTOR_2, pwmChannel2, freq, resolution);

}

void DataHandler::setMotor(){
    for(;;){
        setMotorValues(MOTOR_1_PIN_1, MOTOR_1_PIN_2, pwmChannel1, this->state->motorL);
        setMotorValues(MOTOR_2_PIN_1, MOTOR_2_PIN_2, pwmChannel2, this->state->motorR);
    }
}

void DataHandler::initUltrasons(){
    //pinMode(Trigger_AusgangsPin, OUTPUT);
    //pinMode(Echo_EingangsPin, INPUT);
    setupGPIO(Trigger_AusgangsPin, GPIO_MODE_OUTPUT);
    setupGPIO(Echo_EingangsPin, GPIO_MODE_INPUT);
}

void DataHandler::readUltrasons(){

    // Distance measurement will be started with a 10us long trigger signal
 //digitalWrite(Trigger_AusgangsPin, HIGH);
 //delayMicroseconds(10); 
 //digitalWrite(Trigger_AusgangsPin, LOW);
  
 // Now it will be waited at the echo input till the signal was activated
 // and after that the time will be measured how long it is active 
 //Dauer = pulseIn(Echo_EingangsPin, HIGH);
  
 // Now the distance will be calculated with the recorded time
 //Abstand = Dauer/58.2;
  
 // Check if the measured value is in the permitted range
 //if (Abstand >= maximumRange || Abstand <= minimumRange)
 //{
    // An error message will be shown if it's not
 //     Serial.println("Distance is not in the permitted range");
 //     Serial.println("-----------------------------------");
 //}  
  
 //else
 //{
    // The calculated distance will be shown at the serial output
 //     Serial.print("The distance is: ");
 //     Serial.print(Abstand);
 //     Serial.println("cm");
 //     Serial.println("-----------------------------------");
 //}

}

void DataHandler::initLigth(){
    adc1_config_width(ADC_WIDTH_BIT_12);
    adc1_config_channel_atten(ADC1_CHANNEL_5,ADC_ATTEN_DB_0);
}

void DataHandler::readLigth(){
    this->state->light = adc1_get_raw(ADC1_CHANNEL_5); 
}

void DataHandler::readTemperature(){
    tempSensor.requestTemperaturesByIndex(0);
    this->state->temperature = tempSensor.getTempCByIndex(0); 
}

void DataHandler::setup(){
    this->initMotor();
    this->initUltrasons();
    this->initUltrasons();
    this->initTemperature();
}

void DataHandler::execute(){


}

#endif


