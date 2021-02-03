﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CPUPLinker.Format
{
    class MIF : IFormat
    {
        public const string fileType = "mif";


        public byte[] getOutput(ushort[] machineCode)
        {

            Console.WriteLine("Generating MIF File");
            string file = "--Generated By CPUP Linker\n" +
                            "--Generated On:" + DateTime.Today.ToString("dd/MM/yyyy h:mm tt") + "\n";

            file += "WIDTH=16;\n";
            file += "DEPTH=28000;\n\n";
            file += "ADDRESS_RADIX=BIN;\n";
            file += "DATA_RADIX=BIN;\n\n";
            file += "CONTENT BEGIN\n";



            for (ushort i = 0; i < machineCode.Length; i++)
            {
                file += "   " + UShortToBin(i) + " : " + UShortToBin(machineCode[i]) + ";\n";
            }

            //Handle the trailing whitespace if any
            if (machineCode.Length != 28000)
            {
                //if it's just one entry dont do a range
                if (28000 - machineCode.Length == 1)
                {
                    file += "   " + UShortToBin((ushort)machineCode.Length) + " : " + UShortToBin(0) + ";\n";
                }
                else
                {
                    file += "   [" + UShortToBin((ushort)machineCode.Length) + ".." + UShortToBin(28000) + "]  :  " + UShortToBin(0) + ";\n";
                }
            }
            file += "END;";



            return Encoding.UTF8.GetBytes(file);
        }

        //ushort to 16bit binary string
        public static string UShortToBin(ushort addr)
        {
            ushort a = addr;
            string result = "";
            for (ushort i = 0; i < 16; i++)
            {
                if ((a % 2) == 1)
                {
                    result = "1" + result;
                    a--;
                }
                else
                {
                    result = "0" + result;
                }
                a = unchecked((ushort)(a >> 1));
            }
            return result;

        }
    }
}
