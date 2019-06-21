using Control_Group_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Control_Group_Web.Controllers
{
    public class DataTransferController : Controller
    {


        // GET: DataTransfer
        public ActionResult Index()
        {
            return View();
        }

        // GET: DataTransfer/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DataTransfer/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DataTransfer/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: DataTransfer/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DataTransfer/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: DataTransfer/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DataTransfer/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
