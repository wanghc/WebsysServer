using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using WebsysServer.tool;
using System.Threading;
namespace WebsysServer
{
    class HTTPServer
    {

        //public static ManualResetEvent myEvent = new ManualResetEvent(false);
        HttpListener httpListener;
        List<Thread> threadList = null;
        public Boolean IsAlive = false;
        //int CurrentThreadNum = 0;
        public HTTPServer()
        {
        }
        public void Start (){
            using (httpListener = new HttpListener())
            {
                var d = Properties.Settings.Default;
                IsAlive = true;
                httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
                //httpListener.AuthenticationSchemes = AuthenticationSchemes.Basic;
                String url = string.Format("http://{0}:{1}{2}","*", d.HttpServerPort,  d.HttpServerApplication); 
                String urls = string.Format("https://{0}:{1}{2}","*", d.HttpsServerPort, d.HttpServerApplication);
                //String[] prefixes = { "http://*:11996/", "https://*:21996/" };
                httpListener.Prefixes.Add(url);
                httpListener.Prefixes.Add(urls);
                httpListener.Start();
                Logging.Log(LogLevel.Info,"WebServer Start Successed...");
                //Logging.Log(LogLevel.Info, url);
                while (true)
                {
                    // 没有请求则GetContext处于阻塞状态
                    try { 
                        HttpListenerContext ctx = httpListener.GetContext();

                        //new HTTPRequestHandler(ctx, d.ReqTimeOut).RequestHandler(); //单线程处理

                        /* 多线程处理请求*/
                        // httpListener.BeginGetContext(new AsyncCallback(GetContextCallBack), httpListener);
                            Logging.Error("-------------------client ApartmentState.STA------------");
                            Thread clientThread = new Thread(new HTTPRequestHandler(ctx, d.ReqTimeOut).RequestHandler);
                            clientThread.Name = "C" + clientThread.ManagedThreadId;
                            // 血液净化----要求单线程单元 
                            clientThread.SetApartmentState(ApartmentState.STA); //设置这个参数，指示应用程序的COM线程模型 是 单线程单元
                            clientThread.Start();
                        
                        //myEvent.WaitOne();
                        //threadList.
                    }
                    catch(Exception ex)
                    {
                        //Logging.Error("服务器获得上下文异常-"+ex.Message);
                        //Logging.LogUsefulException(ex);
                    }
                    finally
                    {
                        
                    }
                    //new HTTPServerHandler(ctx).RequestHandler();
                    // new Thread(HandleRequest).StartAsync(ctx);
                }
            }
        }
        private void Stop()
        {
            /*cts.Cancel();
            if (processingTask != null && !processingTask.IsCompleted)
            {
                processingTask.Wait();
            }*/
            if (this.httpListener.IsListening)
            {
                this.httpListener.Stop();
                this.httpListener.Prefixes.Clear();
            }
        }
        public void Dispose()
        {
            this.Stop();
            this.httpListener.Close(); //处理完请求后关闭
            //using (this.cts) { }
            using (this.httpListener) { }
            IsAlive = false;
        }

    }
}
