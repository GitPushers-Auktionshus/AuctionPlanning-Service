﻿using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using RabbitMQ.Client;
using System.Net;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;
using MongoDB.Driver;
using AuctionPlanningServiceAPI.Model;
using MongoDB.Bson;
using AuctionPlanningServiceAPI.Service;

namespace AuctionPlanningServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AuctionPlanningServiceController : ControllerBase
{
    private readonly ILogger<AuctionPlanningServiceController> _logger;

    private readonly IConfiguration _config;

    private readonly IAuctionPlanningRepository _service;

    public AuctionPlanningServiceController(ILogger<AuctionPlanningServiceController> logger, IConfiguration config, IAuctionPlanningRepository service)
    {
        _logger = logger;
        _config = config;
        _service = service;
    }

    //POST - Adds a new auction
    [HttpPost("addAuction")]
    public async Task<Auction> AddAuction(AuctionDTO auctionDTO)
    {
        return await _service.AddAuction(auctionDTO);
    }

    //GET - Return a list of all auctions
    [HttpGet("getAll")]
    public async Task<List<Auction>> GetAllAuctions()
    {
        return await _service.GetAllAuctions();
    }

    //DELETE - Removes an auction
    [HttpDelete("deleteAuction/{id}")]
    public async Task<Auction> DeleteAuction(string id)
    {
        return await _service.DeleteAuction(id);
    }

    // GET - Retrieves an auction by ID
    [HttpGet("getAuction/{id}")]
    public async Task<Auction> GetAuction(string id)
    {
        return await _service.GetAuctionByID(id);
    }

    // PUT - Updates an auction
    [HttpPut("updateAuction/{id}")]
    public async Task<Auction> UpdateAuction(string id, AuctionDTO auctionDTO)
    {
        return await _service.UpdateAuction(id, auctionDTO);
    }


}



