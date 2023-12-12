using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using StarBee_Printing_System.Entities;
using StarBee_Printing_System.Models;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;

namespace StarBee_Printing_System.Controllers.Accounts.Customer
{
    public class ProfileController : Controller
    {
        private readonly CustomerModel customerModel = new CustomerModel();

        public ActionResult CustomerProfile()
        {
            ViewBag.customerInfo = customerModel.CustomerAccount(Session["userEmail"].ToString());
            return View();
        }

        public ActionResult DeleteAccount()
        {
            SendDeleteEmail(Session["userEmail"].ToString());
            return RedirectToAction("OTPVerification", "Authentication");

        }

        public ActionResult EditProfile()
        {
            ViewBag.editInfo = customerModel.CustomerAccount(Session["userEmail"].ToString());

            return View();
        }

        [HttpPost]
        public ActionResult EditProfile(FormCollection col)
        {
            var newFname = ToTitleCase(col["NewFname"].Trim());
            var newLname = ToTitleCase(col["newLname"].Trim());
            var newBname = ToTitleCase(col["newBname"].Trim());
            var newPnum = col["newPnum"].Trim();
            var newAddress = ToTitleCase(col["newAddress"].Trim());

            var customer = customerModel.CustomerAccount(Session["userEmail"].ToString());

            if (customer.FirstName == newFname && customer.LastName == newLname && customer.BusinessName == newBname && customer.PhoneNumber == newPnum && customer.CustomerAddress == newAddress)
            {
                return RedirectToAction("CustomerProfile");
            }
            else
            {
                customerModel.updateProfile(Session["userEmail"].ToString(), newFname, newLname, newPnum, newBname, newAddress);
            }


            return RedirectToAction("CustomerProfile");
        }

        public ActionResult ChangePassword()
        {

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection formCol)
        {
            var customer = customerModel.CustomerAccount(Session["userEmail"].ToString());

            var oldPass = formCol["oldPass"].Trim();
            var newPass = formCol["newPass"].Trim();

            var salt = GenerateSalt();
            var newHashedPassword = HashPassword(newPass, salt);

            var oldSalt = customer.Salt;
            var oldPassword = HashPassword(oldPass, oldSalt);
            var databasePassword = customer.Password;

            if(oldPassword == databasePassword)
            {
                SendChangePasswordEmail(customer.Email);
                Session["newPass"] = newHashedPassword;
                Session["newSalt"] = salt;
                return RedirectToAction("OTPVerification", "Authentication");
            }
            else
            {
                Response.Write("<script>alert('Old Password Doesn't Match');</script>");
                ViewBag.invalidOldPass = "Old Password Doesn't Match. Please try again.";
            }
            return View();


        }
        public void SendChangePasswordEmail(string email)
        {
            var customer = customerModel.CustomerAccount(email);
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    var pinCode = new Random();
                    var customerPinCode = pinCode.Next(1000, 9999);
                    mail.To.Add(email);
                    mail.From = new MailAddress("richardquirante98@gmail.com");
                    mail.Subject = "Change Password";

                    mail.Body = $"Hello Mr./Ms. {customer.LastName}, you have requested to change your password. Your pin code is: <br><br><span style=\"font-family: Arial, sans-serif; font-size: 16px;\">{customerPinCode}</span>";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new System.Net.NetworkCredential("richardquirante98@gmail.com", "vhql cbca rvgd pzjl"); // Replace with sender's email and password

                        smtp.EnableSsl = true;

                        smtp.Send(mail);
                        Session["pinCodeChangePass"] = customerPinCode;
                        Session["emailToReset"] = customer.Email;
                    }

                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
        }
        public void SendDeleteEmail(string email)
        {
            var customer = customerModel.CustomerAccount(email);
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    var pinCode = new Random();
                    var customerPinCode = pinCode.Next(1000, 9999);
                    mail.To.Add(email);
                    mail.From = new MailAddress("richardquirante98@gmail.com");
                    mail.Subject = "Comfirmation Account Deletion";

                    mail.Body = $"Hello Mr./Ms. {customer.LastName}, this pin code is the comfirmation of your account deletion. Your pin code is: <br><br><span style=\"font-family: Arial, sans-serif; font-size: 16px;\">{customerPinCode}</span>";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new System.Net.NetworkCredential("richardquirante98@gmail.com", "vhql cbca rvgd pzjl"); // Replace with sender's email and password

                        smtp.EnableSsl = true;

                        smtp.Send(mail);
                        Session["pinCodeDelete"] = customerPinCode;
                    }

                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
        }

        protected string GenerateSalt()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var saltChars = new char[16];
            for (int i = 0; i < saltChars.Length; i++)
            {
                saltChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(saltChars);
        }

        protected string HashPassword(string password, string salt)
        {
            // Combine the password and salt
            string combinedPassword = password + salt;

            // Choose the hash algorithm (SHA-256 or SHA-512)
            using (var sha256 = SHA256.Create())
            {
                // Conver the combined password string to a byte array
                byte[] bytes = Encoding.UTF8.GetBytes(combinedPassword);

                // Compute the hash value of the byte array
                byte[] hash = sha256.ComputeHash(bytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder result = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    result.Append(hash[i].ToString("x2"));
                }

                return result.ToString();
            }
        }
        static string ToTitleCase(string input)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }
    }
}