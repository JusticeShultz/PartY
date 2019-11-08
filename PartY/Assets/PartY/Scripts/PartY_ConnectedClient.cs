using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization;
using UnityEngine;
using System.Net;

namespace PartY
{
    public class PartY_ConnectedClient
    {
        #region Data
        /// <summary>
        /// For Clients, the connection to the server.
        /// For Servers, the connection to a client.
        /// </summary>
        readonly TcpClient connection;

        readonly byte[] readBuffer = new byte[5000];

        NetworkStream stream
        {
            get
            {
                if (connection.Connected)
                    return connection.GetStream();
                else return null;
            }
        }
        #endregion

        #region Init
        public PartY_ConnectedClient(TcpClient tcpClient)
        {
            this.connection = tcpClient;
            this.connection.NoDelay = true; // Disable Nagle's cache algorithm
        }

        internal void Close()
        {
            connection.Close();
        }
        #endregion

        #region Async Events
        void OnRead(IAsyncResult ar)
        {
            if (stream == null)
            {
                PartY.instance.OnDisconnect(this);
                return;
            }

            int length = stream.EndRead(ar);

            if (length <= 0)
            { // Connection closed
                PartY.instance.OnDisconnect(this);
                return;
            }

            string newMessage = System.Text.Encoding.UTF8.GetString(readBuffer, 0, length);

            //PartY.messageToDisplay += newMessage + Environment.NewLine;

            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
        }

        internal void EndConnect(IAsyncResult ar)
        {
            connection.EndConnect(ar);

            stream.BeginRead(readBuffer, 0, readBuffer.Length, OnRead, null);
        }
        #endregion

        #region API
        internal void SendTextData(string @message)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(@message));
        }

        internal void SendStringData(string variable, string @string)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @string));
        }

        internal void SendVector3Data(string variable, Vector3 @vector3)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @vector3.x + "," + @vector3.y + "," + @vector3.z));
        }

        internal void SendVector2Data(string variable, Vector2 @vector2)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @vector2.x + "," + @vector2.y));
        }

        internal void SendIntegerData(string variable, int @integer)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @integer));
        }

        internal void SendFloatData(string variable, float @floatingPointNumber)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @floatingPointNumber));
        }

        internal void SendUintData(string variable, uint @unsignedInteger)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @unsignedInteger));
        }

        internal void SendLongData(string variable, float @floatingPointNumber)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @floatingPointNumber));
        }

        internal void SendData(string variable, Type @data)
        {
            SendPayload(System.Text.Encoding.UTF8.GetBytes(variable + "," + @data));
        }

        internal void SendPayload(byte[] buffer)
        {
            if (stream == null)
            {
                PartY.instance.OnDisconnect(this);
                return;
            }

            stream.Write(buffer, 0, buffer.Length);
        }
        #endregion
    }
}