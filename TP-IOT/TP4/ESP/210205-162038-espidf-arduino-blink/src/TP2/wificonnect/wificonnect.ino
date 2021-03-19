/*** Basic Wifi connection: wificonnect.ino ***/
#include <SPI.h>
#include <WiFi.h> // https://www.arduino.cc/en/Reference/WiFi
/* Credentials */
const char ssid[] = "GMAP";
const char password[] = "vijx4705"; 
/*------------------------*/
String translateEncryptionType(wifi_auth_mode_t encryptionType) {
   // cf https://www.arduino.cc/en/Reference/WiFiEncryptionType 
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
/*------------------------*/
void print_network_status_light(){ // array of chars
  char s[256];
  sprintf(s,"\tIP address : %s\n", WiFi.localIP().toString().c_str()); Serial.print(s);
  sprintf(s,"\tMAC address : %s\n", WiFi.macAddress().c_str()); Serial.print(s);
  sprintf(s,"\tWifi SSID : %s\n", WiFi.SSID()); Serial.print(s);
  sprintf(s,"\tWifi Signal Strength : %ld\n",WiFi.RSSI()); Serial.print(s);
  sprintf(s,"\tWifi BSSID : %s\n", WiFi.BSSIDstr().c_str()); Serial.print(s);
  sprintf(s,"\tWifi Encryption type : %s\n", translateEncryptionType(WiFi.encryptionType(0))); Serial.print(s);
  // a mon avis bug ! => manque WiFi.encryptionType() !
}
void print_network_status(){ // Utilisation de String !
  String s = "";
  s += "\tIP address : " + WiFi.localIP().toString() + "\n"; // bizarre IPAddress
  s += "\tMAC address : " + String(WiFi.macAddress()) + "\n";
  s += "\tWifi SSID : " + String(WiFi.SSID()) + "\n";
  s += "\tWifi Signal Strength : " + String(WiFi.RSSI()) + "\n";
  s += "\tWifi BSSID : " + String(WiFi.BSSIDstr()) + "\n";
  s += "\tWifi Encryption type : " + translateEncryptionType(WiFi.encryptionType(0))+ "\n";
  // a mon avis bug ! => manque WiFi.encryptionType() !
  Serial.print(s);
}
/*------------------------*/
void connect_wifi(){
 //  Set WiFi to station mode 
 WiFi.mode(WIFI_STA);
 // and disconnect from an AP if
 // it was previously connected
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

/*---- Arduino IDE paradigm : setup+loop -----*/
void setup(){
  Serial.begin(9600);
  while(!Serial);//wait for a serial connection  
  connect_wifi();//connect wifi 
  print_network_status();
}
void loop(){
  // no code
  // WiFi.disconnect();
}
