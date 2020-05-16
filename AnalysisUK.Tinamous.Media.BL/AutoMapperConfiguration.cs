
using AnalysisUK.Tinamous.Media.Domain.Documents;
using AnalysisUK.Tinamous.Media.Messaging.Dtos;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;
using AutoMapper;

namespace AnalysisUK.Tinamous.Media.BL
{
    public static class AutoMapperConfiguration
    {
        public static void Configure()
        {
            MapCommonMessaging();
            MapMembershipUser();
            MapMediaDtos();
        }

        private static void MapCommonMessaging()
        {
            Mapper.CreateMap<LocationDto, LocationDetails>();
            Mapper.CreateMap<LocationDetails, LocationDto>();
        }

        private static void MapMembershipUser()
        {
            Mapper.CreateMap<Membership.Messaging.Dtos.User.UserDto, User>();
        }

        private static void MapMediaDtos()
        {
            Mapper.CreateMap<MediaItemStorageLocationDto, MediaItemStorageLocation>();
            Mapper.CreateMap<MediaItemStorageLocation, MediaItemStorageLocationDto>();

            Mapper.CreateMap<MediaItem, MediaItemDto>()
                .ForMember(x => x.User, options => options.MapFrom(y => new UserSummaryDto {UserId = y.UserId, AccountId = y.AccountId}))
                .ForMember(x => x.DateAdded, options => options.MapFrom(y => y.DateAdded.ToUniversalTime()))
                .ForMember(x => x.LastUpdated, options => options.MapFrom(y => y.LastUpdated.ToUniversalTime()));
        }
    }
}