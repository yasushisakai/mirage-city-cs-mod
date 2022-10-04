using ICities;
using System;
using System.Collections.Generic;

namespace mirage_city_mod
{

    public class CitizenFinder
    {

        private uint cursor = 0;

        public static CitizenFinder Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new CitizenFinder();
                }
                return _instance;
            }
        }

        private static CitizenFinder _instance = null;

        public bool FindCitizen(out CitizenData cd)
        {
            var manager = CitizenManager.instance;
            cd = null;
            if (manager.m_citizenCount == 0) return false;
            while (cd == null)
            {
                for (var i = cursor; i < manager.m_citizens.m_buffer.Length; i++)
                {
                    var citizen = manager.m_citizens.m_buffer[i];
                    if ((citizen.m_flags & Citizen.Flags.Created) != 0 &&
                        !citizen.Dead)
                    {
                        cd = new CitizenData(i);
                        cursor = i;
                        break; // for
                    }
                }
                if (cd == null)
                {
                    cursor = 0; // reset to 0
                } // else breaks the while loop
            }
            return true;
        }
    }

    public class CitizenData
    {
        public string name;
        public ushort age;
        public byte wealth;
        public byte education;
        public CitizenData(string _name, ushort _age, byte _wealth, byte _education)
        {
            name = _name;
            age = _age;
            wealth = _wealth;
            education = _education;
        }

        public CitizenData(uint cid)
        {
            name = CitizenManager.instance.GetCitizenName(cid);
            var citizen = CitizenManager.instance.m_citizens.m_buffer[cid];
            age = citizen.m_age;
            wealth = (byte)citizen.WealthLevel;
            education = (byte)citizen.EducationLevel;
        }
    }
}