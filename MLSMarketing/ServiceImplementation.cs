using MLSMarketing.Framework;
using System.ServiceProcess;
using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Timers;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
//using HtmlAgilityPack;

namespace MLSMarketing
{
    /// <summary>
    /// The actual implementation of the windows service goes here...
    /// </summary>
    [WindowsService("MLSMarketing",
        DisplayName = "MLSMarketing",
        Description = "The description of the MLSMarketing service.",
        EventLogSource = "MLSMarketing",
        StartMode = ServiceStartMode.Automatic)]

    public class ServiceImplementation : IWindowsService
    {
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        
        public void Dispose()
        {
        }

        /// <summary>
        /// This method is called when the service gets a request to start.
        /// </summary>
        /// <param name="args">Any command line arguments</param>
        /// 
        System.Timers.Timer aTimer = new System.Timers.Timer(10000);

        public void OnStart(string[] args)
        {
            Console.WriteLine();
            WriteToConsole(ConsoleColor.Red, "MLS Scraping e Marketing Iniciou em: " + DateTime.Now.ToString());
            Console.WriteLine();
            Console.WriteLine();
            //aTimer = new System.Timers.Timer(10000);
            //System.Threading.Timer T = new System.Threading.Timer(new TimerCallback(OnTimedEvent), null, 0, 30000);
            // Hook up the Elapsed event for the timer.
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            // Set the Interval to 2 seconds (2000 milliseconds).
            aTimer.Interval = (1000);
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (!Directory.Exists(@"c:\MLSMarketing\Log.txt"))
            {
                Directory.CreateDirectory(@"c:\MLSMarketing\");
            }
            int MinutosIntervalo = Convert.ToInt32(ConfigurationManager.AppSettings["MinutosIntervalo"]);
            aTimer.Interval = (60 * MinutosIntervalo * 1000);



            //int NrPaginas = Convert.ToInt32(ConfigurationManager.AppSettings["NrPaginas"]);
            NameValueCollection appSettings = ConfigurationManager.AppSettings as NameValueCollection;

            // Get the collection enumerator.
            IEnumerator appSettingsEnum = appSettings.GetEnumerator();
            string[] keys = appSettings.AllKeys;
            string ProximoHorarioPesq = DateTime.Now.AddMinutes(Convert.ToInt32(MinutosIntervalo)).ToString();


            for (int i = 0; i < keys.Length; i++)
            {
                if (keys[i].ToString().Contains("Uri"))
                {
                    //ImportarCodPostalFreguesia();
                    FixTypes();
                    string imob = keys[i].Replace("Uri", "");
                    var IdImob = ConfigurationManager.AppSettings["IdImob" + imob];
                    //ZeraAgencia(IdImob);
                    this.LoopImob(imob, keys[i]);
                    FixTypes();
                }
            }

            Console.WriteLine();
            WriteToConsole(ConsoleColor.Green, "Termino do Ciclo de Pesquisas em: " + DateTime.Now.ToString());
            WriteToConsole(ConsoleColor.Green, "Aguardando Novo Intervalo de Tempo parametrizado em " + MinutosIntervalo + "min. Proximo inicio em :" + ProximoHorarioPesq);
            Console.WriteLine();
            WriteToConsole(ConsoleColor.Yellow, "MLS Marketing - Digite 'S' + Enter para Sair : ");
            //Process.GetCurrentProcess().Kill();
        }

        public void LoopImob(string imob, string key)
        {
            string htmlcode;
            int ipag = 0;
            int indexVolk = 18;
            int nrPaginas = 0;
            string uri = ConfigurationManager.AppSettings[key];
            while (true)
            {
                ipag++;

                Console.WriteLine();
                WriteToConsole(ConsoleColor.Green, "Pesquisando Pagina: " + ipag + " - Site: " + key);
                Console.WriteLine();
                var uripesq = uri;
                if (ipag > 1)
                {
                    if (key.Contains("Century"))
                    {
                        uripesq = uri.Replace("pageNo=1", "pageNo=" + ipag.ToString());
                    }
                    if (key.Contains("Era") || key.Contains("Sotheby"))
                    {
                        uripesq = uri.Replace("?pg=1", "?pg=" + ipag.ToString());
                    }
                    if (key.Contains("Lux"))
                    {
                        uripesq = uri.Replace("page/1", "page/" + ipag.ToString());
                    }
                    if (key.Contains("Port"))
                    {
                        uripesq = uri.Replace("page/2", "page/" + ipag.ToString());
                    }
                    if (key.Contains("Evolk"))
                    {
                        uripesq = uri.Replace("startIndex=0", "startIndex=" + indexVolk);
                        indexVolk += 18;
                    }
                    if (key.Contains("Sapo"))
                    {
                        uripesq = uri.Replace("pn=1","pn=" + ipag.ToString());
                    }
                    if (key.Contains("Idealista"))
                    {
                        uripesq = uri.Replace("pagina-1", "pagina-" + ipag.ToString());
                    }
                    if (key.Contains("Cjusto"))
                    {
                        uripesq = uri.Replace("o=1", "o=" + ipag.ToString());
                    }
                    if (key.Contains("Casa10") || key.Contains("Mclass") || key.Contains("Mont") || key.Contains("RioMag"))
                    {
                        uripesq = uri + "&pag=" + ipag;
                    }
                    if (key.Contains("Kw") || key.Contains("Mais") || key.Contains("Iad") || key.Contains("Imovirt"))
                    {
                        uripesq = uri + "&page=" + ipag;
                    }
                }
                
                htmlcode = this.WebRequest(uripesq, imob);

                //if (imob == "Century")
                //{
                //    HtmlWeb hw = new HtmlWeb();
                //    HtmlDocument doc = hw.Load(uripesq);
                //    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                //    {
                //        Console.WriteLine(link.OuterHtml);
                //    }
                //}

                if (htmlcode.IndexOf("Nenhum imóvel encontrado") > 0)
                {
                    break;
                }
                if (imob.Contains("Idealista"))
                {
                    if (htmlcode.IndexOf("Seguinte") < 0)
                    {
                        break;
                    }
                }
                if (imob != "Idealista" && imob != "IdealistaL" && imob != "Imovirt" && imob != "ImovirtL" && imob != "Century")
                {
                    if (htmlcode.IndexOf("Lamentamos") > 0)
                    {
                        break;
                    }
                }
                if (htmlcode.IndexOf("Não foi possível encontrar") > 0)
                {
                    break;
                }
                if (htmlcode.IndexOf("Não foram encontrados") > 0)
                {
                    break;
                }
                if (htmlcode.IndexOf("nenhum resultado") > 0)
                {
                    break;
                }
                if (htmlcode.IndexOf("Indique se pretende") > 0)
                {
                    break;
                }
                if (htmlcode.IndexOf("(404)") > 0)
                {
                    break;
                }
                if (htmlcode.IndexOf("(503)") > 0)
                {
                    break;
                }
                if (imob == "Porta")
                {
                    if (ipag > 50)
                    {
                        break;
                    }
                }
                if (imob == "Sotheby")
                {
                    if (ipag > 50)
                    {
                        break;
                    }
                }
                if (imob == "Iad")
                {
                    if (ipag > 20)
                    {
                        break;
                    }
                }

                if (imob == "Century")
                {
                    if (ipag == 1)
                    {
                        var CurrentPagInit = ConfigurationManager.AppSettings["CurrentPagInit" + imob];
                        var CurrentPagFinal = ConfigurationManager.AppSettings["CurrentPagFinal" + imob];
                        var resultCurrentPag = EverythingBetween(htmlcode, CurrentPagInit, CurrentPagFinal);
                        if (resultCurrentPag.Count > 0)
                        {
                            var resultados = resultCurrentPag[0].Replace("(", "").Replace(")", "").Replace(".", "");
                            var nrpaginas = Convert.ToInt32(resultados) / 6;
                            nrPaginas = nrpaginas;
                        }

                    }

                    if (ipag > nrPaginas)
                    {
                        break;
                    }

                }
                if (imob == "Imovirt" || imob == "ImovirtL")
                {
                    if (ipag == 1)
                    {
                        var CurrentPagInit = ConfigurationManager.AppSettings["CurrentPagInit" + imob];
                        var CurrentPagFinal = ConfigurationManager.AppSettings["CurrentPagFinal" + imob];
                        var resultCurrentPag = EverythingBetween(htmlcode, CurrentPagInit, CurrentPagFinal);
                        if (resultCurrentPag.Count > 0)
                        {
                            var resultados = resultCurrentPag[0].Replace("(", "").Replace(")", "").Replace(".", "");
                            var nrpaginas = Convert.ToInt32(resultados);
                            nrPaginas = nrpaginas;
                        }

                    }

                    if (ipag > nrPaginas)
                    {
                        break;
                    }

                }
                if (imob.Contains("Cjusto"))
                {
                    if (ipag == 1)
                    {
                        var CurrentPagInit = ConfigurationManager.AppSettings["CurrentPagInit" + imob];
                        var CurrentPagFinal = ConfigurationManager.AppSettings["CurrentPagFinal" + imob];
                        var resultCurrentPag = EverythingBetween(htmlcode, CurrentPagInit, CurrentPagFinal);
                        if (resultCurrentPag.Count > 0)
                        {
                            var resultados = resultCurrentPag[resultCurrentPag.Count - 2];
                            var nrpaginas = Convert.ToInt32(resultados);
                            nrPaginas = nrpaginas;
                        }

                    }

                    if (ipag > nrPaginas)
                    {
                        break;
                    }

                }

                if (imob == "Era" || imob == "EraLou")
                {
                    var CurrentPagInit = ConfigurationManager.AppSettings["CurrentPagInit" + imob];
                    var CurrentPagFinal = ConfigurationManager.AppSettings["CurrentPagFinal" + imob];
                    var resultCurrentPag = EverythingBetween(htmlcode, CurrentPagInit, CurrentPagFinal);
                    if (resultCurrentPag.Count > 0)
                    {
                        if (ipag > Convert.ToInt32(resultCurrentPag[0].ToString()))
                        {
                            break;
                        }
                    }

                }
                if (imob == "Mclass")
                {
                    var CurrentPagInit = ConfigurationManager.AppSettings["CurrentPagInit" + imob];
                    var CurrentPagFinal = ConfigurationManager.AppSettings["CurrentPagFinal" + imob];
                    var resultCurrentPag = EverythingBetween(htmlcode, CurrentPagInit, CurrentPagFinal);
                    if (resultCurrentPag.Count == 0)
                    {
                        break;
                    }

                }

                ExtractPesquisa(htmlcode, imob);
            }

        }

        /// <summary>
        /// This method is called when the service gets a request to stop.
        /// </summary>
        public void OnStop()
        {

        }


        /// <summary>
        /// This method is called when a service gets a request to pause,
        /// but not stop completely.
        /// </summary>
        public void OnPause()
        {
        }

        /// <summary>
        /// This method is called when a service gets a request to resume 
        /// after a pause is issued.
        /// </summary>
        public void OnContinue()
        {

        }

        /// <summary>
        /// This method is called when the machine the service is running on
        /// is being shutdown.
        /// </summary>
        public void OnShutdown()
        {
        }

        /// <summary>
        /// This method is called when a custom command is issued to the service.
        /// </summary>
        /// <param name="command">The command identifier to execute.</param >
        public void OnCustomCommand(int command)
        {
        }

        // Helper method to write a message to the console at the given foreground color.
        internal static void WriteToConsole(ConsoleColor foregroundColor, string format,
            params object[] formatArguments)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = foregroundColor;

            Console.WriteLine(format, formatArguments);
            Console.Out.Flush();

            Console.ForegroundColor = originalColor;
        }



