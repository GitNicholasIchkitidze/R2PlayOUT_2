//////////////////////////////////////////////////////////////////////////////////
//
// Author: Sase
// Email: sase@stilsoft.net
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
//////////////////////////////////////////////////////////////////////////////////


using System;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using CasparCg.AmcpClient.Abstracts.Command;

namespace CasparCg.AmcpClient.Common
{
    public class AmcpPacket : ICommandPacket
    {
        private readonly string _data;

        public bool IncludePacketId { get; set; }
        public string PacketId { get; }

        public byte[] Data
        {
            get
            {
                var packetId = "";

                if (IncludePacketId)
                    packetId = $"REQ {PacketId} ";

                if (Debugger.IsAttached)
                {
                    Console.WriteLine(DateTime.Now + ": " + _data + Environment.NewLine);
                }
                return Encoding.UTF8.GetBytes($"{packetId}{_data}\r\n");
            }
        }

        public string CommandName { get; set; }


        public AmcpPacket(string data)
        {
            PacketId = Guid.NewGuid().ToString();
            _data = data;
        }
    }
}