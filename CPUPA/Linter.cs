﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CPUPA
{
    class Linter
    {
        public static string[] Lint(string[] file, int stackSize, bool isLib, bool verbose)
        {
            return Lint(new List<string>(file),stackSize,isLib,verbose).ToArray();
        }

        private static int internalIDIndex = 0;
        public static List<string> Lint(List<string> file, int stackSize,bool isLib, bool verbose)
        {
            List<string> data = file;
            //Formatting
            Console.WriteLine("    Formatting File");
            for(int i = 0; i < file.Count; i++)
            {
                //Change tabs to spaces
                data[i] = data[i].Replace('\t', ' ');

                //Remove extra spaces
                data[i] = data[i].Trim();

                //Remove empty lines
                if(data[i] == "")
                {
                    data.RemoveAt(i);
                    i--;
                }else if (data[i].StartsWith("//")) //Remove Comments
                {
                    data.RemoveAt(i);
                    i--;
                }
            }
            Console.WriteLine("    Inserting Default Data");
            //Insert default data
            data = Tables.defaultData.Concat(data).ToList();
            if (!isLib)
            {
                
                data = Tables.defaultProgramData.Concat(data).ToList();
                data[0] = data[0].Replace("$$", "" + stackSize);
            }
            //Add Setup function if needed
            if (!isLib)
            {
                data = Tables.setupCode.Concat(data).ToList();
            }
            //Add Function End Code
            for(int i = 0; i < data.Count; i++)
            {
                if(data[i] == "end")
                {
                    foreach(string s in Tables.functionEnd)
                    {
                        data.Insert(i, s);
                    }
                    i += Tables.functionEnd.Count;
                }
            }

            Console.WriteLine("    Translating Emulated Instructions");

            //Replace translated instructions
            for(int i = 0; i < data.Count; i++)
            {
                if(Tables.translatedInstructions.ContainsKey(data[i].Trim().Split(" ")[0]))
                {
                    string command = data[i];
                    data.RemoveAt(i);

                    //We insert in reverse order. it will be the right way around when done.
                    List<string> instructionCode = Tables.translatedInstructions[command.Trim().Split(" ")[0]];
                    instructionCode.Reverse();
                    
                    foreach (string a in instructionCode)
                    {
                        string s = a;
                        //Replacing with arg 1
                        if (s.Contains("$$"))
                        {
                            //Is there enough arguments?
                            if(command.Trim().Split(" ").Length < 2)
                            {
                                Console.WriteLine("ERROR: Not Enough Arguments To Translate Instruction line: {0}", command);
                                throw new Exception("-9");
                            }

                             s = s.Replace("$$", command.Trim().Split(" ")[1]);
                        }
                        //Replacing with arg 2
                        if (s.Contains("%%"))
                        {
                            //Is there enough arguments?
                            if (command.Trim().Split(" ").Length < 3)
                            {
                                Console.WriteLine("ERROR: Not Enough Arguments To Translate Instruction line: {0}", command);
                                throw new Exception("-9");
                            }

                            s = s.Replace("%%", command.Trim().Split(" ")[2]);
                        }

                        //Insert
                        data.Insert(i, s);
                    }

                    //Increment
                    i += instructionCode.Count - 1;
                }
            }

            //Handle @+/- 
            for(int i = 0; i < data.Count;i++){
                if(data[i].Trim().Contains("@+") && !data[i].Contains("\"")){
                    //Ahead
                    int v = int.Parse(System.Text.RegularExpressions.Regex.Match(data[i].Trim(), "@\\+([0-9]+)").Groups[0].Value.Substring(2));
                    string label = ":CPUPA.INTERNAL_JUMP_" + internalIDIndex++;
                    data[i] = data[i].Replace("@+" + v, label);
                    data.Insert(i + v, label);
                }
                if(data[i].Trim().Contains("@-")&& !data[i].Contains("\"")){
                    //Behind
                    int v = int.Parse(System.Text.RegularExpressions.Regex.Match(data[i].Trim(), "@\\-([0-9]+)").Groups[0].Value.Substring(2));
                    string label = ":CPUPA.INTERNAL_JUMP_" + internalIDIndex++;
                    data[i] = data[i].Replace("@-" + v, label);

                    data.Insert(i - v, label);
                }
            }

            return data;
        }

    }
}
