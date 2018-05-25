﻿using K9.DataAccessLayer.Models;

namespace K9.WebApplication.Services
{
    public interface IDonationService
    {
        void CreateDonation(Donation donation);
        int GetNumberOfIbogasSponsoredToDate();
        int GetNumberOfIbogasSponsoredLast30Days();
        int GetProjectedNumberOfIbogasSponsoredPerYear();
    }
}