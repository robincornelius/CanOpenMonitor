using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CanMonitor
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

            foreach (string line in hexlines)
            {


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
                            UInt32 curaddress = (UInt32)(addr + i) * 2;
                            string opcode_little_endian = line.Substring(9 + i * 8, 8); // line[9 + i * 8:9 + (i + 1) * 8]
                            string opcode = opcode_little_endian.Substring(6, 2) + opcode_little_endian.Substring(4, 2) + opcode_little_endian.Substring(2, 2) + opcode_little_endian.Substring(0, 2);
                            UInt32 opcode_num = Convert.ToUInt32(opcode, 16);
                          //  Console.Write("\t 0x{0:x4} = 0x{1:x8}", curaddress, opcode_num);

                            opcodes.Add(curaddress, opcode_num);
                        }

                      //  Console.Write("\r\n");


                        break;
                }
          
            }

            //break into pages

            uint pagesize = 1024;

            for(uint page=0;page<64;page++)
            {
                UInt32 addrlo;
                UInt32 addrhi;

                addrlo = page * pagesize;
                addrhi = (page + 1) * pagesize;

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

                        uint subaddr = kvp.Key - addrlo;
                        //copy the 4 bytes into the memory array

                        pages[page][subaddr + 0] = src[3];
                        pages[page][subaddr + 1] = src[2];
                        pages[page][subaddr + 2] = src[1];


                    }

                }


            }

           

        }


    }
}
