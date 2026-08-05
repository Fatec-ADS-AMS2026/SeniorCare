using AutoMapper;
using SeniorCareManager.WebAPI.Objects.Dtos.Entities;
using SeniorCareManager.WebAPI.Objects.Models;

namespace SeniorCareManager.WebAPI.Objects.Dtos.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ProductGroupDTO, ProductGroup>().ReverseMap();
        CreateMap<ProductGroup, ProductGroupDTO>(); 
        CreateMap<ProductTypeDTO, ProductType>().ReverseMap();
        CreateMap<ProductType, ProductTypeDTO>();
        CreateMap<PositionDTO, Position>().ReverseMap();
        CreateMap<Position, PositionDTO>();
        CreateMap<Supplier, SupplierDTO>().ReverseMap();
        CreateMap<SupplierDTO, Supplier>();
        CreateMap<UnitOfMeasure, UnitOfMeasureDTO>().ReverseMap();
        CreateMap<UnitOfMeasureDTO, UnitOfMeasure>();
        CreateMap<HealthInsurancePlanDTO, HealthInsurancePlan>().ReverseMap();
        CreateMap<HealthInsurancePlan, HealthInsurancePlanDTO>();
        CreateMap<ManufacturerDTO, Manufacturer>().ReverseMap();
        CreateMap<Manufacturer, ManufacturerDTO>();
        CreateMap<CarrierDTO, Carrier>().ReverseMap();
        CreateMap<Carrier, CarrierDTO>();
        CreateMap<ReligionDTO, Religion>().ReverseMap();
        CreateMap<Religion, ReligionDTO>();

    }
}