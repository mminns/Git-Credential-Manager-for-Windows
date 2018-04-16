/**** Git Credential Manager for Windows ****
 *
 * Copyright (c) Atlassian
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the """"Software""""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
**/

using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
// TODO Win32 GUI-Shared
using Bitbucket.Shared.Helpers;

namespace Atlassian.Bitbucket.Authentication.OAuth
{
    /// <summary>
    /// Implements a simple HTTP server capable of handling Bitbucket OAuth callback requests.
    /// </summary>
    public class SimpleServer
    {
        private static readonly string Auth_Html = "<!doctype html><html lang=\"en\"><head>    <meta charset=\"utf-8\">    <title>SourceTree Authentication</title>    <link rel=\"stylesheet\" href=\"http://aui-cdn.atlassian.com/aui-adg/5.9.19/css/aui.min.css\" media=\"all\">    <script src=\"http://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js\"></script>    <script src=\"http://aui-cdn.atlassian.com/aui-adg/5.9.19/js/aui.min.js\"></script></head><body class=\"aui-page-notification aui-page-size-large\">    <div id=\"page\">        <div class=\"aui-page-panel\">            <div class=\"aui-page-panel-inner\">                <section class=\"aui-page-panel-content\">                    <!-- Can't serve this yet :( -->                    <!--<img src=\"http://localhost:58293/atlassian_software-03.png\"></img>-->                    <h2>Authentication Successful</h2>                    <p>SourceTree has been successfully authenticated. You may now close this page.</p>                </section>            </div>        </div>        <footer id=\"footer\" role=\"contentinfo\">            <section class=\"footer-body\">                <ul>                    <li>&hearts;</li>                </ul>                <div id=\"footer-logo\"><a href=\"http://www.atlassian.com/\" target=\"_blank\">Atlassian</a></div>            </section>        </footer>    </div></body></html>";

        /// <summary>
        /// Async wait for an URL with a timeout
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="RemoteHostException">Throws when there's a timeout, or other error</exception>
        public static async Task<string> WaitForURLAsync(string url, CancellationToken cancellationToken)
        {
            string rawUrl = "";
            try
            {
                using (var listener = new HttpListener { Prefixes = { url } })
                {
                    listener.Start();

                    try
                    {
                        var context = await listener.GetContextAsync().RunWithCancellation(cancellationToken);
                        rawUrl = context.Request.RawUrl;

                        //Serve back a simple auth message.
                        var html = GetSuccessString();
                        context.Response.ContentType = "text/html";
                        context.Response.OutputStream.WriteStringUtf8(html);

                        await Task.Delay(100); //Wait 100ms without this the server closes before the complete response has been written

                        context.Response.Close();
                    }
                    catch (TimeoutException ex)
                    {
                        throw new Exception("Timeout awaiting incoming request.", ex);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failure awating incoming request.", ex);
                    }
                    finally
                    {
                        listener.Stop();
                        //listener.Close();
                    }
                }
            }
            catch(Exception ex)
            {
                // do nothing on macOS see corefx/issues/24562
            }

            return rawUrl;
        }

        /// <summary>
        /// Returns a Success HTML page or a simple success message, if the HTML page cannot be
        /// loaded, to be served back to the user.
        /// </summary>
        /// <returns></returns>
        private static string GetSuccessString()
        {
            var result = "Auth Successful. You may now close this page";

            try
            {
                using (StreamReader reader = new StreamReader(GetHtmlStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return result;
            }

            return result;
        }

        public static Stream GetHtmlStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(Auth_Html);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
