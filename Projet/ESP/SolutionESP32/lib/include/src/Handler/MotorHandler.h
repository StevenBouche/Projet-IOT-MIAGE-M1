#ifndef MotorHandler_H
#define MotorHandler_H

#include <Arduino.h>
#include <analogWrite.h>
#include <cmath> 

#define MIN_SPEED_PWM 200
#define MAX_SPEED_PWM 255
#define MIN_SPEED 0
#define MAX_SPEED 100

typedef enum {
    MOTOR1 = 1,  
    MOTOR2 = 2
} motor_num_t;

class MotorHandler{

    private:

        int IN1, IN2, ENA, IN3, IN4, ENB;
        bool dualMotor;
        int mapVitesse(long v);

    public:

        MotorHandler(int in1, int in2, int enA, int in3, int in4, int enB){
            this->IN1 = in1;
            this->IN2 = in2;
            this->ENA = enA;
            this->IN3 = in3;
            this->IN4 = in4;
            this->ENB = enB;
            this->dualMotor = true;
        }

        MotorHandler(int in1, int in2, int enA){
            this->IN1 = in1;
            this->IN2 = in2;
            this->ENA = enA;
            this->dualMotor = false;
        }

        void init();
        void rotateMotor(motor_num_t motor, long value);
        void rotateMotors(long valueMotor1, long valueMotor2);
        void breakMotor(motor_num_t motor);
        void breakMotors();
};

int MotorHandler::mapVitesse(long v){
  return map(v, MIN_SPEED, MAX_SPEED, MIN_SPEED_PWM, MAX_SPEED_PWM);
}

void MotorHandler::init(){

    pinMode(this->IN1, OUTPUT);
	pinMode(this->IN2, OUTPUT);
	pinMode(this->ENA, OUTPUT);	

    if(this->dualMotor)
    {
        pinMode(this->IN3, OUTPUT);
        pinMode(this->IN4, OUTPUT);
        pinMode(this->ENB, OUTPUT);
    }
}

void MotorHandler::rotateMotors(long valueMotor1, long valueMotor2){
    rotateMotor(MOTOR1,valueMotor1);
    rotateMotor(MOTOR2,valueMotor2);
}

void MotorHandler::rotateMotor(motor_num_t motor, long value){

    int in1, in2, en;

    if(motor==MOTOR1){
        in1 = this->IN1;
        in2 = this->IN2;
        en = this->ENA;
    } else if(motor==MOTOR2 && this->dualMotor) {
        in1 = this->IN3;
        in2 = this->IN4;
        en = this->ENB;
    } 
    else return;

    //Serial.print("value : ");
    //Serial.print(value);

    uint32_t level1 = value > 0 ? HIGH : value < 0 ? LOW : LOW;
    uint32_t level2 = value > 0 ? LOW : value < 0 ? HIGH : LOW;

    //Serial.print("level1 : ");
    //Serial.print(level1);
    //Serial.print(" level2 : ");
    //Serial.println(level2);

    value = abs(value);

    //Serial.print("value abs: ");
    //Serial.print(value);

    value = value < MIN_SPEED ? MIN_SPEED : value > MAX_SPEED ? MAX_SPEED : value;

    digitalWrite(in1, level1);		
	digitalWrite(in2, level2);
	digitalWrite(en, LOW);
    analogWrite(en, mapVitesse(value));

}

void MotorHandler::breakMotor(motor_num_t motor){

    int in1, in2, en;

    if(motor==MOTOR1){
        in1 = this->IN1;
        in2 = this->IN2;
        en = this->ENA;
    } else if(motor==MOTOR2 && this->dualMotor) {
        in1 = this->IN3;
        in2 = this->IN4;
        en = this->ENB;
    }
    else return;

    digitalWrite(in1, LOW);
    digitalWrite(in2, LOW);		
    digitalWrite(en, LOW);

}

void MotorHandler::breakMotors(){
    breakMotor(MOTOR1);
    breakMotor(MOTOR2);
}

#endif