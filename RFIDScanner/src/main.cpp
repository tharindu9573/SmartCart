#include <WiFi.h>
#include <PubSubClient.h>
#include <SPI.h>
#include <MFRC522.h>
#include <WiFiClientSecure.h>
#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>
#include <unordered_map>

// Root CA (same as before)
const char *digicertRootCA = R"EOF(
-----BEGIN CERTIFICATE-----
MIIDjjCCAnagAwIBAgIQAzrx5qcRqaC7KGSxHQn65TANBgkqhkiG9w0BAQsFADBh
MQswCQYDVQQGEwJVUzEVMBMGA1UEChMMRGlnaUNlcnQgSW5jMRkwFwYDVQQLExB3
d3cuZGlnaWNlcnQuY29tMSAwHgYDVQQDExdEaWdpQ2VydCBHbG9iYWwgUm9vdCBH
MjAeFw0xMzA4MDExMjAwMDBaFw0zODAxMTUxMjAwMDBaMGExCzAJBgNVBAYTAlVT
MRUwEwYDVQQKEwxEaWdpQ2VydCBJbmMxGTAXBgNVBAsTEHd3dy5kaWdpY2VydC5j
b20xIDAeBgNVBAMTF0RpZ2lDZXJ0IEdsb2JhbCBSb290IEcyMIIBIjANBgkqhkiG
9w0BAQEFAAOCAQ8AMIIBCgKCAQEAuzfNNNx7a8myaJCtSnX/RrohCgiN9RlUyfuI
2/Ou8jqJkTx65qsGGmvPrC3oXgkkRLpimn7Wo6h+4FR1IAWsULecYxpsMNzaHxmx
1x7e/dfgy5SDN67sH0NO3Xss0r0upS/kqbitOtSZpLYl6ZtrAGCSYP9PIUkY92eQ
q2EGnI/yuum06ZIya7XzV+hdG82MHauVBJVJ8zUtluNJbd134/tJS7SsVQepj5Wz
tCO7TG1F8PapspUwtP1MVYwnSlcUfIKdzXOS0xZKBgyMUNGPHgm+F6HmIcr9g+UQ
vIOlCsRnKPZzFBQ9RnbDhxSJITRNrw9FDKZJobq7nMWxM4MphQIDAQABo0IwQDAP
BgNVHRMBAf8EBTADAQH/MA4GA1UdDwEB/wQEAwIBhjAdBgNVHQ4EFgQUTiJUIBiV
5uNu5g/6+rkS7QYXjzkwDQYJKoZIhvcNAQELBQADggEBAGBnKJRvDkhj6zHd6mcY
1Yl9PMWLSn/pvtsrF9+wX3N3KjITOYFnQoQj8kVnNeyIv/iPsGEMNKSuIEyExtv4
NeF22d+mQrvHRAiGfzZ0JFrabA0UWTW98kndth/Jsw1HKj2ZL7tcu7XUIOGZX1NG
Fdtom/DzMNU+MeKNhJ7jitralj41E6Vf8PlwUHBHQRFXGU7Aj64GxJUTFy8bJZ91
8rGOmaFvE7FBcf6IKshPECBV1/MUReXgRPTqh5Uykw7+U0b6LJ3/iyK5S9kJRaTe
pLiaWN0bfVKfjllDiIGknibVb63dDcY3fe0Dkhvld1927jyNxF1WW6LZZm6zNTfl
MrY=
-----END CERTIFICATE-----
)EOF";

// WiFi credentials
const char *ssid = "Wokwi-GUEST";
const char *password = "";

// Azure IoT Hub settings
const char *mqtt_server = "smart-cart-iot-hub.azure-devices.net";
const int mqtt_port = 8883;
const char *deviceId = "smartcart-cart-1";
const char *sasToken = "SharedAccessSignature sr=smart-cart-iot-hub.azure-devices.net%2Fdevices%2Fsmartcart-cart-1&sig=1N7680ZHyT4QrAJPgVqW55LjXjmqPQhxc1eaS70h0wo%3D&se=1783347727";

WiFiClientSecure wifiClient;
PubSubClient client(wifiClient);

// RFID setup
#define SS_PIN1 5
#define RST_PIN1 22
MFRC522 rfid1(SS_PIN1, RST_PIN1);

#define SS_PIN2 17
#define RST_PIN2 21
MFRC522 rfid2(SS_PIN2, RST_PIN2);

// Speaker
#define SPEAKER_PIN 16

// LED
#define SCANNER_LED_PIN 2
#define PAYMENT_LED_PIN 4

// Display
#define SCREEN_WIDTH 128
#define SCREEN_HEIGHT 64
Adafruit_SSD1306 display(SCREEN_WIDTH, SCREEN_HEIGHT, &Wire, -1);

