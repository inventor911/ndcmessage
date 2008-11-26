using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

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
                reIndex = Utils.StrIndexOf((char)28, str, 3+noStates); // urmatorul separator
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

        public void SaveToFile( string pathToSave )
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
        public char[] StateType = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z', 'b', 'd', 'e', 'f', 'g', 'k', 'm', 'w', '_', '>' };
        public string Numar = "###";
        public char Type = '#';
        public string part1 = "###";
        public string part2 = "###";
        public string part3 = "###";
        public string part4 = "###";
        public string part5 = "###";
        public string part6 = "###";
        public string part7 = "###";
        public string part8 = "###";
        public string numePart1 = "******";
        public string numePart2 = "******";
        public string numePart3 = "******";
        public string numePart4 = "******";
        public string numePart5 = "******";
        public string numePart6 = "******";
        public string numePart7 = "******";
        public string numePart8 = "******";
        public string descriptionType = "";
        public string descriprionPart1 = "******";
        public string descriprionPart2 = "******";
        public string descriprionPart3 = "******";
        public string descriprionPart4 = "******";
        public string descriprionPart5 = "******";
        public string descriprionPart6 = "******";
        public string descriprionPart7 = "******";
        public string descriprionPart8 = "******";

        public StateTable(string strNumarTypeCommon) // mesajul transmis cu numar tip si partea comuna
        {
            if( strNumarTypeCommon.Length != 28 )
                return;

            Numar = strNumarTypeCommon.Substring(0, 3);
            Type = strNumarTypeCommon[3];
            part1 = strNumarTypeCommon.Substring(4, 3);
            part2 = strNumarTypeCommon.Substring(7, 3);
            part3 = strNumarTypeCommon.Substring(10, 3);
            part4 = strNumarTypeCommon.Substring(13, 3);
            part5 = strNumarTypeCommon.Substring(16, 3);
            part6 = strNumarTypeCommon.Substring(19, 3);
            part7 = strNumarTypeCommon.Substring(22, 3);
            part8 = strNumarTypeCommon.Substring(25, 3);

            switch(Type)
            {
                case 'A':
                    descriptionType = "Card Read State";
                    numePart1 = "Screen Number";
                    numePart2 = "Good Read Next State Number";
                    numePart3 = "Error (Misread) Screen Number";
                    numePart4 = "Read Condition 1";
                    numePart5 = "Read Condition 2";
                    numePart6 = "Read Condition 3";
                    numePart7 = "Card Return Flag";
                    numePart8 = "No FIT Match Next State Number";
                    descriprionPart1 = "Display screen that prompts the cardholder to enter the card. While the terminal is waiting for card entry, this screen is displayed.";
                    descriprionPart2 = "State number to which the terminal goes: 1. Following a good read of the card if FITs are not used (Table entry 9 = 000) or 2. If the Financial Institution number on the card matches a Financial Institution number in a FIT.";
                    descriprionPart3 = "******";
                    descriprionPart4 = "******";
                    descriprionPart5 = "******";
                    descriprionPart6 = "******";
                    descriprionPart7 = "******";
                    descriprionPart8 = "******";
                    break;
                case 'B':
                    break;
                case 'C':
                    break;
            }
            
        }

        
    };

}
