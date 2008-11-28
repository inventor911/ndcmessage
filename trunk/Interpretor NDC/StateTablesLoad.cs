using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace Interpretor_NDC
{
    // State tables load
    class StateTablesLoad : CustomisationDataCommands
    {
        public bool[] StateOk = null;
        public string[] StateTableData = null;
        public string MessageAuthenticationCode = "";

        public StateTablesLoad(string str)
            : base(str)
        {
            Name = "State Tables Load";

            int sep = Utils.StrIndexOf((char)28, str, 3);
            // nurara State-urile transmise
            int nrState = GetNoStates(str);

            // nu exista State-uri
            if (nrState < 0)
            {
                StateOk = new bool[1];
                StateTableData = new string[1];
                StateOk[0] = false;
                StateTableData[0] = "ERROR";
            }
            else
            {
                StateOk = new bool[nrState];
                StateTableData = new string[nrState];
                // load state
                for (int i = 0; i < StateTableData.Length; i++)
                {
                    int start = Utils.StrIndexOf((char)28, str, 3 + i);
                    int stop = Utils.StrIndexOf((char)28, str, 3 + i + 1);
                    if (stop < 0)
                        stop = str.Length;

                    StateTableData[i] = str.Substring(start + 1, stop - 1 - start);
                    if (StateTableData[i].Length != 28)
                        StateOk[i] = false;
                    else
                        StateOk[i] = true;

                    sep = stop;
                }
            }
            //FS
            if (sep >= str.Length - 1)
                return;

            MessageAuthenticationCode = str.Substring(sep + 1, 8);
            Trailer = str.Substring(sep + 1 + 8);
        }

        public int GetNoStates(string str) // da numarul de state-uri din str
        {
            int reIndex = Utils.StrIndexOf((char)28, str, 3);   // se sare peste antetul de inceput si se trece la primul state probabil

            if (reIndex < 0)
                return -1;  // nu exista state-uri => eroare

            // numara posibilele state-uri
            int noStates = 0;
            while (reIndex > 0)
            {
                int lastIndex = reIndex;
                noStates++;
                reIndex = Utils.StrIndexOf((char)28, str, 3 + noStates); // urmatorul separator
                // urmeaza un state?
                if (reIndex < -1) // nu mai exista separator
                {
                    // ultimul a fost state sau nu?
                    if (lastIndex + 28 == str.Length - 1) // a fost
                        break;
                    else // nu a fost
                        noStates--;
                }
                else if (reIndex == str.Length - 1) // se termina brusc cu un separator
                    break;
            }
            return noStates;
        }

        public void SaveToFile(string pathToSave)
        {
            FileInfo stateDB = new FileInfo(pathToSave);
            if (!stateDB.Exists)
                return;
            StreamWriter append = stateDB.AppendText();

            for (int i = 0; i < StateTableData.Length; i++)
                append.WriteLine(StateTableData[i]);

            append.Close();

        }

    };


    // State Table
    class StateTable
    {
        public string[] part = new string[8]; // cele 8 parti 8*3
        public string[] descriptionPart = new string[8]; //
        public string[] namePart = new string[8]; //


        public string Numar;
        public char Type;
        public string descriptionType;

        public StateTable(string strNumarTypeCommon) // mesajul transmis cu numar tip si partea comuna
        {
            this.Numar = "###";
            this.Type = '#';
            this.descriptionType = "!!!Maybe is extended!!!";
            for (int i = 0; i < part.Length; i++)
            {
                part[i] = "###";
                descriptionPart[i] = "***";
                namePart[i] = "???";
            }

            if (strNumarTypeCommon.Length != 28)
                return;

            Numar = strNumarTypeCommon.Substring(0, 3);
            Type = strNumarTypeCommon[3];
            for (int i = 0; i < namePart.Length; i++)
            {
                part[i] = strNumarTypeCommon.Substring(3 * i + 4, 3);
            }
        }

        public void particulariseState(string pathConfigXML)
        {
            FileStream fs = new FileStream(pathConfigXML, FileMode.Open);
            XmlReader r = XmlReader.Create(fs);

            while (r.Read()) // catuta descrierea STATE
            {
                if (r.Name == "STATES")
                {
                    while (r.Read()) // trece prin toti subfii
                    {
                        if (r.Name == "State" && r.HasAttributes)
                        {
                            if (r.GetAttribute(0) == Type.ToString())
                            {
                                descriptionType = r.GetAttribute(1);

                                for (int i = 0; i < namePart.Length; i++)
                                {
                                    namePart[i] = r.GetAttribute(i + 2);
                                    descriptionPart[i] = r.GetAttribute(i + 10).Replace("\\r\\n", "\r\n");
                                }
                            }
                        }
                    }

                }
            }
            fs.Close();
        }

        public void SaveToXML(StreamWriter writer)
        {
            /*FileInfo f = new FileInfo(@"State Tables.xml");
            StreamWriter writer = f.AppendText();*/

            // <State>
            writer.WriteLine("<State number=\"{0}\" type=\"{1}\" part1=\"{2}\" part2=\"{3}\" part3=\"{4}\" part4=\"{5}\" >", Numar, Type, part[0], part[1], part[3], part[4]);                
            //</State>
            writer.WriteLine("</State>");

            //writer.Close();
        }
    };

}


