using AnalysisUK.Tinamous.Membership.Messaging.Dtos.User;

namespace AnalysisUK.Tinamous.Media.BL
{
    public interface IMembershipService 
    {
        //Task<User> LoadAsync(Guid accountId, Guid userId);
        void UserUpdated(UserDto userDto);        
    }
}