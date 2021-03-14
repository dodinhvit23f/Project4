﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Project4Aptech.Models;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using Project4Aptech.Repository;

namespace Project4Aptech.Areas.Admin.Controllers
{
    public class CustomersController : Controller
    {
        Repo r = new Repo();
        private DatabaseEntities db = new DatabaseEntities();

        // GET: Admin/Customers
        public async Task<ActionResult> Index()
        {
            return View(await db.Customers.ToListAsync());
        }

        // GET: Admin/Customers/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customers customers = await db.Customers.FindAsync(id);
            if (customers == null)
            {
                return HttpNotFound();
            }
            return View(customers);
        }

        // GET: Admin/Customers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Customers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,PhoneNumber,Address,DOF,balance,email")] Customers customers)
        {
            if (ModelState.IsValid)
            {
                Customers c = db.Customers.Where(m => m.Id == customers.Id).FirstOrDefault();
                if (c == null)
                {
                    if (CheckEmailExist(customers.email))
                    {
                        customers.balance = 0;
                        db.Customers.Add(customers);
                        await db.SaveChangesAsync();
                        CreateAccount(customers.Id, customers.email, customers.Name);
                        return RedirectToAction("Index");
                    }
                    ViewBag.ErrorEmail = "Email already exist !";
                    return View(customers);
                }
                ViewBag.ErrorCMTND = "CMTND already exist !";
                return View(customers);
            }

            return View(customers);
        }

        private bool CheckEmailExist(string email)
        {
            Customers c = db.Customers.Where(m => m.email == email).FirstOrDefault();
            if (c == null) {
                return true;
            }
            return false;
        }

        private void CreateAccount(string id,string email,string Name)
        {
            string password = GeneratePass(Name, id);
            Send(email, password);
            Account account = new Account();
            account.Num_id = id;
            account.Usn = email;
            account.Pwd = r.HashPwd(password);
            account.A_Status = 0;
            db.Account.Add(account);
            db.SaveChanges();
        }
        //Viet trong repo roi viet lai lam` chi?

        public void Send(string mailAdress, string pass)
        {
            var smtpClient = new SmtpClient();
            var msg = new MailMessage();
            msg.To.Add(mailAdress);
            msg.Subject = "Test";
            msg.Body = "Your Password is: " + pass;
            smtpClient.Send(msg);
        }

        //EX: DAO NGOC HAI,id=123456789 -> pass = hai213634
        private string GeneratePass(string name, string id)
        {
            var next = id.Substring(0, 6);
            var stringChars = new char[6];
            //split to get lastname
            string pass = name.Split(null).Last().ToLower();
            //Random from id
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = next[random.Next(next.Length)];
            }
            pass = pass + new String(stringChars);
            return pass;

        }
        public ActionResult AddBalance() {
            return View();
        }
        [HttpPost]
        public ActionResult AddBalance(double money, string idSend, string idReceiver, string mess) {
            Customers Send = db.Customers.Find(idSend);
            Customers Reciver = db.Customers.Find(idReceiver);
            if (Send != null)
            {            
                Reciver.balance += money;
                db.Entry(Reciver).State = EntityState.Modified;
                db.SaveChanges();
                Send.balance -= (money+20000);
                db.Entry(Send).State = EntityState.Modified;
                db.SaveChanges();
                SaveHistory(money,mess,"CT",idSend,idReceiver,20000);
                return RedirectToAction("Index");
            }
            else {             
                Reciver.balance += money;
                db.Entry(Reciver).State = EntityState.Modified;
                db.SaveChanges();
                SaveHistory(money, mess, "CT", "0", idReceiver,20000);
                return RedirectToAction("Index");
            }
        }
        public void SaveHistory(double money, string Mess, string code, string idFrom, string idTo,double fee)
        {
            TransactionHistory history = new TransactionHistory()
            {

                Amount = (decimal)money,
                Message = Mess,
                Code = code,
                SendAccount = idFrom,
                ReceiveAccount = idTo,
                Bank_id = 1,
                Status = "0",
                fee=fee,
                tran_time=DateTime.Now.ToString()
            };
            db.TransactionHistory.Add(history);
            db.SaveChanges();
        }
        public JsonResult getCustomer(string id)
        {
            string Name = "";
            try
            {
                var Cus = db.Customers.Find(id);
                if (Cus != null)
                {
                    Name += Cus.Name;
                }

            }
            catch (Exception e)
            {
            }

            return Json(Name, JsonRequestBehavior.AllowGet);
        }
        // GET: Admin/Customers/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customers customers = await db.Customers.FindAsync(id);
            if (customers == null)
            {
                return HttpNotFound();
            }
            return View(customers);
        }

        // POST: Admin/Customers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,PhoneNumber,Address,DOF,acc_num,balance,email")] Customers customers)
        {
            if (ModelState.IsValid)
            {
                db.Entry(customers).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(customers);
        }

        // GET: Admin/Customers/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customers customers = await db.Customers.FindAsync(id);
            if (customers == null)
            {
                return HttpNotFound();
            }
            return View(customers);
        }

        // POST: Admin/Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Customers customers = await db.Customers.FindAsync(id);
            db.Customers.Remove(customers);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
