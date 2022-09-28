using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICities;
using UnityEngine;
using System.Configuration;

namespace mirage_city_mod
{
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
