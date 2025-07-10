using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventWebsite.Controllers.Api
{
    // A simple class to hold our diagnostic information
    public class DebugInfo
    {
        public string? Name { get; set; }
        public string? Alias { get; set; }
    }

    public class EventsController : UmbracoApiController
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public EventsController(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return StatusCode(500, "Could not get Umbraco context.");
            }

            // Using the Key for your "Events" page from the content tree.
            var eventListingPageKey = Guid.Parse("511d05c9-fa44-474e-8a3b-fd63ae137ce1");
            var eventsRoot = umbracoContext.Content?.GetById(eventListingPageKey);

            if (eventsRoot == null)
            {
                return NotFound("Event Listing Page with the specified Key was not found.");
            }

            // Get all direct children of the listing page.
            var children = eventsRoot.Children<IPublishedContent>();

            if (children == null || !children.Any())
            {
                // This will tell us if the page is found but has no children.
                return Ok(new { message = "Found the Event Listing Page, but it has no children." });
            }

            // Create a list of event details for each child page.
            var events = children.Select(c => {
                var imageContent = c.Value<IPublishedContent>("featuredImage");
                return new {
                    title = c.Value<string>("eventTitle"),
                    date = c.Value<DateTime?>("eventDateTime"),
                    imageUrl = imageContent != null ? imageContent.Url() : null,
                    description = c.Value<string>("description"),
                    categories = c.Value<IEnumerable<string>>("eventCategory"),
                    latitude = c.Value<string>("latitude"),
                    longitude = c.Value<string>("longitude")
                };
            }).ToList();

            return Ok(events);
        }
    }
}
