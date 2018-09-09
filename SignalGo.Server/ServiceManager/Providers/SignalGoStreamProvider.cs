﻿using SignalGo.Server.Helpers;
using SignalGo.Server.Models;
using SignalGo.Shared.DataTypes;
using SignalGo.Shared.Http;
using SignalGo.Shared.IO;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignalGo.Server.ServiceManager.Providers
{
    public class SignalGoStreamProvider : BaseProvider
    {
        public static void StartToReadingClientData(ClientInfo client, ServerBase serverBase)
        {
#if (NET40 || NET35)
            Task.Factory.StartNew(() =>
#else
            Task.Run(() =>
#endif
            {
                int taskId = Task.CurrentId.GetValueOrDefault();
                try
                {
                    serverBase.AddTask(taskId, client.ClientId);
                    Console.WriteLine($"Stream Client Connected: {client.IPAddress}");
                    PipeNetworkStream stream = client.ClientStream;
                    byte firstByte = client.StreamHelper.ReadOneByte(stream);
                    if (firstByte == 0)
                    {
                        DownloadStreamFromClient(client, serverBase);
                    }
                    //download from server and upload from client
                    else
                    {
                        UploadStreamToClient(client, serverBase);
                    }
                    serverBase.DisposeClient(client, "StartToReadingClientData finished");
                }
                catch (Exception ex)
                {
                    serverBase.AutoLogger.LogError(ex, $"{client.IPAddress} {client.ClientId} ServerBase SignalGoStreamProvider StartToReadingClientData");
                    serverBase.DisposeClient(client, "SignalGoStreamProvider StartToReadingClientData exception");
                }
                finally
                {
                    serverBase.RemoveTask(taskId);
                }
            });
        }

        /// <summary>
        /// this method call when client want to upload file or stream to your server
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <param name="client">client</param>
        private static void DownloadStreamFromClient(ClientInfo client, ServerBase serverBase)
        {
            MethodCallbackInfo callback = null;
            string guid = Guid.NewGuid().ToString();
            try
            {
                byte[] bytes = client.StreamHelper.ReadBlockToEnd(client.ClientStream, CompressMode.None, serverBase.ProviderSetting.MaximumReceiveStreamHeaderBlock);
                string json = Encoding.UTF8.GetString(bytes);
                MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, serverBase);

                callback = CallMethod(callInfo.ServiceName, callInfo.Guid, callInfo.MethodName,
                    callInfo.Parameters, client, null, serverBase, null, null,
                   out IStreamInfo streamInfo, out List<HttpKeyAttribute> httpKeyAttributes, out Type serviceType, out System.Reflection.MethodInfo method, out object service, out FileActionResult fileActionResult);
            }
            catch (IOException ex)
            {
                callback = new MethodCallbackInfo();
                callback.IsException = true;
                callback.Data = ServerSerializationHelper.SerializeObject(ex);
                //return;
            }
            catch (Exception ex)
            {
                callback = new MethodCallbackInfo();
                callback.IsException = true;
                callback.Data = ServerSerializationHelper.SerializeObject(ex);
            }
            finally
            {
            }
            SendCallbackData(callback, client, serverBase);
        }

        /// <summary>
        /// this method calll when client want to download file or stream from your server
        /// </summary>
        /// <param name="stream">client stream</param>
        /// <param name="client">client</param>
        private static void UploadStreamToClient(ClientInfo client, ServerBase serverBase)
        {
            MethodCallbackInfo callback = null;
            IStreamInfo streamInfo = null;
            PipeNetworkStream userStream = null;
            PipeNetworkStream stream = client.ClientStream;
            bool isCallbackSended = false;
            try
            {
                byte[] bytes = client.StreamHelper.ReadBlockToEnd(client.ClientStream, CompressMode.None, serverBase.ProviderSetting.MaximumReceiveStreamHeaderBlock);
                string json = Encoding.UTF8.GetString(bytes);
                MethodCallInfo callInfo = ServerSerializationHelper.Deserialize<MethodCallInfo>(json, serverBase);
                callback = CallMethod(callInfo.ServiceName, callInfo.Guid, callInfo.MethodName,
                    callInfo.Parameters, client, null, serverBase, null, null,
                   out streamInfo, out List<HttpKeyAttribute> httpKeyAttributes, out Type serviceType, out System.Reflection.MethodInfo method, out object service, out FileActionResult fileActionResult);

                userStream = streamInfo.Stream;
                long len = streamInfo.Length;
                SendCallbackData(callback, client, serverBase);
                isCallbackSended = true;
                long writeLen = 0;
                while (writeLen < len)
                {
                    bytes = userStream.Read(1024 * 100, out int readCount);
                    stream.Write(bytes.Take(readCount).ToArray());
                    writeLen += readCount;
                }
                userStream.Dispose();
                Console.WriteLine("user stream finished");
                stream.Dispose();

            }
            catch (Exception ex)
            {
                if (streamInfo == null)
                    streamInfo.Dispose();
                stream.Dispose();
                if (userStream != null)
                {
                    userStream.Dispose();
                    Console.WriteLine("user stream disposed");
                }
                if (!isCallbackSended)
                {
                    if (callback == null)
                        callback = new MethodCallbackInfo();
                    callback.IsException = true;
                    callback.Data = ServerSerializationHelper.SerializeObject(ex);
                    SendCallbackData(callback, client, serverBase);
                }
            }
            finally
            {
                //MethodsCallHandler.EndStreamCallAction?.Invoke(client, guid, serviceName, methodName, values, jsonResult, exception);
            }
        }
    }
}