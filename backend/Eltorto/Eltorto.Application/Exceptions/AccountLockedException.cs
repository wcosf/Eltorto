namespace Eltorto.Application.Exceptions;

public class AccountLockedException : Exception
{
    public AccountLockedException()
        : base("Account is locked out")
    {
    }
}
