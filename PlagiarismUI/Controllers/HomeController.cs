using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Common.DataModels;
using System.Security.Cryptography;
using System.IO;
using Common.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using PlagiarismUI.Models;
using Microsoft.AspNetCore.Authorization;

namespace PlagiarismUI.Controllers
{
    public class HomeController : Controller
    {
        private IHostingEnvironment hostingEnv;

        public HomeController(IHostingEnvironment env)
        {
            this.hostingEnv = env;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UploadFile(IFormFile file)
        {
            try
            {
                
                //Read the name and a byte array of the uploaded file
                Document doc = new Document
                {
                    DocName = file.FileName
                };

                var md5 = MD5.Create();
                using (BinaryReader binaryReader = new BinaryReader(file.OpenReadStream()))
                {
                    doc.DocContent = binaryReader.ReadBytes((int)file.Length);
                    doc.DocHash = Convert.ToBase64String(md5.ComputeHash(doc.DocContent)) + User.Identity.Name;
                    doc.DocUser = User.Identity.Name;
                }


                


                IRawProcessing preprocessingClient = ServiceProxy.Create<IRawProcessing>
                (new Uri("fabric:/PlagiarismServices/RawProcessingService"));
                var result = preprocessingClient.DocumentReceivedAsync(doc);




                ViewBag.Message = $"File uploaded \n {file.Length} bytes uploaded successfully";
            }
            catch
            {
                ViewBag.Message = $"No file was given";
            }
            return View();

        }

        [Authorize]
        public IActionResult UploadDocument()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
