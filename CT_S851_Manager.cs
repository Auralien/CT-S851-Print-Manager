using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace PrintManager
{
    class CT_S851_Manager
    {
        private Socket ClientSocket = null;
        private Encoding Unicode = Encoding.Unicode;
        private Encoding UserEncoding;

        private const int CodePage_PC866_Russian = 17;

        /// <summary>
        /// Method opens connection to printer with printer's IP address and default port 9100
        /// </summary>
        /// <param name="PrinterAddress">Printer's IP address (e.g. "192.168.1.117")</param>
        /// <returns>
        /// 0  - success
        /// >0 - errors
        /// </returns>
        public int ConnectToPrinter(string PrinterAddress)
        {
            return ConnectToPrinter(PrinterAddress, 9100);
        }

        /// <summary>
        /// Method opens connection to printer with printer's IP address and port number
        /// </summary>
        /// <param name="PrinterAddress">Printer's IP address (e.g. "192.168.1.117")</param>
        /// <param name="PrinterPort">Printer's Port (e.g. 9100)</param>
        /// <returns>
        /// 0  - success
        /// >0 - errors
        /// </returns>
        public int ConnectToPrinter(string PrinterAddress, int PrinterPort)
        {
            // Create new socket
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect to remote printer with its IPEndPoint
            IPAddress printerIPAddress = IPAddress.Parse(PrinterAddress);
            IPEndPoint printerIPEndPoint = new IPEndPoint(printerIPAddress, PrinterPort);

            try { this.ClientSocket.Connect(printerIPEndPoint); }
            catch (ArgumentNullException)
            {
                this.ClientSocket = null;
                return 1;
            }
            catch (ArgumentOutOfRangeException)
            {
                this.ClientSocket = null;
                return 2;
            }
            catch (SocketException)
            {
                this.ClientSocket = null;
                return 3;
            }
            catch (ObjectDisposedException)
            {
                this.ClientSocket = null;
                return 4;
            }
            catch (ArgumentException)
            {
                this.ClientSocket = null;
                return 5;
            }
            catch (Exception)
            {
                this.ClientSocket = null;
                return 6;
            }

            return 0;
        }

        /// <summary>
        /// Method closes connection to remote printer
        /// </summary>
        /// <returns>
        /// 0 - success
        /// 1 - socket error
        /// 2 - attempt to get access to closed socket
        /// </returns>
        public int DisconnectFromPrinter()
        {
            try { this.ClientSocket.Shutdown(SocketShutdown.Both); }
            catch (SocketException) { return 1; }
            catch (ObjectDisposedException) { return 2; }

            this.ClientSocket.Close();
            return 0;
        }

        /// <summary>
        /// Method sets custom user encoding
        /// </summary>
        /// <param name="UserEncodingToSet"></param>
        /// <returns>
        /// 0  - success
        /// >0 - error
        /// </returns>
        public int SetUserEncoding(Encoding UserEncodingToSet)
        {
            this.UserEncoding = UserEncodingToSet;

            switch (this.UserEncoding.CodePage)
            {
                case 866:
                    if (SetCodepage_PC866() == 0)   return 0;
                    else                            return 1;
                default:
                    break;
            }

            return 2;
        }

        /// <summary>
        /// Method cuts the ticket
        /// </summary>
        public void TicketCut()
        {
            //char[] ticketCut = new char[] { '\x001B', 'J', '\x000A', '\x000A', '\x000A', '\x000A', '\x000A', '\x000A', '\x000A', '\x000A', '\x000A', '\x001B', 'i' };
            char[] ticketCut = new char[] { '\x000A', '\x000A', '\x000A', '\x000A', '\x000A', '\x000A', '\x000A', '\x000A', '\x000A', '\x001B', 'i' };
            byte[] ticketCutBytes = this.UserEncoding.GetBytes(ticketCut);
            this.ClientSocket.Send(ticketCutBytes);
        }

        /// <summary>
        /// Method sets Russian codepage PC866
        /// </summary>
        /// <returns>
        /// 0  - success
        /// >0 - error
        /// </returns>
        private int SetCodepage_PC866()
        {
            return SetCodepage(CodePage_PC866_Russian);
        }

        /// <summary>
        /// Method sets custom codepage
        /// </summary>
        /// <param name="CodepageCode">Codepage code</param>
        /// <returns>
        /// 0  - success
        /// >0 - error
        /// </returns>
        private int SetCodepage(int CodepageCode)
        {
            char[] Command = new char[] { '\x001B', 't', (char)CodepageCode };
            byte[] CommandBytes = UserEncoding.GetBytes(Command);

            try { this.ClientSocket.Send(CommandBytes); }
            catch (SocketException) { return 1; }
            catch (ArgumentNullException) { return 2; }
            catch (ObjectDisposedException) { return 3; }
            catch (Exception) { return 4; }
            return 0;
        }

        /// <summary>
        /// Method prints Message string with custom encoding
        /// </summary>
        /// <param name="Message">Message text</param>
        /// <param name="MessageEncoding">Message encoding</param>
        /// <returns>
        /// 0  - success
        /// >0 - error
        /// </returns>
        public int PrintString(string Message, Encoding MessageEncoding)
        {
            byte[] Command = MessageEncoding.GetBytes(Message);
            byte[] CommandBytes = Encoding.Convert(MessageEncoding, UserEncoding, Command);

            try { this.ClientSocket.Send(CommandBytes); }
            catch (SocketException) { return 1; }
            catch (ArgumentNullException) { return 2; }
            catch (ObjectDisposedException) { return 3; }
            catch (Exception) { return 4; }
            return 0;
        }

        /// <summary>
        /// Method prints Message string with Unicode encoding
        /// </summary>
        /// <param name="Message">Message text</param>
        /// <returns>
        /// 0  - success
        /// >0 - error
        /// </returns>
        public int PrintString(string Message)
        {
            return PrintString(Message, Encoding.Unicode);
        }

        /// <summary>
        /// Method sets font size
        /// </summary>
        /// <param name="FontSizeCode">Code of font size: 1 - Small, 2 - Medium, 3 - Large</param>
        /// <returns>
        /// 0  - success
        /// >0 - error
        /// </returns>
        public int SetFontSize(int FontSizeCode)
        {
            int FontSize;
            switch (FontSizeCode)
            {
                case 1: FontSize = 2; break;
                case 2: FontSize = 1; break;
                case 3: FontSize = 0; break;
                default: FontSize = 0; break;
            }

            char[] Command = new char[] { '\x001B', 'M', (char)FontSize };
            byte[] CommandBytes = this.UserEncoding.GetBytes(Command);
            try { this.ClientSocket.Send(CommandBytes); }
            catch (SocketException) { return 1; }
            catch (ArgumentNullException) { return 2; }
            catch (ObjectDisposedException) { return 3; }
            catch (Exception) { return 4; }
            return 0;
        }

        /// <summary>
        /// Method sets font weight
        /// </summary>
        /// <param name="FontWeightCode">Code of font weight: 0 - normal, 1 - bold</param>
        /// <returns>
        /// 0  - success
        /// >0 - error
        /// </returns>
        public int SetFontWeight(int FontWeightCode)
        {
            int FontWeight;
            switch (FontWeightCode)
            {
                case 0: FontWeight = 0; break;
                case 1: FontWeight = 1; break;
                default: FontWeight = 0; break;
            }

            char[] Command = new char[] { '\x001B', 'G', (char)FontWeight };
            byte[] CommandBytes = this.UserEncoding.GetBytes(Command);
            try { this.ClientSocket.Send(CommandBytes); }
            catch (SocketException) { return 1; }
            catch (ArgumentNullException) { return 2; }
            catch (ObjectDisposedException) { return 3; }
            catch (Exception) { return 4; }
            return 0;
        }

        /// <summary>
        /// Method sets black/white inverted printing mode
        /// </summary>
        /// <param name="code">Code of inverted printing mode: 0 - normal, 1 - inverted</param>
        /// <returns>
        /// 0  - success
        /// >0 - error
        /// </returns>
        public int SetInvertedPrintingMode(int code)
        {
            int InvertedPrintingMode;
            switch (code)
            {
                case 0: InvertedPrintingMode = 0; break;
                case 1: InvertedPrintingMode = 1; break;
                default: InvertedPrintingMode = 0; break;
            }

            char[] Command = new char[] { '\x001D', 'B', (char)InvertedPrintingMode };
            byte[] CommandBytes = this.UserEncoding.GetBytes(Command);
            try { this.ClientSocket.Send(CommandBytes); }
            catch (SocketException) { return 1; }
            catch (ArgumentNullException) { return 2; }
            catch (ObjectDisposedException) { return 3; }
            catch (Exception) { return 4; }
            return 0;
        }
    }
}
