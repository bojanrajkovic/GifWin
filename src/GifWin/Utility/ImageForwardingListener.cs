using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace GifWin.Utility
{
    class ImageForwardingListener
    {
        static readonly Lazy<ImageForwardingListener> lazy = 
            new Lazy<ImageForwardingListener> (() => new ImageForwardingListener ());

        static readonly Random r = new Random ();

        public static ImageForwardingListener Instance => lazy.Value;

        readonly HttpListener forwarder;
        Task forwarderAcceptor;

        public Uri ForwarderBase { get; private set; }

        internal ImageForwardingListener ()
        {
            forwarder = new HttpListener ();
        }

        public void Start ()
        {
            while (true) {
                var port = r.Next (49152, 65535);
                ForwarderBase = new Uri ($"http://localhost:{port}/");
                forwarder.Prefixes.Add (ForwarderBase.ToString ());
                try {
                    forwarder.Start ();
                    break;
                } catch (Exception) {
                }
            }

            forwarderAcceptor = Task.Factory.StartNew (async () => {
                while (true) {
                    var ctx = await forwarder.GetContextAsync ();
                    var action = ctx.Request.Url.Segments[1];

                    if (action == "forward") {
                        var qs = ctx.Request.QueryString;
                        var source = qs["source"];

                        if (!string.IsNullOrWhiteSpace (source)) {
                            var decodedSource = Uri.UnescapeDataString (source);
                            using (var client = new HttpClient ()) {
                                try {
                                    var sourceResp = await client.GetStreamAsync (decodedSource);
                                    ctx.Response.StatusCode = 200;
                                    await sourceResp.CopyToAsync (ctx.Response.OutputStream);
                                    ctx.Response.Close ();
                                } catch (Exception) {
                                    ctx.Response.StatusCode = 404;
                                    ctx.Response.Close ();
                                }
                            }
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        public string BuildForwardingUrl (string value)
        {
            var urlBuilder = new UriBuilder (ForwarderBase);
            urlBuilder.Path = "/forward";
            urlBuilder.Query = QueryString.Create ("source", value).ToString ().TrimStart ('?');
            return urlBuilder.ToString ();
        }
    }
}
