﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoCoordinatePortable;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NowaiterApi.Interfaces;
using NowaiterApi.Interfaces.Repository;
using NowaiterApi.Interfaces.Service;
using NowaiterApi.Models.ViewModel;


namespace NowaiterApi.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IRestaurantRepository _restaurantRepository;
        private readonly IStatusRepository _statusRepository;

        public SearchController(IRestaurantService restaurantService, IRestaurantRepository restaurantRepository,
            IStatusRepository statusRepository)
        {
            _restaurantRepository = restaurantRepository;
            _statusRepository = statusRepository;
            _restaurantService = restaurantService;

        }

        /**
         * Method returns all the restaurants stored in the database
         */
        [Route("list")]
        [HttpGet]
        public IActionResult List()
        {
            return Ok(_restaurantRepository.GetAllRestaurants());
        }

        /**
         * Get method to retrieve the restaurant information using the name 
         */
        [Route("{name}")]
        [HttpGet]
        public IActionResult SearchByName(string name)
        {
            // If list is empty, there return error 
            if (!_restaurantService.RestaurantExist(name))
            {
                throw new ArgumentException($"Could not find any restaurants with the keyword {name}");
            }

            // Creating view model with relevant information for user
            List<RestaurantAvailabilityViewModel> currentAvailability = new List<RestaurantAvailabilityViewModel>();

            // Add each item into view model list 
            foreach (var restaurant in _restaurantRepository.GetRestaurantsListByName(name))
            {
                currentAvailability.Add(new RestaurantAvailabilityViewModel
                {
                    RestaurantId = restaurant.RestaurantId,
                    Name = restaurant.Name,
                    Phone = restaurant.Phone,
                    Address1 = restaurant.Address1,
                    DriveThru = _statusRepository.GetRestaurantStatusById(restaurant.RestaurantId).DriveThru,
                    InStore = _statusRepository.GetRestaurantStatusById(restaurant.RestaurantId).InStore
                });
            }

            // Return the result
            return Ok(currentAvailability);
        }

        /**
         * GET method to return the restaurants nearest when give lat and long coordinates. 
         */
        [Route("{lat, lng}")]
        [HttpGet]
        public IActionResult SearchByDistance(long lat, long lng)
        {

            // Return a 200 with the sorted list with the closest location to furthest 
            return Ok(_restaurantService.RestaurantSearchByDistance(_restaurantRepository.GetAllRestaurants(), lat, lng).OrderBy(x => x.DistanceTo));
        }

    }
}