using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;
using System.Collections.Generic;
using System.Linq;
using System;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.PublishedCache;

namespace EventWebsite.Controllers
{
    [Route("umbraco/api/eventsapi/v1")]
    public class EventsApiController : UmbracoApiController
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public EventsApiController(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        [HttpGet("getall")]
        public IActionResult GetAllEvents()
        {
            if (!_umbracoContextAccessor.TryGetUmbracoContext(out var umbracoContext))
            {
                return StatusCode(500, "Could not get Umbraco context.");
            }

            var contentCache = umbracoContext.Content;
            if (contentCache == null)
            {
                return NotFound("Published content cache not available.");
            }

            var eventsPage = contentCache.GetAtRoot()
                                         .FirstOrDefault()?
                                         .DescendantsOfType("eventListingPage")
                                         .FirstOrDefault();

            if (eventsPage == null)
            {
                return NotFound("Could not find the main 'Events' listing page.");
            }

            // --- THE FINAL FIX ---
            // We now use the correct alias "eventPage" that your test confirmed.
            var events = eventsPage.ChildrenOfType("eventPage")
                                   .Where(x => x.IsPublished())
                                   .OrderBy(x => x.Value<DateTime>("eventDateTime"));

            if (!events.Any())
            {
                return NotFound("No events found with the alias 'eventPage'.");
            }

            // Transform the events into a simpler format for the app
            var eventDtos = events.Select(evt => new
            {
                Id = evt.Id,
                Title = evt.Value<string>("eventTitle"),
                DateTime = evt.Value<DateTime>("eventDateTime"),
                Location = evt.Value<string>("location"),
                Description = evt.Value<string>("description"),
                ImageUrl = evt.HasValue("featuredImage") ? evt.Value<IPublishedContent>("featuredImage")?.Url(mode: UrlMode.Absolute) : null,
                Categories = evt.Value<IEnumerable<string>>("eventCategory")
            });

            return Ok(eventDtos);
        }
    }
}