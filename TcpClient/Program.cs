using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;

public class clnt
{
    public static void Main()
    {
        try
        {
            // Update JSON file path to D:\NodeRed\IncomingRecords.txt
            string jsonFilePath = @"D:\NodeRed\";
            List<Record> records = ReadRecordsFromJson(jsonFilePath + "IncomingRecords.txt");
            var ipAddress = records[0].ipAddress;

            if (records.Count == 0)
            {
                Console.WriteLine("No records found in the JSON file.");
                return;
            }
            int[] portsList = [8562, 8568, 8521];
            // Iterate over each record and send requests sequentially
            foreach (var port in portsList)
            {
                List<GateRecord> gateRecords = ReadGateRecordsFromJson(jsonFilePath + "updatedSlots.txt");

                // Split ipAddress to get IP and Port
                string[] parts = ipAddress.Split(':');
                string ip = parts[0];
                //int port = int.Parse(parts[1]);

                // Assuming you want to send 'maximum' and 'minimum' fields as part of the message
                string message = $"{ipAddress}:{port}";

                // Add logic to handle inputgate and outputgate if required
                foreach (var gate in gateRecords)
                {
                    message += $":{gate.parkingSlots}:{gate.minimumSlots}";
                }

                TCPClient(ip, port, message);

                System.Threading.Thread.Sleep(10000); // 10 seconds delay
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
        }
    }

    public static List<Record> ReadRecordsFromJson(string filePath)
    {
        List<Record> records = new List<Record>();

        try
        {
            string json = File.ReadAllText(filePath);
            records = JsonConvert.DeserializeObject<List<Record>>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading JSON file: {ex.Message}");
        }

        return records;
    }

    public static List<GateRecord> ReadGateRecordsFromJson(string filePath)
    {
        List<GateRecord> records = new List<GateRecord>();

        try
        {
            string[] jsonLines = File.ReadAllLines(filePath);

            foreach (string line in jsonLines)
            {
                // Deserialize each line (assuming each line is a separate JSON object)
                GateRecord record = JsonConvert.DeserializeObject<GateRecord>(line);

                // Add the record to the list
                records.Add(record);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading JSON file: {ex.Message}");
        }

        return records;
    }

    public static void TCPClient(string ip, int port, string message)
    {
        try
        {
            // Example of TCP client connection
            TcpClient tcpclnt = new TcpClient();
            Console.WriteLine("Connecting to {0}:{1}.....", ip, port);

            try
            {
                // Attempt to connect with a timeout of 5 seconds
                IAsyncResult result = tcpclnt.BeginConnect(ip, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(5000); // 5 seconds timeout

                if (success && tcpclnt.Connected)
                {
                    tcpclnt.EndConnect(result);
                    Console.WriteLine("Connected");

                    // Send message
                    SendMessage(tcpclnt, message);
                }
                else
                {
                    Console.WriteLine($"Connection to {ip}:{port} timed out or failed.");
                }

                tcpclnt.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to {ip}:{port}: {ex.Message}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in TCPClient: {e.Message}");
        }
    }

    private static void SendMessage(TcpClient client, string endpoint)
    {
        try
        {
            Stream stm = client.GetStream();
            byte[] ba = System.Text.Encoding.ASCII.GetBytes(endpoint);
            stm.Write(ba, 0, ba.Length);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error in SendMessage: {e.Message}");
        }
    }
}

public class Record
{
    public string host { get; set; }
    public int port { get; set; }
    public string ipAddress { get; set; }
    public int controllerId { get; set; }
}

public class GateRecord
{
    public int parkingSlots { get; set; }
    public int minimumSlots { get; set; }
}