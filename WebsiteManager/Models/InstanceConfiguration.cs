using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebsiteManager.Models
{
    public class InstanceConfiguration
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        //public virtual ICollection<InstanceConfigurationMedia> InstanceConfigurationMedia { get; set; }
    }
}
