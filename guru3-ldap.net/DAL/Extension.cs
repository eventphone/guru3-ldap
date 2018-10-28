using System.ComponentModel.DataAnnotations.Schema;

namespace eventphone.guru3.ldap.DAL
{
    [Table("core_extension")]
    public class Extension
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("extension")]
        public string Number { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("location")]
        public string Location { get; set; }

        [Column("inPhonebook")]
        public bool InPhonebook { get; set; }

        [Column("event_id")]
        public int EventId { get; set; }

        [ForeignKey(nameof(EventId))]
        public Event Event { get; set; }
    }
}