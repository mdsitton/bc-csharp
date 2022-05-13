using System;

namespace Org.BouncyCastle.Pkix
{
    [Serializable]
    public class PkixNameConstraintValidatorException : Exception
    {
        public PkixNameConstraintValidatorException(String msg) : base(msg)
        {
        }
    }
}
