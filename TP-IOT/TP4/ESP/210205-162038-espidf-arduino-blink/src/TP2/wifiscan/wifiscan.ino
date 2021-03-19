/*** Scan Wifi Network : wifiscan.ino ***/
#include <WiFi.h>
String translateEncryptionType(wifi_auth_mode_t encryptionType) {
  switch (encryptionType) {
    case (WIFI_AUTH_OPEN):
      return "Open";
    case (WIFI_AUTH_WEP):
      return "WEP";
    case (WIFI_AUTH_WPA_PSK):
      return "WPA_PSK";
    case (WIFI_AUTH_WPA2_PSK):
      return "WPA2_PSK";
    case (WIFI_AUTH_WPA_WPA2_PSK):
      return "WPA_WPA2_PSK";
    case (WIFI_AUTH_WPA2_ENTERPRISE):
      return "WPA2_ENTERPRISE";
  }
}

void print_network_status(int i){ // i : SSID index 
  String s = "";
  s += "\tIP address : " + WiFi.localIP().toString() + "\n"; // bizarre IPAddress
  s += "\tMAC address : " + String(WiFi.macAddress()) + "\n";
  s += "\tWifi SSID : " + String(WiFi.SSID(i)) + "\n";
  s += "\tWifi Signal Strength : " + String(WiFi.RSSI(i)) + "\n";
  s += "\tWifi BSSID : " + String(WiFi.BSSIDstr(i)) + "\n";
  s += "\tWifi Encryption type : " + translateEncryptionType(WiFi.encryptionType(i))+ "\n";
  Serial.print(s);
}

void setup() {
  Serial.begin(9600);

  int N = WiFi.scanNetworks(); // Scan Networks
  
  if (N>0){ // Print descriptions if some ?
    Serial.print("\n-------------------\n");
    Serial.print("Networks found : #");
    Serial.print(N);
    Serial.print("\n"); 
    for (int i=0 ; i<N ; i++){
      char s[100];
      sprintf(s,"SSID %d : \n",i); Serial.print(s);
      print_network_status(i);
      delay(1000);  
    }
  } 
}

void loop() {
  // no code
}
