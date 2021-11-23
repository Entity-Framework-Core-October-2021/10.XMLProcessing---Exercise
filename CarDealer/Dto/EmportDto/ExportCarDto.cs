using System.Collections.Generic;
using System.Xml.Serialization;

namespace CarDealer.Dto.EmportDto
{
    [XmlType("car")]
    public class ExportCarDto
    {
        [XmlAttribute("make")]
        public string Make { get; set; }

        [XmlAttribute("model")]
        public string Model { get; set; }

        [XmlAttribute("travelled-distance")]
        public long TravelledDistance { get; set; }

        [XmlArray(ElementName = "parts")]
        public List<ExportCarPartsDto> Parts { get; set; }
    }
}