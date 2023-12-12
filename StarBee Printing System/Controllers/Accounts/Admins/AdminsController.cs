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

namespace StarBee_Printing_System.Controllers.Accounts.Admin
{
    public class AdminsController : Controller
    {
        private readonly CustomerModel customerModel = new CustomerModel();
        private readonly AdminModel adminModel = new AdminModel();

        public AdminsController()
        {
            adminModel = new AdminModel();

        }

        public ActionResult AdminProfile()
        {
            ViewBag.adminInfo = adminModel.AdminAccount(Session["userEmail"].ToString());
            return View();
        }

        public ActionResult ChangeAdminPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangeAdminPassword(FormCollection adminCol)
        {

            var admin = adminModel.AdminAccount(Session["userEmail"].ToString());

            var oldPass = adminCol["oldPass"].Trim();
            var newPass = adminCol["newPass"].Trim();

            var salt = GenerateSalt();
            var newHashedPassword = HashPassword(newPass, salt);

            var oldSalt = admin.Salt;
            var oldPassword = HashPassword(oldPass, oldSalt);
            var databasePassword = admin.Password;

            if (oldPassword == databasePassword)
            {
                SendChangePasswordEmail(admin.Email);
                Session["newAdminPass"] = newHashedPassword;
                Session["newAdminSalt"] = salt;
                return RedirectToAction("AdminOTPVerification", "Authentication");
            }
            else
            {
                Response.Write("<script>alert('Old Password Doesn't Match');</script>");
                ViewBag.invalidOldPass = "Old Password Doesn't Match. Please try again.";
            }
            return View();

        }

        public ActionResult EditAdminProfile()
        {
            ViewBag.editAdminInfo = adminModel.AdminAccount(Session["userEmail"].ToString());
            return View();
        }

        [HttpPost]
        public ActionResult EditAdminProfile(FormCollection col)
        {

            var newFname = ToTitleCase(col["NewFname"].Trim());
            var newLname = ToTitleCase(col["newLname"].Trim());
            var newPnum = col["newPnum"].Trim();

            var admin = adminModel.AdminAccount(Session["userEmail"].ToString());

            if (admin.FirstName == newFname && admin.LastName == newLname && admin.PhoneNumber == newPnum )
            {
                return RedirectToAction("AdminProfile");
            }
            else
            {
                adminModel.updateProfile(Session["userEmail"].ToString(), newFname, newLname, newPnum);
            }
            return RedirectToAction("AdminProfile");
        }

        public ActionResult NewAdmin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewAdmin(FormCollection adminform)
        {
            try
            {
                var fname = ToTitleCase(adminform["adminFname"].Trim());
                var lname = ToTitleCase(adminform["adminLname"].Trim());
                var pnum = adminform["adminPnum"].Trim();
                var email = adminform["adminEmail"].Trim();
                var pass = adminform["adminPass"].Trim();

                var salt = GenerateSalt();
                string hashedPassword = HashPassword(pass, salt);

                if (customerModel.CustomerAccount(email) != null || adminModel.AdminAccount(email) != null)
                {
                    Response.Write("<script>alert('Email account already exist! Try another Email');</script>");
                }
                else
                {
                    SendComfirmationEmail(email, lname);

                    AdminField newAdmin = new AdminField
                    {
                        FirstName = fname,
                        LastName = lname,
                        PhoneNumber = pnum,
                        Email = email,
                        Password = hashedPassword,
                        Salt = salt,
                        Status = "Active",
                        Created_at = DateTime.Now,
                        Updated_at = DateTime.Now
                    };
                    Session["PendingRegistration"] = newAdmin;

                    Response.Write("<script>alert('Account Successfully Registered!');</script>");
                    return RedirectToAction("RegisrationAdminOTPVerification");
                }

            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('"+ex.Message+"');</script>");
                return View();
            }
            return View();
        }

