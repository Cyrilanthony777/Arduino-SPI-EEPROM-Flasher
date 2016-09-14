#include <SPI.h>

#define READ_CMD 0x03
#define PAGE_CMD 0x02
#define ERSE_CMD 0x60
#define WREN_CMD 0x06
#define WRDE_CMD 0x04
#define RESE_CMD 0x05
#define WRSR_CMD 0x01
#define UNLK_CMD 0x98
  
#define CS_PIN 10

long num_bytes;
byte wrbuff[256];

byte readStatusReg() 
{
  byte resp;
  digitalWrite(CS_PIN, LOW);
  SPI.transfer(RESE_CMD);
  resp = SPI.transfer(0x00);
  PORTB=0b00000100;
  return resp;
}



void read_eeprom(long num_bytes) 
{
  long addr;
  byte resp;
  digitalWrite(CS_PIN, LOW);
  /* transmit read command with 3 byte start address */
  SPI.transfer(READ_CMD);
  SPI.transfer(0x00);
  SPI.transfer(0x00);
  SPI.transfer(0x00);
  for (addr = 0; addr <= num_bytes; addr++) {
    resp = SPI.transfer(0xff);
   Serial.write(resp);
  }
  digitalWrite(CS_PIN, HIGH);
}

void write_page(int pog)
{

 byte resp,ad1,ad2,ad3,ad0;
 int idx = 0;
 ad1=0;
 ad2=0;
 ad3=0;
 resp = 0x00;
 long pgs;
 pgs = pog;
 long addr = 0;
 addr = 256 * pgs;
 ad0 = (byte)((addr >> 24)&0xFF);
 ad1 = (byte)((addr >> 16)&0xFF);
 ad2 = (byte)((addr >> 8)&0xFF);
 ad3 = (byte)(addr&0xFF);
  digitalWrite(CS_PIN, LOW);
  SPI.transfer(WREN_CMD);
  PORTB=0b00000100;
  delayMicroseconds(200);
  digitalWrite(CS_PIN, LOW);
  SPI.transfer(PAGE_CMD);
  SPI.transfer(ad1);
  SPI.transfer(ad2);
  SPI.transfer(ad3);
  for(int b=0;b<256;b++)
  {
   
   SPI.transfer(wrbuff[b]);
  }
  
  PORTB=0b00000100;
  resp = readStatusReg();
  while(resp == 0x03)
  {
    delayMicroseconds(100);
    resp = readStatusReg();
  }

  digitalWrite(CS_PIN, LOW);
  SPI.transfer(READ_CMD);
  SPI.transfer(ad1);
  SPI.transfer(ad2);
  SPI.transfer(ad3);
  for(int i=0;i<256;i++)
    {
      
      if(wrbuff[i]==SPI.transfer(0xFF))
      {
        idx = 1;
      }
      else
      {
        idx = 2;
        break;
      }
      
    }
  
  PORTB=0b00000100;
  
  
   for(int i=0;i<=255;i++)
    {
      wrbuff[i]=0x00;
      
    }
    delay(2);
    if(idx == 1)
    {
      Serial.write(0xD7);
    }
    else
    {
      Serial.write(0x0F);
    }
  
  
}

void eraseChip()
{
  digitalWrite(CS_PIN, LOW);
  SPI.transfer(WREN_CMD);
  digitalWrite(CS_PIN, HIGH);
  digitalWrite(CS_PIN, LOW);
  SPI.transfer(ERSE_CMD);
  digitalWrite(CS_PIN, HIGH);
}


void setup()
{
  Serial.begin(115200);
  SPI.setClockDivider(SPI_CLOCK_DIV4);
    
  SPI.setBitOrder(MSBFIRST);
  Serial.setTimeout(100);

  pinMode(CS_PIN, OUTPUT);
  digitalWrite(CS_PIN, HIGH);
  SPI.begin();
}



