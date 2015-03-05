﻿using Microsoft.AspNet.Builder;

namespace WelcomePageSample
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseRequestServices();

            app.UseWelcomePage();
        }
    }
}