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
        [UserAuthorization(IdType = typeof(HouseholdCreator))]
        public IHttpActionResult Create(int? Id, CategoryBindingModel categoryBinding)
        {
            // Id being the Id of the Household
            if (ModelState is null || !ModelState.IsValid)
                return BadRequest(ModelState);

            if (DbContext.Households.FirstOrDefault(p => p.Id == Id).Categories.Any(p => p.Name == categoryBinding.Name))
            {
                return BadRequest("That name is already in use, please choose another.");
            }

            var household = DbContext.Households.FirstOrDefault(p => p.Id == Id);
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
        [UserAuthorization(IdType = typeof(CategoryCreator))]
        public IHttpActionResult Edit(int? Id, CategoryBindingModel categoryBinding)
        {
            if (ModelState is null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var category = DbContext.Categories.FirstOrDefault(p => p.Id == Id);
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
        [UserAuthorization(IdType = typeof(CategoryCreator))]
        public IHttpActionResult Delete(int? Id)
        {
            var categoryCheck = DbContext.Categories.Where(p => p.Id == Id && !p.Transactions.Any()).FirstOrDefault();

            if(categoryCheck != null)
            {
                var removeCategory = DbContext.Categories.Remove(categoryCheck);

                if(removeCategory != null)
                {
                    DbContext.SaveChanges();
                    return Ok();
                } else
                {
                    return InternalServerError();
                }

            } else
            {
                return BadRequest("Cannot delete a category while it contains transactions.");
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

            var categories = DbContext.Categories.Where(p => p.Household.Members.Any(i => i.Id == userId)).ToList();
            if (categories is null)
                return NotFound();

            var categoriesView = categories.Select(p => HouseholdHelpers.MapCategoryToView(p));

            return Ok(categoriesView);
        }

        // >Getting the categories by the household Id, user must be a member of the household
        // GET api/category/getallbyhousehold
        [HttpGet]
        [Route("getallbyhousehold/{id:int}")]
        [UserAuthorization(IdType = typeof(HouseholdHouseMember))]
        public IHttpActionResult GetAll(int? Id)
        {
            var userId = User.Identity.GetUserId();
            if (userId is null)
                return Unauthorized();

            var categories = DbContext.Households.FirstOrDefault(p => p.Id == Id).Categories.ToList();
            if (categories is null)
                return BadRequest("No Categories for this household");

            var categoriesView = categories.Select(p => HouseholdHelpers.MapCategoryToView(p));

            return Ok(categoriesView);
        }

        // === Extras for debugging. ===
        // GET api/category/getbyId/2
        [HttpGet]
        [Route("getbyId/{id:int}", Name = "GetCategoryById")]
        [UserAuthorization(IdType = typeof(CategoryHouseMember))]
        public IHttpActionResult GetById(int? Id)
        {
            if (Id is null)
                return BadRequest("Id is invalid");

            var category = DbContext.Categories.FirstOrDefault(p => p.Id == Id);
            if (category is null)
                return NotFound();

            var categoryView = HouseholdHelpers.MapCategoryToView(category);

            return Ok(categoryView);
        }

        // GET api/category/getName/2
        [HttpGet]
        [Route("getName/{id:int}")]
        [UserAuthorization(IdType = typeof(CategoryHouseMember))]
        public IHttpActionResult GetName(int? Id)
        {
            if (Id is null)
                return BadRequest("Id is invalid");

            var categoryName = DbContext.Categories.FirstOrDefault(p => p.Id == Id).Name;
            if (categoryName is null)
                return NotFound();

            return Ok(categoryName);
        }
    }
}
