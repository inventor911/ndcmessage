using System;
using System.Collections.Generic;
using System.Text;

namespace Interpretor_NDC
{
    class ConfigurationParametersLoad : CustomisationDataCommands
    {
        public char h_reserved = (char)0;
        public string i_reserved = "000";
        public string j_reserved = "000";
        public string k_reserved = "000";
        public string l_reserved = "000";
        public string SupplyMode = "000";
        public string n_reserved = "000";
        public string LUNO2 = "";
        public string[] Timer = null;
        public string[] Number = null;

        public ConfigurationParametersLoad(string str)
            : base(str)
        {
            Name = "Configuration Parameters Load";

            int sep1 = Utils.StrIndexOf((char)28, str, 3);
            int sep2 = Utils.StrIndexOf((char)28, str, 4);

            switch(sep2 - sep1)
            {
            case 4:
                SupplyMode = str.Substring(sep1 + 1, 3);
                break;
            case 5:
                h_reserved = Convert.ToChar(str.Substring(sep1 + 1, 1));
                SupplyMode = str.Substring(sep1 + 2, 3);
                break;
            case 8:
                h_reserved = Convert.ToChar(str.Substring(sep1 + 1, 1));
                i_reserved = str.Substring(sep1 + 2, 3);
                SupplyMode = str.Substring(sep1 + 5, 3);
                break;
            case 11:
                h_reserved = Convert.ToChar(str.Substring(sep1 + 1, 1));
                i_reserved = str.Substring(sep1 + 2, 3);
                j_reserved = str.Substring(sep1 + 5, 3);
                SupplyMode = str.Substring(sep1 + 8, 3);
                break;
            case 14:
                h_reserved = Convert.ToChar(str.Substring(sep1 + 1, 1));
                i_reserved = str.Substring(sep1 + 2, 3);
                j_reserved = str.Substring(sep1 + 5, 3);
                k_reserved = str.Substring(sep1 + 8, 3);
                SupplyMode = str.Substring(sep1 + 11, 3);
                break;
            case 17:
                h_reserved = Convert.ToChar(str.Substring(sep1 + 1, 1));
                i_reserved = str.Substring(sep1 + 2, 3);
                j_reserved = str.Substring(sep1 + 5, 3);
                k_reserved = str.Substring(sep1 + 8, 3);
                l_reserved = str.Substring(sep1 + 11, 3);
                SupplyMode = str.Substring(sep1 + 13, 3);
                break;
            case 26:
                h_reserved = Convert.ToChar(str.Substring(sep1 + 1, 1));
                i_reserved = str.Substring(sep1 + 2, 3);
                j_reserved = str.Substring(sep1 + 5, 3);
                k_reserved = str.Substring(sep1 + 8, 3);
                l_reserved = str.Substring(sep1 + 11, 3);
                SupplyMode = str.Substring(sep1 + 13, 3);
                n_reserved = str.Substring(sep1 + 15, 9);
                break;
            }

            if (str.Length == sep2 + 1)
                return;

            sep1 = Utils.StrIndexOf((char)28, str, 5);
            if(sep1 != sep2+1)
                LUNO2 = str.Substring(sep2 + 1, 3);

            short nrTimers = 0;
            int pos = sep1 + 1;
            short nr;

            do
            {
                nr = Convert.ToInt16(str.Substring(pos, 2));
                if (nr == nrTimers)
                    nrTimers++;
                else
                    break;
                pos += 5;
            } while (true);

            Timer = new string[nrTimers];
            Number = new string[nrTimers];
            pos = sep1 + 1;
            for (int i = 0; i < nrTimers; i++)
            {
                Timer[i] = str.Substring(pos, 2);
                Number[i] = str.Substring(pos + 2, 3);
                pos += 5;
            }
            
            Trailer = str.Substring(pos + 1);
        }
    }
}