        private void PesquisarEnviar()
        {
            
           
        }

        private string WebRequest(string url, string imob)
        {
            string htmlCode;
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent", "Chrome");
                    if (!imob.Contains("Cjusto"))
                    {
                        client.Encoding = Encoding.UTF8;
                    }
                    else
                    {
                        client.Encoding = Encoding.Default;
                    }
                    htmlCode = client.DownloadString(url);
                }

                return htmlCode;

            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }

        private void ExtractPesquisa(string htmlcode, string imob)
        {
            List<mlsPesq> mlspesqList = new List<mlsPesq>();
            mlsPesq mlspesq = new mlsPesq();
            HtmlRemoval htmlRemoval = new HtmlRemoval();
            dbHelper dbhelper = new dbHelper();

            bool addLink = true;
            var PesqLinkInit = ConfigurationManager.AppSettings["PesqLinkInit" + imob];
            var PesqLinkFinal = ConfigurationManager.AppSettings["PesqLinkFinal" + imob];
            var resultsLink = EverythingBetween(htmlcode, PesqLinkInit, PesqLinkFinal);
            for (int i = 0; i < resultsLink.Count; i++)
            {
                mlsPesq mlspesq1 = new mlsPesq();
                if (imob == "Era" || imob == "EraLou") { mlspesq1.Link = "http://www.era.pt" + resultsLink[i]; }
                else if (imob == "Casa10" || imob == "Casa10Lou") { mlspesq1.Link = "http://casa10.pt" + resultsLink[i]; }
                else if (imob == "Century" || imob == "Casa10Lou") { mlspesq1.Link = "https://www.century21global.com" + resultsLink[i]; }
                
                else if (imob.Contains("Idealista")) { mlspesq1.Link = "https://www.idealista.pt/imovel/" + resultsLink[i] + "/"; }
                else if (imob == "Mclass") { mlspesq1.Link = "http://markiclass.com/imovel" + resultsLink[i]; }
                else if (imob == "Mais") { mlspesq1.Link = "https://www.maisconsultores.pt" + resultsLink[i]; }
                else if (imob == "Lux") { mlspesq1.Link = "https://www.luxnlust.com/pt/property/" + resultsLink[i]; }
                else if (imob == "Sotheby") { mlspesq1.Link = "http://www.sothebysrealtypt.com" + resultsLink[i]; }
                else if (imob == "Porta") { mlspesq1.Link = "https://www.portadafrente.com" + resultsLink[i].Replace("?page=2", ""); }
                
                else if (imob == "Mont")
                {
                    if (resultsLink[i].Length < 2)
                    {
                        continue;
                    }
                    mlspesq1.Link = "http://montrigues.net" + resultsLink[i];
                    addLink = true;
                    foreach (var item in mlspesqList)
                    {
                        if (item.Link == mlspesq1.Link)
                        {
                            addLink = false;
                        }
                    }
                }
                else if (imob == "Iad")
                {
                    if (resultsLink[i].Length < 2)
                    {
                        continue;
                    }
                    mlspesq1.Link = "https://www.iadportugal.pt/anuncio" + resultsLink[i];
                    addLink = true;
                    foreach (var item in mlspesqList)
                    {
                        if (item.Link == mlspesq1.Link)
                        {
                            addLink = false;
                        }
                    }
                }
                else if (imob == "Sapo")
                {
                    if (resultsLink[i].Length < 2)
                    {
                        continue;
                    }
                    mlspesq1.Link = "https://casa.sapo.pt" + resultsLink[i];
                    addLink = true;
                    foreach (var item in mlspesqList)
                    {
                        if (item.Link == mlspesq1.Link)
                        {
                            addLink = false;
                        }
                    }
                }
                else if (imob == "RioMag" || imob == "RioMagLou")
                {
                    if (resultsLink[i].Length < 2)
                    {
                        continue;
                    }
                    if (resultsLink[i].Contains("class="))
                    {
                        continue;
                    }
                    mlspesq1.Link = "http://riomagicdeluxe.com/imovel/" + resultsLink[i];
                    addLink = true;
                    foreach (var item in mlspesqList)
                    {
                        if (item.Link == mlspesq1.Link)
                        {
                            addLink = false;
                        }
                    }
                }

                else { mlspesq1.Link = resultsLink[i]; }
                if (addLink)
                {
                    mlspesqList.Add(mlspesq1);
                }
            }
            foreach (var item in mlspesqList)
            {

                //Inicio da pesquisa de detalhes de cada item da pagina de pesquisa
                if (imob.Contains("Idealista"))
                {
                    Thread.Sleep(5500);
                }
                htmlcode = WebRequest(item.Link, imob);
                if (htmlcode.IndexOf("(503)") > 0)
                {
                    continue;
                }

                item.Nome = string.Empty;
                item.Telefone = string.Empty;
                item.TipoAnunciante = "Profissional";
                item.Endereco = "Endereço Amigável";

                if (imob == "Sapo")
                {
                    var DetMediadorInit = ConfigurationManager.AppSettings["DetMediadorInit" + imob];
                    var DetMediadorFinal = ConfigurationManager.AppSettings["DetMediadorFinal" + imob];
                    if (htmlcode.IndexOf(DetMediadorInit) < 0)
                    {
                        continue;
                    }
                    item.TipoAnunciante = "Particular";

                    var DetTelefoneInit = ConfigurationManager.AppSettings["DetTelefoneInit" + imob];
                    var DetTelefoneFinal = ConfigurationManager.AppSettings["DetTelefoneFinal" + imob];
                    if (htmlcode.IndexOf(DetTelefoneInit) > 0)
                    {
                        var resultsTelefone = ExtractFromString(htmlcode.Substring(htmlcode.IndexOf(DetTelefoneInit)), DetTelefoneInit, DetTelefoneFinal);
                        item.Telefone = resultsTelefone[0];
                    }
                    else
                    {
                        item.Telefone = "n/a";
                    }
                    var DetNomeInit = ConfigurationManager.AppSettings["DetNomeInit" + imob];
                    var DetNomeFinal = ConfigurationManager.AppSettings["DetNomeFinal" + imob];
                    var resultsNome = ExtractFromString(htmlcode.Substring(htmlcode.IndexOf(DetNomeInit)), DetNomeInit, DetNomeFinal);
                    if (resultsNome.Count > 0)
                    {
                        item.Nome = WebUtility.HtmlDecode(htmlRemoval.StripTagsCharArray(resultsNome[0])).Replace(item.Telefone, "");
                    }
                    else
                    {
                        item.Nome = "n/a";
                    }

                    item.Telefone = item.Telefone.Trim();
                    item.Nome = item.Nome.Trim();
                    item.TipoAnunciante = "Particular";
                }

                if (imob.Contains("Idealista"))
                {
                    var DetTelefoneInit = ConfigurationManager.AppSettings["DetTelefoneInit" + imob];
                    var DetTelefoneFinal = ConfigurationManager.AppSettings["DetTelefoneFinal" + imob];
                    if (htmlcode.IndexOf(DetTelefoneInit) > 0)
                    {
                        var resultsTelefone = ExtractFromString(htmlcode.Substring(htmlcode.IndexOf(DetTelefoneInit)), DetTelefoneInit, DetTelefoneFinal);
                        item.Telefone = resultsTelefone[0];
                        var DetNomeInit = ConfigurationManager.AppSettings["DetNomeInit" + imob];
                        var DetNomeFinal = ConfigurationManager.AppSettings["DetNomeFinal" + imob];
                        var resultsNome = ExtractFromString(htmlcode.Substring(htmlcode.IndexOf(DetNomeInit)), DetNomeInit, DetNomeFinal);
                        if (resultsNome.Count > 0)
                        {
                            item.Nome = WebUtility.HtmlDecode(htmlRemoval.StripTagsCharArray(resultsNome[0])).Replace(item.Telefone, "");

                            if (item.Nome.Contains("Particular") || item.Nome.Contains("Profissional"))
                            {
                                if (item.Nome.IndexOf("Particular") > 0)
                                {
                                    item.Nome = item.Nome.Substring(item.Nome.IndexOf("Particular"), item.Nome.Length - item.Nome.IndexOf("Particular"));//.Replace("Particular", "");

                                }
                                else
                                {
                                    item.Nome = item.Nome.Substring(item.Nome.IndexOf("Profissional"), item.Nome.Length - item.Nome.IndexOf("Profissional"));//.Replace("Profissional", "");

                                }
                            }
                            else
                            {
                                item.Nome = "n/a";
                            }
                        }
                        else
                        {
                            item.Nome = "n/a";
                        }
                    }
                    else
                    {
                        item.Telefone = "n/a";
                    }

                    item.Telefone = item.Telefone.Trim();
                    item.Nome = item.Nome.Trim();
                    if (item.Nome.Contains("Particular"))
                    {
                        item.TipoAnunciante = "Particular";
                    }
                }


                if (imob.Contains("Imovirt") || imob.Contains("Era") || imob.Contains("Cjusto"))
                {
                    var DetTelefoneInit = ConfigurationManager.AppSettings["DetTelefoneInit" + imob];
                    var DetTelefoneFinal = ConfigurationManager.AppSettings["DetTelefoneFinal" + imob];
                    if (htmlcode.IndexOf(DetTelefoneInit) > 0)
                    {
                        var resultsTelefone = ExtractFromString(htmlcode.Substring(htmlcode.IndexOf(DetTelefoneInit)), DetTelefoneInit, DetTelefoneFinal);
                        item.Telefone = WebUtility.HtmlDecode(htmlRemoval.StripTagsCharArray(resultsTelefone[0]));
                    }
                    else
                    {
                        item.Telefone = "n/a";
                    }
                    var DetNomeInit = ConfigurationManager.AppSettings["DetNomeInit" + imob];
                    var DetNomeFinal = ConfigurationManager.AppSettings["DetNomeFinal" + imob];
                    if (htmlcode.IndexOf(DetNomeInit) > 0)
                    {
                        var resultsNome = ExtractFromString(htmlcode.Substring(htmlcode.IndexOf(DetNomeInit)), DetNomeInit, DetNomeFinal);
                        if (resultsNome.Count > 0)
                        {
                            item.Nome = WebUtility.HtmlDecode(htmlRemoval.StripTagsCharArray(resultsNome[0])).Replace(item.Telefone, "");
                        }
                        else
                        {
                            item.Nome = "n/a";
                        }

                    }
                    else
                    {
                        item.Nome = "n/a";

                    }

                    item.Telefone = item.Telefone.Trim();
                    item.Nome = item.Nome.Trim();
                    if (imob.Contains("Cjusto"))
                    {
                        item.TipoAnunciante = "Particular";

                    }
                    else
                    {
                        item.TipoAnunciante = "Profissional";

                    }

                }


                var DetTituloInit = ConfigurationManager.AppSettings["DetTituloInit" + imob];
                var DetTituloFinal = ConfigurationManager.AppSettings["DetTituloFinal" + imob];
                if (htmlcode.IndexOf(DetTituloInit) == -1)
                {
                    continue;
                }
                var resultsTitulo = ExtractFromString(htmlcode.Substring(htmlcode.IndexOf(DetTituloInit)), DetTituloInit, DetTituloFinal);
                if (resultsTitulo.Count != 0) { item.Titulo = SubstituiCaracteres(resultsTitulo[0]); } else { item.Titulo = "N/A"; }
                item.Titulo = item.Titulo.TrimStart().TrimEnd();
                item.Titulo = item.Titulo.Replace("                                                                  ", "");
                if (imob == "Mont")
                {
                    if (item.Titulo.IndexOf(",") > 0)
                    {
                        item.Titulo = item.Titulo.Substring(0, item.Titulo.IndexOf(","));
                    }
                    if (item.Titulo.IndexOf("<br") > 0)
                    {
                        item.Titulo = item.Titulo.Substring(0, item.Titulo.IndexOf("<br"));
                    }
                }
                if (imob == "Sapo")
                {
                    if (item.Titulo.IndexOf(",") > 0)
                    {
                        item.Titulo = item.Titulo.Substring(0, item.Titulo.IndexOf(","));
                    }
                }
                if (imob.Contains("Century") || imob.Contains("Cjusto"))
                {
                    item.Titulo = WebUtility.HtmlDecode(htmlRemoval.StripTagsCharArray(item.Titulo));
                    item.Titulo = item.Titulo.Replace("www.century21global.com","").Replace("Portugal","");
                }

                var DetDescricaoInit = ConfigurationManager.AppSettings["DetDescricaoInit" + imob];
                var DetDescricaoFinal = ConfigurationManager.AppSettings["DetDescricaoFinal" + imob];
                if (htmlcode.IndexOf(DetDescricaoInit) > 0)
                {
                    var resultsDescricao = ExtractFromString(htmlcode.Substring(htmlcode.IndexOf(DetDescricaoInit)), DetDescricaoInit, DetDescricaoFinal);
                    if (resultsDescricao.Count != 0) { item.Descricao = resultsDescricao[0]; } else { item.Descricao = "N/A"; }
                }
                else
                {
                    var resultsDescricao = ExtractFromString(htmlcode, DetDescricaoInit, DetDescricaoFinal);
                    if (resultsDescricao.Count != 0) { item.Descricao = resultsDescricao[0]; } else { item.Descricao = "N/A"; }
                }

                var DetIdInit = ConfigurationManager.AppSettings["DetIdInit" + imob];
                var DetIdFinal = ConfigurationManager.AppSettings["DetIdFinal" + imob];
                var resultsIdDet = EverythingBetween(htmlcode.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetIdInit, DetIdFinal);
                if (resultsIdDet.Count != 0) { item.Id = SubstituiCaracteres(resultsIdDet[0].Trim()); } else { item.Id = imob.ToUpper(); }
                if (imob == "Era" || imob == "EraLou" || imob == "Mais" || imob == "Iad" || imob == "Century" || imob.Contains("Cjusto"))
                {
                    item.Id = imob.ToUpper() + item.Id.Trim().Replace("                            ", "");
                }

                if (imob.Contains("Idealista"))
                {
                    if (item.Id == imob.ToUpper())
                    {
                        item.Id = item.Id + item.Link.Replace("https://www.idealista.pt/imovel/", "").Replace("/","");
                    }
                }

                if (imob.Contains("Kw"))
                {
                    if (htmlcode.IndexOf(item.Id + ".\"") > 0)
                    {
                        item.Telefone = htmlcode.Substring(htmlcode.IndexOf(item.Id + ".\"") - 13, 11);
                    }
                    else
                    {
                        item.Telefone = "n/a";
                    }
                    var DetNomeInit = ConfigurationManager.AppSettings["DetNomeInit" + imob];
                    var DetNomeFinal = ConfigurationManager.AppSettings["DetNomeFinal" + imob];
                    if (htmlcode.IndexOf(DetNomeInit) > 0)
                    {
                        var resultsNome = ExtractFromString(htmlcode.Substring(htmlcode.IndexOf(DetNomeInit)), DetNomeInit, DetNomeFinal);
                        if (resultsNome.Count > 0)
                        {
                            item.Nome = WebUtility.HtmlDecode(htmlRemoval.StripTagsCharArray(resultsNome[0])).Replace(item.Telefone, "");
                        }
                        else
                        {
                            item.Nome = "n/a";
                        }

                    }
                    else
                    {
                        item.Nome = "n/a";

                    }

                    item.Telefone = item.Telefone.Trim();
                    item.Nome = item.Nome.Trim();
                    item.TipoAnunciante = "Profissional";

                }

                var DetValorInit = ConfigurationManager.AppSettings["DetValorInit" + imob];
                var DetValorFinal = ConfigurationManager.AppSettings["DetValorFinal" + imob];
                var resultsValorDet = EverythingBetween(htmlcode.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetValorInit, DetValorFinal);
                if (resultsValorDet.Count != 0) { item.Valor = SubstituiCaracteres(resultsValorDet[0]); } else { item.Valor = "0"; }
                if (item.Valor.ToUpper().Contains("CONSULTA"))
                {
                    item.Valor = "0";
                }
                item.Valor = item.Valor.Replace("</strong", "").Replace(".", "").Replace(",", "").Replace("€", "").Replace(" ", "").Replace("(Sob consulta)", "0").Replace("Sob consulta", "0").Replace(" ", "").Replace("SobConsulta", "0").Replace("class=\"preco-novo-imovel\"", "");
                if (imob == "Lux")
                {
                    item.Valor = (Convert.ToDecimal(item.Valor) / 100).ToString();
                }
                if (imob == "Sapo")
                {
                    item.Valor = item.Valor.Replace("-","").Replace("/","");
                }

                var DetTipoInit = ConfigurationManager.AppSettings["DetTipoInit" + imob];
                var DetTipoFinal = ConfigurationManager.AppSettings["DetTipoFinal" + imob];
                var resultsTipoDet = EverythingBetween(htmlcode, DetTipoInit, DetTipoFinal);
                if (resultsTipoDet.Count != 0) { item.Tipo = SubstituiCaracteres(resultsTipoDet[0]); } else { item.Tipo = "Indefinido"; }
                if (imob == "Kw" || imob =="KwN"){if (item.Tipo.IndexOf(" ") > 0){item.Tipo = item.Tipo.Substring(0, item.Tipo.IndexOf(" "));}}
                if (imob == "Lux") { item.Tipo = item.Tipo.TrimStart(); if (item.Tipo.IndexOf(" ") > 0) { item.Tipo = item.Tipo.Substring(0, item.Tipo.IndexOf(" ")); } }
                if (imob == "Era" || imob == "EraLou")
                {
                    if (item.Tipo != "Indefinido")
                    {
                        item.Tipo = resultsTipoDet[1];
                    }
                }
                if (imob == "Porta")
                {
                    item.Tipo = item.Titulo.Replace("T", ",").Substring(item.Titulo.IndexOf(","));
                    var tipo = item.Tipo.Split(',');
                    item.Tipo = tipo[1].TrimStart();
                }
                if (imob == "Iad" || imob == "Sapo" || imob.Contains("Idealista"))
                {
                    item.Tipo = item.Titulo.Substring(0, item.Titulo.IndexOf(" "));
                }
                if (imob == "CjustoAp")
                {
                    item.Tipo = "Apartamento";
                }
                if (imob == "CjustoMo")
                {
                    item.Tipo = "Moradia";
                }
                item.Tipo = item.Tipo.Trim();

                var DetQuartosInit = ConfigurationManager.AppSettings["DetQuartosInit" + imob];
                var DetQuartosFinal = ConfigurationManager.AppSettings["DetQuartosFinal" + imob];
                var resultsQuartosDet = EverythingBetween(htmlcode.Replace("\r","").Replace("\n", "").Replace("\t", ""), DetQuartosInit.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetQuartosFinal.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("maior",""));
                
                if (resultsQuartosDet.Count() != 0)
                {
                    resultsQuartosDet[0] = htmlRemoval.StripTagsCharArray(resultsQuartosDet[0]);
                }
                if (imob == "Sotheby")
                {
                    for (int i = 0; i < resultsQuartosDet.Count(); i++)
                    {
                        if (resultsQuartosDet[i].Contains("m"))
                        {
                            item.AreaB = resultsQuartosDet[i].Replace("m", "").Replace(".", "");
                            if (item.AreaB.Contains(","))
                            {
                                item.AreaB = item.AreaB.Substring(0, item.AreaB.IndexOf(","));
                            }
                            item.Quartos = "0";
                        }
                        else
                        {
                            item.Quartos = resultsQuartosDet[i];
                            item.AreaB = "0";
                        }
                        if (item.Titulo.ToUpper().Contains("QUARTO"))
                        {
                            item.Quartos = item.Titulo.Substring(item.Titulo.ToUpper().IndexOf("QUARTO") - 3, 3).Trim();
                        }
                    }
                }
                else
                {
                    if (resultsQuartosDet.Count != 0) { item.Quartos = SubstituiCaracteres(resultsQuartosDet[0]); } else { item.Quartos = "0"; }
                    item.Quartos = item.Quartos.Replace("Triplex", "").Replace("T", "").Replace("Duplex", "").Replace("desde", "").Replace("interiores", "").Trim();
                }

                if (imob == "Sapo")
                {
                    var strtitulo = item.Titulo.ToUpper().Replace("TE","").Replace("TÉ","");
                    if (strtitulo.IndexOf(" T") > 0)
                    {
                        item.Quartos = strtitulo.Substring(strtitulo.IndexOf(" T") + 2, 2).Replace("+", "");
                    }
                    else
                    {
                        item.Quartos = "0";
                    }
                }

                if (imob == "Century")
                {
                    if (htmlcode.IndexOf(DetQuartosInit) > 0)
                    {
                        item.Quartos = htmlcode.Substring(htmlcode.IndexOf(DetQuartosInit) - 3, 3);
                    }
                    else
                    {
                        item.Quartos = "0";
                    }
                }

                if (item.Quartos != null)
                {
                    item.Quartos = item.Quartos.Replace("maior", "").Replace("ou", "").Trim();
                }
                else
                {
                    item.Quartos = "1";
                }
                item.Suites = "0";

                if (imob == "Kw" || imob == "KwN" || imob == "Porta")
                {
                    var DetSuitesInit = ConfigurationManager.AppSettings["DetSuitesInit" + imob];
                    var DetSuitesFinal = ConfigurationManager.AppSettings["DetSuitesFinal" + imob];
                    var resultsSuitesDet = EverythingBetween(htmlcode.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetSuitesInit.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetSuitesFinal.Replace("\r", "").Replace("\n", "").Replace("\t", ""));
                    if (resultsSuitesDet.Count() > 0)
                    {
                        item.Suites = resultsSuitesDet[0];
                        if (imob == "Kw" || imob == "KwN")
                        item.Quartos = (Convert.ToInt32(item.Quartos) + Convert.ToInt32(resultsSuitesDet[0])).ToString();
                    }
                    else
                    {
                        item.Suites = "0";
                    }
                }

                var DetBanhoInit = ConfigurationManager.AppSettings["DetBanhoInit" + imob];
                var DetBanhoFinal = ConfigurationManager.AppSettings["DetBanhoFinal" + imob];
                var resultsBanhoDet = EverythingBetween(htmlcode.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetBanhoInit.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetBanhoFinal.Replace("\r", "").Replace("\n", "").Replace("\t", ""));
                if (resultsBanhoDet.Count != 0) {
                    item.Banho = htmlRemoval.StripTagsCharArray(resultsBanhoDet[0]).Replace("x", ""); }
                else { item.Banho = "0"; }
                if (item.Banho.Trim().Length == 0)
                {
                    item.Banho = "0";
                }
                if (imob.Contains("Idealista"))
                {
                    if (resultsBanhoDet.Count > 0)
                    {
                        item.Banho = htmlcode.Substring(htmlcode.IndexOf(DetBanhoInit) - 2, 2).Replace(",","").Trim();
                        if (item.Banho == "")
                        {
                            item.Banho = "0";
                        }
                    }
                }
                if (imob == "Century")
                {
                    if (htmlcode.IndexOf(DetBanhoInit) > 0)
                    {
                        item.Banho = htmlcode.Substring(htmlcode.IndexOf(DetBanhoInit) - 3, 3);
                    }
                    else
                    {
                        item.Banho = "0";
                    }
                }

                var DetGaragemInit = ConfigurationManager.AppSettings["DetGaragemInit" + imob];
                var DetGaragemFinal = ConfigurationManager.AppSettings["DetGaragemFinal" + imob];
                var resultsGaragemDet = EverythingBetween(htmlcode.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetGaragemInit.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetGaragemFinal.Replace("\r", "").Replace("\n", "").Replace("\t", ""));
                if (resultsGaragemDet.Count != 0) {
                    item.Garagem = htmlRemoval.StripTagsCharArray(resultsGaragemDet[0].Replace("x", "")); } else { item.Garagem = "0"; }

                var DetAreaInit = ConfigurationManager.AppSettings["DetAreaInit" + imob];
                var DetAreaFinal = ConfigurationManager.AppSettings["DetAreaFinal" + imob];
                var resultsAreaDet = EverythingBetween(htmlcode.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetAreaInit.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetAreaFinal.Replace("\r", "").Replace("\n", "").Replace("\t", ""));
                if (resultsAreaDet.Count != 0) { item.Area = SubstituiCaracteres(resultsAreaDet[0]); } else { item.Area = "0"; }
                if (item.Area.Contains(","))
                {
                    item.Area = item.Area.Substring(0, item.Area.IndexOf(","));
                }
                item.Area = item.Area.Replace("sqft", "").Replace(".", "").Replace("desde","").Replace("m²", "").Replace("m&sup2;", "").Replace("ha", "").Replace("<supm2", "").Replace("m2", "");
                if (imob == "Evolk")
                {
                    item.Area = Convert.ToInt32((Convert.ToInt32(item.Area) / 10.764)).ToString();
                }

                var DetAreaBInit = ConfigurationManager.AppSettings["DetAreaBInit" + imob];
                var DetAreaBFinal = ConfigurationManager.AppSettings["DetAreaBFinal" + imob];
                var resultsAreaBDet = EverythingBetween(htmlcode.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetAreaBInit.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetAreaBFinal.Replace("\r", "").Replace("\n", "").Replace("\t", ""));
                if (imob.Contains("Cjusto"))
                {
                    if (resultsAreaBDet.Count > 0)
                    {
                        item.AreaB = htmlcode.Substring(htmlcode.IndexOf(DetAreaBInit) - 4, 4);
                        item.AreaB = htmlRemoval.StripTagsCharArray(item.AreaB);
                    }
                    else
                    {
                        item.AreaB = "0";
                    }
                }
                else if (imob != "Sotheby")
                {
                    if (resultsAreaBDet.Count != 0) { item.AreaB = SubstituiCaracteres(resultsAreaBDet[0]); } else
                    {
                        item.AreaB = "0";
                    }
                    if (item.AreaB.Contains(","))
                    {
                        item.AreaB = item.AreaB.Substring(0, item.AreaB.IndexOf(","));
                    }
                    item.AreaB = item.AreaB.Replace("sqft", "").Replace(".", "").Replace("desde", "").Replace("m²", "").Replace("m&sup2;", "").Replace("ha", "").Replace("<supm2", "").Replace("m2", "");
                }
                else
                {
                    if (resultsAreaBDet.Count != 0) { item.AreaB = SubstituiCaracteres(resultsAreaBDet[0].Substring(0, resultsAreaBDet[0].IndexOf("m")).Replace(".", "")); }
                    else
                    {
                        var resultsAreaBDet2 = EverythingBetween(htmlcode.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetAreaBInit.Replace("\r", "").Replace("\n", "").Replace("\t", ""), DetAreaBFinal.Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("area", "area_terreno"));
                        if (resultsAreaBDet2.Count != 0)
                        {
                            item.AreaB = SubstituiCaracteres(resultsAreaBDet2[0].Substring(0,resultsAreaBDet2[0].IndexOf("m")).Replace(".",""));
                        }
                        else
                        {
                            item.AreaB = "0";
                        }
                    }
                    if (item.AreaB.Contains(","))
                    {
                        item.AreaB = item.AreaB.Substring(0, item.AreaB.IndexOf(","));
                    }
                    item.AreaB = item.AreaB.Replace("sqft", "").Replace(".", "").Replace("desde", "").Replace("m²", "").Replace("m&sup2;", "").Replace("ha", "").Replace("<supm2", "").Replace("m2", "");
                }
                item.AreaB = item.AreaB.Trim();

                var DetConcelhoInit = ConfigurationManager.AppSettings["DetConcelhoInit" + imob];
                var DetConcelhoFinal = ConfigurationManager.AppSettings["DetConcelhoFinal" + imob];
                var resultsConcelhoDet = EverythingBetween(htmlcode, DetConcelhoInit, DetConcelhoFinal);
                if (resultsConcelhoDet.Count != 0) { item.Concelho = resultsConcelhoDet[0]; item.Concelho = WebUtility.HtmlDecode(htmlRemoval.StripTagsCharArray(item.Concelho)); } else { item.Concelho = "N/A"; }
                item.Concelho = item.Concelho.Replace("u00e7", "ç").Replace("u00f3", "ô").Replace("u00e9", "é").Replace("u00f5", "õ").Replace("\\", "");
                if (imob == "Kw" || imob == "KwN")
                {
                    item.Concelho = item.Concelho.Replace("›", ",");
                    var strconcelho = item.Concelho.Split(',');
                    item.Concelho = strconcelho[0].TrimEnd();
                }
                if (imob == "Era" || imob == "Casa10" || imob == "Casa10Lou" || imob == "Mclass" || imob == "RioMag" || imob == "Sotheby" || imob == "Century")
                {
                    item.Concelho = "Lisboa," + item.Concelho;
                }
                if (imob == "EraLou" || imob == "RioMagLou")
                {
                    item.Concelho = "Loures," + item.Concelho;
                }
                if (imob == "Evolk" || imob == "Mais")
                {
                    var strConcelho = item.Concelho.Split(',');
                    if (item.Concelho.Contains("Loures"))
                    {
                        item.Concelho = "Loures," + strConcelho[strConcelho.Count() - 2];
                    }
                    else
                    {
                        item.Concelho = "Lisboa," + strConcelho[strConcelho.Count() - 2];
                    }
                }
                if (imob == "Lux")
                {
                    item.Concelho = item.Concelho.TrimStart();
                    item.Concelho = item.Concelho.Substring(item.Concelho.IndexOf("/"));
                    item.Concelho = item.Concelho.Replace("/", "").Trim();
                }
                if (imob == "Porta")
                {
                    var strconcelho = item.Concelho.Split(',');
                    item.Concelho = strconcelho[0];
                }
                if (item.Concelho == "N/A")
                {
                    item.Concelho = "Lisboa";
                }

                if (imob == "Sapo")
                {
                    var Concelho = resultsConcelhoDet[0].Split(',');
                    if (Concelho[1].Trim() == "Lisboa")
                    {
                        item.Concelho = "Lisboa," + Concelho[0];
                    }
                    else
                    {
                        item.Concelho = "Lisboa," + Concelho[1];
                    }
                }
                if (imob == "Idealista")
                {
                    item.Concelho = "Lisboa," + item.Concelho.Replace("Lisboa","").Replace(",","");
                }
                if (imob == "IdealistaL")
                {
                    item.Concelho = "Loures," + item.Concelho.Replace("Loures", "").Replace(",", "");
                }
                if (imob == "Imovirt")
                {
                    var concelho = item.Concelho.Split(',');
                    if (concelho.Count() == 2)
                    {
                        item.Concelho = concelho[1] + "," + concelho[0];
                    }
                    else
                    {
                        item.Concelho = "Lisboa" + "," + concelho[0];
                    }
                }
                if (imob == "ImovirtL")
                {
                    var concelho = item.Concelho.Split(',');
                    if (concelho.Count() == 2)
                    {
                        item.Concelho = concelho[1] + "," + concelho[0];
                    }
                    else
                    {
                        item.Concelho = "Loures" + "," + concelho[0];
                    }
                }
                if (imob == "Century")
                {
                    string cep = string.Empty;
                    var resultsConcelhoC = ExtractFromString(htmlcode, DetConcelhoInit, DetConcelhoFinal);
                    if (resultsConcelhoC.Count > 0)
                    {
                        resultsConcelhoC[0] = resultsConcelhoC[0].Replace("\r", "").Replace("\n", "").Trim();
                        if (resultsConcelhoC[0].IndexOf(",") > 0)
                        {
                            item.Concelho = resultsConcelhoC[0];
                            var cf = item.Concelho.Split(',');
                            cep = cf[1].Replace("Lisboa", "").Trim();
                        }
                        else
                        {
                            item.Concelho = "Lisboa," + resultsConcelhoC[0];
                            cep = resultsConcelhoC[0].Replace("Lisboa", "").Trim();
                        }
                    }
                    else
                    {
                        item.Concelho = "Lisboa,Lisboa";
                    }

                    if (cep != string.Empty)
                    {
                        var strsql = "select * from CodigoPostal where CodigoPostalRaiz = '" + cep + "'";
                        var CodigoPostal = dbhelper.SelecioneCodigopostal(strsql);
                        if (CodigoPostal.Count > 0)
                        {
                            item.Concelho = CodigoPostal.First().Concelho + "," + CodigoPostal.First().Freguesia;
                            item.Endereco = CodigoPostal.First().Rua;
                        }
                        else
                        {
                            item.Concelho = string.Empty;
                            continue;
                        }
                    }
                }
                if (imob.Contains("Cjusto"))
                {
                    item.Concelho = "Lisboa," + item.Concelho;
                }

                var DetImagemDetalheInit = ConfigurationManager.AppSettings["DetImagemDetalheInit" + imob];
                var DetImagemDetalheFinal = ConfigurationManager.AppSettings["DetImagemDetalheFinal" + imob];
                var resultsImagemDet = EverythingBetween(htmlcode, DetImagemDetalheInit, DetImagemDetalheFinal);
                

                List<mlsDetails> mlsDetailsList = new List<mlsDetails>();
                for (int i = 0; i < resultsImagemDet.Count; i++)
                {
                    mlsDetails mlsDetails = new mlsDetails();
                    if (imob == "Kw" || imob == "KwN")
                    {
                        if (resultsImagemDet[i].Contains("lres"))
                        {
                            continue;
                        }
                    }
                    if (imob == "Mais")
                    {
                        if (resultsImagemDet[i].Contains("148x131"))
                        {
                            continue;
                        }
                        resultsImagemDet[i] = "https://www.maisconsultores.pt" + resultsImagemDet[i];
                    }
                    if (imob.Contains("Idealista"))
                    {
                        resultsImagemDet[i] = "https://img3.idealista.pt" + resultsImagemDet[i].Replace(",WEB_DETAIL", "");
                    }
                    if (imob.Contains("Cjusto"))
                    {
                        resultsImagemDet[i] = resultsImagemDet[i] + "?rule=play";
                    }
                    if (imob == "Imovirt" || imob == "ImovirtL")
                    {
                        resultsImagemDet[i] = "https://apollo-ireland.akamaized.net" + resultsImagemDet[i];
                    }
                    if (imob == "Sapo")
                    {
                        if (resultsImagemDet[i].Contains("80x60"))
                        {
                            continue;
                        }
                        resultsImagemDet[i] = "https://media.casasapo.pt/Z" + resultsImagemDet[i];
                    }
                    if (imob == "Evolk")
                    {
                        resultsImagemDet[i] = "https://www.engelvoelkers.com" + resultsImagemDet[i];
                    }
                    if (imob == "Iad")
                    {
                        resultsImagemDet[i] = "https://www.iadportugal.pt/cache/ad_photo_thumb" + resultsImagemDet[i];
                    }
                    if (imob == "Porta")
                    {
                        resultsImagemDet[i] = "https://static" + resultsImagemDet[i];
                    }
                    if (imob == "Mclass" || imob == "Casa10" || imob == "Casa10Lou" || imob == "Mont" || imob == "RioMag" || imob == "RioMagLou")
                    {
                        resultsImagemDet[i] = "http:" + resultsImagemDet[i].Replace("Z140x105", "Z640x480").Replace("Z220x165", "Z640x480");
                    }
                    if (imob == "Era" || imob == "EraLou" || imob == "Sotheby")
                    {
                        resultsImagemDet[i] = resultsImagemDet[i].Replace("&amp;","&");
                    }
                    if (imob == "Lux")
                    {
                        resultsImagemDet[i] = "https://www.luxnlust.com/wp-content/uploads" + resultsImagemDet[i];
                    }
                    if (imob == "Century")
                    {
                        resultsImagemDet[i] = "https://www.century21global.com/personal/c21/sslProxy.action?url=http%3A%" + resultsImagemDet[i];
                    }
                    mlsDetails.LinkImagem = resultsImagemDet[i];
                    mlsDetailsList.Add(mlsDetails);
                }
                if (mlsDetailsList.Count == 0)
                {
                    mlsDetails mlsDetails = new mlsDetails();
                    mlsDetails.LinkImagem = "n/a";
                    mlsDetailsList.Add(mlsDetails);

                }
                item.MlsDetails = mlsDetailsList;
            }

            var IdProcomp = ConfigurationManager.AppSettings["IdProcomp"];
            var IdUsuario = ConfigurationManager.AppSettings["IdUsuario"];
            var IdImob = ConfigurationManager.AppSettings["IdImob" + imob];
            List<Imoveis> imoveis = new List<Imoveis>();
            string strSql;

            foreach (var item in mlspesqList)
            {
                if (item.Concelho == null || item.Concelho == string.Empty)
                {
                    continue;
                }
                var freguesia = RetornaFreguesia(item.Concelho, imob);

                // pesquisa no banco se imovel ja foi inserido
                strSql = "select * from imoveis where LinkAnuncio = '" + item.Link + "'";
                imoveis = dbhelper.SelecionePesq(strSql);
                if (imoveis.Count == 0 && !item.Titulo.ToUpper().Contains("VENDIDO") && !item.Titulo.ToUpper().Contains("VENDIDA"))
                {
                    strSql = "INSERT INTO [dbo].[Imoveis] ([Descricao],[Id_Imobiliaria],[LinkImagemPrincipal],[LinkAnuncio],[Id_Owner],[property_title],[property_type]";
                    strSql += ",[property_offer] ,[property_price],[property_lot_size],[property_area]";
                    strSql += ",[property_bedrooms],[property_suites] ,[property_bathrooms] ";
                    strSql += ",[property_parking], [property_city]";
                    strSql += ",[property_freguesia] ,[property_estado_imovel] ,[property_estado_angariacao] ,[property_dt_incluido] ,[property_concelho]";
                    strSql += ",[property_zona] ,[Id_Freguesia],[property_avaliacao_preco] ,[Id_Usuario]";
                    strSql += ",[Id_ProComp],[property_Eficiencia_Energetica]";
                    strSql += ",[property_comissao],[property_comissao_tipo]";
                    strSql += ",[Id_Externo],[roperty_map_address], [property_address], [property_dt_alterado], [property_exercise_room], [chkInserted], [TelefoneProprietario], [NomeProprietario], [TipoAnunciante]) VALUES ";
                    strSql += "('" + item.Descricao.Replace("'","") + "', " + IdImob + ", '" + item.MlsDetails.First().LinkImagem + "', '" + item.Link + "', 7, '" + item.Titulo.Replace("'","") + "', '" + item.Tipo + "', 'Venda', ";
                    strSql += item.Valor.Replace(".", "").Replace(" ", "") + ", " + item.AreaB.Replace("n/d","0").Replace("hectares", "") + ", " + item.Area.Replace("n/d","0").Trim().Replace("De","").Replace("a", "").Replace(" ", "") + ", " + item.Quartos.Replace("n/d", "0").Replace("-","0").Replace(",","").Replace("ou", "").Replace("superior", "").Trim() + ", " + item.Suites + ", " + item.Banho.Replace("n/d", "0").Replace("ou","").Replace(" ", "") + ", " + item.Garagem + ", 'Lisboa', ";
                    strSql += "'" + freguesia.DesignacaoCC + "', 'Usado', 'Ativo', '" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + "-" + DateTime.Now.Year + "', '" + freguesia.DesignacaoFR;
                    strSql += "', 'Centro', " + freguesia.Id_Freguesia + ", 'Razoavel', " + IdUsuario + ", " + IdProcomp.Trim() + ", 'A', 5, 'Percentual', '" + item.Id.Trim() + "', ' ', '" + item.Endereco + "', '" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + "-" + DateTime.Now.Year + "', '1', 'True', '" + item.Telefone + "','" + item.Nome + "','" + item.TipoAnunciante + "')";
                    int Id_Imovel = dbhelper.ExecutaEscalar(strSql);
                    if (Id_Imovel == 0)
                    {
                        Console.WriteLine("Erro ao Inserir Imóvel: " + item.Id + " - " + item.Titulo + " / " + DateTime.Now.ToString());
                    }
                    foreach (var itemImg in item.MlsDetails)
                    {
                        strSql = "INSERT INTO [dbo].[ImoveisImg] ([Id_Imovel],[LinkImagem]) VALUES (";
                        strSql += Id_Imovel + ", '" + itemImg.LinkImagem + "')";
                        dbhelper.ExecutaQuery(strSql);
                    }
                    Console.WriteLine("Novo Imóvel inserido: " + item.Id + " - " + item.Titulo + " / " + DateTime.Now.ToString());
                }
                else
                {
                    Console.WriteLine("Imóvel Já Existe: " + item.Id);
                }
            }
        }

