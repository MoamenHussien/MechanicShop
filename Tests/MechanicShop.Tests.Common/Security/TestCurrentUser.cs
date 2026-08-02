using MechanicShop.Infrastructure.Identity;

namespace MechanicShop.Tests.Common.Security;

public class TestCurrentUser : IUser
{
    private AppUser? _currentUser;

    public void Returns(AppUser currentUser)
    {
        _currentUser = currentUser;
    }

    Guid? IUser.Id =>
    _currentUser!.Id == Guid.Empty
        ? UserFactory.CreateUser().Id
        : _currentUser.Id;
}
