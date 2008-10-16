using System;
using System.Collections.Generic;
using System.Text;

namespace Interpretor_NDC
{
    // State tables load
    class StateTablesLoad : CustomisationDataCommands
    {
        public string[] StateNumber = null;
        public string[] StateTableData = null;
        public string MessageAuthenticationCode = "";

        public StateTablesLoad(string str)
            : base(str)
        {
            Name = "State Tables Load";

            int sep = Utils.StrIndexOf('∟', str, 3);
            // nurara State-urile transmise
            int nrState = Utils.StrCount('∟', str, sep + 1);

            // nu exista State-uri
            if (nrState < 0)
            {
                StateNumber = new string[1];
                StateTableData = new string[1];
                StateNumber[0] = "ERROR";
                StateTableData[0] = "ERROR";
            }
            else
            {
                StateNumber = new string[nrState];
                StateTableData = new string[nrState];
                // load state
                for (int i = 0; i < StateTableData.Length; i++)
                {
                    StateNumber[i] = str.Substring(sep + 1, 3);
                    int lengthState = str.IndexOf('∟', sep + 1) - sep - 1 - 3;
                    StateTableData[i] = str.Substring(sep + 1 + 3, lengthState);
                    sep = str.IndexOf('∟', sep + 1);
                }
            }
            //FS
            // Optional
            if (str.Length - 1 < sep + 1) // afara din string
                return;

            MessageAuthenticationCode = str.Substring(sep + 1, 8);
            Trailer = str.Substring(sep + 1 + 8);
        }

    };

}