void setup()
{
  pinMode(SPEAKER_PIN, OUTPUT);
  pinMode(SCANNER_LED_PIN, OUTPUT);
  pinMode(PAYMENT_LED_PIN, OUTPUT);

  Wire.begin(21, 22);

  if (!display.begin(SSD1306_SWITCHCAPVCC, 0x3C))
  {
    Serial.println(F("SSD1306 allocation failed"));
    for (;;)
      ;
  }

  Serial.begin(115200);
  WiFi.begin(ssid, password);

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }
  Serial.println("WiFi connected");

  wifiClient.setCACert(digicertRootCA);
  client.setServer(mqtt_server, mqtt_port);
  client.setBufferSize(1024);

  SPI.begin();
  rfid1.PCD_Init();
  rfid2.PCD_Init();

  display.setTextSize(2);
  display.setTextColor(SSD1306_WHITE);
  display.clearDisplay();
  Serial.println("Setup is done");
}

void reconnect()
{
  while (!client.connected())
  {
    Serial.print("Attempting MQTT connection...");
    String mqttUsername = String(mqtt_server) + "/" + String(deviceId) + "/?api-version=2021-04-12";
    if (client.connect(deviceId, mqttUsername.c_str(), sasToken))
    {
      Serial.println("connected");
    }
    else
    {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      delay(5000);
    }
  }
}

String getUid(MFRC522 &reader)
{
  String uidStr = String(reader.uid.uidByte[0], HEX);
  for (byte i = 1; i < reader.uid.size; i++)
  {
    uidStr += ":";
    uidStr += String(reader.uid.uidByte[i], HEX);
  }

  uidStr.toUpperCase();
  return uidStr;
}

String getProductName(const String &uid)
{
  struct Entry
  {
    const char *uid;
    const char *name;
  };

  static const Entry productMap[] = {
      {"04:11:22:33:44:55:66", "Sparkling Water 1.5L"},
      {"11:22:33:44", "Brown Bread Loaf"},
      {"55:66:77:88", "Free Range Eggs (12)"},
      {"AA:BB:CC:DD", "Cola 330ml Can"},
      {"C0:FF:EE:99", "Orange Juice 1L"},
      {"DE:AD:BE:EF", "Cola 330ml Can"}};

  for (auto &entry : productMap)
  {
    if (uid == entry.uid)
    {
      return String(entry.name);
    }
  }
  return "Unknown Product";
}

void loop()
{
  if (!client.connected())
  {
    reconnect();
  }
  client.loop();

  display.setCursor(0, 0);

  // Reader 1 → Azure IoT Hub - For Product Scan
  if (rfid1.PICC_IsNewCardPresent() && rfid1.PICC_ReadCardSerial())
  {
    digitalWrite(SCANNER_LED_PIN, HIGH);
    tone(SPEAKER_PIN, 500, 400);

    String uidStr = getUid(rfid1);
    Serial.println("Reader1 UID: " + uidStr);

    display.println(getProductName(uidStr));
    display.display();

    String payload = "{";
    payload += "\"Uid\":\"" + uidStr + "\",";
    payload += "\"CartId\":1,";
    payload += "\"Timestamp\":\"2026-06-06T08:30:00Z\",";
    payload += "\"EventType\":\"1\"";
    payload += "}";

    String topic = "devices/" + String(deviceId) + "/messages/events/";
    client.publish(topic.c_str(), payload.c_str());

    rfid1.PICC_HaltA();
    rfid1.PCD_StopCrypto1();

    delay(750);
    display.clearDisplay();
    display.display();
    digitalWrite(SCANNER_LED_PIN, LOW);
    noTone(SPEAKER_PIN);
  }

  // Reader 2 → Azure IoT Hub - For Payments
  if (rfid2.PICC_IsNewCardPresent() && rfid2.PICC_ReadCardSerial())
  {
    digitalWrite(PAYMENT_LED_PIN, HIGH);
    tone(SPEAKER_PIN, 500, 400);

    String uidStr = getUid(rfid2);
    Serial.println("Reader2 UID: " + uidStr);

    display.println("Contactless Payment");
    display.display();

    String payload = "{";
    payload += "\"Uid\":\"" + uidStr + "\",";
    payload += "\"CartId\":1,";
    payload += "\"Timestamp\":\"2026-06-06T08:30:00Z\",";
    payload += "\"EventType\":\"2\"";
    payload += "}";

    String topic = "devices/" + String(deviceId) + "/messages/events/";
    client.publish(topic.c_str(), payload.c_str());

    rfid2.PICC_HaltA();
    rfid2.PCD_StopCrypto1();

    delay(750);
    display.clearDisplay();
    display.display();
    digitalWrite(PAYMENT_LED_PIN, LOW);
    noTone(SPEAKER_PIN);
  }

  delay(50);
}
