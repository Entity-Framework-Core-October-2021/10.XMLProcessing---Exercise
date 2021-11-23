using System.Xml.Serialization;

namespace CarDealer.Dto.ImportDto
{
    [XmlType("partId")]
    public class PartIdDto
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
    }
}