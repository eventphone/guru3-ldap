using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eventphone.guru3.ldap.DAL
{
    [Table("core_location")]
    public class Location
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("display_name")]
        [StringLength(128)]
        public string DisplayName{ get; set; }
    }
}