using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Threading;

namespace ClassiCraft {
    static class Heartbeat {
        static readonly TimeSpan Timeout = TimeSpan.FromSeconds( 10 );
        static readonly TimeSpan Delay = TimeSpan.FromSeconds( 25 );
        const string UrlFileName = "externalurl.txt";

        static readonly string Salt = GetRandomNumber( 16 );
        static Uri uri;


        public static void Start() {
            Thread heartbeatThread = new Thread( Beat ) { IsBackground = true };
            heartbeatThread.Start();
        }


        static void Beat() {
            while ( true ) {
                try {
                    string requestUri =
                        String.Format( "{0}?public={1}&max={2}&users={3}&port={4}&version=7&salt={5}&name={6}",
                                       "http://www.classicube.net/heartbeat.jsp",
                                       "True",
                                       Config.maxPlayers,
                                       Player.PlayerList.Count,
                                       Config.Port,
                                       Uri.EscapeDataString( Salt ),
                                       Uri.EscapeDataString( Config.Name ) );

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create( requestUri );
                    request.Method = "GET";
                    request.Timeout = (int)Timeout.TotalMilliseconds;
                    request.CachePolicy = new HttpRequestCachePolicy( HttpRequestCacheLevel.BypassCache );
                    request.UserAgent = "ClassiCraft";
                    request.ServicePoint.BindIPEndPointDelegate = BindIPEndPointCallback;

                    using ( HttpWebResponse response = (HttpWebResponse)request.GetResponse() ) {
                        var rs = response.GetResponseStream();
                        if ( rs == null ) continue;
                        using ( StreamReader responseReader = new StreamReader( rs ) ) {
                            string responseText = responseReader.ReadToEnd().Trim();
                            Uri newUri;
                            if ( Uri.TryCreate( responseText, UriKind.Absolute, out newUri ) ) {
                                if ( newUri != uri ) {
                                    File.WriteAllText( UrlFileName, newUri.ToString() );
                                    uri = newUri;
                                    Server.Log( "URL: " + uri.ToString() );
                                    Server.Log( "Url also saved to " + UrlFileName + "..." );
                                }
                            }
                        }
                    }
                    Thread.Sleep( Delay );

                } catch {
                    Server.Log( "Failed to send heartbeat to classicube.net..." );
                }
            }
        }

        public static string GetRandomNumber( int digitCount ) {
            string digits = "";
            Random rnd = new Random();
            for ( int i = 0; i < digitCount; i++ ) {
                int rndNumber = rnd.Next( 9 );
                if ( rndNumber > ( 9 / 2 ) ) {
                    string alphabet = "abcdefghijklmnopqrstuvwxyz";
                    char rndChar = alphabet[rnd.Next( 6 )];
                    digits += rndChar;
                } else {
                    digits += rndNumber;
                }
            }
            return digits;
        }

        static IPEndPoint BindIPEndPointCallback( ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount ) {
            return new IPEndPoint( IPAddress.Any, 0 );
        }
    }
}