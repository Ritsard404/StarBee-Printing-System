using System;
using System.Collections.Generic;
using StarBee_Printing_System.Entities;
using StarBee_Printing_System.Models;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net;

namespace StarBee_Printing_System.Controllers.Accounts.Admins
{
    public class ManagementController : Controller
    {
        private readonly CustomerModel customerModel = new CustomerModel();
        private readonly AdminModel adminModel = new AdminModel();

        public ManagementController()
        {
            customerModel = new CustomerModel();
            adminModel = new AdminModel();
        }
        // GET: Management
        public ActionResult AdminAccounts()
        {
            ViewBag.allAdmins = adminModel.findAll();
            return View();
        }
        // GET: Management
        public ActionResult CustomerAccounts()
        {
            ViewBag.allCustomer = customerModel.findAll();
            return View();
        }

        [HttpGet]
        public ActionResult SuspendCustomerAccount(string id)
        {
            customerModel.suspendCustomer(id);
            return RedirectToAction("CustomerAccounts");
        }

        [HttpGet]
        public ActionResult UnsuspendCustomerAccount(string id)
        {
            customerModel.unsuspendCustomer(id);
            return RedirectToAction("CustomerAccounts");
        }

        [HttpGet]
        public ActionResult DeleteCustomerAccount(string id)
        {
            customerModel.deleteCustomer(id);
            return RedirectToAction("CustomerAccounts");
        }

        [HttpGet]
        public ActionResult SuspendAdminAccount(string id)
        {
            adminModel.suspendAdmin(id);
            return RedirectToAction("AdminAccounts");
        }

        [HttpGet]
        public ActionResult UnsuspendAdminAccount(string id)
        {
            adminModel.unsuspendAdmin(id);
            return RedirectToAction("AdminAccounts");
        }

        [HttpGet]
        public ActionResult DeleteAdminAccount(string id)
        {
            adminModel.deleteAdmin(id);
            return RedirectToAction("AdminAccounts");
        }
    }
}