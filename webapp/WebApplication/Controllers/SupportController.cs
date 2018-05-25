﻿using K9.Base.WebApplication.Config;
using K9.Base.WebApplication.Controllers;
using K9.Base.WebApplication.ViewModels;
using K9.Globalisation;
using K9.SharedLibrary.Extensions;
using K9.SharedLibrary.Helpers;
using K9.SharedLibrary.Models;
using K9.WebApplication.Config;
using K9.WebApplication.Models;
using K9.WebApplication.Services;
using NLog;
using System;
using System.Web.Mvc;
using K9.DataAccessLayer.Models;
using K9.WebApplication.Services.Stripe;

namespace K9.WebApplication.Controllers
{
    public class SupportController : BaseController
    {
        private readonly ILogger _logger;
        private readonly IMailer _mailer;
        private readonly IStripeService _stripeService;
        private readonly IDonationService _donationService;
        private readonly StripeConfiguration _stripeConfig;
        private readonly WebsiteConfiguration _config;

        public SupportController(ILogger logger, IDataSetsHelper dataSetsHelper, IRoles roles, IMailer mailer, IOptions<WebsiteConfiguration> config, IAuthentication authentication, IFileSourceHelper fileSourceHelper, IStripeService stripeService, IOptions<StripeConfiguration> stripeConfig, IDonationService donationService)
            : base(logger, dataSetsHelper, roles, authentication, fileSourceHelper)
        {
            _logger = logger;
            _mailer = mailer;
            _stripeService = stripeService;
            _donationService = donationService;
            _stripeConfig = stripeConfig.Value;
            _config = config.Value;
        }

        [HttpGet]
        public ActionResult ContactUs()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContactUs(ContactUsViewModel model)
        {
            try
            {
                _mailer.SendEmail(
                    model.Subject,
                    model.Body,
                    _config.SupportEmailAddress,
                    _config.CompanyName,
                    model.EmailAddress,
                    model.Name);

                return RedirectToAction("ContactUsSuccess");
            }
            catch (Exception ex)
            {
                _logger.Error(ex.GetFullErrorMessage());
                return View("FriendlyError");
            }
        }

        public ActionResult ContactUsSuccess()
        {
            return View();
        }

        [Route("donate/start")]
        public ActionResult DonateStart()
        {
            return View(new StripeModel
            {
                DonationAmount = 10
            });
        }

        [Route("sponsor-iboga/start")]
        public ActionResult SponsorIbogaStart()
        {
            return View(new StripeModel
            {
                NumberOfTrees = 1,
                LocalisedCurrencyThreeLetters = "EUR" // We need to fix the currency to sponsor a tree
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Donate(StripeModel model)
        {
            model.PublishableKey = _stripeConfig.PublishableKey;
            return View(model);
        }

        [Route("sponsor-iboga")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SponsorIboga(StripeModel model)
        {
            if (model.NumberOfTrees < 1)
            {
                // Silly, but you never know - may as well procedd with minimum one
                model.NumberOfTrees = 1;
            }
            model.PublishableKey = _stripeConfig.PublishableKey;
            return View(model);
        }

        [Route("donate/success")]
        public ActionResult DonationSuccess()
        {
            return View();
        }

        [Route("sponsor-iboga/success")]
        public ActionResult SponsorSuccess()
        {
            return View();
        }

        [HttpPost]
        [Route("donate/processing")]
        [ValidateAntiForgeryToken]
        public ActionResult DonateProcess(StripeModel model)
        {
            try
            {
                model.Description = Dictionary.DonationToBOTF;
                _stripeService.Charge(model);
                _donationService.CreateDonation(new Donation
                {
                    Currency = model.LocalisedCurrencyThreeLetters,
                    Customer = model.StripeBillingName,
                    CustomerEmail = model.StripeEmail,
                    DonationDescription = model.Description,
                    DonatedOn = DateTime.Now,
                    DonationAmount = model.AmountToDonate
                });
                return RedirectToAction("DonationSuccess");
            }
            catch (Exception ex)
            {
                _logger.Error($"SupportController => Donate => Donation failed: {ex.Message}");
                ModelState.AddModelError("", ex.Message);
            }

            return View("Donate", model);
        }

        [HttpPost]
        [Route("sponsor-iboga/processing")]
        [ValidateAntiForgeryToken]
        public ActionResult SponsorProcess(StripeModel model)
        {
            try
            {
                model.Description = Dictionary.SponsorIbogaTree;
                var transactionId = _stripeService.Charge(model);
                _donationService.CreateDonation(new Donation
                {
                    StripeId = transactionId,
                    Currency = model.LocalisedCurrencyThreeLetters,
                    Customer = model.StripeBillingName,
                    CustomerEmail = model.StripeEmail,
                    DonationDescription = model.Description,
                    DonatedOn = DateTime.Now,
                    DonationAmount = model.DonationAmount,
                    NumberOfIbogas = model.NumberOfTrees
                });
                return RedirectToAction("SponsorSuccess");
            }
            catch (Exception ex)
            {
                _logger.Error($"SupportController => Sponsor => Sponsor failed: {ex.Message}");
                ModelState.AddModelError("", ex.Message);
            }

            return View("SponsorIboga", model);
        }
        
        public override string GetObjectName()
        {
            throw new NotImplementedException();
        }
    }
}
