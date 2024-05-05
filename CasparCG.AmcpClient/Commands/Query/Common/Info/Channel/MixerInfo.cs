﻿//////////////////////////////////////////////////////////////////////////////////
//
// Author: Sase
// Email: sase@stilsoft.net
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
//////////////////////////////////////////////////////////////////////////////////


using System.Xml.Serialization;

namespace CasparCg.AmcpClient.Commands.Query.Common.Info.Channel
{
    [XmlRoot(ElementName = "mixer")]
    public class MixerInfo
    {
        [XmlElement(ElementName = "mix-time")]
        public int MixTime { get; set; }
    }
}