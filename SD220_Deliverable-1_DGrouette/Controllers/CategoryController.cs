﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SD220_Deliverable_1_DGrouette.Models;
using SD220_Deliverable_1_DGrouette.Models.Bindings;
using SD220_Deliverable_1_DGrouette.Models.Domain;
using SD220_Deliverable_1_DGrouette.Models.Filters;
using SD220_Deliverable_1_DGrouette.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SD220_Deliverable_1_DGrouette.Controllers
{
    [RoutePrefix("api/category")]
    [Authorize]
    public class CategoryController : ApiController
    {
        // Using the same DB instance
        private ApplicationUserManager UserManager => ControllerContext.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
        private ApplicationDbContext DbContext => ControllerContext.Request.GetOwinContext().Get<ApplicationDbContext>();

        // >The owner of the household should be able to create categories.
        // POST api/category/create
        [HttpPost]
        [Route("create/{id:int}")]
        [CreatorAuthorization]
        public IHttpActionResult Create(int? id, CategoryBindingModel categoryBinding)
        {
            // Id being the id of the Household
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var household = DbContext.Households.FirstOrDefault(p => p.Id == id);
            if (household is null)
                return NotFound();

            var category = new Category
            {
                DateCreated = DateTime.Now,
                DateUpdated = null,
                Name = categoryBinding.Name,
                Description = categoryBinding.Description
            };

            household.Categories.Add(category);
            DbContext.SaveChanges();

            var categoryView = HouseholdHelpers.MapCategoryToView(category);

            return Created(Url.Link(
                "GetCategoryById",
                new { category.Id }),
                categoryView
            );
        }

        // >The owner of the household should be able to edit categories.
        // POST api/category/edit/2
        [HttpPost]
        [Route("edit/{id:int}")]
        [CategoryAuthorization]
        public IHttpActionResult Edit(int? id, CategoryBindingModel categoryBinding)
        {
            if (!ModelState.IsValid) // ++Q : Turn into filter.
                return BadRequest(ModelState);

            var category = DbContext.Categories.FirstOrDefault(p => p.Id == id);
            if (category is null)
                return NotFound();

            category.DateUpdated = DateTime.Now;
            category.Name = categoryBinding.Name;
            category.Description = categoryBinding.Description;
            DbContext.SaveChanges();

            var categoryView = HouseholdHelpers.MapCategoryToView(category);
            return Ok(categoryView);
        }

        // >The owner of the household should be able to delete categories.
        // POST api/category/delete/2
        [HttpDelete]
        [Route("delete/{id:int}")]
        [CategoryAuthorization]
        public IHttpActionResult Delete(int? id)
        {
            var removedCategory = DbContext.Categories.Remove(DbContext.Categories.FirstOrDefault(p => p.Id == id));

            if (removedCategory != null)
            {
                DbContext.SaveChanges();
                return Ok();
            }
            else
            {
                return InternalServerError();
            }
        }

        // >Registered users should be able to view a list of all categories from the households they are in.
        // GET api/category/getall
        [HttpGet]
        [Route("getall")]
        public IHttpActionResult GetAll()
        {
            var userId = User.Identity.GetUserId();
            if (userId is null)
                return NotFound();

            var categories = DbContext.Categories.Where(p => p.Household.Users.Any(i => i.Id == userId)).ToList();
            if (categories is null)
                return NotFound();

            var categoriesView = categories.Select(p => HouseholdHelpers.MapCategoryToView(p));

            return Ok(categoriesView);
        }


        // === Extras for debugging. ===
        // GET api/category/getbyid/2
        [HttpGet]
        [Route("getbyid/{id:int}", Name = "GetCategoryById")]
        public IHttpActionResult GetById(int? Id)
        {
            if (Id is null)
                return BadRequest();

            var category = DbContext.Categories.FirstOrDefault(p => p.Id == Id);
            if (category is null)
                return NotFound();

            var categoryView = HouseholdHelpers.MapCategoryToView(category);

            return Ok(categoryView);
        }
    }
}