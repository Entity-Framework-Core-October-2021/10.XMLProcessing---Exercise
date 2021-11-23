using AutoMapper;
using CarDealer.Dto.ImportDto;
using CarDealer.Models;

namespace CarDealer
{
    public class CarDealerProfile : Profile
    {
        public CarDealerProfile()
        {
            CreateMap<ImportSaleDto, Sale>();
        }
    }
}
