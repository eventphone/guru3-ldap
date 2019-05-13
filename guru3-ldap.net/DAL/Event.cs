using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eventphone.guru3.ldap.DAL
{
    [Table("core_event")]
    public class Event
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [MaxLength(128)]
        public string Name { get; set; }

        [Column("location")]
        [MaxLength(128)]
        public string Location { get; set; }

        [Column("registrationStart")]
        public DateTime? RegistrationStart { get; set; }

        [Column("end")]
        public DateTime? End { get; set; }

        [Column("descriptionDE")]
        public string DescriptionDe { get; set; }

        [Column("isPermanentAndPublic")]
        public bool IsPermanentAndPublic { get; set; }
    }
}