        public static List<string> EverythingBetween(string source, string start, string end)
        {
            var results = new List<string>();

            string pattern = string.Format(
                "{0}({1}){2}",
                Regex.Escape(start),
                ".+?",
                 Regex.Escape(end));

            foreach (Match m in Regex.Matches(source, pattern))
            {
                string newstr;
                newstr = m.Groups[1].Value.Replace("<strong>", "").Replace("<em>", "").Replace("&nbsp;", " ").Replace("'", "");

                var pos1 = newstr.LastIndexOf(start);
                if (pos1 > 0)
                {
                    pos1 += start.Length;
                    newstr = newstr.Substring(pos1, newstr.Length - pos1);
                    results.Add(newstr);
                }
                else { results.Add(newstr); }
            }

            return results;
        }

        private static List<string> ExtractFromString(string text, string startString, string endString)
        {
            List<string> matched = new List<string>();
            int indexStart = 0, indexEnd = 0;
            bool exit = false;
            while (!exit)
            {
                indexStart = text.IndexOf(startString);
                indexEnd = text.IndexOf(endString);
                if (indexStart != -1 && indexEnd != -1 && indexEnd > indexStart)
                {
                    matched.Add(text.Substring(indexStart + startString.Length,
                        indexEnd - indexStart - startString.Length));
                    text = text.Substring(indexEnd + endString.Length);
                }
                else
                    exit = true;
            }
            //for (int i = 0; i < matched.Count; i++)
            //{
            //    matched[i] = matched[i].Replace("&nbsp;", "")
            //        .Replace("<div>", "")
            //        .Replace("</div>", "")
            //        .Replace("'", "")
            //        .Replace("<br>", "\n")
            //        .Replace("<span>","")
            //        .Replace("</span>", "")
            //        .Replace("<b>","")
            //        .Replace("<p>","")
            //        .Replace("</b>", "")
            //        .Replace("</p>", "")
            //        .Replace("</U>", "")
            //        .Replace("<U>", "")
            //        .Replace("</I>", "")
            //        .Replace("<I>", "");
            //}
            return matched;
        }

