using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3.Model;
using Amazon.S3;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Amazon.Lambda.Serialization;
using ThirdParty.Json.LitJson;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Util;
using System.Web;



using Amazon.S3.Util;
using System.Text.Json;
// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace c2panalyze2;

public class Function
{
    private readonly IAmazonS3 _s3Client;

    public Function()

    {

        _s3Client = new AmazonS3Client();

    }

    public async Task<string> FunctionHandlerSign(S3Event evnt, ILambdaContext context)

    {

        var s3Event = evnt.Records?.FirstOrDefault();

        if (s3Event == null)
        {
            return "No S3 event detected.";
        }

        string s3BucketPathSigned = "data_sign";

        try
        {
            s3BucketPathSigned = Environment.GetEnvironmentVariable("s3BucketPathSigned").TrimStart('/');
        }
        catch
        {
        }

        string s3BucketPath = "data";

        try
        {
            s3BucketPath = Environment.GetEnvironmentVariable("s3BucketPath").TrimStart('/');
        }
        catch
        {
        }



        string bucketName = s3Event.S3.Bucket.Name;

        string fileName = s3Event.S3.Object.Key;

        Console.WriteLine("s3BucketPath " + s3BucketPath);
        Console.WriteLine("s3BucketPathSigned " + s3BucketPathSigned);
        Console.WriteLine("bucketName " + bucketName);
        Console.WriteLine("fileName " + fileName);

        string extension = System.IO.Path.GetExtension(fileName);

        string _outputDirectory = "/tmp/" + fileName.Replace(extension, "");

        string _tmpFilename = "/tmp/" + fileName;

        string _tmpFilenameSigned = "/tmp/" + fileName.Replace(extension, "") + "_signed" + extension;

        Console.WriteLine("_tmpFilename " + _tmpFilename);

        Console.WriteLine("_tmpFilenameSigned " + _tmpFilenameSigned);

        Console.WriteLine("_outputDirectory " + _outputDirectory);


        try

        {

            Console.WriteLine("get file");
            var getRequest = new GetObjectRequest

            {
                BucketName = bucketName,
                Key = fileName

            };
            var response = _s3Client.GetObjectAsync(getRequest).GetAwaiter().GetResult();
            response.WriteResponseStreamToFileAsync(_tmpFilename, false, new CancellationTokenSource().Token).GetAwaiter().GetResult();

        }
        catch (Exception e)
        {
            Console.WriteLine("get File failed " + e.Message + "@" + e.StackTrace);
        }


        try
        {

            processC2PA run3 = new processC2PA(_tmpFilename, _outputDirectory);

            string result3 = run3.runSign(_tmpFilenameSigned);

            Console.WriteLine("runSign Result " + result3);

            s3Load s3Loader1 = new s3Load("", "", "eu-central-1");

            List<string> _ingredientFiles1 = new List<string>();
            _ingredientFiles1.Add(_tmpFilenameSigned);

            Console.WriteLine("Upload file Sign " + _tmpFilenameSigned);

            string s3result1 = s3Loader1.putS3Files(_ingredientFiles1, bucketName, s3BucketPathSigned).GetAwaiter().GetResult();

            Console.WriteLine("putS3Files Result " + s3result1);

            File.Delete(_tmpFilenameSigned);
            File.Delete(_tmpFilename);
        }
        catch (Exception e)
        {
            Console.WriteLine("RunSign or Upload failed Error " + e.Message + "@" + e.StackTrace);
        }


        return "ok";
    }


}