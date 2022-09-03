using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;

namespace mirage_city_mod
{
    public class Constants
    {
        // my address;
        public static readonly string IP = "18.27.123.81";
        public static readonly UInt16 PORT = 9000;
        public static readonly string Address = $"{IP}:{PORT}";
    }

    public class MirageCityMod : IUserMod
    {
        public string Name
        {
            get { return "Mirage City"; }
        }

        public string Description
        {
            get { return "Mod for Mirage City Project. This communicates with the backend server and receives commands and handle them."; }
        }
    }
}
