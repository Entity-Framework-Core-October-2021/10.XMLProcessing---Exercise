using System.Xml.Serialization;

namespace CarDealer.Dto.EmportDto
{
    [XmlType("sale")]
    public class ExportSalesWithAppliedDiscountDto
    {
        [XmlElement(ElementName = "car")]
        public ExportCarDto Car { get; set; }

        [XmlElement("discount")]
        public decimal Discount { get; set; }

        [XmlElement("customer-name")]
        public string CustomerName { get; set; }

        [XmlElement("price")]
        public decimal Price { get; set; }

        [XmlAttribute("price-withdiscount")]
        public decimal PriceWithDiscount { get; set; }
    }
}
