const int A1p = 2;
const int B1p = 3;
const int C1p = 4;
const int D1p = 5;
const int A2p = 6;
const int B2p = 7;
const int C2p = 12;
const int D2p = 13;

const int Out1p = 9;
const int Out2p = 10;
const int In1p = A3;
const int In2p = A4;

int A1v;
int B1v;
int C1v;
int D1v;
int A2v;
int B2v;
int C2v;
int D2v;
int Out1v;
int Out2v;

int mode; //0 = 2 terminal, 1 = 3 terminal
const int analogStep = 8;

void setup() {
  // initialize serial communications at 9600 bps:
  Serial.begin(9600);
  TCCR1B = TCCR1B & 0b11111000 | 0x01; //Set clk to 32k
  
  pinMode(In1p, INPUT);
  pinMode(In2p, INPUT);
  
  digitalWrite(A1p, 0);
  digitalWrite(B1p, 0);
  digitalWrite(C1p, 0);
  digitalWrite(D1p, 0);
  digitalWrite(A2p, 0);
  digitalWrite(B2p, 0);
  digitalWrite(C2p, 0);
  digitalWrite(D2p, 0);
  analogWrite(Out1p, 128);
  analogWrite(Out2p, 150); //Correct 150
  delay(100);
}

void loop() {
  delay(10);
  if(readSerial() == 1){
    analogWrite(Out1p,Out1v);
    analogWrite(Out2p,Out2v);
    delay(100);
    int readVal1 = analogRead(In1p);
    int readVal2 = analogRead(In2p);
    delay(10);
    //Serial.println(String(readVal1) + " " + String(readVal2) + " " + String(Out1v));
    //delay(10);
    
    Serial.print(readVal1);
    delay(10);
    Serial.print(" ");
    delay(10);
    Serial.println(readVal2);
    delay(10);
  }
}

int readSerial(){
  if(Serial.available() > 0){  
    if(Serial.read() == 's'){
      int pinvals[8];
      int i = 0;
      while(Serial.available() > 0){
        char c = Serial.read();
        int j = c - '0';
        //pinvals[i++] = j;
        if(i<=1)
          pinvals[i++] = j;
        else if(j == 1)
          pinvals[i++] = 0;
        else
          pinvals[i++] = 1;
        delay(1);
      }
      A1v = pinvals[0];
      B1v = pinvals[1];
      C1v = pinvals[2];
      D1v = pinvals[3];
      A2v = pinvals[4];
      B2v = pinvals[5];
      C2v = pinvals[6];
      D2v = pinvals[7];
      digitalWrite(A1p, A1v);
      digitalWrite(B1p, B1v);
      digitalWrite(C1p, C1v);
      digitalWrite(D1p, D1v);
      digitalWrite(A2p, A2v);
      digitalWrite(B2p, B2v);
      digitalWrite(C2p, C2v);
      digitalWrite(D2p, D2v);
      String s = String(A1v) + String(B1v) + String(C1v) + String(D1v) + String(A2v) + String(B2v) + String(C2v) + String(D2v);
      Serial.println(s);
      return 0;
    }
    else{
      String s = "";
      int i = 0;
      while(Serial.available() > 0){
        char c = Serial.read();
        if(c != ' '){
          s += c;
        }else{
          if(i == 0){
            i++;
            Out1v = s.toInt();
            s = "";
          }
        }
        delay(1);
      }
      Out2v = s.toInt();
      return 1;
    }
    return 0;
  }
}
