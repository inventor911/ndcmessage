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

    struct PartStateTable
    {
        public string value; // cele 8 parti 8*3
        public string description;//
        public string name; //
        public string mask;
        public PartStateTable(string _value, string _description, string _name, string _mask)
        {
            value = _value;
            description = _description;
            name = _name;
            mask = _mask;
        }
        /*public PartStateTable()
         {
             PartStateTable("","","","");
         }*/
    };

    // State Table
    class StateTable
    {
        public PartStateTable[] PART;// = new PartStateTable[10];
        public string textTable;

        public static List<StateTable> ListOfStateTables = new List<StateTable>();
        public static List<StateTable> ListOfStateTablesAll = new List<StateTable>();

        public StateTable FindStateTabelsInListAll( string numberState )
        {
            for (int i = 0; i < ListOfStateTablesAll.Count; i++)
            {
                if (ListOfStateTablesAll[i].PART[0].value == numberState)
                    return ListOfStateTablesAll[i];
            }                
            return null;
        }

        public StateTable FindStateTabelsInList(string numberState)
        {
            for (int i = 0; i < ListOfStateTables.Count; i++)
            {
                if (ListOfStateTables[i].PART[0].value == numberState)
                    return ListOfStateTables[i];
            }
            return null;
        }

        public StateTable(string strNumarTypeCommon) // mesajul transmis cu numar tip si partea comuna
        {
            textTable = strNumarTypeCommon;
            PART = new PartStateTable[30];

            PART[0].value = strNumarTypeCommon.Substring(0, 3);
            PART[0].name = "###";
            PART[0].description = "";
            PART[1].value = strNumarTypeCommon.Substring(3, 1);
            PART[1].name = "###";
            PART[1].description = "";

            if (strNumarTypeCommon.Length < 28)
                return;


            for (int i = 2; i < 10; i++)
            {
                PART[i].value = strNumarTypeCommon.Substring(3 * (i - 2) + 4, 3);
                PART[i].name = "###";
                PART[0].description = "###";
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
                    PART = new PartStateTable[30];

                    PART[0].name = "Number";
                    PART[0].description = "Number";
                    PART[0].value = textTable.Substring(0, 3);

                    PART[1].name = "Type";
                    PART[1].description = Node.Attributes["descriptionType"].Value;
                    PART[1].value = textTable.Substring(3, 1);

                    if (textTable.Length < 28)
                        return;

                    for (int i = 2; i < 10; i++)
                    {
                        PART[i].name = Node.Attributes[i].Value;
                        PART[i].description = Node.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                        PART[i].value = textTable.Substring(3 * (i - 2) + 4, 3);
                    }

                    XmlNode childrenNode = Node.FirstChild; // este extins?
                    while (childrenNode != null)
                    {
# region Extended state
                        if (childrenNode.Name == "ExtensionState")
                        {
                            if (PART[1].value == "K")
                            {
                                StateTable SText1 = FindStateTabelsInListAll("_");
                                if (SText1 == null)
                                    return;

                                PART[10].value = SText1.textTable.Substring(0, 3);
                                PART[11].value = SText1.textTable.Substring(3, 1);

                                PART[10].name = "Extended state K";
                                PART[10].description = "This state is extended";
                                PART[11].name = "Type";
                                PART[11].description = childrenNode.Attributes["descriptionType"].Value;

                                for (int i = 2; i < 10; i++)
                                {
                                    PART[i + 10].name = childrenNode.Attributes[i].Value.Replace("\\r\\n", "\r\n");
                                    PART[i + 10].description = childrenNode.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                                    PART[i + 10].value = SText1.textTable.Substring(3 * (i - 2) + 4, 3);
                                }

                                XmlNode childrenChildrenNode = childrenNode.FirstChild;
                                while( childrenChildrenNode != null )
                                {
                                    StateTable SText2 = FindStateTabelsInListAll(PART[19].value);
                                    if (SText1 == null)
                                        return;
                                    if (SText1.textTable[3] != 'Z')
                                        return;

                                    PART[20].value = SText1.textTable.Substring(0, 3);
                                    PART[21].value = SText1.textTable.Substring(3, 1);

                                    PART[20].name = "Extended state _";
                                    PART[20].description = "This state is extended";
                                    PART[21].name = "Type";
                                    PART[21].description = childrenNode.Attributes["descriptionType"].Value;

                                    for (int i = 2; i < 10; i++)
                                    {
                                        PART[i + 20].name = childrenNode.Attributes[i].Value.Replace("\\r\\n", "\r\n");
                                        PART[i + 20].description = childrenNode.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                                        PART[i + 20].value = SText1.textTable.Substring(3 * (i - 2) + 4, 3);
                                    }
                                }
                            }
                            else if (Convert.ToInt16(PART[9].value) > 7 && PART[1].value == "I")
                            {
                                StateTable SText1 = FindStateTabelsInListAll(PART[9].value);
                                if (SText1 == null)
                                    return;

                                if (SText1.textTable[3] != 'Z')
                                    return;

                                PART[10].value = SText1.textTable.Substring(0, 3);
                                PART[11].value = SText1.textTable.Substring(3, 1);

                                PART[10].name = "Extended state I";
                                PART[10].description = "This state is extended";
                                PART[11].name = "Type";
                                PART[11].description = childrenNode.Attributes["descriptionType"].Value;

                                for (int i = 2; i < 10; i++)
                                {
                                    PART[i + 10].value = childrenNode.Attributes[i].Value.Replace("\\r\\n", "\r\n");
                                    PART[i + 10].description = childrenNode.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                                    PART[i + 10].name = SText1.textTable.Substring(3 * (i - 2) + 4, 3);
                                }
                            }
                            else if (PART[1].value == "X" && PART[6].value != "000")
                            {
                                StateTable SText1 = FindStateTabelsInListAll(PART[6].value);

                                if (SText1 == null)
                                    return;
                                if (SText1.textTable[3] != 'Z')
                                    return;

                                PART[10].value = SText1.textTable.Substring(0, 3);
                                PART[11].value = SText1.textTable.Substring(3, 1);

                                PART[10].name = "Extended state " + PART[1];
                                PART[10].description = "This state is extended";
                                PART[11].name = "Type";
                                PART[11].description = childrenNode.Attributes["descriptionType"].Value;

                                for (int i = 2; i < 10; i++)
                                {
                                    PART[i + 10].name = childrenNode.Attributes[i].Value.Replace("\\r\\n", "\r\n");
                                    PART[i + 10].description = childrenNode.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                                    PART[i + 10].value = SText1.textTable.Substring(3 * (i - 2) + 4, 3);
                                }
                            }
                            else if (PART[1].value == "Y" && PART[6].value != "000" && PART[9].value != "000")
                            {
                                List<StateTable> tempList = new List<StateTable>();

                                StateTable SText1 = FindStateTabelsInListAll(PART[6].value);
                                if (SText1 != null)
                                {
                                    if (SText1.textTable[3] == 'Z')
                                        tempList.Add(SText1);
                                }

                                SText1 = FindStateTabelsInListAll(PART[9].value);
                                if (SText1 != null)
                                {
                                    if (SText1.textTable[3] == 'Z')
                                        tempList.Add(SText1);
                                }

                                for (int i = 0; i < tempList.Count; i++)
                                {
                                    PART[i + 10].value = tempList[i].textTable.Substring(0, 3);
                                    PART[i + 11].value = tempList[i].textTable.Substring(3, 1);

                                    PART[i + 10].name = "Extended state " + PART[1].value;
                                    PART[i + 10].description = "This state is extended";
                                    PART[i + 11].name = "Type";
                                    PART[i + 11].description = childrenNode.Attributes["descriptionType"].Value;

                                    for (int j = 2; j < 10; j++)
                                    {
                                        PART[i + j + 10].name = childrenNode.Attributes[j].Value.Replace("\\r\\n", "\r\n");
                                        PART[i + j + 10].description = childrenNode.Attributes[j + 8].Value.Replace("\\r\\n", "\r\n");
                                        PART[i + j + 10].value = tempList[i].textTable.Substring(3 * (j - 2) + 4, 3);
                                    }
                                }
                            }
                            else if (PART[9].value != "000")
                            {
                                StateTable SText1 = FindStateTabelsInListAll(PART[9].value);

                                if (SText1 == null)
                                    return;
                                if (SText1.textTable[3] != 'Z')
                                    return;

                                PART[10].value = SText1.textTable.Substring(0, 3);
                                PART[11].value = SText1.textTable.Substring(3, 1);

                                PART[10].name = "Extended state " + PART[1].value;
                                PART[10].description = "This state is extended";
                                PART[11].name = "Type";
                                PART[11].description = childrenNode.Attributes["descriptionType"].Value;

                                for (int i = 2; i < 10; i++)
                                {
                                    PART[i + 10].name = childrenNode.Attributes[i].Value.Replace("\\r\\n", "\r\n");
                                    PART[i + 10].description = childrenNode.Attributes[i + 8].Value.Replace("\\r\\n", "\r\n");
                                    PART[i + 10].value = SText1.textTable.Substring(3 * (i - 2) + 4, 3);
                                }
                            }
                        }
# endregion
# region Set mask
                        else if (childrenNode.Name == "Mask")
                            for (int i = 2; i < 10; i++)
                                PART[i].mask = childrenNode.Attributes[i-2].Value;
# endregion
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
                writer.Write("\t<State number=\"{0}\" type=\"{1}\" ", ListOfStateTables[count].PART[0].value, ListOfStateTables[count].PART[1].value);
                for (int i = 0; i < ListOfStateTables[count].PART.Length && ListOfStateTables[count].PART[i].value != null; i++)
                    writer.Write("part{0}=\"{1}\" ", i + 1, ListOfStateTables[count].PART[i].value);
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
                if (ListOfStateTablesAll[i].PART[1].value != "Z" && ListOfStateTablesAll[i].PART[1].value != "_")
                    ListOfStateTables.Add(ListOfStateTablesAll[i]);
            }
        }
    };

}


