/*
    This file is part of CanOpenMonitor.

    CanOpenMonitor is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    CanOpenMonitor is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with CanOpenMonitor.  If not, see <http://www.gnu.org/licenses/>.
 
    Copyright(c) 2019 Robin Cornelius <robin.cornelius@gmail.com>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FlashLoader
{
    class IntelHex
    {

        string[] hexlines;

        Dictionary<UInt32, UInt32> opcodes;

        public Dictionary<UInt32, byte[]> pages;
        public int maxpageno = 0;
        
        public void loadhex(string filename)
        {
           // try
            {
                hexlines = System.IO.File.ReadAllLines(filename);

                parsehex();

            }
           // catch(Exception e)
            {

            }


        }

        public void parsehex()
        {
            //:NNAAAATT......CC    (format)
            /*
             * - NN represents the number of bytes in the record (not including the header & checksum)
            - AAAA is the lower 16 bits of the data address
            - TT is the Record Type, which I'll get to in a second.
            - CC is the least-significant byte of the checksum
            - ....... is NN bytes of data
            */

            opcodes = new Dictionary<UInt32, UInt32>();
            pages = new Dictionary<UInt32, byte[]>();

            UInt32 ext_address = 0;

            int lineno = 0;
            UInt32 maxpage = 0;

            foreach (string line in hexlines)
            {

                lineno++;

                byte len = Convert.ToByte(line.Substring(1, 2), 16);
                UInt16 addrlo = Convert.ToUInt16(line.Substring(3, 4), 16);
                byte recortype = Convert.ToByte(line.Substring(7, 2), 16);

               

                switch (recortype)
                {
                    case 1:
                        //EOF
                        break;
                    case 4:
                        //extended;
                        ext_address = Convert.ToUInt16(line.Substring(9, 4), 16);
                        ext_address = ext_address << 16;

                        break;
                    case 0:
                        //regular payload
                        UInt32 addr = (addrlo + ext_address) /2;
                       // Console.Write("data: {0} bytes at address 0x{1:x8} = ", len, addr);
                        for(int i=0; i<len/4;i++)
                        {
                            UInt32 curaddress = (UInt32)(addr + (i * 2));
                            string opcode_little_endian = line.Substring(9 + i * 8, 8); // line[9 + i * 8:9 + (i + 1) * 8]
                            string opcode = opcode_little_endian.Substring(6, 2) + opcode_little_endian.Substring(4, 2) + opcode_little_endian.Substring(2, 2) + opcode_little_endian.Substring(0, 2);
                            UInt32 opcode_num = Convert.ToUInt32(opcode, 16);


                            UInt32 currentpage = curaddress / (1024 * 2);

                            if (currentpage > maxpage && currentpage != 0x1f00)
                            {
                                maxpage = currentpage; //wrong
                            }

                            if (curaddress==0)
                            {
                                Console.WriteLine("ADDR 0");

                                 Console.WriteLine("MODDING GOTO TO POINT TO BOOTLOADER");
                                 opcode_num = 0x00040800; //GOTO 0x800 
                               

                            }

                            if (curaddress == 2)
                            {
                                Console.WriteLine("RESET VECTOR");
                                continue;
                            }

                            if (curaddress>=4 && curaddress < 0x200)
                            {
                                Console.WriteLine("Vector table");
                            }

                            if (curaddress>0x557FE)
                            {
                                Console.WriteLine("Ignoring PIC config settings for the moment??");
                                continue;
                            }

                        //    Console.WriteLine("line {0} - base {1:x6} ext {2:x6} address: {3:x6} opcode: {4:x6}", lineno, addrlo, ext_address, curaddress, opcode_num);

                            opcodes.Add(curaddress, opcode_num);
                        }

                      //  Console.Write("\r\n");


                        break;
                }
          
            }

            //break into pages

            uint pagesize = 1024; //instructions (3 bytes per instruction) //4th byte is a phantom not written out


            //fudgy
            for(uint page=0;page<341;page++)
            {
                UInt32 addrlo;
                UInt32 addrhi;

                addrlo = page * (pagesize*2); //16bit increment
                addrhi = (page + 1) * (pagesize*2); //16 bit increment

                foreach(KeyValuePair <UInt32, UInt32 > kvp in opcodes)
                {
                    if(kvp.Key>=addrlo && kvp.Key<addrhi)
                    {
                        //its in the page

                        if(!pages.ContainsKey(page))
                        {
                            Console.WriteLine(String.Format("Creating data for page {0} at addr 0x{1:x4}", page,addrlo));
                            pages.Add(page, new byte[pagesize*3]); //24 bits per address

                            if (page > maxpageno)
                                maxpageno = (int) page;
                        }

                        //bytes of instruction (32bit)
                        byte[] src = BitConverter.GetBytes(kvp.Value);

                        //we may need some 24bit fudgery here???

                        //step by 2 above as addresses are 16bit increment
                        uint subaddr = 3*((kvp.Key/2) - (addrlo/2));
                        //copy the 4 bytes into the memory array

                        //endianness ???
                        pages[page][subaddr + 0] = src[0];
                        pages[page][subaddr + 1] = src[1];
                        pages[page][subaddr + 2] = src[2];

                        
                    }

                }


            }


            {
                uint currentpage = 5;

                byte[] sdodata = new byte[3 * 1024 + 3]; //pagesize + header;

                UInt32 pageaddr = currentpage * 2 * 1024; //awesome

                byte[] pagebits = BitConverter.GetBytes(pageaddr);

                //24 bit page address
                sdodata[0] = pagebits[0]; //LSB
                sdodata[1] = pagebits[1];
                sdodata[2] = pagebits[2]; //c# MSB
            }

            int x = 0;
            x++;

           

        }


    }
}
