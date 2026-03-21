using AutoMapper;
using Eltorto.Application.DTOs;
using Eltorto.Domain.Entities;

namespace Eltorto.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Category mappings
        CreateMap<Category, CategoryDto>();
        CreateMap<Category, CategoryWithCakesDto>();
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Cake mappings
        CreateMap<Cake, CakeListDto>()
            .ForMember(dest => dest.FillingName,
                opt => opt.MapFrom(src => src.Filling != null ? src.Filling.Name : null));
        CreateMap<Cake, CakeDetailDto>();
        CreateMap<CreateCakeDto, Cake>();
        CreateMap<UpdateCakeDto, Cake>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Filling mappings
        CreateMap<Filling, FillingDto>();
        CreateMap<Filling, FillingWithCakesDto>();
        CreateMap<CreateFillingDto, Filling>();
        CreateMap<UpdateFillingDto, Filling>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Testimonial mappings
        CreateMap<Testimonial, TestimonialDto>();
        CreateMap<Testimonial, TestimonialListDto>();
        CreateMap<CreateTestimonialDto, Testimonial>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(_ => false))
            .ForMember(dest => dest.Response, opt => opt.Ignore());
        CreateMap<UpdateTestimonialDto, Testimonial>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CakeName,
                opt => opt.Ignore())
            .ForMember(dest => dest.FillingName,
                opt => opt.Ignore());

        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "New"));

        // Page mappings
        CreateMap<Page, PageDto>();
        CreateMap<UpdatePageDto, Page>()
            .ForMember(dest => dest.Slug, opt => opt.Ignore())
            .ForMember(dest => dest.ContentBlocks, opt => opt.Ignore());

        // ContentBlock mappings
        CreateMap<ContentBlock, ContentBlockDto>();
        CreateMap<CreateContentBlockDto, ContentBlock>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PageId, opt => opt.Ignore());
        CreateMap<UpdateContentBlockDto, ContentBlock>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PageId, opt => opt.Ignore());

        // Slider mappings
        CreateMap<SliderItem, SliderItemDto>();
        CreateMap<CreateSliderItemDto, SliderItem>();
        CreateMap<UpdateSliderItemDto, SliderItem>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // ContactSettings mappings
        CreateMap<ContactSettings, ContactSettingsDto>();
        CreateMap<UpdateContactSettingsDto, ContactSettings>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}