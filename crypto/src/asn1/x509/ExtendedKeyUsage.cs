using System;
using System.Collections;

using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Asn1.X509
{
    /**
     * The extendedKeyUsage object.
     * <pre>
     *      extendedKeyUsage ::= Sequence SIZE (1..MAX) OF KeyPurposeId
     * </pre>
     */
    public class ExtendedKeyUsage : Asn1Encodable
    {
        public static ExtendedKeyUsage GetInstance(Asn1TaggedObject obj, bool explicitly)
        {
            return GetInstance(Asn1Sequence.GetInstance(obj, explicitly));
        }

        public static ExtendedKeyUsage GetInstance(object obj)
        {
            if (obj is ExtendedKeyUsage)
                return (ExtendedKeyUsage)obj;
            if (obj is X509Extension)
                return GetInstance(X509Extension.ConvertValueToObject((X509Extension)obj));
            if (obj == null)
                return null;
            return new ExtendedKeyUsage(Asn1Sequence.GetInstance(obj));
        }

        public static ExtendedKeyUsage FromExtensions(X509Extensions extensions)
        {
            return GetInstance(X509Extensions.GetExtensionParsedValue(extensions, X509Extensions.ExtendedKeyUsage));
        }

        internal readonly IDictionary usageTable = Platform.CreateHashtable();
        internal readonly Asn1Sequence seq;

        private ExtendedKeyUsage(Asn1Sequence seq)
        {
            this.seq = seq;

            foreach (Asn1Encodable element in seq)
            {
                DerObjectIdentifier oid = DerObjectIdentifier.GetInstance(element);

                this.usageTable[oid] = oid;
            }
        }

        public ExtendedKeyUsage(params KeyPurposeID[] usages)
        {
            this.seq = new DerSequence(usages);

            foreach (KeyPurposeID usage in usages)
            {
                this.usageTable[usage] = usage;
            }
        }

        public ExtendedKeyUsage(IEnumerable usages)
        {
            Asn1EncodableVector v = new Asn1EncodableVector();

            foreach (object usage in usages)
            {
                DerObjectIdentifier oid = DerObjectIdentifier.GetInstance(usage);

                v.Add(oid);
                this.usageTable[oid] = oid;
            }

            this.seq = new DerSequence(v);
        }

        public bool HasKeyPurposeId(KeyPurposeID keyPurposeId)
        {
            return usageTable.Contains(keyPurposeId);
        }

        /**
         * Returns all extended key usages.
         * The returned ArrayList contains DerObjectIdentifier instances.
         * @return An ArrayList with all key purposes.
         */
        public IList GetAllUsages()
        {
            return Platform.CreateArrayList(usageTable.Values);
        }

        public int Count
        {
            get { return usageTable.Count; }
        }

        public override Asn1Object ToAsn1Object()
        {
            return seq;
        }
    }
}
