using AutoMapper;
using CarDealer.Data;
using CarDealer.Dto.ImportDto;
using CarDealer.Models;
using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CarDealer.Dto.EmportDto;
using System.Text;

namespace CarDealer
{
    public class StartUp
    {
        private static readonly IMapper mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CarDealerProfile>();
        }));

        public static void Main(string[] args)
        {
            CarDealerContext dbContext = new CarDealerContext();

            //ResetDatabase(dbContext);

            ////09. Import Suppliers
            //string suppliersAsXml = File.ReadAllText("../../../Datasets/suppliers.xml");
            //Console.WriteLine(ImportSuppliers(dbContext, suppliersAsXml));

            ////10. Import Parts
            //string partsAsXml = File.ReadAllText("../../../Datasets/parts.xml");
            //Console.WriteLine(ImportParts(dbContext, partsAsXml));

            ////11. Import Cars
            //string carsAsXml = File.ReadAllText("../../../Datasets/cars.xml");
            //Console.WriteLine(ImportCars(dbContext, carsAsXml));

            ////12. Import Customers
            //string customersAsXml = File.ReadAllText("../../../Datasets/customers.xml");
            //Console.WriteLine(ImportCustomers(dbContext, customersAsXml));

            ////13. Import Sales -> with AutoMapper
            //string salesAsXml = File.ReadAllText("../../../Datasets/sales.xml");
            //Console.WriteLine(ImportSales(dbContext, salesAsXml));

            //14. Export Cars With Distance
            //Console.WriteLine(GetCarsWithDistance(dbContext));

            //15. Export Cars From Make BMW
            //Console.WriteLine(GetCarsFromMakeBmw(dbContext));

            //16. Export Local Suppliers
            //Console.WriteLine(GetLocalSuppliers(dbContext));

            //17. Export Cars With Their List Of Parts
            //Console.WriteLine(GetCarsWithTheirListOfParts(dbContext));

            //18. Export Total Sales By Customer
            //Console.WriteLine(GetTotalSalesByCustomer(dbContext));

            //19. Export Sales With Applied Discount
            Console.WriteLine(GetSalesWithAppliedDiscount(dbContext));
        }

        public static string ImportSuppliers(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Suppliers");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportSupplierDto[]), xmlRoot);

            using StringReader stringReader = new StringReader(inputXml);

            ImportSupplierDto[] dtos = (ImportSupplierDto[])xmlSerializer.Deserialize(stringReader);

            ICollection<Supplier> suppliers = new HashSet<Supplier>();

            foreach (ImportSupplierDto supplierDto in dtos)
            {
                Supplier s = new Supplier()
                {
                    Name = supplierDto.Name,
                    IsImporter = bool.Parse(supplierDto.IsImporter)
                };

                suppliers.Add(s);
            }

            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            return $"Successfully imported {suppliers.Count}";
        }

        public static string ImportParts(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Parts");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ImportPartDto>), xmlRoot);

            using StringReader stringReader = new StringReader(inputXml);

            var dtos = (List<ImportPartDto>)xmlSerializer.Deserialize(stringReader);

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ImportPartDto, Part>();
            });

            IMapper mapper = configuration.CreateMapper();

            var parts = mapper.Map<List<Part>>(dtos);

            int[] suppliers = context.Suppliers.Select(s => s.Id).ToArray();

            parts = parts.Where(p => suppliers.Contains(p.SupplierId)).ToList();

            context.Parts.AddRange(parts);
            context.SaveChanges();

            return $"Successfully imported {parts.Count}";
        }

        public static string ImportCars(CarDealerContext context, string inputXml)
        {
            var xmlSerializer = new XmlSerializer(typeof(List<ImportCarDto>), new XmlRootAttribute("Cars"));

            var carsDto = (List<ImportCarDto>)xmlSerializer.Deserialize(new StringReader(inputXml));

            var partsIdInDb = context.Parts.Select(c => c.Id).ToList();

            var cars = new List<Car>();

            var carParts = new List<PartCar>();

            foreach (var carDTO in carsDto)
            {
                var configuration = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<ImportCarDto, Car>();
                });

                IMapper mapper = configuration.CreateMapper();

                var newCar = mapper.Map<Car>(carDTO);

                var parts = carDTO.PartIds
                   .Where(pdto => partsIdInDb.Contains(pdto.Id))
                   .Select(p => p.Id)
                   .Distinct()
                   .ToList();

                cars.Add(newCar);

                foreach (var partId in parts)
                {
                    var newPartCar = new PartCar
                    {
                        PartId = partId,
                        Car = newCar
                    };

                    carParts.Add(newPartCar);
                }
            }

            context.Cars.AddRange(cars);

            context.PartCars.AddRange(carParts);

            context.SaveChanges();

            return $"Successfully imported {cars.Count}";
        }

        public static string ImportCustomers(CarDealerContext context, string inputXml)
        {
            XmlRootAttribute xmlRoot = new XmlRootAttribute("Customers");
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportCustomerDto[]), xmlRoot);

            using StringReader stringReader = new StringReader(inputXml);

            ImportCustomerDto[] dtos = (ImportCustomerDto[])xmlSerializer.Deserialize(stringReader);

            ICollection<Customer> customers = new HashSet<Customer>();

            foreach (ImportCustomerDto customerDto in dtos)
            {
                Customer c = new Customer()
                {
                    Name = customerDto.Name,
                    BirthDate = DateTime.Parse(customerDto.BirthDate, CultureInfo.InvariantCulture),
                    IsYoungDriver = bool.Parse(customerDto.IsYoungDriver)
                };

                customers.Add(c);
            }

            context.Customers.AddRange(customers);
            context.SaveChanges();

            return $"Successfully imported {customers.Count}";
        }

        public static string ImportSales(CarDealerContext context, string inputXml)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImportSaleDto[]), new XmlRootAttribute("Sales"));

            ImportSaleDto[] dtos = (ImportSaleDto[])xmlSerializer.Deserialize(new StringReader(inputXml));

            var sales = mapper.Map<Sale[]>(dtos)
                                                .Where(s => context.Cars.Any(c => c.Id == s.CarId))
                                                .ToList();

            context.Sales.AddRange(sales);
            context.SaveChanges();

            return $"Successfully imported {sales.Count}";
        }

        public static string GetCarsWithDistance(CarDealerContext context)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCarsWithDistanceDto[]), new XmlRootAttribute("cars"));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            StringBuilder sb = new StringBuilder();
            using StringWriter stringWriter = new StringWriter(sb);

            ExportCarsWithDistanceDto[] carsDtos = context.Cars
                                                               .Where(c => c.TravelledDistance > 2000000)
                                                               .OrderBy(c => c.Make)
                                                               .ThenBy(c => c.Model)
                                                               .Take(10)
                                                               .Select(c => new ExportCarsWithDistanceDto
                                                               {
                                                                   Make = c.Make,
                                                                   Model = c.Model,
                                                                   TravelledDistance = c.TravelledDistance.ToString()
                                                               })
                                                               .ToArray();

            xmlSerializer.Serialize(stringWriter, carsDtos, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetCarsFromMakeBmw(CarDealerContext context)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportCarsBMWDto[]), new XmlRootAttribute("cars"));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            StringBuilder sb = new StringBuilder();
            using StringWriter stringWriter = new StringWriter(sb);

            var bmwDtos = context.Cars
                                      .Where(x => x.Make == "BMW")
                                      .OrderBy(c => c.Model)
                                      .ThenByDescending(c => c.TravelledDistance)
                                      .Select(c => new ExportCarsBMWDto
                                      {
                                          Id = c.Id,
                                          Model = c.Model,
                                          TraveledDistance = c.TravelledDistance
                                      })
                                      .ToArray();

            xmlSerializer.Serialize(stringWriter, bmwDtos, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var stringBuilder = new StringBuilder();

            var cars = context
                .Cars
                .OrderByDescending(c => c.TravelledDistance)
                .ThenBy(c => c.Model)
                .Select(c => new ExportCarDto
                {

                    Make = c.Make,
                    Model = c.Model,
                    TravelledDistance = c.TravelledDistance,
                    Parts = c.PartCars
                    .Select(x => new ExportCarPartsDto
                    {
                        Name = x.Part.Name,
                        Price = x.Part.Price
                    })
                    .OrderByDescending(p => p.Price)
                    .ToList()
                })
                .Take(5)
                .ToList();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ExportCarDto>), new XmlRootAttribute("cars"));

            var namespaces = new XmlSerializerNamespaces();

            namespaces.Add(string.Empty, string.Empty);

            xmlSerializer.Serialize(new StringWriter(stringBuilder), cars, namespaces);

            return stringBuilder.ToString().TrimEnd();
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportLocalSuppliersDto[]), new XmlRootAttribute("suppliers"));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            StringBuilder sb = new StringBuilder();
            using StringWriter streamWriter = new StringWriter(sb);

            var supplierDtos = context.Suppliers
                                                .Where(s => !s.IsImporter)
                                                .Select(s => new ExportLocalSuppliersDto
                                                {
                                                    Id = s.Id,
                                                    Name = s.Name,
                                                    PartsCount = s.Parts.Count
                                                })
                                                .ToArray();

            xmlSerializer.Serialize(streamWriter, supplierDtos, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(ExportTotalSalesByCustomerDto[]), new XmlRootAttribute("customers"));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            StringBuilder sb = new StringBuilder();
            using StringWriter stringWriter = new StringWriter(sb);

            var salesByCustomerDtos = context.Customers
                                                      .Where(c => c.Sales.Any())
                                                      .Select(c => new ExportTotalSalesByCustomerDto
                                                      {
                                                          FullName = c.Name,
                                                          BoughtCars = c.Sales.Count,
                                                          SpentMoney = c.Sales.Sum(s => s.Car.PartCars.Sum(x => x.Part.Price))
                                                      })
                                                      .OrderByDescending(c => c.SpentMoney)
                                                      .ToArray();

            xmlSerializer.Serialize(stringWriter, salesByCustomerDtos, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ExportSalesWithAppliedDiscountDto>), new XmlRootAttribute("sales"));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            StringBuilder sb = new StringBuilder();
            using StringWriter stringWriter = new StringWriter(sb);

            var sales = context.Sales
                                     .Take(10)
                                     .Select(s => new ExportSalesWithAppliedDiscountDto
                                     {
                                         Car = new ExportCarDto
                                         {
                                             Make = s.Car.Make,
                                             Model = s.Car.Model,
                                             TravelledDistance = s.Car.TravelledDistance
                                         },
                                         CustomerName = s.Customer.Name,
                                         Discount = s.Discount,
                                         Price = s.Car.PartCars.Sum(x => x.Part.Price),
                                         PriceWithDiscount = s.Car.PartCars.Sum(p => p.Part.Price) -
                                                            s.Car.PartCars.Sum(p => p.Part.Price) * s.Discount / 100
                                     })
                                     .ToList();

            xmlSerializer.Serialize(stringWriter, sales, namespaces);

            return sb.ToString().TrimEnd();
        }
        private static void ResetDatabase(CarDealerContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }
    }
}