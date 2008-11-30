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
        public string[] part = null; // cele 8 parti 8*3
        public string[] descriptionPart = null; //
        public string[] namePart = null; //
        public string[] mask = null;
        public string textTable;
        public int extension = 0;  // 0-neextins, 1-simplu extins, 2-dubluextins

        public static List<StateTable> ListOfStateTables = new List<StateTable>();
        public static List<StateTable> ListOfStateTablesAll = new List<StateTable>();

        public StateTable FindStateTabelsInListAll( string numberState )
        {
            for (int i = 0; i < ListOfStateTablesAll.Count; i++)
            {
                if (ListOfStateTablesAll[i].part[0] == numberState)
                    return ListOfStateTablesAll[i];
            }                
            return null;
        }

        public StateTable FindStateTabelsInList(string numberState)
        {
            for (int i = 0; i < ListOfStateTables.Count; i++)
            {
                if (ListOfStateTables[i].part[0] == numberState)
                    return ListOfStateTables[i];
            }
            return null;
        }

        public StateTable(string strNumarTypeCommon) // mesajul transmis cu numar tip si partea comuna
        {
            textTable = strNumarTypeCommon;
            part = new string[30];
            descriptionPart = new string[30];
            namePart = new string[30];

            part[0] = strNumarTypeCommon.Substring(0, 3);
            namePart[0] = "###";
            descriptionPart[0] = "";
            part[1] = strNumarTypeCommon.Substring(3, 1);
            namePart[1] = "###";
            descriptionPart[1] = "";

            if (strNumarTypeCommon.Length < 28)
                return;


            for (int i = 2; i < 10; i++)
            {
                part[i] = strNumarTypeCommon.Substring(3 * (i - 2) + 4, 3);
                namePart[i] = "###";
                descriptionPart[i] = "###";
            }
        }

        public void particulariseState(string pathConfigXML)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(pathConfigXML);

            XmlNode Node = xmlDoc.DocumentElement.FirstChild;
            while( Node != null)
            {
                if (Node.Attributes["type"].Value == textTable[3].ToString())
                {                  
                    part = new string[30];
                    descriptionPart = new string[30];
                    namePart = new string[30];

                    namePart[0] = "Number";
                    descriptionPart[0] = "Number";
                    part[0] = textTable.Substring(0, 3);

                    namePart[1] = "Type";
                    descriptionPart[1] = Node.Attributes["descriptionType"].Value;
                    part[1] = textTable.Substring(3, 1);

                    if (textTable.Length < 28)
                        return;

                    for (int i = 2; i < 10; i++)
                    {
                        namePart[i] = Node.Attributes[i].Value;
                        descriptionPart[i] = Node.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                        part[i] = textTable.Substring(3 * (i - 2) + 4, 3);
                    }

                    XmlNode childrenNode = Node.FirstChild; // este extins?
                    while (childrenNode != null)
                    {
                        if (childrenNode.Name == "ExtensionState")
                        {
                            if (part[1] == "K")
                            {
                                StateTable SText1 = FindStateTabelsInListAll("_");
                                if (SText1 == null)
                                    return;
                                
                                part[10] = SText1.textTable.Substring(0, 3);
                                part[11] = SText1.textTable.Substring(3, 1);

                                namePart[10] = "Extended state K";
                                descriptionPart[10] = "This state is extended";
                                namePart[11] = "Type";
                                descriptionPart[11] = childrenNode.Attributes["descriptionType"].Value;

                                for (int i = 2; i < 10; i++)
                                {
                                    namePart[i + 10] = childrenNode.Attributes[i].Value.Replace("\\r\\n", "\r\n");
                                    descriptionPart[i + 10] = childrenNode.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                                    part[i + 10] = SText1.textTable.Substring(3 * (i - 2) + 4, 3);
                                }

                                XmlNode childrenChildrenNode = childrenNode.FirstChild;
                                while( childrenChildrenNode != null )
                                {
                                    StateTable SText2 = FindStateTabelsInListAll(part[19]);
                                    if (SText1 == null)
                                        return;
                                    if (SText1.textTable[3] != 'Z')
                                        return;

                                    part[20] = SText2.textTable.Substring(0, 3);
                                    part[21] = SText2.textTable.Substring(3, 1);

                                    namePart[20] = "Extended state _";
                                    descriptionPart[20] = "This state is extended";
                                    namePart[21] = "Type";
                                    descriptionPart[21] = childrenNode.Attributes["descriptionType"].Value;

                                    for (int i = 2; i < 10; i++)
                                    {
                                        namePart[i + 20] = childrenNode.Attributes[i].Value.Replace("\\r\\n", "\r\n");
                                        descriptionPart[i + 20] = childrenNode.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                                        part[i + 20] = SText1.textTable.Substring(3 * (i - 2) + 4, 3);
                                    }
                                }
                            }                            
                            else if (Convert.ToInt16(part[9]) > 7 && part[1] == "I")
                            {
                                StateTable SText1 = FindStateTabelsInListAll(part[9]);
                                if (SText1 == null)
                                    return;

                                if (SText1.textTable[3] != 'Z')
                                    return;

                                part[10] = SText1.textTable.Substring(0, 3);
                                part[11] = SText1.textTable.Substring(3, 1);

                                namePart[10] = "Extended state I";
                                descriptionPart[10] = "This state is extended";
                                namePart[11] = "Type";
                                descriptionPart[11] = childrenNode.Attributes["descriptionType"].Value;

                                for (int i = 2; i < 10; i++)
                                {
                                    namePart[i + 10] = childrenNode.Attributes[i].Value.Replace("\\r\\n", "\r\n");
                                    descriptionPart[i + 10] = childrenNode.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                                    part[i + 10] = SText1.textTable.Substring(3 * (i - 2) + 4, 3);
                                }
                            }
                            else if (part[1] == "X" && part[6] != "000")
                            {
                                StateTable SText1 = FindStateTabelsInListAll(part[6]);

                                if (SText1 == null)
                                    return;
                                if (SText1.textTable[3] != 'Z')
                                    return;

                                part[10] = SText1.textTable.Substring(0, 3);
                                part[11] = SText1.textTable.Substring(3, 1);

                                namePart[10] = "Extended state " + part[1];
                                descriptionPart[10] = "This state is extended";
                                namePart[11] = "Type";
                                descriptionPart[11] = childrenNode.Attributes["descriptionType"].Value;

                                for (int i = 2; i < 10; i++)
                                {
                                    namePart[i + 10] = childrenNode.Attributes[i].Value.Replace("\\r\\n", "\r\n");
                                    descriptionPart[i + 10] = childrenNode.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                                    part[i + 10] = SText1.textTable.Substring(3 * (i - 2) + 4, 3);
                                }
                            }
                            else if (part[1] == "Y" && part[6] != "000" && part[9] != "000")
                            {
                                List<StateTable> tempList = new List<StateTable>();
                                
                                StateTable SText1 = FindStateTabelsInListAll(part[6]);
                                if (SText1 != null)
                                {
                                    if (SText1.textTable[3] == 'Z')
                                        tempList.Add(SText1);
                                }

                                SText1 = FindStateTabelsInListAll(part[9]);
                                if (SText1 != null)
                                {
                                    if (SText1.textTable[3] == 'Z')
                                        tempList.Add(SText1);
                                }

                                for (int i = 0; i < tempList.Count; i++)
                                {
                                    part[i + 10] = tempList[i].textTable.Substring(0, 3);
                                    part[i + 11] = tempList[i].textTable.Substring(3, 1);

                                    namePart[i+10] = "Extended state " + part[1];
                                    descriptionPart[i+10] = "This state is extended";
                                    namePart[i+11] = "Type";
                                    descriptionPart[i+11] = childrenNode.Attributes["descriptionType"].Value;

                                    for (int j = 2; j < 10; j++)
                                    {
                                        namePart[i + j + 10] = childrenNode.Attributes[j].Value.Replace("\\r\\n", "\r\n");
                                        descriptionPart[i+ j + 10] = childrenNode.Attributes[j + 8].Value.Replace("\\r\\n", "\r\n");
                                        part[i + j + 10] = tempList[i].textTable.Substring(3 * (j - 2) + 4, 3);
                                    }
                                }
                            }
                            else if (part[9] != "000")
                            {
                                StateTable SText1 = FindStateTabelsInListAll(part[9]);

                                if (SText1 == null)
                                    return;
                                if (SText1.textTable[3] != 'Z')
                                    return;

                                part[10] = SText1.textTable.Substring(0, 3);
                                part[11] = SText1.textTable.Substring(3, 1);

                                namePart[10] = "Extended state " + part[1];
                                descriptionPart[10] = "This state is extended";
                                namePart[11] = "Type";
                                descriptionPart[11] = childrenNode.Attributes["descriptionType"].Value;

                                for (int i = 2; i < 10; i++)
                                {
                                    namePart[i + 10] = childrenNode.Attributes[i].Value.Replace("\\r\\n", "\r\n");
                                    descriptionPart[i + 10] = childrenNode.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                                    part[i + 10] = SText1.textTable.Substring(3 * (i - 2) + 4, 3);
                                }
                            }                        
                            
                        }
                        childrenNode = childrenNode.NextSibling;
                    }                    
                }
                Node = Node.NextSibling;
            }
        }

        public static void SaveToXML(string strPath)
        {
            FileInfo f = new FileInfo(strPath);
            StreamWriter writer = f.CreateText();

            writer.WriteLine("<STATES>");
            for (int count = 0; count < ListOfStateTables.Count; count++)
            {
                ListOfStateTables[count].particulariseState(@"C:\Config.xml");
                // <State>
                writer.Write("\t<State number=\"{0}\" type=\"{1}\" ", ListOfStateTables[count].part[0], ListOfStateTables[count].part[1]);
                for (int i = 0; i < ListOfStateTables[count].part.Length && ListOfStateTables[count].part[i] != null; i++)
                    writer.Write("part{0}=\"{1}\" ", i + 1, ListOfStateTables[count].part[i]);
                writer.WriteLine(">");
                //</State>
                writer.WriteLine("\t</State>");

            }
            writer.WriteLine("</STATES>");
            writer.Close();            
        }

        public static void ParticulariseAll()
        {
            ListOfStateTables.Clear();
            
            // remove Z, _
            for (int i = 0; i < ListOfStateTablesAll.Count; i++)
            {
                if (ListOfStateTablesAll[i].part[1] != "Z" && ListOfStateTablesAll[i].part[1] != "_")
                    ListOfStateTables.Add(ListOfStateTablesAll[i]);
            }
        }

    };

}


