﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Forum.Models;
using PagedList;

namespace Forum.Controllers
{
    public class HomeController : Controller
    {
        private DatabaseContext db = new DatabaseContext();

        public ViewResult Index(string sortQuestion, string currentFilter, string questionGroup, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortQuestion;
            ViewBag.MainParm = String.IsNullOrEmpty(sortQuestion) ? "name desc" : "";
            ViewBag.SubmainParm = sortQuestion == "date" ? "date desc" : "date";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewBag.CurrentFilter = searchString;

            var questions = from q in db.Questions select q;

            if (!String.IsNullOrEmpty(searchString))
            {
                questions = questions.Where(q => q.QuestionName.ToUpper().Contains(searchString.ToUpper())
                                       || q.QuestionDescription.ToUpper().Contains(searchString.ToUpper()));
            }

            switch (sortQuestion)
            {
                case "name desc":
                    questions = questions.OrderByDescending(q => q.QuestionName);
                    break;
                case "date":
                    questions = questions.OrderBy(q => q.DateTime);
                    break;
                case "date desc":
                    questions = questions.OrderByDescending(q => q.DateTime);
                    break;
                default:
                    questions = questions.OrderBy(q => q.QuestionName);
                    break;
            }

            var questionGroupLst = new List<string>();
            var questionGroupsQry = from q in db.Questions select q.QuestionGroups.QuestionGroupName;
            questionGroupLst.AddRange(questionGroupsQry.Distinct());
            ViewBag.questionGroup = new SelectList(questionGroupLst);

            if (!String.IsNullOrEmpty(questionGroup))
            {
                questions = questions.Where(q => q.QuestionGroups.QuestionGroupName == questionGroup);
            }

            int PageSize = 50;
            int PageNumber = (page ?? 1);
            return View(questions.ToPagedList(PageNumber, PageSize));
        }

        public ActionResult Details(int id = 0)
        {
            Questions questions = db.Questions.Find(id);

            if (questions == null)
            {
                return HttpNotFound();
            }
            return View(questions);
        }

        public ActionResult Create()
        {
            ViewBag.QuestionGroupId = new SelectList(db.QuestionGroups, "QuestionGroupId", "QuestionGroupName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Questions Question)
        {
            if (ModelState.IsValid)
            {
                db.Questions.Add(Question);
                db.SaveChanges();
                return RedirectToAction("Index");
            }           
            ViewBag.QuestionGroupId= new SelectList(db.QuestionGroups, "QuestionGroupId", "QuestionGroupName", Question.QuestionGroupId);
            return View(Question);
        }

        public ActionResult Edit(int id = 0)
        {
            Session["QuestionId"] = id;
            Questions Question = db.Questions.Find(id);
            if (Question == null)
            {
                return HttpNotFound();
            }
            ViewBag.QuestionGroupSelected = new SelectList(db.QuestionGroups, "QuestionGroupId", "QuestionGroupName");
            return View(Question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Questions Question)
        {
            Question.QuestionId = (int)Session["QuestionId"];
            if (ModelState.IsValid)
            {
                db.Entry(Question).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.QuestionGroupSelected = new SelectList(db.QuestionGroups, "QuestionGroupId", "QuestionGroupName", Question.QuestionGroupId);
            return View(Question);
        }

        public ActionResult Delete(int id = 0)
        {
            Questions Question = db.Questions.Find(id);
            if (Question == null)
            {
                return HttpNotFound();
            }
            return View(Question);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Questions Question = db.Questions.Find(id);
            db.Questions.Remove(Question);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}