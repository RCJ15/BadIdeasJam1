using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace DebugTools
{
    /// <summary>
    /// The logger window that appears at the bottom of the <see cref="DebugConsole"/>.
    /// </summary>
    public class DebugConsoleLogger : DebugConsoleWindow
    {
        private static StringBuilder _stringBuilder = new StringBuilder();

        private const int MAX_LOG_AMOUNT = 999;
        private static List<Log> _messageLogs = new List<Log>();
        private Vector2 _logScrollPosition;

        // Constructor
        public DebugConsoleLogger()
        {
            Application.logMessageReceivedThreaded += LogMessage;
        }

        // Destructor
        ~DebugConsoleLogger()
        {
            Application.logMessageReceivedThreaded -= LogMessage;
        }

        /// <summary>
        /// Adds a new log using all of the assigned information.
        /// </summary>
        public static void LogMessage(string message, LogType logType = LogType.Log)
        {
            LogMessage(message, null, logType);
        }

        /// <summary>
        /// Adds a new log using all of the assigned information.
        /// </summary>
        public static void LogMessage(string message, string stackTrace, LogType logType = LogType.Log)
        {
            _messageLogs.Insert(0, new Log(message, stackTrace, logType));

            if (_messageLogs.Count > MAX_LOG_AMOUNT)
            {
                _messageLogs.RemoveAt(MAX_LOG_AMOUNT);
            }
        }

        /// <summary>
        /// Clears all logs.
        /// </summary>
        public static void ClearLog()
        {
            _messageLogs.Clear();
        }

        public override void OnOpen()
        {
            foreach (Log log in _messageLogs)
            {
                log.Expanded = false;
            }

            _logScrollPosition = Vector2.zero;
        }

        public override void Draw(Rect rect, Event evt)
        {
            GUILayout.BeginArea(rect);

            // Begin scroll view
            _logScrollPosition = GUILayout.BeginScrollView(_logScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(rect.width), GUILayout.Height(rect.height));

            GUIStyle logStyle = GUI.skin.customStyles[5];

            // Draw logs
            foreach (Log log in _messageLogs)
            {
                log.Draw(logStyle);

                GUILayout.Space(4);
            }

            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }


        /// <summary>
        /// Data for a single log in the <see cref="DebugConsoleLogger"/>.
        /// </summary>
        public class Log
        {
            public const char EXPAND_ICON = '▶';
            public const char COLLAPSE_ICON = '▼';

            /// <summary>
            /// The message of the log.
            /// </summary>
            public string Message { get; private set; }
            /// <summary>
            /// Where the log came from.
            /// </summary>
            public string StackTrace { get; private set; }
            /// <summary>
            /// What type of log this is.
            /// </summary>
            public LogType LogType { get; private set; }
            /// <summary>
            /// The time at which this log was received.
            /// </summary>
            public string DateReceived { get; private set; }
            /// <summary>
            /// Wether or not the <see cref="StackTrace"/> of this log is expanded.
            /// </summary>
            public bool Expanded { get; set; } = false;

            /// <summary>
            /// If this log can be expanded or not.
            /// </summary>
            public bool IsExpandable => !string.IsNullOrEmpty(StackTrace);

            private string _cachedExpandedToString = null;
            private string _cachedCollapsedToString = null;

            /// <summary>
            /// Draws this log using <see cref="GUILayout"/>.
            /// </summary>
            public void Draw(GUIStyle style)
            {
                if (IsExpandable)
                {
                    if (GUILayout.Button(ToString(), style))
                    {
                        Expanded = !Expanded;
                    }
                }
                else
                {
                    GUILayout.Label(ToString(), style);
                }
            }

            public Log(string message, string stackTracke, LogType logType)
            {
                Message = message;
                StackTrace = stackTracke;
                LogType = logType;
                DateReceived = DateTime.Now.ToString("H:mm:ss"); // Hours:Minutes:Seconds / 23:59:59 format
            }

            public override string ToString()
            {
                if (Expanded)
                {
                    if (_cachedExpandedToString == null)
                    {
                        _cachedExpandedToString = ToStringInternal();
                    }

                    return _cachedExpandedToString;
                }
                else
                {
                    if (_cachedCollapsedToString == null)
                    {
                        _cachedCollapsedToString = ToStringInternal();
                    }

                    return _cachedCollapsedToString;
                }
            }

            private string ToStringInternal()
            {
                _stringBuilder.Clear();

                if (IsExpandable)
                {
                    if (Expanded)
                    {
                        _stringBuilder.Append(COLLAPSE_ICON);
                    }
                    else
                    {
                        _stringBuilder.Append(EXPAND_ICON);
                    }

                    _stringBuilder.Append(' ');
                }

                string dateFormat;

                switch (LogType)
                {
                    case LogType.Exception:
                    case LogType.Error:
                    case LogType.Assert:
                        dateFormat = "<color=red>[<b>{0}</b>]</color>";
                        break;
                    case LogType.Warning:
                        dateFormat = "<color=yellow>[<b>{0}</b>]</color>";
                        break;
                    default:
                        dateFormat = "[<b>{0}</b>]";
                        break;
                }

                _stringBuilder.Append(string.Format(dateFormat, DateReceived));

                _stringBuilder.Append('\t');

                _stringBuilder.Append(Message);

                if (IsExpandable && Expanded)
                {
                    _stringBuilder.Append('\n');
                    _stringBuilder.Append(StackTrace);
                }

                return _stringBuilder.ToString().Trim();
            }
        }
    }
}
