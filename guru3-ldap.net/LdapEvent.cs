﻿namespace eventphone.guru3.ldap
{
    public class LdapEvent
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }
    }

    public class LdapExtension
    {   
        public string Number { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public string Event { get; set; }
    }
}