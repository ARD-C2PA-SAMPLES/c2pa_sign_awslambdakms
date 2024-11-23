﻿using Amazon.KeyManagementService;

MemoryStream input = new MemoryStream();

try
{
    using (Stream stdin = Console.OpenStandardInput())
    {
        byte[] buffer = new byte[2048];
        int bytes;
        while ((bytes = stdin.Read(buffer, 0, buffer.Length)) > 0)
        {
            input.Write(buffer, 0, bytes);
        }
    }
    var client = new AmazonKeyManagementServiceClient();

    var signResponse = await client.SignAsync(new Amazon.KeyManagementService.Model.SignRequest()
    {
        KeyId = "<put in here your KMS KeyId>",
        MessageType = MessageType.RAW,
        SigningAlgorithm = SigningAlgorithmSpec.ECDSA_SHA_256,
        Message = input
    });

    MemoryStream output = new System.IO.MemoryStream();

    signResponse.Signature.CopyTo(output);

    output.Position = 0;

    output.CopyTo(Console.OpenStandardOutput());


}
catch (System.Exception e)
{ Console.WriteLine(e.Message); }
