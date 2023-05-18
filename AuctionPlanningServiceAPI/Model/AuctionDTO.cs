using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;

namespace AuctionPlanningServiceAPI.Model
{
    public class AuctionDTO
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ArticleID { get; set; }

        public AuctionDTO()
        {
        }
    }
}

