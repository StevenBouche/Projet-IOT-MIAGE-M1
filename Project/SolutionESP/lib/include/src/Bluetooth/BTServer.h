#ifndef BTServer_h
#define BTServer_h

#include <Arduino.h>

#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEServer.h>
#include <BLE2902.h>
#include <functional>

#define SERVICE_UUID        "4fafc201-1fb5-459e-8fcc-c5c9c331914b"
#define CHARACTERISTIC_UUID "beb5483e-36e1-4688-b7f5-ea07361b26a8"


class BTServer : public BLEServerCallbacks, public BLECharacteristicCallbacks{

  private:
    BLEServer *server;
    BLEService *serviceGPS;
    BLECharacteristic *characteristicGPS;
    bool clientConnected;
    std::function<void(String message)> onDataReceived;

    void onConnect(BLEServer* pServer);
    void onDisconnect(BLEServer* pServer);
    void onWrite (BLECharacteristic * characteristic);

  public:
  
    BTServer(){
       
    }

    void setup(std::function<void(String message)> onDataReceived);

};

void BTServer::onConnect(BLEServer* pServer) {
  this->clientConnected = true;
  Serial.print("Event connection BLE");
};
 
void BTServer::onDisconnect(BLEServer* pServer) {
  this->clientConnected = false;
  Serial.print("Event disconnection BLE");
}

void BTServer::onWrite(BLECharacteristic *characteristic) {
          
  std::string rxValue = characteristic->getValue(); 

  if (rxValue.length() > 0) {

    Serial.print("Receive data from BLE");

    String messageTemp;
    for (int i = 0; i < rxValue.length(); i++) {
      messageTemp += rxValue[i];
    }
    this->onDataReceived(messageTemp);
  }
}

void BTServer::setup(std::function<void(String message)> onDataReceived){

   this->onDataReceived = onDataReceived;

  Serial.println("Setup BLE Server.");

  BLEDevice::init("ESP32-BLE");

  this->server = BLEDevice::createServer();

  this->server->setCallbacks(this);

  this->serviceGPS = this->server->createService(SERVICE_UUID);

  this->characteristicGPS = this->serviceGPS->createCharacteristic(
    CHARACTERISTIC_UUID,
    BLECharacteristic::PROPERTY_WRITE
  );

  this->characteristicGPS->setCallbacks(this);

  this->serviceGPS->start();

  // BLEAdvertising *pAdvertising = pServer->getAdvertising();  // this still is working for backward compatibility
  BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->addServiceUUID(SERVICE_UUID);
  pAdvertising->setScanResponse(true);
  pAdvertising->setMinPreferred(0x06);  // functions that help with iPhone connections issue
  pAdvertising->setMinPreferred(0x12);
  BLEDevice::startAdvertising();

  Serial.println("Setup BLE Server finished.");

}

#endif
