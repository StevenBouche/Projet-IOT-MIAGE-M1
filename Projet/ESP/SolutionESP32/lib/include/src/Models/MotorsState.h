#ifndef MotorsState_H
#define MotorsState_H

class MotorsState{

    private:
        long motorL,  motorR;

    public:

        MotorsState() {
            motorL = 0;
            motorR = 0;
        }

        long getMotorL(){
            return this->motorL;
        }

        void setMotorL(long m){
            this->motorL = m;
        }

        long getMotorR(){
            return this->motorR;
        }

        void setMotorR(long m){
            this->motorR = m;
        }

        void print(){
            Serial.println("MotorL : " + String(motorL));
            Serial.println("MotorR : " + String(motorR));
        }

};

#endif