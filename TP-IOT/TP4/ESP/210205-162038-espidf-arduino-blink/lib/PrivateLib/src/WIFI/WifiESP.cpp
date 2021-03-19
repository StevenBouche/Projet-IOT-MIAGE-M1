#include <SPI.h>
#include <WiFi.h>
#include <Arduino.h>


void print_network_status_light(){ 
    String s = "";
    s += "\tIP address : " + WiFi.localIP().toString() + "\n"; // bizarre IPAddress
    s += "\tMAC address : " + String(WiFi.macAddress()) + "\n";
    s += "\tWifi SSID : " + String(WiFi.SSID()) + "\n";
    s += "\tWifi Signal Strength : " + String(WiFi.RSSI()) + "\n";
    s += "\tWifi BSSID : " + String(WiFi.BSSIDstr()) + "\n";
    // a mon avis bug ! => manque WiFi.encryptionType() !
    Serial.print(s);
}

void print_network_status(){ 
    String s = "";
    s += "\tIP address : " + WiFi.localIP().toString() + "\n"; // bizarre IPAddress
    s += "\tMAC address : " + String(WiFi.macAddress()) + "\n";
    s += "\tWifi SSID : " + String(WiFi.SSID()) + "\n";
    s += "\tWifi Signal Strength : " + String(WiFi.RSSI()) + "\n";
    s += "\tWifi BSSID : " + String(WiFi.BSSIDstr()) + "\n";
    // a mon avis bug ! => manque WiFi.encryptionType() !
    Serial.print(s);
}

void connect_wifi(char* ssid, char* password){
 
    WiFi.mode(WIFI_STA);
    WiFi.disconnect();

    delay(100); // ms

    Serial.println(String("\nAttempting to connect AP of SSID : ")+ssid);

    WiFi.begin(ssid, password);

    while(WiFi.status() != WL_CONNECTED){
        delay(1000); // ms
        Serial.print(".");
    }

    Serial.print("\nWiFi connected : \n");
}