void loop() 
{
  String buff;
  int pg;
  int dcrc;
  byte crc;
  crc = 0x00;
  dcrc = 0x00;
  byte xtbuf[32];
   byte comm[8];
    comm[0] = 0x00;
    comm[1] = 0x00;
    comm[2] = 0x00;
    comm[3] = 0x00;
    comm[4] = 0x00;
    comm[5] = 0x00;
    comm[6] = 0x00;
    comm[7] = 0x00;
    comm[8] = 0x00;
    comm[9] = 0x00;
   long pag =0;
   
   num_bytes = 0;
  
  /* wait for the integer with the requested number of bytes */
        
    if(Serial.available()==41)
    {
    comm[0] = Serial.read();
    comm[1] = Serial.read();
    comm[2] = Serial.read();
    comm[3] = Serial.read();// Header
    comm[4] = Serial.read();// page highbyte / ad0
    comm[5] = Serial.read();// page low bytw / ad1
    comm[6] = Serial.read();// page/8 value / ad2
    comm[7] = Serial.read();// dummy / ad3
    comm[8] = Serial.read();// cmd
   
    for(int i=0;i<32;i++)
       {
         xtbuf[i]=Serial.read();
       }
    if(comm[8]==0x7A)
    {
      if(comm[6]==0x01)
      {
        for(int i=0;i<32;i++)
        {
          wrbuff[i]=xtbuf[i];
        }
      }
       else if(comm[6]==0x02)
      {
        for(int i=32;i<64;i++)
        {
          wrbuff[i]=xtbuf[i-32];
        }
      }
       else if(comm[6]==0x03)
      {
        for(int i=64;i<96;i++)
        {
          wrbuff[i]=xtbuf[i-64];
        }
      }
        else if(comm[6]==0x04)
      {
        for(int i=96;i<128;i++)
        {
          wrbuff[i]=xtbuf[i-96];
        }
      }
       else if(comm[6]==0x05)
      {
        for(int i=128;i<160;i++)
        {
          wrbuff[i]=xtbuf[i-128];
        }
      }
      else if(comm[6]==0x06)
      {
        for(int i=160;i<192;i++)
        {
          wrbuff[i]=xtbuf[i-160];
        }
      }
       else if(comm[6]==0x07)
      {
        for(int i=192;i<224;i++)
        {
          wrbuff[i]=xtbuf[i-192];
        }
      }
       else if(comm[6]==0x08)
      {
        for(int i=224;i<256;i++)
        {
          wrbuff[i]=xtbuf[i-224];
        }
       wrbuff[255] = xtbuf[31];
      }
      
      for(int i=0;i<32;i++)
      {
        crc ^= xtbuf[i];
      }
    }

      //Serial.print(crc);
        
       
    }

   

    if((comm[0]== 0xFF)&&(comm[1]==0xEE)&&(comm[2]==0xFF)&&(comm[3]==0xEE))
    {
      if(comm[8]==0xAA)
      {
        byte datarx[3];
        datarx[0] = comm[4];
        datarx[1] = comm[5];
        datarx[2] = comm[6];
        datarx[3] = comm[7];
        num_bytes = (long)datarx[0]<<24;
        num_bytes += (long)datarx[1]<<16;
        num_bytes += (long)datarx[2]<<8;
        num_bytes += (long)datarx[3];
        read_eeprom(num_bytes);
        //Serial.print(num_bytes);
      }
      else if(comm[8]==0xEE)
      {
       eraseChip();
      }
      else if (comm[8]==0x05)
      {
        readStatusReg();
      }
       else if(comm[8]==0xCC)
      {
        pg = int(word(comm[4],comm[5]));
        write_page(pg);
      }
      else if(comm[8]==0xDD)
      {
       for(int i=0;i<=255;i++)
       {
        Serial.write(wrbuff[i]);
       }
      }
     else if(comm[8]==0x7A)
     {
        Serial.write(crc);
     }
     else if(comm[8]==0xDC)
     {
      for(int i=0;i<256;i++)
      {
        dcrc ^= wrbuff[i];
      }
      Serial.write(dcrc);
         
      
     }
      else
      {
       Serial.println("INVALID COMMAND");
      }
      
    }

      for(int i=0;i<32;i++)
    {
      xtbuf[i]=0x00;
    }
    
  
}