        private string SubstituiCaracteres(string StrOriginal)
        {
            return StrOriginal.Replace("&Uacute;", "Ú")
                            .Replace("&uacute;", "ú")
                            .Replace("&Atilde;", "Ã")
                            .Replace("&atilde;", "ã")
                            .Replace("&Otilde;", "Õ")
                            .Replace("&otilde;", "õ")
                            .Replace("&Aacute;", "Á")
                            .Replace("&aacute;", "á")
                            .Replace("&Iacute;", "Í")
                            .Replace("&iacute;", "í")
                            .Replace("&Eacute;", "É")
                            .Replace("&eacute;", "é")
                            .Replace("&Ccedil;", "Ç")
                            .Replace("&ccedil;", "ç")
                            .Replace("&Ecirc;", "Ê")
                            .Replace("&ecirc;", "ê")
                            .Replace("&Acirc;", "Â")
                            .Replace("&acirc;", "â")
                            .Replace("<span class=\"openSansSB t12 cinza17\" title=", "").Replace("</span>", "")
                            .Replace("< span class=\"openSansR t12 cinza17\" title=", "")
                            .Replace("<span class=\"openSansR t12 cinza17\" title=", "")
                            .Replace(">", "").Replace("999", "").Replace("'", "").Replace("\r", "").Replace("\n", "").Replace("\t", "").Replace("<span","");
        }

