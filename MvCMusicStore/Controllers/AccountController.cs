﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvCMusicStore.Models;
using System.Net;

namespace MvCMusicStore.Controllers
{
    public class AccountController : Controller
    {
        private AccountEntities db = new AccountEntities();
        // GET: Account
        public ActionResult Index()
        {
            using (db)
            {
                return View(db.userAccount.ToList());
            }
        }

        // GET: /Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public ActionResult Register(UserAccount account)
        {
            if (ModelState.IsValid)
            {
                using (db)
                {
                    db.userAccount.Add(account);
                    db.SaveChanges();
                }
                ModelState.Clear();
                ViewBag.Message = account.Username + " successfully registered.";
            }
            return View();
        }

        // GET: /Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public ActionResult Login(UserAccount user)
        {
            using (db)
            {
                var usr = db.userAccount.Where(u => u.Username == user.Username
                                                && u.Password == user.Password).FirstOrDefault();
                if (usr != null)
                {
                    MigrateShoppingCart(usr.Username);

                    Session["UserID"] = usr.UserID.ToString();
                    Session["Username"] = usr.Username.ToString();
                    return RedirectToAction("LoggedIn");
                }
                else
                {
                    ModelState.AddModelError("", "Username or password is incorrect.");
                }
            }
            return View();
        }

        // GET: /Account/LoggedIn
        public ActionResult LoggedIn()
        {
            if (Session["UserID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            return View();
        }

        // POST: /Account/Logout
        [HttpPost]
        public ActionResult Logout(UserAccount user)
        {
            using (db)
            {
                Session["UserID"] = null;
                Session["Username"] = null;
                EmptyCart();
                return RedirectToAction("LoggedOut");
            }
        }

        // GET: /Account/LoggedOut
        public ActionResult LoggedOut()
        {
            if (Session["UserID"] == null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Logout");
            }
        }

        // GET: Account/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserAccount account = db.userAccount.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // POST: Account/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserAccount account = db.userAccount.Find(id);
            db.userAccount.Remove(account);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void MigrateShoppingCart(string UserName)
        {
            // Associate shopping cart items with logged-in user
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.MigrateCart(UserName);
            Session[ShoppingCart.CartSessionKey] = UserName;
        }

        private void EmptyCart()
        {
            // Empty cart when user logs out
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.EmptyCart();
        }
    }
}