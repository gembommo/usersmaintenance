using System;

namespace MyState.WebApplication.DataStore.Helpers
{
    public class CodeGenerator
    {
        public virtual string GeneratePhoneVerificationCode(string deviceType, string deviceId, string phone)
        {
            return String.Format("{0}_12345", phone);
        }
    }
}
