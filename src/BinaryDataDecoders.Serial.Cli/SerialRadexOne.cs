﻿using BinaryDataDecoders.IO.Pipelines;
using BinaryDataDecoders.IO.Ports;
using BinaryDataDecoders.Quarta.RadexOne;
using BinaryDataDecoders.ToolKit;
using System;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BinaryDataDecoders.Serial.Cli
{
    public class RadexOneFactory
    {
        public ISegmenter GetSegmenter(OnSegmentReceived received) =>
              Segment.StartsWith(0x7a)
                     .AndIsLength(12)
                     .ExtendedWithLengthAt<ushort>(4, Endianness.Little)
                     .WithOptions(SegmentionOptions.SkipInvalidSegment)
                     .ThenDo(received);
    }
    [SerialPort(BaudRate = 9600)]
    public class SerialRadexOne
    {
        public static void Execute()
        {
            //var ws = Enumerable.Range(0, 100)
            //                   .Select(i => new WriteSettingsRequest((ushort)i, (AlarmSettings)(i % 4), (ushort)(i * 10)))
            //                   .ToArray();
            //var sws = ws.AsSpan();
            //var swsd = MemoryMarshal.Cast<WriteSettingsRequest, byte>(sws);
            //var swsa = swsd.ToArray();

            var factory = new RadexOneFactory();
            var decoder = new RadexOneDecoder();

            var segmenter = factory.GetSegmenter(data =>
            {
                var result = decoder.Decode(data);
                Console.WriteLine(result);

                return Task.FromResult(0);
            });

            var portNames = SerialPort.GetPortNames().OrderBy(s => s);
            foreach (var portName in portNames)
                Console.WriteLine(portName);

            Console.WriteLine($"Enter Port: (Default { portNames.FirstOrDefault()})");
            var portNameInput = Console.ReadLine();
            var serialPort = new PortProvider().GetRadexOnePort(string.IsNullOrWhiteSpace(portNameInput) ? portNames.FirstOrDefault() : portNameInput);

            using var port = serialPort;
            using var cts = new CancellationTokenSource();

            port.Open();

            Console.Write("Enter to exit");

            Task.WaitAll(
              Task.Run(async () => await Program.ReadLineAsync().ContinueWith(t => cts.Cancel(false))),
              Task.Run(async () =>
              {
                  while (!cts.IsCancellationRequested)
                  {
                      try
                      {
                          await port.BaseStream.Follow().With(segmenter).RunAsync(cts.Token);
                          cts.Cancel(true);
                      }
                      catch (Exception ex)
                      {
                          Console.Error.WriteLine(ex.Message);
                      }
                  }
              }),
              Task.Run(async () =>
              {
                  ushort x = 0;
                  while (!cts.IsCancellationRequested)
                  {
                      x++;
                      try
                      {
                          IRadexObject requestObject = (x % 10) switch
                          {
                              8 => new ReadSettingsRequest(x),

                              1 => new ReadSerialNumberRequest(x),
                              2 => new ReadSerialNumberRequest(x),

                              3 => new DevicePing(x),
                              0 => new DevicePing(x),

                              //4 => new WriteSettingsRequest(x, AlarmSettings.Audio, 30),
                              //5 => new WriteSettingsRequest(x, AlarmSettings.Audio, 30),
                              //6 => new WriteSettingsRequest(x, AlarmSettings.Audio, 30),

                              _ => new ReadValuesRequest(x)
                          };
                          var requestBuffer = new byte[Marshal.SizeOf(requestObject)];
                          IntPtr ptr = Marshal.AllocHGlobal(requestBuffer.Length);
                          Marshal.StructureToPtr(requestObject, ptr, true);
                          Marshal.Copy(ptr, requestBuffer, 0, requestBuffer.Length);
                          Marshal.FreeHGlobal(ptr);

                          var hex = requestBuffer.ToHexString();

                          //7BFF 2000 _600 1800 ____ 4600 __08 _C00 F3F7
                          await port.BaseStream.WriteAsync(requestBuffer, 0, requestBuffer.Length, cts.Token);
                      }
                      catch (Exception ex)
                      {
                          Console.Error.WriteLine(ex.Message);
                      }
                      if (!cts.IsCancellationRequested)
                      {
                          await Task.Delay(1000);
                      }
                  }
              })
              );
        }
    }
}
