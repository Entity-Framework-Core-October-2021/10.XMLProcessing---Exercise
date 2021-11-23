using System.Collections.Generic;
using System.Xml.Serialization;

namespace CarDealer.Dto.EmportDto
{
    [XmlType("car")]
    public class ExportCarsWithPartsDto
    {
        [XmlAttribute("make")]
        public string Make { get; set; }

        [XmlArray(ElementName = "parts")]
        public ICollection<ExportCarPartsDto> Parts { get; set; }
    }
}
