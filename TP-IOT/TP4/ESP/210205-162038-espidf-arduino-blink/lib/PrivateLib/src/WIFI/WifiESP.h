#ifndef WifiESP_h
#define WifiESP_h

#endif

#include <Arduino.h>
#include <SPI.h>
#include <WiFi.h>

void print_network_status_light();
void print_network_status();
void connect_wifi(char* ssid, char* password);
char* getAddrMac();
