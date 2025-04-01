using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using WerServer.Helpers;
using WerServer.Models;

namespace WerServer.Handlers;

public class WebServer
{
    static HttpListener listener;
    private Thread listenThread1;
    private List<string> prefixes;

    public WebServer()
    {
        this.prefixes = new List<string>() { "http://*:8888/" };
    }

    public WebServer(List<string> prefixes)
    {
        this.prefixes = prefixes;
    }

    public bool Start()
    {
        bool success = true;
        if (!HttpListener.IsSupported)
        {
            throw new Exception("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
        }

        // Create a listener.
        listener = new HttpListener();
        // Add the prefixes.
        foreach (string s in this.prefixes)
        {
            listener.Prefixes.Add(s);
            Console.WriteLine($"server on {s}...");
        }
        listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
        try
        {
            listener.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR: Cannot start web server! Reason: " + e.Message + " Ensure the server is running as Administrator and that the server is not already running.");
            success = false;
        }

        if (success)
        {
            try
            {
                listenThread1 = new Thread(new ParameterizedThreadStart(startlistener));
                listenThread1.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: Failed to start listening. Reason:", e.Message + " Ensure the port for the server is not already in use.");
            }
            Console.WriteLine("Listening...");
        }

        return success;
    }

    private void startlistener(object s)
    {
        while (true)
        {
            ////blocks until a client has connected to the server
            ProcessRequest();
        }
    }

    private void ProcessRequest()
    {
        var result = listener.BeginGetContext(ListenerCallback, listener);
        result.AsyncWaitHandle.WaitOne();
    }

    private async void ListenerCallback(IAsyncResult result)
    {
        HttpListenerContext context = listener.EndGetContext(result);
        Uri uri = context.Request.Url;
        Console.WriteLine($"Revived request for {uri}");

        List<DataFormat> dataValues = new List<DataFormat>();
        //get data from body
        string cleaned_data;
        if (context.Request.HasEntityBody)
        {
            string data_text = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();
            dataValues.AddRange(RequestDataHelper.GetValues(System.Web.HttpUtility.UrlDecode(data_text)));
        }
        else cleaned_data = string.Empty;

        //get data form url
        dataValues.AddRange(RequestDataHelper.GetValues(context.Request.QueryString));
        cleaned_data = ObjectsHelper.ToJson(dataValues);

        // Get a response stream and write the response to it.
        HttpListenerResponse response = context.Response;
        response.StatusCode = 200;
        response.StatusDescription = "OK";
        response.AddHeader("Access-Control-Allow-Origin", "*");
        response.AddHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS");
        response.AddHeader("Access-Control-Max-Age", "1000");
        response.AddHeader("Access-Control-Allow-Header", "Content-Type");
        response.ContentType = "application/json; charset=utf-8";
        //append the data response
        byte[] buffer;
        Stream output = new MemoryStream();
        Console.WriteLine("Setup completed...");
        try
        {
            switch (uri.LocalPath.ToLower())
            {
                case "/printerlist":
                case "printerlist":
                    //get the printer list to show
                    Console.WriteLine("Getting printer List...");
                    buffer = Encoding.ASCII.GetBytes(ObjectsHelper.ToJson(WindowsManagement.PopulateInstalledPrinters()));
                    response.ContentLength64 = buffer.Length;
                    output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    break;
                default:
                    Console.WriteLine("Printing document...");
                    WindowsPrint winPrint = new WindowsPrint(dataValues);
                    bool print = await winPrint.PrintUrl();
                    if (print)
                    {
                        Console.WriteLine("Print successful");
                        buffer = Encoding.ASCII.GetBytes(ObjectsHelper.ToJson(print));
                    }
                    else
                    {
                        Console.WriteLine("Error printing document...");
                        buffer = Encoding.ASCII.GetBytes(ObjectsHelper.ToJson(print, "Error printing document"));
                    }
                    winPrint.Dispose();
                    response.ContentLength64 = buffer.Length;
                    output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    // must close the output stream.
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to print. Reason: {ex.Message}", ex);
            buffer = Encoding.ASCII.GetBytes(ObjectsHelper.ToJson(false, ex.Message));
            response.ContentLength64 = buffer.Length;
            output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            if (ex.InnerException is AuthenticationException)
            {
                Console.WriteLine($"ERROR: {ex.InnerException.Message}");
            }
        }
        finally
        {
            // must close the output stream.
            output.Close();
        }
        context.Response.Close();
    }
}
