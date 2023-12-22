using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace SomiodAPI.Models
{
	public class Application
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime Creation_dt { get; set; }

		public List<Container> Containers { get; set; }
	}
}