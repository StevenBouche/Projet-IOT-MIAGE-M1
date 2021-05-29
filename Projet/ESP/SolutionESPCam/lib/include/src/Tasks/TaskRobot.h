#ifndef TaskRobot_H
#define TaskRobot_H

#include "freertos/FreeRTOS.h"
#include "freertos/task.h"

class TaskRobot {

    private: 
        BaseType_t core;
        TaskHandle_t taskHandle;
        const char * name;
        uint32_t usStackDepth;
        TaskFunction_t taskFunction = [](void* _this){ reinterpret_cast<TaskRobot*>(_this)->onTask(); };

        void onTask(){

            if(running) return;

            running = true;

            Serial.print("Start task : ");
            Serial.println(this->name);

            this->task();

            Serial.print("Stop task : ");
            Serial.println(this->name);

            vTaskDelete(taskHandle);
        }

    protected:

        bool running;
        virtual void execute();
        virtual void task();
        virtual void stop();

    public:
    
        TaskRobot(const char * name, uint32_t usStackDepth, int core){
            this->running = false;
            this->name = name;
            this->usStackDepth = usStackDepth;
            this->core = core;
        }

        void executeTask(){
            Serial.print("Init task : ");
            Serial.println(this->name);
            this->execute();
            xTaskCreatePinnedToCore(taskFunction, this->name, this->usStackDepth, this, 1, &taskHandle, this->core);
        }

        void stopTask(){
            if(!this->running)
                return;
            this->running = false;
            this->stop();
        }

        bool isRunning(){
            return this->running;
        }
};

#endif