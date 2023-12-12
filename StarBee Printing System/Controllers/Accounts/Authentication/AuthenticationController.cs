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

namespace StarBee_Printing_System.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly CustomerModel customerModel = new CustomerModel();
        private readonly AdminModel adminModel = new AdminModel();

        public AuthenticationController()
        {
            customerModel = new CustomerModel();
        }

        public ActionResult Logout()
        {
            // Clear all session variables
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("LogIn");
        }
        public ActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(FormCollection formcol)
        {
            try
            {
                var email = formcol["userEmail"].Trim();
                var pass = formcol["userPass"].Trim();

                if (IsValidCustomer(email, pass))
                {
                    var customer = customerModel.CustomerAccount(email);

                    Response.Write("<>scriptalert('Welcome dear customer!');</script>");
                    Session["userEmail"] = email;
                    Session["userName"] = customer.LastName;
                    Session["userBusiness"] = customer.BusinessName;
                    Session["role"] = "Customer";
                    return RedirectToAction("Home", "Home");
                }
                else if (IsValidAdmin(email, pass))
                {
                    var admin = adminModel.AdminAccount(email);

                    Response.Write("<>scriptalert('Welcome dear customer!');</script>");
                    Session["userEmail"] = email;
                    Session["userName"] = admin.LastName;
                    Session["role"] = "Admin";

                    return RedirectToAction("Home", "Home");
                }
                else
                {
                    ViewBag.invalidEmail = "Couldn't find your account.";
                }
                return View();

            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "');</script>");
            }

            return View();
        }

        protected bool IsValidAdmin(string email, string pass)
        {
            try
            {
                string hashedPasswordFromDatabase;
                string salt;
                var admin = adminModel.AdminAccount(email);
                if (admin != null)
                {

                    hashedPasswordFromDatabase = admin.Password;
                    salt = admin.Salt;

                    // Hash the entered password with the retrieved salt
                    string enteredPasswordHash = HashPassword(pass, salt);

                    if (string.Equals(hashedPasswordFromDatabase, enteredPasswordHash))
                    {
                        return true;
                    }
                    else
                    {
                        ViewBag.invalidPass = "Wrong password. Try again or click forgot password to reset it.";
                        return false;
                    }


                    // Compare the hashed passwords
                    //return string.Equals(hashedPasswordFromDatabase, enteredPasswordHash);
                }
                return false;


            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "');</script>");
            }
            return true;
        }

        protected bool IsValidCustomer(string email, string pass)
        {
            try
            {
                string hashedPasswordFromDatabase;
                string salt;
                var customer = customerModel.CustomerAccount(email);
                if (customer != null)
                {

                    hashedPasswordFromDatabase = customer.Password;
                    salt = customer.Salt;

                    // Hash the entered password with the retrieved salt
                    string enteredPasswordHash = HashPassword(pass, salt);

                    if (string.Equals(hashedPasswordFromDatabase, enteredPasswordHash))
                    {
                        return true;
                    }
                    else
                    {
                        ViewBag.invalidPass = "Wrong password. Try again or click forgot password to reset it.";
                        return false;
                    }


                    // Compare the hashed passwords
                    //return string.Equals(hashedPasswordFromDatabase, enteredPasswordHash);
                }
                return false;


            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "');</script>");
            }
            return true;
        }

        public ActionResult Index()
        {
            //List<Customer> custome = customerModel.findAll();
            //List<string> emails = custome.Select(cust => cust.Email).ToList();
            //foreach (var i in emails)
            //{
            //    Response.Write(i);
            //}
            ViewBag.customers = customerModel.CustomerAccount("r@gmail.com");


            //if (customerModel.CustomerExist("joe@example.com"))
            //{
            //    Response.Write("Naa may solod bai");
            //}
            //else
            //{
            //    Response.Write("Wlay solod bai");

            //}

            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        public ActionResult ForgotPassword(FormCollection col)
        {
            var email = col["email"].Trim();
            var customer = customerModel.CustomerAccount(email);
            var admin = adminModel.AdminAccount(email);

            if (customer != null)
            {
               
                SendPasswordResetEmail(email);
                return RedirectToAction("OTPVerification");
            }
            else if (admin != null)
            {
                SendPasswordResetEmail(email);
                return RedirectToAction("AdminOTPVerification");


            }
            else
            {
                ViewBag.invalidEmail = "Invalid Email!";
            }

            return View();
        }


        public void SendPasswordResetEmail(string email)
        {
            var customer = customerModel.CustomerAccount(email);
            var admin = adminModel.AdminAccount(email);
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    var pinCode = new Random();
                    var customerPinCode = pinCode.Next(1000, 9999);
                    var adminPinCode = GenerateRandomText();
                    mail.To.Add(email);
                    mail.From = new MailAddress("richardquirante98@gmail.com");
                    mail.Subject = "Reset Password";

                    if (admin != null)
                    {
                        mail.Body = $"Hello admin {admin.LastName}, you have requested a password reset. Your pin code is: <br><br><span style=\"font-family: Arial, sans-serif; font-size: 16px;\">{adminPinCode}</span>";
                        mail.IsBodyHtml = true;

                        using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                        {
                            smtp.Credentials = new System.Net.NetworkCredential("richardquirante98@gmail.com", "vhql cbca rvgd pzjl"); // Replace with sender's email and password

                            smtp.EnableSsl = true;

                            smtp.Send(mail);
                            Session["pinCode"] = adminPinCode;
                            Session["emailToReset"] = admin.Email;
                        }
                    }
                    else
                    {
                        mail.Body = $"Hello Mr./Ms. {customer.LastName}, you have requested a password reset. Your pin code is: <br><br><span style=\"font-family: Arial, sans-serif; font-size: 16px;\">{customerPinCode}</span>";
                        mail.IsBodyHtml = true;

                        using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                        {
                            smtp.Credentials = new System.Net.NetworkCredential("richardquirante98@gmail.com", "vhql cbca rvgd pzjl"); // Replace with sender's email and password

                            smtp.EnableSsl = true;

                            smtp.Send(mail);
                            Session["pinCode"] = customerPinCode;
                            Session["emailToReset"] = customer.Email;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
        }
        public ActionResult OTPVerification()
        {
            return View();
        }

        [HttpPost]
        public ActionResult OTPVerification(FormCollection formcol)
        {
            var OTPCode = formcol["otp1"] +  formcol["otp2"] +  formcol["otp3"] +  formcol["otp4"];

            if (Session["pinCode"] != null && Session["pinCode"].ToString() == OTPCode)
            {
                Session["pinCode"] = null;
                return RedirectToAction("ResetPassword");
            }
            else if(Session["pinCodeChangePass"] != null && Session["pinCodeChangePass"].ToString() == OTPCode)
            {
                customerModel.resetPassword(Session["userEmail"].ToString(),
                                            Session["newPass"].ToString(),
                                            Session["newSalt"].ToString());

                Session["newPass"] = null;
                Session["newSalt"] = null;
                Session["pinCodeChangePass"] = null;
                return RedirectToAction("CustomerProfile", "Profile");
            }
            else if(Session["pinCodeDelete"] != null && Session["pinCodeDelete"].ToString() == OTPCode)
            {
                customerModel.DeleteAccount(Session["userEmail"].ToString());

                // Clear all session variables
                Session.Clear();
                Session.Abandon();
                Response.Write("<script>alert('Your account is successfully deleted!');</script>");
                return RedirectToAction("LogIn");
            }
            else
            {
                ViewBag.invalidOTP = "Please input the OTP Code that we sent to your email.";
            }
            return View();
        }

        public ActionResult AdminOTPVerification()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AdminOTPVerification(FormCollection formcol)
        {
            var OTPCode = formcol["pinCode"];

            if (Session["pinCode"] != null && Session["pinCode"].ToString() == OTPCode)
            {
                Session["pinCode"] = null;
                return RedirectToAction("ResetPassword");
            }
            else if (Session["pinCodeAdminChangePass"] != null && Session["pinCodeAdminChangePass"].ToString() == OTPCode)
            {
                adminModel.resetPassword(Session["userEmail"].ToString(),
                                            Session["newAdminPass"].ToString(),
                                            Session["newAdminSalt"].ToString());

                Session["newAdminPass"] = null;
                Session["newAdminSalt"] = null;
                Session["pinCodeAdminChangePass"] = null;
                return RedirectToAction("AdminProfile", "Admins");

            }
            else if (Session["pinCodeAdminDelete"] != null && Session["pinCodeAdminDelete"].ToString() == OTPCode)
            {
                adminModel.DeleteAccount(Session["userEmail"].ToString());

                // Clear all session variables
                Session.Clear();
                Session.Abandon();
                Response.Write("<script>alert('Your account is successfully deleted!');</script>");
                return RedirectToAction("LogIn");

            }
            else
            {
                Response.Write("<script>alert('Please input the OTP Code that we sent to your email.');</script>");
                ViewBag.invalidOTP = "Please input the OTP Code that we sent to your email.";
            }
            return View();

        }

        public ActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(FormCollection formcoll)
        {
            
            var newPass = formcoll["newPass"].Trim();

            var salt = GenerateSalt();
            var hashedNewPassword = HashPassword(newPass, salt);

            var customer = customerModel.CustomerAccount(Session["emailToReset"].ToString());
            var admin = adminModel.AdminAccount(Session["emailToReset"].ToString());

            if (customer != null) {
                customerModel.resetPassword(customer.Email, hashedNewPassword, salt);
                return RedirectToAction("LogIn");
            }
            else if(admin != null)
            {
                adminModel.resetPassword(admin.Email, hashedNewPassword, salt);
                return RedirectToAction("LogIn");
            }

            return View();
        }


        public ActionResult SuccessReset()
        {
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
    }
}