using System;
using System.Collections.Generic;
using System.Text;

namespace Interpretor_NDC
{
    class FITDataLoad : CustomisationDataCommands
    {
        public string[] FITNumber = null;
        public string[] FITData = null;
        public string MAC = "";

        public FITDataLoad(string str, int nrOfFIT)
            : base(str)
        {
            Name = "FIT Data Load";

            int sep1 = Utils.StrIndexOf((char)28, str, 3);
            int sep2 = Utils.StrIndexOf((char)28, str, 4);

            FITData = new string[nrOfFIT];
            FITNumber = new string[nrOfFIT];

            for (int i = 0; i < nrOfFIT; i++)
            {
                FITNumber[i] = str.Substring(sep1 + 1 + 5 * i, 2);
                FITData[i] = str.Substring(sep1 + 1 + 5 * i + 2, sep2-sep1);
                sep1 = sep2;
                sep2 = Utils.StrCount((char)28, str, sep1);
            }
            
            if (sep2 == -1)
                return;

            MAC = str.Substring(sep2 + 1, 8);
            Trailer = str.Substring( sep2 + 8 + 1 );
        }
    }
}