        private Freguesia RetornaFreguesia(string Concelho, string imob)
        {
            dbHelper dbhelper = new dbHelper();
            List<Freguesia> freguesia = new List<Freguesia>();

            string[] concelho = Concelho.Split(',');
            string strSql = string.Empty;
            string Id_Freguesia = "1";
            string DesignacaoCC = "Lisboa", DesignacaoFR = "Lisboa";

            if (concelho.Count() != 1 && concelho.Count() != 2)
            {
                strSql = "select * from Freguesia where Id_Freguesia = " + Id_Freguesia;
                freguesia = dbhelper.SelecioneFreguesia(strSql);
            }
            if (concelho.Count() == 2)
            {
                strSql = "select * from Freguesia where DesignacaoCC like '%" + concelho[0].TrimEnd().TrimStart() + "%' and DesignacaoFR like '%" + concelho[1].TrimEnd().TrimStart() + "%'";
                freguesia = dbhelper.SelecioneFreguesia(strSql);
                DesignacaoCC = concelho[0].TrimEnd().TrimStart();
                DesignacaoFR = concelho[1].TrimEnd().TrimStart();
            }
            if (concelho.Count() == 1)
            {
                strSql = "select * from Freguesia where DesignacaoCC = 'Lisboa' and DesignacaoFR = '" + concelho[0].TrimEnd().TrimStart() + "'";
                freguesia = dbhelper.SelecioneFreguesia(strSql);
                DesignacaoCC = "Lisboa";
                DesignacaoFR = concelho[0].TrimEnd().TrimStart();
            }
            if (freguesia.Count == 0)
            {
                if (DesignacaoFR.Contains("/") || DesignacaoFR.Length < 4)
                {
                    strSql = "select * from Freguesia where Id_Freguesia = " + Id_Freguesia;
                    freguesia = dbhelper.SelecioneFreguesia(strSql);
                }
                else
                {
                    strSql = "INSERT INTO [dbo].[Freguesia] ([Distrito],[Designacao],[DesignacaoCC],[DesignacaoFR]) VALUES";
                    strSql += "('11', 'Lisboa', '" + DesignacaoCC + "', '" + DesignacaoFR + "')";
                    Id_Freguesia = dbhelper.ExecutaEscalar(strSql).ToString();
                    strSql = "select * from Freguesia where Id_Freguesia = " + Id_Freguesia;
                    freguesia = dbhelper.SelecioneFreguesia(strSql);
                }
            }

            return freguesia.First();
        }

