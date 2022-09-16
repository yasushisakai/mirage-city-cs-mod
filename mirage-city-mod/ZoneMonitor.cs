using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace mirage_city_mod
{

    public class ZoneMonitor
    {
        // public List<ZoneBlock> used;

        public void checkZoneblocks()
        {
            ref ZoneBlock[] blocks = ref ZoneManager.instance.m_blocks.m_buffer;
            var netManager = NetManager.instance;
            var nodes = netManager.m_nodes.m_buffer;

            string log = "";
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                if ((block.m_flags & ZoneBlock.FLAG_CREATED) != 0)
                {

                    var segment = netManager.m_segments.m_buffer[block.m_segment];
                    var sn = segment.m_startNode;
                    var en = segment.m_endNode;
                    var start = nodes[sn].m_position;
                    var end = nodes[en].m_position;
                    var nodeString = $"{start.x}, {start.y}, {start.z}, {end.x}, {end.y}, {end.z}";

                    var segRelation = "";
                    if (i == segment.m_blockEndLeft)
                    {
                        segRelation = "endleft";
                    }
                    else if (i == segment.m_blockEndRight)
                    {
                        segRelation = "endright";
                    }
                    else if (i == segment.m_blockStartRight)
                    {
                        segRelation = "startright";
                    }
                    else if (i == segment.m_blockStartLeft)
                    {
                        segRelation = "startleft";
                    }
                    else
                    {
                        segRelation = "unknown";
                    }

                    var rows = block.RowCount;
                    for (int x = 0; x < ZoneBlock.COLUMN_COUNT; x++)
                    {

                        var pos = block.m_position;
                        var angle = block.m_angle;
                        for (int z = 0; z < rows; z++)
                        {
                            ulong select = (ulong)(1L << ((z << 3) | x));
                            if ((block.m_valid & select) != 0 &&
                            (block.m_shared) == 0) // &&
                            /*
                            !block.IsOccupied1(x, z) &&
                            !block.IsOccupied2(x, z))
                            */
                            {
                                var zone = block.GetZone(x, z);
                                string zoneType;
                                switch (zone)
                                {
                                    case ItemClass.Zone.ResidentialLow:
                                        zoneType = "ResLow";
                                        break;
                                    case ItemClass.Zone.ResidentialHigh:
                                        zoneType = "ResHigh";
                                        break;
                                    case ItemClass.Zone.CommercialLow:
                                        zoneType = "ComLow";
                                        break;
                                    case ItemClass.Zone.CommercialHigh:
                                        zoneType = "ComHigh";
                                        break;
                                    case ItemClass.Zone.Industrial:
                                        zoneType = "Ind";
                                        break;
                                    case ItemClass.Zone.Office:
                                        zoneType = "Office";
                                        break;
                                    case ItemClass.Zone.Unzoned:
                                        zoneType = "UnZone";
                                        break;
                                    default:
                                        zoneType = "Unknown";
                                        break;
                                }
                                log += $"{i}, {nodeString}, {segRelation}, {x}, {z}, {zoneType} \n";
                            }
                        }
                    }
                }
            }

            // dump
            string file = @"C:\Users\yasushi\Desktop\blocks.csv";
            File.WriteAllText(file, log);
            Debug.Log(log);
        }


    }

}