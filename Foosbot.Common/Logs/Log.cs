﻿using Foosbot.Common.Logs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Foosbot
{
    /// <summary>
    /// Log singleton Class.
    /// Can be accessed from any part of program.
    /// </summary>
    /// <author>Joseph Gleyzer</author>
    /// <date>04.02.2016</date>
    public sealed class Log
    {
        #region Common Log

        /// <summary>
        /// Common Log Synchronization Token
        /// </summary>
        private static object _commonToken = new Object();

        /// <summary>
        /// Common Log Singleton Instance
        /// </summary>
        private static Log _common;

        /// <summary>
        /// Common Log Property of Singleton
        /// </summary>
        public static Log Common
        {
            get
            {
                if (_common == null)
                {
                    lock (_commonToken)
                    {
                        if (_common == null)
                            _common = new Log(eLogType.Common);
                    }
                }
                return _common;
            }
        }

        #endregion Common Log

        #region Image Processing

        /// <summary>
        /// Image Preocessing Log Synchronization Token
        /// </summary>
        private static object _imageProcessingToken = new Object();

        /// <summary>
        /// Image Preocessing Log Singleton Instance
        /// </summary>
        private static Log _imageProcessing;

        /// <summary>
        /// Image Preocessing Log Property of Singleton
        /// </summary>
        public static Log ImageProcessing
        {
            get
            {
                if (_imageProcessing == null)
                {
                    lock (_imageProcessingToken)
                    {
                        if (_common == null)
                            _imageProcessing = new Log(eLogType.ImageProcessing);
                    }
                }
                return _imageProcessing;
            }
        }

        #endregion Image Processing

        /// <summary>
        /// Printing Logger Thread
        /// </summary>
        private Thread _printerThread;

        /// <summary>
        /// Log Message Queue
        /// </summary>
        private Queue<LogMessage> _messageQ;

        /// <summary>
        /// Log Queue Token
        /// </summary>
        private object _token = new Object();

        /// <summary>
        /// Current Log Type
        /// </summary>
        private eLogType _type;

        /// <summary>
        /// Private Log Singleton Constructor
        /// </summary>
        private Log() { }

        /// <summary>
        /// Private Log Singleton Constructor - in use
        /// </summary>
        /// <param name="type">Type of log to instantiate and start</param>
        private Log(eLogType type)
        {
            _type = type;
            _messageQ = new Queue<LogMessage>();
            StartLogger();
        }

        /// <summary>
        /// Starts Execution of logging background thread
        /// </summary>
        private void StartLogger()
        {
            _printerThread = new Thread(() =>
            {
                while (true)
                {
                    if (_messageQ.Count != 0)
                    {
                        lock (_token)
                        {
                            LogMessage message = _messageQ.Dequeue();
                            using (StreamWriter file = File.AppendText(String.Format("{0}.log", _type.ToString())))
                            {
                                file.WriteLine("{0}\t{1}\t{2}", DateTime.Now.ToString("HH:mm:ss"), message.Category, message.Description);
                            }
                        }
                    }
                }
            });
            _printerThread.IsBackground = true;
            _printerThread.Start();
        }

        /// <summary>
        /// Print Log Method
        /// </summary>
        /// <param name="message">Message to print</param>
        /// <param name="category">Category of message</param>
        private void Print(string message, eLogCategory category)
        {
            LogMessage m = new LogMessage(message, category);
            lock (_token)
            {
                _messageQ.Enqueue(m);
            }
        }
        
        /// <summary>
        /// Debug Print
        /// </summary>
        /// <param name="message">Message to print</param>
        public void Debug(string message)
        {
            Print(message, eLogCategory.Debug);
        }

        /// <summary>
        /// Info Print
        /// </summary>
        /// <param name="message">Message to print</param>
        public void Info(string message)
        {
            Print(message, eLogCategory.Info);
        }

        /// <summary>
        /// Warning Print
        /// </summary>
        /// <param name="message">Message to print</param>
        public void Warning(string message)
        {
            Print(message, eLogCategory.Warning);
        }

        /// <summary>
        /// Error Print
        /// </summary>
        /// <param name="message">Message to print</param>
        public void Error(string message)
        {
            Print(message, eLogCategory.Error);
        }
    }

}