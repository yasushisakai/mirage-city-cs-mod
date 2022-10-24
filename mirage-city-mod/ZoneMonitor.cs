using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

namespace mirage_city_mod
{
    public class ZoneMonitor
    {
        public bool checkBlock(int _id)
        {
            if (_id == 0) return false;

            var block = ZoneManager.instance.m_blocks.m_buffer[_id];
            return (block.m_flags & ZoneBlock.FLAG_CREATED) != 0;
        }
        public string Info()
        {
            ref NetSegment[] segments = ref NetManager.instance.m_segments.m_buffer;

            var blocks = new List<Block>();
            var blockIds = new HashSet<int>();

            var nodeIds = new HashSet<int>();
            var nodes = new List<Node>();

            var edges = new List<Edge>();

            for (int i = 0; i < segments.Length; i++)
            {
                var seg = segments[i];
                if (
                    (seg.m_flags & NetSegment.Flags.Created) != 0 &&
                    (seg.m_flags & NetSegment.Flags.Deleted) == 0)
                {
                    var el = seg.m_blockEndLeft;
                    var er = seg.m_blockEndRight;
                    var sr = seg.m_blockStartRight;
                    var sl = seg.m_blockStartLeft;
                    var sd = seg.m_startDirection;
                    var ed = seg.m_endDirection;
                    var sum = el + er + sr + sl;

                    if (checkBlock(el)) blockIds.Add(el);
                    if (checkBlock(er)) blockIds.Add(er);
                    if (checkBlock(sl)) blockIds.Add(sl);
                    if (checkBlock(sr)) blockIds.Add(sr);

                    if (sum != 0)
                    {
                        var edge = new Edge(i, seg.m_startNode, seg.m_endNode, sl, sr, el, er, sd, ed);
                        edges.Add(edge);
                        nodeIds.Add(seg.m_startNode);
                        nodeIds.Add(seg.m_endNode);
                    }
                }
            }

            var blocksString = "blocks: ";
            foreach (var b in blockIds)
            {
                blocksString += $"{b}, ";
                var block = ZoneManager.instance.m_blocks.m_buffer[b];
                var rows = Math.Min(block.RowCount, 8);
                var bl = new Block(b);
                for (int x = 0; x < ZoneBlock.COLUMN_COUNT; x++)
                {
                    for (int z = 0; z < rows; z++)
                    {
                        ulong select = (ulong)(1L << ((z << 3) | x));
                        if ((block.m_valid & select) != 0 &&
                        (block.m_shared) == 0)
                        {

                            var zone = block.GetZone(x, z);
                            bl.AppendCell(new Cell(x, z, zone));
                        }
                    }
                }
                if (!bl.Empty()) { blocks.Add(bl); }
            }

            foreach (var n in nodeIds)
            {
                var nd = NetManager.instance.m_nodes.m_buffer[n].m_position;
                var newNode = new Node(n, nd.x, nd.y, nd.z);
                nodes.Add(newNode);
            }

            var network = new Network(blocks, edges, nodes);

            return network.Serialize();
        }

        public static void ChangeLandUseBlock(UInt16 index, ItemClass.Zone type)
        {
            ref var block = ref ZoneManager.instance.m_blocks.m_buffer[index];
            var rows = Math.Min(block.RowCount, 8);
            Debug.Log($"changing: b:{index}, rows:{rows}, zone:{type.ToString()}");
            for (int x = 0; x < ZoneBlock.COLUMN_COUNT; x++)
            {
                for (int z = 0; z < rows; z++)
                {
                    ulong select = (ulong)(1L << ((z << 3) | x));
                    if ((block.m_valid & select) != 0 &&
                    (block.m_shared) == 0)
                    {
                        block.SetZone(x, z, ItemClass.Zone.Unzoned);
                        block.SetZone(x, z, type);
                    }
                }
            }
            block.RefreshZoning((ushort)index);
        }

