using AutoMapper;
using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain;

namespace Restaurant_Delivery.API.Configuration.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Dish mappings
            CreateMap<Dish, DishDto>().ReverseMap();

            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Dishes, opt => opt.MapFrom(src => src.Dishes)); // Map nested DishBasket to DishBasketDto
            CreateMap<OrderDto, Order>();

            CreateMap<OrderCreateDto, Order>()
                .ForMember(dest => dest.Dishes, opt => opt.Ignore()); // Dishes will be handled separately

            CreateMap<Order, OrderInfoDto>().ReverseMap();

            // DishBasket mappings
            CreateMap<DishBasket, DishBasketDto>().ReverseMap();

            // User mappings
            CreateMap<User, UserDto>().ReverseMap();

            // DishPagedList mappings
            CreateMap<DishPagedList, DishPagedListDto>().ReverseMap();
        }
    }
}