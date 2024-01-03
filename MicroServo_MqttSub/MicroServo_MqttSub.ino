#include <Servo.h>
#include <Wire.h>
#include <UnoWiFiDevEd.h>

#define SERVO 6  // Digital Pin 6 PWM
#define CONNECTOR "mqtt"
#define TOPIC "app01/cont01"

Servo s;      // Servo variable
int pos = 0;  // Servo position

void setup() {
  Ciao.begin();
  s.attach(SERVO);
  Serial.begin(9600);
  s.write(0);  // Initialize the servo at position zero
}

void loop() {
  CiaoData data = Ciao.read(CONNECTOR, TOPIC);
  if (!data.isEmpty()) {
    const char* value = data.get(2);
    Serial.println(value);

    if (strcmp(value, "on") == 0) {
      while (strcmp(value, "off") != 0) {
        for (pos = 0; pos < 180; pos++) {
          s.write(pos);
          delay(15);
        }
        delay(1000);
        for (pos = 180; pos >= 0; pos--) {
          s.write(pos);
          delay(15);
        }
        CiaoData data = Ciao.read(CONNECTOR, TOPIC);
        if (!data.isEmpty()) {
          value = data.get(2);
          Serial.println(value);
        }
      }
    } else if (strcmp(value, "off") == 0) {
      s.write(0);
      delay(2000);
    }
  }
}