        public void FixTypes()
        {
            string strsql = string.Empty;

            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Águas%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Andar%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Ateliê%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Condomínio%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Construção%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Empreendimento%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Espaço%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Estalagem%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Excepcional%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Fazenda%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Fazenda/Quinta%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Geminada%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Imóvel%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Massamá-Salão%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Mercearia%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Moinho%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Monte%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Padaria / Pastelaria%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Padaria/confeitaria%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Palacete%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Palácio%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Parque%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Parqueamento%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Penthouse%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Porto%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Predio%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Propriedade%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Restaurante%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Snack%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Torre%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Venda%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Venda/Arrendo%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Vila%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Vivenda%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Arrecadação%';";
            strsql += "update Imoveis set property_type = 'Café' where property_type like '%Cafetaria%';";
            strsql += "update Imoveis set property_type = 'Café' where property_type like '%Bar%';";
            strsql += "update Imoveis set property_type = 'Loja' where property_type like '%Comércio%';";
            strsql += "update Imoveis set property_type = 'Prédio' where property_type like '%Edifício%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Escritórios%';";
            strsql += "update Imoveis set property_type = 'Garagem' where property_type like '%Garagem-Tapada%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Vivenda%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Vivenda%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Vivenda%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Vivenda%';";
            strsql += "update Imoveis set property_type = 'Outros' where property_type like '%Vivenda%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Apartamento%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Indefinido%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Lisbon%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%São%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Sky%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%TASSO%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Apart%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Ampla%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Belas%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Fant%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%FANT%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Grande%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Espect%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Grande%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Grande%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Grande%';";
            strsql += "update Imoveis set property_type = 'Garagem' where property_type like '%garagem%';";
            strsql += "update Imoveis set property_type = 'Garagem' where property_type like '%Estacionamento%';";
            strsql += "update Imoveis set property_type = 'Escritório' where property_type like '%Escritórios%';";

            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Único%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Valor%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Tróia%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Venha%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Tres%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%TRES%';";

            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Vende%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Yield%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Grande%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Benfica%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Cascais%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Cedência%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%ERICEIRA%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Espetacular%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%ESTREAR%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Excelente%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Fabulo%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Espet%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Magnifica%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Open%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Santa%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Lindo%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%LINDO%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Luciano%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Luís%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Belíssimo%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Magnifico%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Soberbo%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%T1%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%T2%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%T3%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%T4%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%T5%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%T6%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Maravilhoso%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Martinhal%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Oeiras%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Ótima%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Ótimo%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Renovado%';";
            strsql += "update Imoveis set property_type = 'Moradia' where property_type like '%Resid%';";
            strsql += "update Imoveis set property_type = 'Terreno' where property_type = '';";
            strsql += "update Imoveis set property_type = 'Terreno' where property_type like '%Terreno%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Loft%';";
            strsql += "update Imoveis set property_type = 'Andar' where property_type like '%Andar%';";
            strsql += "update Imoveis set property_type = 'Bar' where property_type like '%Bar%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Duplex%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Triplex%';";
            strsql += "update Imoveis set property_type = 'Espaço' where property_type like '%Espaço%';";
            strsql += "update Imoveis set property_type = 'Loja' where property_type like '%Lojas%';";
            strsql += "update Imoveis set property_type = 'Terreno' where property_type like '%Lote%';";
            strsql += "update Imoveis set property_type = 'Moradia' where property_type like '%Moradia%';";
            strsql += "update Imoveis set property_type = 'Moradia' where property_type like '%Casa%';";
            strsql += "update Imoveis set property_type = 'Propriedade' where property_type like '%Propriedade%';";
            strsql += "update Imoveis set property_type = 'Propriedade' where property_type like '%Propriedade%';";
            strsql += "update Imoveis set property_type = 'Comércio' where property_type like '%Comercial%';";
            strsql += "update Imoveis set property_type = 'Moradia' where property_type like '%Habitação%';";
            strsql += "update Imoveis set property_type = 'Hotel' where property_type like '%Hôtel%';";
            strsql += "update Imoveis set property_type = 'Loja' where property_type like '%Estabelecimento%';";
            strsql += "update Imoveis set property_type = 'Quinta' where property_type like '%Quintinha%';";

            strsql += "update Freguesia set DesignacaoFR = REPLACE(DesignacaoFR, 'class=\"light\"&rsaquo; Lisboa  class=\"light\"&rsaquo; Lisboa', '')";
            strsql += "update Freguesia set DesignacaoFR = REPLACE(DesignacaoFR, '/<input type=\"hidden\" id=\"ControlRT_zona\" value=\"Paço de Arcos', '')";
            strsql += "update Freguesia set DesignacaoFR = REPLACE(DesignacaoFR, '/<input type=\"hidden\" id=\"ControlRT_zona\" value=\"Lisboa', '')";
            strsql += "update Freguesia set DesignacaoFR = REPLACE(DesignacaoFR, '/<input type=\"hidden\" id=\"ControlRT_zona\" value=\"Expo', '')";
            strsql += "update Freguesia set DesignacaoFR = REPLACE(DesignacaoFR, '/<input type=\"hidden\" id=\"ControlRT_zona\" value=\"Amoreiras', '')";
            strsql += "update Freguesia set DesignacaoFR = REPLACE(DesignacaoFR, '\" /<input type=\"hidden\" id=\"ControlRT_zona\" value=\"Sete Rios', '')";
            strsql += "update Freguesia set DesignacaoFR = REPLACE(DesignacaoFR, '\" /<input type=\"hidden\" id=\"ControlRT_zona\" value=\"Oeiras Park', '')";
            strsql += "update Freguesia set DesignacaoFR = REPLACE(DesignacaoFR, ' class=\"light\"&rsaquo; Loures  class=\"light\"&rsaquo; Lisboa', '')";
            strsql += "update Freguesia set DesignacaoFR = REPLACE(DesignacaoFR, '\"', '')";
            strsql += "update Freguesia set DesignacaoFR = 'ZZZZZZZZZZZ' where rtrim(DesignacaoFR) = ''";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'APA';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'T0';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'T1';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'T2';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'T3';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'T4';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = '|';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%2%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Alameda%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type like '%Anjos%';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'Ao';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'A';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'Aprtamento';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'Baixa-Chiado';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'Benfica-muito';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'No';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'NO';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'Oportunidade';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'OPORTUNIDADE';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'PARTICULAR';";
            strsql += "update Imoveis set property_type = 'Apartamento' where property_type = 'Rentabilidade:';";
            dbHelper dbhelper = new dbHelper();
            dbhelper.ExecutaQuery(strsql);
        }

        public void ZeraAgencia(string imob)
        {
            string strsql = "delete Imoveis where Id_Imobiliaria = " + imob; 
            dbHelper dbhelper = new dbHelper();
            dbhelper.ExecutaQuery(strsql);
        }

        public void ImportarCodPostalFreguesia()
        {
            string line,StrSql;
            CultureInfo pt = CultureInfo.GetCultureInfo("pt-BR");
            dbHelper dbHelper = new dbHelper();
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\SVN\Projetos.Net\MLSMarketing\MLSMarketing\bin\Debug\todos_cp.txt", Encoding.GetEncoding(pt.TextInfo.ANSICodePage), true);
            while ((line = file.ReadLine()) != null)
            {
                var linem = line.Split(';');
                if (linem[0] == "11")
                {
                    StrSql = "insert into CodigoPostal (CodigoPostalRaiz,Distrito,Concelho,Freguesia) values ";
                    StrSql += "('" + linem[14] + "-" + linem[15] + "','Lisboa','" + linem[3] + "','" + linem[16] + "')";
                    dbHelper.ExecutaQuery(StrSql);
                }
            }
        }


    }
}
