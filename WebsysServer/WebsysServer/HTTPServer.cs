using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using WebsysServer.tool;
using System.Threading;
using System.Threading.Tasks;
namespace WebsysServer
{
    class HTTPServer
    {

        private readonly ManualResetEvent stopRequested = new ManualResetEvent(false);
        private readonly int maxConcurrency = 100; // 最大并发请求数
        private readonly Semaphore threadPoolSemaphore;
        //public static ManualResetEvent myEvent = new ManualResetEvent(false);
        HttpListener httpListener;
        List<Thread> threadList = null;
        public Boolean IsAlive = false;
        public Form1 mainForm;
        //int CurrentThreadNum = 0;
        public HTTPServer()
        {
            threadPoolSemaphore = new Semaphore(maxConcurrency, maxConcurrency);
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
                try
                {
                    httpListener.Start();
                    Logging.Log(LogLevel.Info,"WebServer Start Successed...");
                }
                catch(Exception ex)
                {
                    Logging.Error("启动监听服务失败");
                    Logging.Error(ex);
                    Logging.Error("解决办法：使用管理员进入cmd,运行下面4句语句");
                    Logging.Error("netsh http delete urlacl url = http://*:11996/");
                    Logging.Error("netsh http delete urlacl url = https://*:21996/");
                    Logging.Error("netsh http add urlacl url = http://*:11996/ user=\"\\Everyone\"");
                    Logging.Error("netsh http add urlacl url = https://*:21996/ user=\"\\Everyone\"");
                }
                
                
                //Logging.Log(LogLevel.Info, url);
                while (!stopRequested.WaitOne(0))
                {
                    // 没有请求则GetContext处于阻塞状态
                    try {
                        HttpListenerContext ctx = httpListener.GetContext();
                        //可以用来判定白名单(request.RemoteEndPoint.Address.ToString() == "::1" || request.RemoteEndPoint.Address.ToString() == "127.0.0.1")
                        if (ctx.Request.IsLocal)
                        {
                            if (!threadPoolSemaphore.WaitOne(3)) {  // 获取一个信号量, 当请求数达到max值时,最多等待3秒
                                ctx.Response.StatusCode = (int)HttpStatusCode.RequestEntityTooLarge;
                                ctx.Response.StatusDescription = "Too many requests";
                                ctx.Response.Close();
                                continue;
                            };
                            Task.Factory.StartNew(state =>
                            {
                                HttpListenerContext context = (HttpListenerContext)state;
                                try
                                {
                                    // 创建新线程处理（如果 RequestHandler 需要 STA）
                                    Thread handlerThread = new Thread(() =>
                                    {
                                        new HTTPRequestHandler(context, d.ReqTimeOut, mainForm).RequestHandler();
                                    });
                                    handlerThread.Name = "C" + handlerThread.ManagedThreadId;
                                    handlerThread.SetApartmentState(ApartmentState.STA);
                                    handlerThread.IsBackground = true;
                                    handlerThread.Start();
                                    handlerThread.Join(); // 可选：等待线程完成
                                }
                                finally
                                {
                                    threadPoolSemaphore.Release(); // 释放信号量
                                }
                            }, ctx, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
                        
                            //new HTTPRequestHandler(ctx, d.ReqTimeOut).RequestHandler(); //单线程处理

                            /* 多线程处理请求*/
                            // httpListener.BeginGetContext(new AsyncCallback(GetContextCallBack), httpListener);
                            // Logging.Error("-------------------client ApartmentState.STA------------");
                            /*20250610修改成信号量控制线程数量
                            Thread clientThread = new Thread(new HTTPRequestHandler(ctx, d.ReqTimeOut, mainForm).RequestHandler);
                            clientThread.Name = "C" + clientThread.ManagedThreadId;
                            // 血液净化----要求单线程单元 
                            clientThread.SetApartmentState(ApartmentState.STA); //设置这个参数，指示应用程序的COM线程模型 是 单线程单元                                                                            
                                                                                // 在多线程条件下，主线程如果关闭，那么子线没有跑完的情况下。 子线还在跑。                        
                                                                                // 前台线程；后台线程；                        
                                                                                // 前台线程：                        
                                                                                // 只有所有的前台程序都关闭了，才能完成程序的关闭；
                                                                                // 后台线程：
                                                                                // 只要所有的前台线程结束，后台线程自动结束。   自动。
                                                                                // 默认新创建的线程，都是前台线程。只有所有前台线程都结束，后台才自动结束，完成整个程序的关闭。
                                                                                // 所以为了关闭线程，我们将client线程，设置为后台线程。
                                                                                // 将线程设置为后台线程
                            clientThread.IsBackground = true;
                            clientThread.Start();*/
                            //myEvent.WaitOne();
                            //threadList.
                        }else{
                            // 如果不是本地请求，则拒绝访问
                            ctx.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            ctx.Response.StatusDescription = "Access Forbidden";
                            ctx.Response.Close();
                        }
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
            //stopRequested.Set();
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
