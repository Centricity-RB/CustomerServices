using System.ComponentModel;

namespace Core.Enums
{
    public enum SignInType
    {
        [Description("sms")]
        PhoneNumber = 1,
        [Description("email")]
        Email
    }
}