        public static bool GetLandUse(UInt16 index, int x, int z, out ItemClass.Zone zone)
        {

            zone = ItemClass.Zone.None;

            if (x < 0 || x > ZoneBlock.COLUMN_COUNT)
            {
                return false;
            }

            if (z < 0 || z > 8)
            {
                return false;
            }

            var block = ZoneManager.instance.m_blocks.m_buffer[index];
            if ((block.m_flags & ZoneBlock.FLAG_CREATED) != 0)
            {
                ulong select = (ulong)(1L << ((z << 3) | x));
                if ((block.m_valid & select) != 0 &&
                (block.m_shared) == 0)
                {
                    zone = block.GetZone(x, z);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }

    public class Node
    {
        public float x;
        public float y;
        public float z;
        public int id;

        public Node(int _id, float _x, float _y, float _z)
        {
            id = _id;
            x = _x;
            y = _y;
            z = _z;
        }

        public string Serialize()
        {
            return "{" + $"\"id\": {id}, \"pos\":[{x}, {y}, {z}]" + "}";
        }
    }

    public class Edge
    {
        public int id;
        public int start;
        public int end;

        // blocks 
        public int sl;
        public int sr;
        public int el;
        public int er;

        public Vector3 sd;
        public Vector3 ed;

        public Edge(
            int _id,
            int _start,
            int _end,
            int _sl,
            int _sr,
            int _el,
            int _er,
            Vector3 _sd,
            Vector3 _ed)
        {
            id = _id;
            start = _start;
            end = _end;
            sl = _sl;
            sr = _sr;
            el = _el;
            er = _er;
            sd = _sd;
            ed = _ed;
        }

        public string Serialize()
        {

            var sdString = $"[{sd.x}, {sd.y}, {sd.z}]";
            var edString = $"[{ed.x}, {ed.y}, {ed.z}]";

            // return "{" + $"\"id\":{id}, \"nodes\": [{start}, {end}], \"sl\": {sl}, \"sr\": {sr}, \"el\":{el}, \"er\":{er}, \"sd\":{sdString}, \"ed\":{edString}" + "}";
            return "{" + $"\"id\":{id}, \"start\": {start}, \"end\":{end}, \"sl\": {sl}, \"sr\": {sr}, \"el\":{el}, \"er\":{er}, \"sd\":{sdString}, \"ed\":{edString}" + "}";
        }
    }

    public class Block
    {
        public int id;

        public List<Cell> cells;

        public Block(int _id)
        {
            id = _id;
            cells = new List<Cell>();
        }

        public bool Empty()
        {
            return cells.Count == 0;
        }

        public void AppendCell(Cell _cell)
        {
            cells.Add(_cell);
        }

        public string SerializeCells()
        {
            var content = String.Join(",", cells.Select(c => c.Serialize()).ToArray());
            return $"[{content}]";
        }

        public string Serialize()
        {
            var o = "{";
            o += $"\"id\": {id}, ";
            var cells = SerializeCells();
            o += $"\"cells\" : {cells}";
            o += "}";
            return o;
        }
    }

    public class Cell
    {
        public int x;
        public int z;
        // public string land_use;
        public ItemClass.Zone zone;


        public Cell(int _x, int _z, ItemClass.Zone _zone)
        {
            x = _x;
            z = _z;
            zone = _zone;
        }

        // TODO: enums may already have int mappings...
        private int zoneToInt()
        {
            switch (zone)
            {
                case ItemClass.Zone.ResidentialLow:
                    return 1;
                case ItemClass.Zone.ResidentialHigh:
                    return 2;
                case ItemClass.Zone.CommercialLow:
                    return 3;
                case ItemClass.Zone.CommercialHigh:
                    return 4;
                case ItemClass.Zone.Industrial:
                    return 5;
                case ItemClass.Zone.Office:
                    return 6;
                case ItemClass.Zone.None:
                    return 0;
                case ItemClass.Zone.Unzoned:
                    return 99;
                default:
                    return 0;
            }
        }

        public static ItemClass.Zone IdtoZone(int id)
        {
            switch (id)
            {
                case 1:
                    return ItemClass.Zone.ResidentialLow;
                case 2:
                    return ItemClass.Zone.ResidentialHigh;
                case 3:
                    return ItemClass.Zone.CommercialLow;
                case 4:
                    return ItemClass.Zone.CommercialHigh;
                case 5:
                    return ItemClass.Zone.Industrial;
                case 6:
                    return ItemClass.Zone.Office;
                case 99:
                    return ItemClass.Zone.Unzoned;
                default:
                    return ItemClass.Zone.None;
            }
        }

        public string Serialize()
        {
            // return "{" + $"\"x\":{x}, \"z\": {z}, \"land_use\": \"{land_use}\"" + "}";
            return $"[{x},{z},{zoneToInt()}]";
        }
    }

    public class Network
    {
        public List<Block> blocks;
        public List<Edge> edges;
        public List<Node> nodes;

        public Network(List<Block> _blocks, List<Edge> _edges, List<Node> _nodes)
        {
            blocks = _blocks;
            edges = _edges;
            nodes = _nodes;
        }

        public string Serialize()
        {
            var blocksString = String.Join(",", blocks.Select(b => b.Serialize()).ToArray());
            var edgesString = String.Join(",", edges.Select(e => e.Serialize()).ToArray());
            var nodesString = String.Join(",", nodes.Select(n => n.Serialize()).ToArray());

            return "{" + $"\"nodes\":[{nodesString}], \"edges\":[{edgesString}], \"blocks\":[{blocksString}]" + "}";
        }

    }

}