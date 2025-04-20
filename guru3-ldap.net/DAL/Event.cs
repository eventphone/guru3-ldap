using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eventphone.guru3.ldap.DAL
{
    [Table("core_event")]
    public class Event
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("name")]
        [StringLength(128)]
        public string Name { get; set; }

        [Column("location_id")]
        public long? LocationId { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location Location { get; set; }

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