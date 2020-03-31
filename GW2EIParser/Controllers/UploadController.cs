﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GW2EIParser.Controllers
{
    public static class UploadController
    {
        private static string UploadDPSReportsEI(FileInfo fi)
        {
            return UploadToDPSR(fi, "https://dps.report/uploadContent?generator=ei");
        }
        private static string UploadDPSReportsRH(FileInfo fi)
        {
            return UploadToDPSR(fi, "https://dps.report/uploadContent?generator=rh");

        }
        private static string UploadRaidar(/*FileInfo fi*/)
        {
            //string fileName = fi.Name;
            //byte[] fileContents = File.ReadAllBytes(fi.FullName);
            //Uri webService = new Uri(@"https://www.gw2raidar.com/api/v2/encounters/new");
            //HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, webService);
            //requestMessage.Headers.ExpectContinue = false;

            //MultipartFormDataContent multiPartContent = new MultipartFormDataContent("----MyGreatBoundary");
            //ByteArrayContent byteArrayContent = new ByteArrayContent(fileContents);
            //byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");
            //multiPartContent.Add(byteArrayContent, "file", fileName);
            //requestMessage.Content = multiPartContent;

            //HttpClient httpClient = new HttpClient();
            //try
            //{
            //    Task<HttpResponseMessage> httpRequest = httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
            //    HttpResponseMessage httpResponse = httpRequest.Result;
            //    HttpStatusCode statusCode = httpResponse.StatusCode;
            //    HttpContent responseContent = httpResponse.Content;

            //    if (responseContent != null)
            //    {
            //        Task<String> stringContentsTask = responseContent.ReadAsStringAsync();
            //        String stringContents = stringContentsTask.Result;
            //        int first = stringContents.IndexOf('{');
            //        int length = stringContents.LastIndexOf('}') - first + 1;
            //        String JSONFormat = stringContents.Substring(first, length);
            //        DPSReportsResponseItem item = JsonConvert.DeserializeObject<DPSReportsResponseItem>(JSONFormat);
            //        String logLink = item.permalink;
            //        return logLink;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    return ex.Message;
            //    // Console.WriteLine(ex.Message);
            //}
            return "";
        }
        private class DPSReportsResponseItem
        {
            public string Permalink { get; set; }
        }
        private static string UploadToDPSR(FileInfo fi, string URI)
        {
            string fileName = fi.Name;
            byte[] fileContents = File.ReadAllBytes(fi.FullName);
            const int tentatives = 5;
            string res = "Upload process failed";
            for (int i = 0; i < tentatives; i++)
            {
                var webService = new Uri(@URI);
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, webService);
                requestMessage.Headers.ExpectContinue = false;

                var multiPartContent = new MultipartFormDataContent("----MyGreatBoundary");
                var byteArrayContent = new ByteArrayContent(fileContents);
                byteArrayContent.Headers.Add("Content-Type", "application/octet-stream");
                multiPartContent.Add(byteArrayContent, "file", fileName);
                //multiPartContent.Add(new StringContent("generator=ei"), "gen", "ei");
                requestMessage.Content = multiPartContent;

                var httpClient = new HttpClient();
                try
                {
                    Task<HttpResponseMessage> httpRequest = httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
                    HttpResponseMessage httpResponse = httpRequest.Result;
                    HttpStatusCode statusCode = httpResponse.StatusCode;
                    HttpContent responseContent = httpResponse.Content;

                    if (responseContent != null)
                    {
                        Task<string> stringContentsTask = responseContent.ReadAsStringAsync();
                        string stringContents = stringContentsTask.Result;
                        int first = stringContents.IndexOf('{');
                        int length = stringContents.LastIndexOf('}') - first + 1;
                        string JSONFormat = stringContents.Substring(first, length);
                        DPSReportsResponseItem item = JsonConvert.DeserializeObject<DPSReportsResponseItem>(JSONFormat, new JsonSerializerSettings
                        {
                            ContractResolver = new DefaultContractResolver()
                            {
                                NamingStrategy = new CamelCaseNamingStrategy()
                            }
                        });
                        return item.Permalink;
                    }
                }
                catch (Exception e)
                {
                    Exception finalException = e;
                    while (finalException.InnerException != null)
                    {
                        finalException = finalException.InnerException;
                    }
                    res = finalException.Message;
                }
                finally
                {
                    byteArrayContent.Dispose();
                    httpClient.Dispose();
                    requestMessage.Dispose();
                }
            }        
            return res;
        }

        public static string[] UploadOperation(GridRow row, FileInfo fInfo)
        {
            //Upload Process
            Task<string> DREITask = null;
            Task<string> DRRHTask = null;
            Task<string> RaidarTask = null;
            string[] uploadresult = new string[3] { "", "", "" };
            if (Properties.Settings.Default.UploadToDPSReports)
            {
                row.BgWorker.UpdateProgress(row, " 45% - Uploading to DPSReports using EI...", 45);
                DREITask = Task.Run(() => UploadDPSReportsEI(fInfo));
                if (DREITask != null)
                {
                    uploadresult[0] = DREITask.Result;
                }
                else
                {
                    uploadresult[0] = "Failed to Define Upload Task";
                }
            }
            row.BgWorker.ThrowIfCanceled(row);
            if (Properties.Settings.Default.UploadToDPSReportsRH)
            {
                row.BgWorker.UpdateProgress(row, " 45% - Uploading to DPSReports using RH...", 45);
                DRRHTask = Task.Run(() => UploadDPSReportsRH(fInfo));
                if (DRRHTask != null)
                {
                    uploadresult[1] = DRRHTask.Result;
                }
                else
                {
                    uploadresult[1] = "Failed to Define Upload Task";
                }
            }
            row.BgWorker.ThrowIfCanceled(row);
            if (Properties.Settings.Default.UploadToRaidar)
            {
                row.BgWorker.UpdateProgress(row, " 45% - Uploading to Raidar...", 45);
                RaidarTask = Task.Run(() => UploadRaidar(/*fInfo*/));
                if (RaidarTask != null)
                {
                    uploadresult[2] = RaidarTask.Result;
                }
                else
                {
                    uploadresult[2] = "Failed to Define Upload Task";
                }
            }
            row.BgWorker.ThrowIfCanceled(row);
            return uploadresult;
        }

    }

}
