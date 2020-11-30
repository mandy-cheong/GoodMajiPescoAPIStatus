using goodmaji;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

/// <summary>
/// Summary description for APIHelper
/// </summary>
public class APIHelper
{
    public string Url { get; set; }
    public string ContentType { get; set; }
    public string Method { get; set; }
    public string RequestData { get; set; }
    public string ResponseData { get; set; }

    public APIHelper()
    {
    }
   
    public RVal GETApi()
    {
        HttpWebRequest request = GenerateRequest();
        HttpWebResponse response = null;
        request.Method = "GET";
        var rval = new RVal();
        try
        {
            response = (HttpWebResponse)request.GetResponse();          
        }
       
        catch (WebException e)
        {
            //網站回應錯誤,
            response = (HttpWebResponse)e.Response;
        }
       finally
        {
            rval = GetResponseMsg(response);
        }
        return rval;
    }

   

    public RVal PostApi()
    {
        HttpWebRequest request = GenerateRequest();
        request.Method = "POST";
        HttpWebResponse response=null;
        var rval = new RVal();
        try
        {
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(RequestData);
                streamWriter.Flush();
                streamWriter.Close();
            }
            response = (HttpWebResponse)request.GetResponse();

        }

        catch (WebException e)
        {
            //網站回應錯誤,
            response = (HttpWebResponse)e.Response;
        }
        finally
        {
            rval = GetResponseMsg(response);
        }
        return rval;
    }

    public HttpWebRequest GenerateRequest()
    {
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(Url);
        var prescoAPI = new PRESCOAPI();
        if (!string.IsNullOrEmpty(ContentType))
            httpWebRequest.ContentType = ContentType;
        httpWebRequest.Headers.Add("Authorization", prescoAPI.PostToken());

        return httpWebRequest;
    }

   

    public RVal GetResponseMsg(HttpWebResponse response)
    {
        using (var streamReader = new StreamReader(response.GetResponseStream()))
        {
            ResponseData = streamReader.ReadToEnd();
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return new RVal { RStatus = true, RMsg = ResponseData };
                case HttpStatusCode.BadRequest:
                    return new RVal { RStatus = false, RMsg = ResponseData };
                default:
                    return new RVal { RStatus = false, RMsg = ResponseData };
            }
        }
    }
    
}
