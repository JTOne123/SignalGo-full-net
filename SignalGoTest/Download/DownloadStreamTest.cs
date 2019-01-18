﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalGoTest2.Models;
using SignalGoTest2Services.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace SignalGoTest.Download
{
    [TestClass]
    public class DownloadStreamTest
    {
        [TestMethod]
        public void TestDownload()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerStreamModel service = client.RegisterStreamServiceInterfaceWrapper<ITestServerStreamModel>();
            SignalGo.Shared.Models.StreamInfo<string> result = service.DownloadImage("hello world", new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
            byte[] bytes = new byte[1024];
            int readLen = result.Stream.ReadAsync(bytes, 1024).GetAwaiter().GetResult();
            System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);
        }

        [TestMethod]
        public async Task TestDownloadAsync()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            ITestServerStreamModel service = client.RegisterStreamServiceInterfaceWrapper<ITestServerStreamModel>();
            SignalGo.Shared.Models.StreamInfo<string> result = await service.DownloadImageAsync("hello world", new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
            byte[] bytes = new byte[1024];
            int readLen = await result.Stream.ReadAsync(bytes, 1024);
            System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);
        }

        [TestMethod]
        public void TestUpload()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] bytes = new byte[1024 * 512];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(i / 255);
                }
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                ITestServerStreamModel service = client.RegisterStreamServiceInterfaceWrapper<ITestServerStreamModel>();
                string result = service.UploadImage("hello world", new SignalGo.Shared.Models.StreamInfo()
                {
                    Length = memoryStream.Length,
                    Stream = memoryStream
                }, new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
                Assert.IsTrue(result == "success");
            }
        }

        [TestMethod]
        public async Task TestUploadAsync()
        {
            GlobalInitalization.Initialize();
            SignalGo.Client.ClientProvider client = GlobalInitalization.InitializeAndConnecteClient();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] bytes = new byte[1024 * 512];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(i / 255);
                }
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                ITestServerStreamModel service = client.RegisterStreamServiceInterfaceWrapper<ITestServerStreamModel>();
                string result = await service.UploadImageAsync("hello world", new SignalGo.Shared.Models.StreamInfo()
                {
                    Length = memoryStream.Length,
                    Stream = memoryStream
                }, new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
                Assert.IsTrue(result == "success");
            }
        }




        [TestMethod]
        public void TestDownloadCross()
        {
            GlobalInitalization.Initialize();
            ITestServerStreamModel service = new SignalGoTest2Services.StreamServices.TestServerStreamModel("localhost", 1132);
            SignalGo.Shared.Models.StreamInfo<string> result = service.DownloadImage("hello world", new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
            byte[] bytes = new byte[1024];
            int readLen = result.Stream.ReadAsync(bytes, 1024).GetAwaiter().GetResult();
            System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);

        }

        [TestMethod]
        public async Task TestDownloadCrossAsync()
        {
            GlobalInitalization.Initialize();
            ITestServerStreamModel service = new SignalGoTest2Services.StreamServices.TestServerStreamModel("localhost", 1132);
            SignalGo.Shared.Models.StreamInfo<string> result = await service.DownloadImageAsync("hello world", new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
            byte[] bytes = new byte[1024];
            int readLen = await result.Stream.ReadAsync(bytes, 1024);
            System.Diagnostics.Trace.Assert(result.Data == "hello return" && readLen == 4 && bytes[0] == 2 && bytes[1] == 5 && bytes[2] == 8 && bytes[3] == 9);
        }

        [TestMethod]
        public void TestUploadCross()
        {
            GlobalInitalization.Initialize();
            ITestServerStreamModel service = new SignalGoTest2Services.StreamServices.TestServerStreamModel("localhost", 1132);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] bytes = new byte[1024 * 512];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(i / 255);
                }
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                string result = service.UploadImage("hello world", new SignalGo.Shared.Models.StreamInfo()
                {
                    Length = memoryStream.Length,
                    Stream = memoryStream
                }, new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
                Assert.IsTrue(result == "success");
            }
        }

        [TestMethod]
        public async Task TestUploadCrossAsync()
        {
            GlobalInitalization.Initialize();
            ITestServerStreamModel service = new SignalGoTest2Services.StreamServices.TestServerStreamModel("localhost", 1132);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] bytes = new byte[1024 * 512];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = (byte)(i / 255);
                }
                memoryStream.Write(bytes, 0, bytes.Length);
                memoryStream.Seek(0, SeekOrigin.Begin);
                string result = await service.UploadImageAsync("hello world", new SignalGo.Shared.Models.StreamInfo()
                {
                    Length = memoryStream.Length,
                    Stream = memoryStream
                }, new TestStreamModel() { Name = "test name", Values = new System.Collections.Generic.List<string>() { "value test 1", "value test 2" } });
                Assert.IsTrue(result == "success");
            }
        }
    }
}
