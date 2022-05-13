using System;

namespace Org.BouncyCastle.Security.Certificates
{
    [Serializable]
    public class CertificateEncodingException : CertificateException
    {
        public CertificateEncodingException() : base() { }
        public CertificateEncodingException(string msg) : base(msg) { }
        public CertificateEncodingException(string msg, Exception e) : base(msg, e) { }
    }
}
