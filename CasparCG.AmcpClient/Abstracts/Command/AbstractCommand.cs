//////////////////////////////////////////////////////////////////////////////////
//
// Author: Sase
// Email: sase@stilsoft.net
//
// This software may be modified and distributed under the terms
// of the MIT license. See the LICENSE file for details.
//
//////////////////////////////////////////////////////////////////////////////////


using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataReceivedEventArgs = CasparCg.AmcpClient.Common.EventsArgs.DataReceivedEventArgs;
using CasparCg.AmcpClient.Common;


namespace CasparCg.AmcpClient.Abstracts.Command
{
    public abstract class AbstractCommand<TPacket, TParser, TParserResponse, TResponse>
        where TPacket : ICommandPacket
        where TParser : ICommandParser<TPacket, TParserResponse>, new()
        where TResponse : AbstractCommandResponse<TParserResponse>, new()
    {
        private object _lockExecute;

        //public TParserResponse outParserResponceExec;

        internal ICommandConnection Connection { get; set; }
        public int ResponseTimeout { get; set; } = 2000;

        
        protected abstract TPacket GetPacket();


        //public Task<TParserResponse> ExecuteAsync(ICommandConnection connection)
        public Task<TResponse> ExecuteAsync(ICommandConnection connection)
        {
            Connection = connection;

            //string dataRequested = "";
            var commandClone = (AbstractCommand<TPacket, TParser, TParserResponse, TResponse>)MemberwiseClone();
            var a = Task.Run(() => commandClone.Execute(connection/*, out dataRequested*/));

            //AmcpResponse.DataRequested = dataRequested;


            return a;
        }

        //public TParserResponse Execute(ICommandConnection connection)
        public TResponse Execute(ICommandConnection connection/*, out string dataRequested*/)
        {
            if (_lockExecute == null)
                _lockExecute = new object();

            lock (_lockExecute)
            {
                Connection = connection;
                //outParserResponceExec = null;

                if (connection == null)
                    throw new ArgumentNullException(nameof(connection), "Command connection cannot be null.");

                var packet = GetPacket();
                var parser = new TParser();
                var response = new TResponse();
                var parserCompleteEvent = new AutoResetEvent(false);

                parser.CommandPacket = packet;

                var parserResponse = default(TParserResponse);
                parser.ParserComplete += (s, e) =>
                {
                    Debug.WriteLine("Parser complete");
                    parserResponse = e.Response;
                    parserCompleteEvent.Set();
                };

                Exception parserException = null;
                parser.ParserError += (s, e) =>
                {
                    Debug.WriteLine("Parser error");
                    parserException = e.Exception;
                    parserCompleteEvent.Set();
                };

                EventHandler<DataReceivedEventArgs> dataReceivedEventHandler = (s, e) => parser.Parse(e.Data);

                if (ResponseTimeout > 0)
                {
                    Debug.WriteLine("Attach parser");
                    connection.DataReceived += dataReceivedEventHandler;
                }

                try
                {
                    Debug.WriteLine("Send: " + Encoding.UTF8.GetString(packet.Data).Replace("\r\n", ""));
                    CasparCg.AmcpClient.Common.AmcpResponse.DataRequested = Encoding.UTF8.GetString(packet.Data).Replace("\r\n", "");
                    //dataRequested = Encoding.UTF8.GetString(packet.Data).Replace("\r\n", "");

                    connection.Send(packet.Data);

                    // If response timeout is 0, do not wait parser to complete 
                    if (ResponseTimeout > 0)
                    {
                        // Wait parser to complete
                        var isParserComplete = parserCompleteEvent.WaitOne(ResponseTimeout);

                        Debug.WriteLine("Deattach parser");
                        connection.DataReceived -= dataReceivedEventHandler;

                        if (parserException != null)
                            ExceptionDispatchInfo.Capture(parserException).Throw();

                        if (!isParserComplete)
                            throw new TimeoutException("Command response timeout.");

                        if (parserResponse != null)
                            response.ProcessData(parserResponse);
                    }
                }
                catch
                {
                    if (ResponseTimeout > 0)
                    {
                        Debug.WriteLine("Deattach parser");
                        connection.DataReceived -= dataReceivedEventHandler;
                    }
                    return response;
                    //throw;
                }


                //return parserResponse;
                //outParserResponceExec = parserResponse;
                return response;
            }
        }
    }
}