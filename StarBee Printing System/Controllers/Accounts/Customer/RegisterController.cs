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

namespace StarBee_Printing_System.Controllers
{
    public class RegisterController : Controller
    {
        private readonly CustomerModel customerModel = new CustomerModel();
        private readonly AdminModel adminModel = new AdminModel();

        public RegisterController()
        {
            customerModel = new CustomerModel();
        }
        // GET: Register
        public ActionResult CustomerForm()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CustomerForm(FormCollection  coll)
        {      
            try
            {
                var fname = ToTitleCase(coll["custFname"].Trim());
                var lname = ToTitleCase(coll["custLname"].Trim());
                var pnum = coll["custPnum"].Trim();
                var bname = ToTitleCase(coll["custBname"].Trim());
                var address = ToTitleCase(coll["custAddress"].Trim());
                var email = coll["custEmail"].Trim();
                var pass = coll["custPass"].Trim();

                var salt = GenerateSalt();
                string hashedPassword = HashPassword(pass, salt);

                if (customerModel.CustomerAccount(email) != null || adminModel.AdminAccount(email) != null)
                {
                    Response.Write("<script>alert('Email account already exist! Try another Email');</script>");
                }
                else
                {
                    SendComfirmationEmail(email, lname);


                    Customer newCustomer = new Customer
                    {
                        FirstName = fname,
                        LastName = lname,
                        PhoneNumber = pnum,
                        BusinessName = bname,
                        CustomerAddress = address,
                        Email = email,
                        Password = hashedPassword,
                        Salt = salt,
                        Status = "Active",
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now
                    };

                    Session["PendingRegistration"] = newCustomer;

                    //Response.Write("<script>alert('Account Successfully Registered!');</script>");
                    return RedirectToAction("RegisrationOTPVerification");
                }

            }
            catch (Exception ex)
            {
                ViewBag.error = ex;
                return View();
            }

            return View();
        }
        public void SendComfirmationEmail(string email, string lastname)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    var pinCode = new Random();
                    var customerPinCode = pinCode.Next(1000, 9999);
                    mail.To.Add(email);
                    mail.From = new MailAddress("richardquirante98@gmail.com");
                    mail.Subject = "Registration Comfirmation";

                   
                    mail.Body = $"Hello Mr./Ms. {lastname}, this is a comfirmation email account message for your regisration. Your pin code is: <br><br>{customerPinCode}";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new System.Net.NetworkCredential("richardquirante98@gmail.com", "vhql cbca rvgd pzjl"); // Replace with sender's email and password

                        smtp.EnableSsl = true;

                        smtp.Send(mail);
                        Session["pinCode"] = customerPinCode;
                    }
                    

                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
        }
        public ActionResult RegisrationOTPVerification()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisrationOTPVerification(FormCollection formcol)
        {
            var OTPCode = formcol["otp1"] + formcol["otp2"] + formcol["otp3"] + formcol["otp4"];
            Customer pendingCustomer = (Customer)Session["PendingRegistration"];

            if (Session["pinCode"] != null && Session["pinCode"].ToString() == OTPCode)
            {
                customerModel.create(pendingCustomer);
                return RedirectToAction("LogIn", "Authentication");
            }
            else
            {
                ViewBag.invalidOTP = "Please input the OTP Code that we sent to your email.";
                Response.Write("<script>alert('Invalid! Please input the OTP Code that we sent to your email.');</script>");

            }
            return View();
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
                // Convert the combined password string to a byte array
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