using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventWebsite.Controllers
{
    public class EventsController : RenderController
    {
        private readonly ILogger<EventsController> _logger;

        public EventsController(
            ILogger<EventsController> logger,
            ICompositeViewEngine viewEngine, 
            IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, viewEngine, umbracoContextAccessor)
        {
            _logger = logger;
        }

        public override IActionResult Index()
        {
            // Find all event pages in the content tree
            var allEvents = new List<IPublishedContent>();
            
            try {
                // Try to get events using the most reliable method first
                var eventsFromRoot = CurrentPage != null 
                    ? CurrentPage.Root()
                        .DescendantsOfType("event")
                        .Where(x => x.IsPublished())
                        .ToList()
                    : new List<IPublishedContent>();
                    
                if (eventsFromRoot.Any())
                {
                    _logger.LogInformation($"Found {eventsFromRoot.Count} events using DescendantsOfType method");
                    allEvents = eventsFromRoot;
                }
                else if (CurrentPage != null)
                {
                    // Fallback to searching for any content with event in the name
                    var eventsFromSearch = CurrentPage.Root()
                        .Descendants()
                        .Where(x => x.ContentType.Alias.Contains("event") && x.IsPublished())
                        .ToList();
                        
                    _logger.LogInformation($"Found {eventsFromSearch.Count} events using fallback search");
                    allEvents = eventsFromSearch;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding events");
            }
            
            // Order events by date if available
            var orderedEvents = allEvents
                .OrderByDescending(x => x.HasValue("eventDateTime") 
                    ? x.Value<DateTime>("eventDateTime") 
                    : DateTime.MinValue)
                .ToList();
            
            // Pass the events to the view
            ViewData["AllEvents"] = orderedEvents;
            _logger.LogInformation($"Passing {orderedEvents.Count} events to view");
            
            return CurrentTemplate(CurrentPage);
        }
    }
}
