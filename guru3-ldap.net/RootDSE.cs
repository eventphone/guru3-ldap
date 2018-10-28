using System.Collections.Generic;
using zivillian.ldap.Attributes;
using zivillian.ldap.ObjectClasses;

namespace eventphone.guru3.ldap
{
    public class RootDSE : OrganizationObjectClass, IRootDSEObjectClass, IDcObjectObjectClass
    {
        public RootDSE()
        {
            ObjectClass.Entries.Add("dcObject");
            Dc = new DcAttribute();
        }

        public DcAttribute Dc { get; }

        public AltServerAttribute AltServer { get; set; }
        
        public NamingContextsAttribute NamingContexts { get; set; }
        
        public SupportedControlAttribute SupportedControl { get; set; }
        
        public SupportedExtensionAttribute SupportedExtension { get; set; }
        
        public SupportedFeaturesAttribute SupportedFeatures { get; set; }
        
        public SupportedLDAPVersionAttribute SupportedLDAPVersion { get; set; }
        
        public SupportedSASLMechanismsAttribute SupportedSASLMechanisms { get; set; }

        protected override void GetAttributes(List<AbstractLdapAttribute> result)
        {
            result.Add(Dc);
            result.Add(AltServer);
            result.Add(NamingContexts);
            result.Add(SupportedControl);
            result.Add(SupportedExtension);
            result.Add(SupportedFeatures);
            result.Add(SupportedLDAPVersion);
            result.Add(SupportedSASLMechanisms);

            base.GetAttributes(result);
        }
    }
}