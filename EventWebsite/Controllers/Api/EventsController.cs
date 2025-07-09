// --- Step 1: Add necessary using statements ---
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions; // Required for .Value<T>() and other helpful extensions

namespace EventWebsite.Controllers.Api // Corrected namespace to match your project
{
    // --- Step 2: Define the API model for your Event ---
    public class EventApiModel
    {
        public int Id { get; set; }
        public string? EventTitle { get; set; }
        public DateTime EventDateTime { get; set; }
        public string? FeaturedImageUrl { get; set; }
        public IEnumerable<string>? EventCategories { get; set; }
        public string? Longitude { get; set; }
        public string? Latitude { get; set; }
        public string? Url { get; set; }
    }

    // --- Step 3: Create your Events API Controller ---
    public class EventsController : UmbracoApiController
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        // --- Step 4: Inject Umbraco services ---
        public EventsController(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        // --- Step 5: Create the API Action Method ---
        [HttpGet]
        public IActionResult GetAll()
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return StatusCode(500, "Could not get Umbraco context.");
            }

            var eventsRoot = umbracoContext.Content?.GetAtRoot().FirstOrDefault()?
                .DescendantsOrSelf<IPublishedContent>().FirstOrDefault(x => x.ContentType.Alias == "eventListingPage");

            if (eventsRoot == null)
            {
                return NotFound("Event Listing Page not found. Make sure a content node with alias 'eventListingPage' exists.");
            }

            // --- THIS IS THE FIX for the warning ---
            // Added a '?' to safely handle cases where there are no children, preventing the warning.
            var eventNodes = eventsRoot.Children<IPublishedContent>()?.Where(x => x.ContentType.Alias == "eventPage");

            if (eventNodes == null || !eventNodes.Any())
            {
                return Ok(new List<EventApiModel>()); // Return an empty list if no events exist
            }

            // --- Step 6: Map the Umbraco content to your API model ---
            var eventApiModels = eventNodes.Select(node =>
            {
                var featuredImage = node.Value<IPublishedContent>("featuredImage");

                return new EventApiModel
                {
                    Id = node.Id,
                    EventTitle = node.Value<string>("eventTitle"),
                    EventDateTime = node.Value<DateTime>("eventDateTime"),
                    FeaturedImageUrl = featuredImage?.Url(mode: UrlMode.Absolute),
                    EventCategories = node.Value<IEnumerable<string>>("eventCategory"),
                    Longitude = node.Value<string>("longitude"),
                    Latitude = node.Value<string>("latitude"),
                    Url = node.Url(mode: UrlMode.Absolute)
                };
            }).ToList();

            return Ok(eventApiModels);
        }
    }
}
