using System;
using System.Windows;
using System.Net;
using GuerrillaNtp;
using System.ComponentModel;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Globalization;

namespace utck_for_pc_bang
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        BackgroundWorker timer_native = new BackgroundWorker();
        BackgroundWorker timer_ntp = new BackgroundWorker();
        BackgroundWorker timer_http = new BackgroundWorker();
        BackgroundWorker multi_sync_ntp = new BackgroundWorker();


        public MainWindow()
        {
            InitializeComponent();
            timer_native.DoWork += timer_native_DoWork;
            timer_native.WorkerReportsProgress = true;
            timer_native.WorkerSupportsCancellation = true;
            timer_ntp.DoWork += timer_ntp_DoWork;
            timer_ntp.WorkerReportsProgress = true;
            timer_ntp.WorkerSupportsCancellation = true;
            timer_http.DoWork += timer_http_DoWork;
            timer_http.WorkerReportsProgress = true;
            timer_http.WorkerSupportsCancellation = true;

            multi_sync_ntp.DoWork += multi_sync_ntp_Dowork;
            multi_sync_ntp.WorkerReportsProgress = true;
            multi_sync_ntp.WorkerSupportsCancellation = true;

            user_count.Navigate("https://blog.naver.com/rta111/222644210496");

            string url = "https://raw.githubusercontent.com/shkalak51/SNU_Physics_Experiment/master/utckpcbang";
            using (WebClient client = new WebClient())
            {
                string ssss = client.DownloadString(url);
                int index = ssss.IndexOfAny(new char[] { '\r', '\n' });
                ssss = index == -1 ? ssss : ssss.Substring(0, index);
                //MessageBox.Show(ssss);
                if (ssss == 1.ToString())
                {
                    sync_ntp();
                    sync_http();
                    initial();
                }
                else if (ssss == 0.ToString())
                {
                    MessageBox.Show("제작자가 배포를 중단하였습니다");
                    Application.Current.Shutdown();
                }
                else
                {
                    MessageBox.Show("신규버전이 업데이트 되었습니다");
                    System.Diagnostics.Process.Start(ssss);
                    Application.Current.Shutdown();
                }
            }
        }

        int global_delay = 3;
        int utc = 9;
        DateTime accuratetime_ntp = DateTime.Now, accuratetime_http = DateTime.Now, nowTime = DateTime.Now;
        double diff_ntp = 0, diff_milli_ntp = 0, diff_http = 0, diff_milli_http = 0;
        int hours_ntp, minutes_ntp, seconds_ntp, milliseconds_ntp;
        int hours_http, minutes_http, seconds_http, milliseconds_http;
        string ntp_name;
        TimeSpan offset_ntp, offset_http;

        private void multi_sync_ntp_Click(object sender, RoutedEventArgs e)
        {
            button_multi_sync_ntp.IsEnabled = false;
            button_sync_ntp.IsEnabled = false;
            multi_sync_ntp.RunWorkerAsync(100);
        }
        private void multi_sync_http_Click(object sender, RoutedEventArgs e)
        {
            button_multi_sync_http.IsEnabled = false;
            button_sync_http.IsEnabled = false;
            multi_sync_http();
            button_multi_sync_http.IsEnabled = true;
            button_sync_http.IsEnabled = true;
        }

        private void always_on_top(object sender, RoutedEventArgs e)
        {
            this_window.Topmost = true;
            
        }

        
        private void not_on_top(object sender, RoutedEventArgs e)
        {
            this_window.Topmost = false;
        }

        private void initial()
        {
            accuratetime_ntp = DateTime.Now;
            nowTime = DateTime.Now;
            diff_ntp = 0;
            diff_milli_ntp = 0;
            hours_ntp = nowTime.Hour;
            minutes_ntp = nowTime.Minute;
            seconds_ntp = nowTime.Second;
            milliseconds_ntp = nowTime.Millisecond;
            timer_native.RunWorkerAsync(1);
            timer_ntp.RunWorkerAsync(1);
            timer_http.RunWorkerAsync(1);
        }


        private void on_closing(object sender, CancelEventArgs e)
        {
            string url = "https://raw.githubusercontent.com/shkalak51/SNU_Physics_Experiment/master/utckpcbang_ad";

            using (WebClient client = new WebClient())
            {
                string ssss = client.DownloadString(url);
                int index = ssss.IndexOfAny(new char[] { '\r', '\n' });
                ssss = index == -1 ? ssss : ssss.Substring(0, index);
                if (ssss != 0.ToString())
                {
                    MessageBox.Show("광고 하나만 띄우겠습니다 ^^ 그냥 닫아주시면 됩니다");
                    System.Diagnostics.Process.Start(ssss);
                    Application.Current.Shutdown();
                }
            }
        }

        public void get_ntp()
        {
            try
            {
                ntp_name = "time2.kriss.re.kr";
                using (var ntp = new NtpClient(Dns.GetHostAddresses(ntp_name)[0]))
                    offset_ntp = ntp.GetCorrectionOffset();
            }
            catch
            {
                try
                {
                    ntp_name = "time.kriss.re.kr";
                    using (var ntp = new NtpClient(Dns.GetHostAddresses(ntp_name)[0]))
                        offset_ntp = ntp.GetCorrectionOffset();
                }
                catch
                {
                    try
                    {
                        ntp_name = "time.google.com";
                        using (var ntp = new NtpClient(Dns.GetHostAddresses(ntp_name)[0]))
                            offset_ntp = ntp.GetCorrectionOffset();
                    }
                    catch
                    {
                        ntp_name = "ntp server error";
                        MessageBox.Show("시간서버에 연결할 수 없습니다");
                        offset_ntp = TimeSpan.Zero;
                    }
                }

            }
        }

        public void sync_ntp()
        {
            get_ntp();
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ntpbox.Text = "사용된 ntp서버 : " + ntp_name;
            }));

            accuratetime_ntp = DateTime.UtcNow + offset_ntp;
            nowTime = DateTime.Now;
            accuratetime_ntp = accuratetime_ntp.AddHours(utc);
            diff_ntp = -(nowTime.Ticks - accuratetime_ntp.Ticks);
            diff_milli_ntp = diff_ntp / 10000;

            if (diff_milli_ntp < 0)
            {
                if (Math.Abs(diff_milli_ntp) >= 1000)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_ntp.Text = "사용자의 pc가 " + string.Format("{0:f4}", -diff_milli_ntp / 1000) + "초 빠릅니다.";
                    }));
                }
                else
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_ntp.Text = "사용자의 pc가 " + string.Format("{0:f0}", -diff_milli_ntp) + "밀리초 빠릅니다.";
                    }));
                }
            }
            else
            {
                if (Math.Abs(diff_milli_ntp) >= 1000)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_ntp.Text = "사용자의 pc가 " + string.Format("{0:f4}", diff_milli_ntp / 1000) + "초 느립니다.";
                    }));
                }
                else
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_ntp.Text = "사용자의 pc가 " + string.Format("{0:f0}", diff_milli_ntp) + "밀리초 느립니다.";
                    }));
                }
            }
        }

        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);
            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }
        public void sync_http()
        {
            DateTime time_before = DateTime.Now;
            string str = http_url.Text;
            try
            {
                var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(str);
                var response = myHttpWebRequest.GetResponse();
                string todaysDates = response.Headers["date"];
                DateTime time_http = DateTime.ParseExact(todaysDates,
                                           "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                                           CultureInfo.InvariantCulture.DateTimeFormat,
                                           DateTimeStyles.AssumeUniversal);
                DateTime time_after = DateTime.Now;
                offset_http = time_http.Add(time_before - time_after) - DateTime.Now;
                httpbox.Text = http_url.Text + "에서 시간을 불러왔습니다";

                diff_milli_http = offset_http.TotalMilliseconds;
            }
            catch
            {
                try
                {
                    str = "https://" + http_url.Text;
                    var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(str);
                    var response = myHttpWebRequest.GetResponse();
                    string todaysDates = response.Headers["date"];
                    DateTime time_http = DateTime.ParseExact(todaysDates,
                                               "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                                               CultureInfo.InvariantCulture.DateTimeFormat,
                                               DateTimeStyles.AssumeUniversal);
                    DateTime time_after = DateTime.Now;
                    offset_http = time_http.Add(time_before - time_after) - DateTime.Now;
                    httpbox.Text = http_url.Text + "에서 시간을 불러왔습니다";

                    diff_milli_http = offset_http.TotalMilliseconds;
                    http_url.Text = str;
                }
                catch
                {
                    try
                    {
                        str = "http://" + http_url.Text;
                        var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(str);
                        var response = myHttpWebRequest.GetResponse();
                        string todaysDates = response.Headers["date"];
                        DateTime time_http = DateTime.ParseExact(todaysDates,
                                                   "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                                                   CultureInfo.InvariantCulture.DateTimeFormat,
                                                   DateTimeStyles.AssumeUniversal);
                        DateTime time_after = DateTime.Now;
                        offset_http = time_http.Add(time_before - time_after) - DateTime.Now;
                        httpbox.Text = http_url.Text + "에서 시간을 불러왔습니다";

                        diff_milli_http = offset_http.TotalMilliseconds;
                        http_url.Text = str;
                    }
                    catch
                    {
                        try
                        {
                            str = "https://www." + http_url.Text;
                            var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(str);
                            var response = myHttpWebRequest.GetResponse();
                            string todaysDates = response.Headers["date"];
                            DateTime time_http = DateTime.ParseExact(todaysDates,
                                                       "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                                                       CultureInfo.InvariantCulture.DateTimeFormat,
                                                       DateTimeStyles.AssumeUniversal);
                            DateTime time_after = DateTime.Now;
                            offset_http = time_http.Add(time_before - time_after) - DateTime.Now;
                            httpbox.Text = http_url.Text + "에서 시간을 불러왔습니다";

                            diff_milli_http = offset_http.TotalMilliseconds;
                            http_url.Text = str;
                        }
                        catch
                        {
                            try
                            {
                                str = "http://www" + http_url.Text;
                                var myHttpWebRequest = (HttpWebRequest)WebRequest.Create(str);
                                var response = myHttpWebRequest.GetResponse();
                                string todaysDates = response.Headers["date"];
                                DateTime time_http = DateTime.ParseExact(todaysDates,
                                                           "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                                                           CultureInfo.InvariantCulture.DateTimeFormat,
                                                           DateTimeStyles.AssumeUniversal);
                                DateTime time_after = DateTime.Now;
                                offset_http = time_http.Add(time_before - time_after) - DateTime.Now;
                                httpbox.Text = http_url.Text + "에서 시간을 불러왔습니다";

                                diff_milli_http = offset_http.TotalMilliseconds;
                                http_url.Text = str;
                            }
                            catch
                            {
                                httpbox.Text = "url 연결 실패";
                            }
                        }
                    }
                }
            }
            if (diff_milli_http < 0)
            {
                if (Math.Abs(diff_milli_http) >= 1000)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_http.Text = "사용자의 pc가 " + string.Format("{0:f4}", -diff_milli_http / 1000) + "초 빠릅니다.";
                    }));
                }
                else
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_http.Text = "사용자의 pc가 " + string.Format("{0:f0}", -diff_milli_http) + "밀리초 빠릅니다.";
                    }));
                }
            }
            else
            {
                if (Math.Abs(diff_milli_http) >= 1000)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_http.Text = "사용자의 pc가 " + string.Format("{0:f4}", diff_milli_http / 1000) + "초 느립니다.";
                    }));
                }
                else
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_http.Text = "사용자의 pc가 " + string.Format("{0:f0}", diff_milli_http) + "밀리초 느립니다.";
                    }));
                }
            }
        }

        private void sync_ntp_Click(object sender, RoutedEventArgs e)
        {
            //int i = DateTime.Now.Millisecond;
            button_multi_sync_ntp.IsEnabled = false;
            button_sync_ntp.IsEnabled = false;
            sync_ntp();
            //int f = DateTime.Now.Millisecond;
            //debug.Text = "동기화 명령에 " + Convert.ToString(f - i) + "밀리초 소모되었습니다";
            button_multi_sync_ntp.IsEnabled = true;
            button_sync_ntp.IsEnabled = true    ;
        }
        private void sync_http_Click(object sender, RoutedEventArgs e)
        {
            button_multi_sync_http.IsEnabled = false;
            button_sync_http.IsEnabled = false;
            sync_http();
            button_multi_sync_http.IsEnabled = true;
            button_sync_http.IsEnabled = true;
        }

        public void timer_native_DoWork(object sender, DoWorkEventArgs e)
        {
            int delay = (int)e.Argument;
            while (true)
            {
                Delay(global_delay);
                nowTime = DateTime.Now;
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    box_hour_native.Text = Convert.ToString(nowTime.Hour);
                    box_min_native.Text = Convert.ToString(nowTime.Minute);
                    box_sec_native.Text = Convert.ToString(nowTime.Second);
                    box_milli_native.Text = Convert.ToString(nowTime.Millisecond);
                }));
            }
        }
        public void timer_ntp_DoWork(object sender, DoWorkEventArgs e)
        {
            int delay = (int)e.Argument;
            while (true)
            {
                Delay(global_delay);
                nowTime = DateTime.Now;
                hours_ntp = Convert.ToInt32(TimeSpan.FromTicks(nowTime.Ticks + Convert.ToInt64(diff_ntp)).Hours);
                minutes_ntp = Convert.ToInt32(TimeSpan.FromTicks(nowTime.Ticks + Convert.ToInt64(diff_ntp)).Minutes);
                seconds_ntp = Convert.ToInt32(TimeSpan.FromTicks(nowTime.Ticks + Convert.ToInt64(diff_ntp)).Seconds);
                milliseconds_ntp = Convert.ToInt32(TimeSpan.FromTicks(nowTime.Ticks + Convert.ToInt64(diff_ntp)).Milliseconds);
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    box_hour_ntp.Text = Convert.ToString(hours_ntp);
                    box_min_ntp.Text = Convert.ToString(minutes_ntp);
                    box_sec_ntp.Text = Convert.ToString(seconds_ntp);
                    box_milli_ntp.Text = Convert.ToString(milliseconds_ntp);
                }));
            }
        }
        public void timer_http_DoWork(object sender, DoWorkEventArgs e)
        {
            int delay = (int)e.Argument;
            DateTime http_time;
            while (true)
            {
                Delay(global_delay);
                http_time = DateTime.Now + offset_http;
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    box_hour_http.Text = http_time.Hour.ToString();
                    box_min_http.Text = http_time.Minute.ToString();
                    box_sec_http.Text = http_time.Second.ToString();
                    box_milli_http.Text = http_time.Millisecond.ToString();
                }));
            }
        }

        public void multi_sync_ntp_Dowork(object sender, DoWorkEventArgs e)
        {
            int delay = (int)e.Argument;
            List<TimeSpan> timespans = new List<TimeSpan>();
            for(int i = 0; i < 10; i++)
            {
                get_ntp();
                timespans.Add(offset_ntp);
                Delay(delay);

            }
            offset_ntp = new TimeSpan(Convert.ToInt64(timespans.Average(t => t.Ticks)));

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ntpbox.Text = "사용된 ntp서버 : " + ntp_name;
            }));

            accuratetime_ntp = DateTime.UtcNow + offset_ntp;
            nowTime = DateTime.Now;
            accuratetime_ntp = accuratetime_ntp.AddHours(utc);
            diff_ntp = -(nowTime.Ticks - accuratetime_ntp.Ticks);
            diff_milli_ntp = diff_ntp / 10000;


            if (diff_milli_ntp < 0)
            {
                if (Math.Abs(diff_milli_ntp) >= 1000)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_ntp.Text = "사용자의 pc가 " + string.Format("{0:f4}", -diff_milli_ntp / 1000) + "초 빠릅니다.";
                    }));
                }
                else
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_ntp.Text = "사용자의 pc가 " + string.Format("{0:f0}", -diff_milli_ntp) + "밀리초 빠릅니다.";
                    }));
                }
            }
            else
            {
                if (Math.Abs(diff_milli_ntp) >= 1000)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_ntp.Text = "사용자의 pc가 " + string.Format("{0:f4}", diff_milli_ntp / 1000) + "초 느립니다.";
                    }));
                }
                else
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_ntp.Text = "사용자의 pc가 " + string.Format("{0:f0}", diff_milli_ntp) + "밀리초 느립니다.";
                    }));
                }
            }
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                button_sync_ntp.IsEnabled = true;
                button_multi_sync_ntp.IsEnabled = true;
            }));
        }

        public void multi_sync_http()
        {
            DateTime time_before, time_after, time_http;
            string todaysDates;
            List<TimeSpan> timespans = new List<TimeSpan>();
            diffbox_http.Visibility = System.Windows.Visibility.Hidden;
            diffbox_fake.Visibility = System.Windows.Visibility.Visible;
            diffbox_fake.Text = diffbox_http.Text;
            for (int i = 0; i < 20; i++)
            {
                sync_http();
                timespans.Add(offset_http);
                Delay(200);
            }
            offset_http = new TimeSpan(Convert.ToInt64(timespans.Average(t => t.Ticks)));
            diff_milli_http = offset_http.TotalMilliseconds;
            if (diff_milli_http < 0)
            {
                if (Math.Abs(diff_milli_http) >= 1000)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_http.Text = "사용자의 pc가 " + string.Format("{0:f4}", -diff_milli_http / 1000) + "초 빠릅니다.";
                    }));
                }
                else
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_http.Text = "사용자의 pc가 " + string.Format("{0:f0}", -diff_milli_http) + "밀리초 빠릅니다.";
                    }));
                }
            }
            else
            {
                if (Math.Abs(diff_milli_http) >= 1000)
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_http.Text = "사용자의 pc가 " + string.Format("{0:f4}", diff_milli_http / 1000) + "초 느립니다.";
                    }));
                }
                else
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        diffbox_http.Text = "사용자의 pc가 " + string.Format("{0:f0}", diff_milli_http) + "밀리초 느립니다.";
                    }));
                }
            }
            diffbox_http.Visibility = System.Windows.Visibility.Visible;
            diffbox_fake.Visibility = System.Windows.Visibility.Hidden;
        }

    }
}
