﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Http;
using Microsoft.Framework.WebEncoders;

namespace StatusCodePagesSample
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseRequestServices();

            app.UseErrorPage(ErrorPageOptions.ShowAll);
            app.UseStatusCodePages(); // There is a default response but any of the following can be used to change the behavior.

            // app.UseStatusCodePages(context => context.HttpContext.Response.SendAsync("Handler, status code: " + context.HttpContext.Response.StatusCode, "text/plain"));
            // app.UseStatusCodePages("text/plain", "Response, status code: {0}");
            // app.UseStatusCodePagesWithRedirects("~/errors/{0}"); // PathBase relative
            // app.UseStatusCodePagesWithRedirects("/base/errors/{0}"); // Absolute
            // app.UseStatusCodePages(builder => builder.UseWelcomePage());
            // app.UseStatusCodePagesWithReExecute("/errors/{0}");

            // "/[?statuscode=400]"
            app.Use(async (context, next) =>
            {
                // Check for ?statuscode=400
                var requestedStatusCode = context.Request.Query["statuscode"];
                if (!string.IsNullOrEmpty(requestedStatusCode))
                {
                    context.Response.StatusCode = int.Parse(requestedStatusCode);

                    // To turn off the StatusCode feature - For example the below code turns off the StatusCode middleware 
                    // if the query contains a disableStatusCodePages=true parameter.
                    var disableStatusCodePages = context.Request.Query["disableStatusCodePages"];
                    if (disableStatusCodePages == "true")
                    {
                        var statusCodePagesFeature = context.GetFeature<IStatusCodePagesFeature>();
                        if (statusCodePagesFeature != null)
                        {
                            statusCodePagesFeature.Enabled = false;
                        }
                    }

                    await Task.FromResult(0);
                }
                else
                {
                    await next();
                }
            });

            // "/errors/400"
            app.Map("/errors", error =>
            {
                error.Run(async context =>
                {
                    var htmlEncoder = context.ApplicationServices.GetHtmlEncoder();
                    var builder = new StringBuilder();
                    builder.AppendLine("<html><body>");
                    builder.AppendLine("An error occurred, Status Code: " + htmlEncoder.HtmlEncode(context.Request.Path.ToString().Substring(1)) + "<br>");
                    var referrer = context.Request.Headers["referer"];
                    if (!string.IsNullOrEmpty(referrer))
                    {
                        builder.AppendLine("<a href=\"" + htmlEncoder.HtmlEncode(referrer) + "\">Retry " + htmlEncoder.HtmlEncode(referrer) + "</a><br>");
                    }
                    builder.AppendLine("</body></html>");
                    await context.Response.SendAsync(builder.ToString(), "text/html");
                });
            });

            app.Run(async context =>
            {
                var htmlEncoder = context.ApplicationServices.GetHtmlEncoder();
                // Generates the HTML with all status codes.
                var builder = new StringBuilder();
                builder.AppendLine("<html><body>");
                builder.AppendLine("<a href=\"" +
                    htmlEncoder.HtmlEncode(context.Request.PathBase.ToString()) + "/missingpage/\">" +
                    htmlEncoder.HtmlEncode(context.Request.PathBase.ToString()) + "/missingpage/</a><br>");

                var space = string.Concat(Enumerable.Repeat("&nbsp;", 12));
                builder.AppendFormat("<br><b>{0}{1}{2}</b><br>", "Status Code", space, "Status Code Pages");
                for (int statusCode = 400; statusCode < 600; statusCode++)
                {
                    builder.AppendFormat("{0}{1}{2}{3}<br>",
                        statusCode,
                        space + space,
                        string.Format("<a href=\"?statuscode={0}\">[Enabled]</a>{1}", statusCode, space),
                        string.Format("<a href=\"?statuscode={0}&disableStatusCodePages=true\">[Disabled]</a>{1}", statusCode, space));
                }

                builder.AppendLine("</body></html>");
                await context.Response.SendAsync(builder.ToString(), "text/html");
            });
        }
    }
}