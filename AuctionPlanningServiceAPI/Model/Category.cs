using System;
using MongoDB.Bson.Serialization.Attributes;

namespace AuctionPlanningServiceAPI.Model
{
	public class Category
	{
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
        public string ItemDescription { get; set; }
		public DateTime AuctionDate { get; set; }

        public Category()
		{
		}
	}
}