        public ActionResult DeleteAccount()
        {
            SendDeleteEmail(Session["userEmail"].ToString());
            return RedirectToAction("AdminOTPVerification", "Authentication");

        }
        public void SendDeleteEmail(string email)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    var admin = adminModel.AdminAccount(email);
                    var pinCode = new Random();
                    var adminPinCode = GenerateRandomText();
                    mail.To.Add(email);
                    mail.From = new MailAddress("richardquirante98@gmail.com");
                    mail.Subject = "Comfirmation Account Deletion";


                    mail.Body = $"Hello Mr./Ms. {admin.LastName}, this pin code is the comfirmation of your account deletion. Your pin code is: <br><br><span style=\"font-family: Arial, sans-serif; font-size: 16px;\">{adminPinCode}</span>";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new System.Net.NetworkCredential("richardquirante98@gmail.com", "vhql cbca rvgd pzjl"); // Replace with sender's email and password

                        smtp.EnableSsl = true;

                        smtp.Send(mail);
                        Session["pinCodeAdminDelete"] = adminPinCode;
                    }


                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
        }
        public void SendChangePasswordEmail(string email)
        {
            var admin = adminModel.AdminAccount(email);
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    var pinCode = new Random();
                    var adminPinCode = GenerateRandomText();
                    mail.To.Add(email);
                    mail.From = new MailAddress("richardquirante98@gmail.com");
                    mail.Subject = "Change Password";

                    mail.Body = $"Hello Mr./Ms. {admin.LastName}, you have requested to change your password. Your pin code is: <br><br><span style=\"font-family: Arial, sans-serif; font-size: 16px;\">{adminPinCode}</span>";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new System.Net.NetworkCredential("richardquirante98@gmail.com", "vhql cbca rvgd pzjl"); // Replace with sender's email and password

                        smtp.EnableSsl = true;

                        smtp.Send(mail);
                        Session["pinCodeAdminChangePass"] = adminPinCode;
                        Session["emailToReset"] = admin.Email;
                    }

                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
        }

        public void SendComfirmationEmail(string email, string lastname)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    var pinCode = new Random();
                    var adminPinCode = GenerateRandomText();
                    mail.To.Add(email);
                    mail.From = new MailAddress("richardquirante98@gmail.com");
                    mail.Subject = "Registration Comfirmation";


                    mail.Body = $"Hello Mr./Ms. {lastname}, this is a comfirmation email account message for your regisration. Your pin code is: <br><br><span style=\"font-family: Arial, sans-serif; font-size: 16px;\">{adminPinCode}</span>";
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new System.Net.NetworkCredential("richardquirante98@gmail.com", "vhql cbca rvgd pzjl"); // Replace with sender's email and password

                        smtp.EnableSsl = true;

                        smtp.Send(mail);
                        Session["pinCode"] = adminPinCode;
                    }


                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
        }
        public ActionResult RegisrationAdminOTPVerification()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RegisrationAdminOTPVerification(FormCollection formcol)
        {
            var OTPCode = formcol["pinCode"];
            AdminField pendingAdmin = (AdminField)Session["PendingRegistration"];

            if (Session["pinCode"] != null && Session["pinCode"].ToString() == OTPCode)
            {
                adminModel.create(pendingAdmin);
                return RedirectToAction("LogIn", "Authentication");
            }
            else
            {
                ViewBag.invalidOTP = "Please input the OTP Code that we sent to your email. If you did'nt see it go to spam.";
                Response.Write("<script>alert('Invalid OTP Code! Please input the OTP Code that we sent to your email. If you did'nt see it go to spam.');</script>");

            }
            return View();
        }

        protected string GenerateRandomText()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder stringBuilder = new StringBuilder();

            Random random = new Random();
            for (int i = 0; i < 9; i++)
            {
                int index = random.Next(chars.Length);
                stringBuilder.Append(chars[index]);
            }

            return stringBuilder.ToString();
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