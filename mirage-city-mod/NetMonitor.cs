using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace mirage_city_mod
{

    public class NodeMonitor
    {
        public void checkNodes()
        {
            var rawNodes = NetManager.instance.m_nodes;

            foreach (var node in NetManager.instance.m_nodes.m_buffer)
            {
                var t = node.GetType();
                Debug.Log(t.ToString());
            }
        }
    }

}