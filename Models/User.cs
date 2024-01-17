using System;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy.Models
{
	public class User
	{
		public long Id { get; set; }
        public string name { get; set; }
        [Required]
        public string username { get; set; }
        public string mail { get; set; }
        [Required]
        public string password { get; set; }
    
	}
}


