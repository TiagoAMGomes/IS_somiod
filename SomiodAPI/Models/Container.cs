using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SomiodAPI.Models
{
	public class Container
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime Creation_dt { get; set; }

		public List<Data> DataItems { get; set; }
		public List<Subscription> Subscriptions { get; set; }
	}
}