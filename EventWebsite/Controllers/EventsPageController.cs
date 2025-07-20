using EventWebsite.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Website.Controllers; // This is the correct namespace for SurfaceController

namespace EventWebsite.Controllers
{
    public class EventsPageController : SurfaceController
    {
        private readonly IContentService? _contentService;

        // This is the simplified constructor for modern Umbraco versions
        public EventsPageController(
            IUmbracoContextAccessor umbracoContextAccessor,
            IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger,
            IPublishedUrlProvider publishedUrlProvider)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            // We get the Content Service from the ServiceContext
            _contentService = services.ContentService;
        }

        // This action handles showing the form.
        // It needs a physical page to render on.
        public IActionResult Create()
        {
            var model = new CreateEventViewModel();
            // Use null conditional operator to avoid warning
            return model != null 
                ? View("CreateEventForm", model) 
                : View("CreateEventForm", new CreateEventViewModel());
        }

        // This action handles the form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HandleCreate(CreateEventViewModel model)
        {
            // If the form has validation errors, show it again
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            // Check if content service is available
            if (_contentService == null)
            {
                ModelState.AddModelError("", "Content service is not available. Please try again later.");
                return CurrentUmbracoPage();
            }

            // --- ⚠️ IMPORTANT ⚠️ ---
            // Replace with the ID of your "Events" listing page.
            // In Umbraco: Content > Events > Info tab > ID.
            int parentId = 1074; // <--- REPLACE 1074 WITH YOUR ID

            // Replace "event" with the alias of your event document type.
            // In Umbraco: Settings > Document Types > Your Event Type > Alias.
            var newEvent = _contentService.Create(model.EventTitle, parentId, "event"); // <-- CHECK YOUR ALIAS

            // Set the property values
            newEvent.SetValue("eventTitle", model.EventTitle);
            newEvent.SetValue("eventDateTime", model.EventDateTime);
            newEvent.SetValue("description", model.Description);
            newEvent.SetValue("eventCategory", model.Category);

            _contentService.SaveAndPublish(newEvent);

            TempData["SuccessMessage"] = "Your event has been created successfully!";

            // Redirect back to the page the form was on
            return RedirectToCurrentUmbracoPage();
        }
    }
}