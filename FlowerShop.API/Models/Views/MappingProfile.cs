using AutoMapper;
using FlowerShop.API.Models.Entities;

namespace FlowerShop.API.Models.Views;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        #region Auth
        CreateMap<RegisterInputResource, Customer>();
        CreateMap<User, LoginOutputResource>();
        CreateMap<Customer, LoginOutputResource>();
        #endregion

        #region Role
        CreateMap<RoleInputResource, Role>();
        CreateMap<Role, RoleOutputResource>();
        CreateMap<Permission, PermissionOutputResource>();
        #endregion

        #region Product
        CreateMap<ProductInputResource, Product>();
        CreateMap<Product, ProductOutputResource>();
        #endregion

        #region ProductCategory
        CreateMap<ProductCategoryInputResource, ProductCategory>();
        CreateMap<ProductCategory, ProductCategoryOutputResource>();
        #endregion

        #region ProductTag
        CreateMap<ProductTagInputResource, ProductTag>();
        CreateMap<ProductTag, ProductTagOutputResource>();
        #endregion

        #region Media
        CreateMap<Media, MediaOutputResource>();
        #endregion

        #region Orders
        CreateMap<Order, OrderOutputResource>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.DeliveryMode, opt => opt.MapFrom(src => src.DeliveryMode.ToString()))
            .ForMember(dest => dest.ShippingAddress, opt => opt.Ignore()); 

        CreateMap<OrderItem, OrderItemOutputResource>();
        #endregion

        #region GoogleUserInfo
        CreateMap<GoogleUserInfoResource, Customer>()
            .ForMember(dest => dest.ProviderAccountId, opt => opt.MapFrom(src => src.Sub))
            .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Picture))
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(_ => "GOOGLE"))
            .ForMember(dest => dest.EmailVerifiedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .AfterMap((_, dest) =>
            {
                dest.CreatedAt = dest.CreatedAt == default ? DateTime.UtcNow : dest.CreatedAt;
                dest.UpdatedAt = DateTime.UtcNow;
            });
        #endregion

        #region
        CreateMap<Customer, CustomerOutputResource>();
        CreateMap<CustomerAddress, CustomerAddressOutputResource>();
        CreateMap<CustomerInputResource, Customer>();
        #endregion

        #region 
        CreateMap<CustomerAddressInputResource, CustomerAddress>();
        CreateMap<CustomerAddress, CustomerAddressOutputResource>();
        #endregion
    }
}
