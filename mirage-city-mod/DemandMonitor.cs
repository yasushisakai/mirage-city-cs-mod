using UnityEngine;

namespace mirage_city_mod
{


    public class DemandMonitor : MonoBehaviour
    {

        public static DemandMonitor Instance { get; private set; }

        public int residential { get; set; }

        public int commercial { get; set; }

        public int industrial { get; set; }

        public void setDemands(int _res, int _com, int _ind)
        {
            residential = _res;
            commercial = _com;
            industrial = _ind;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }


    }

}